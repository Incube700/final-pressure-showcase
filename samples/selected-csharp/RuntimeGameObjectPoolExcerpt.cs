// Curated FINAL PRESSURE portfolio excerpt.
// Pool storage, diagnostics, helpers, and project types are omitted.
// This file demonstrates production spawn/release invariants; it is not standalone compilable code.

using System.Collections.Generic;
using UnityEngine;

namespace FinalPressure.Gameplay.Features.FX
{
    public static partial class RuntimeGameObjectPool
    {
        public static GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null,
            float scaleMultiplier = 1f,
            float lifetime = 0f,
            bool stripColliders = true)
        {
            if (prefab == null)
                return null;
            if (IsFinite(position) == false || IsFinite(rotation) == false || IsFinite(scaleMultiplier) == false)
            {
                RecordInvalidPosition(prefab.name);
                return null;
            }

            int key = prefab.GetEntityId().GetHashCode();
            GameObject instance = TakeOrCreate(prefab, key, stripColliders);
            PooledGameObject handle = GetHandle(instance);
            handle.ResetForSpawn();
            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(parent, false);
            instanceTransform.SetPositionAndRotation(position, rotation);
            instanceTransform.localScale = handle.BaseScale * Mathf.Max(0.05f, scaleMultiplier);
            handle.MarkSpawned();
            ActiveInstances.Add(instance);
            instance.SetActive(true);
            handle.RestorePresentationState();
            RestartPresentation(instance);
            ArmLifetime(instance, lifetime);
            RecordSpawn(handle.FamilyId, IsAtWorldOrigin(instance));
            return instance;
        }

        public static void ReleaseOrDestroy(GameObject instance)
        {
            if (instance == null)
                return;

            PooledGameObject handle = instance.GetComponent<PooledGameObject>();
            if (handle == null)
            {
                ActiveInstances.Remove(instance);
                KillTweens(instance);
                StopPresentation(instance);
                instance.GetComponent<VfxLifecycleTag>()?.NotifyPoolRelease(visibleAfterRelease: false);
                DestroyRuntimeObject(instance);
                return;
            }

            if (handle.TryMarkReleased() == false)
                return;

            ActiveInstances.Remove(instance);
            KillTweens(instance);
            StopPresentation(instance);
            handle.DisablePresentationState();

            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(GetRoot(), false);
            instanceTransform.localPosition = Vector3.zero;
            instanceTransform.localRotation = Quaternion.identity;
            instanceTransform.localScale = handle.BaseScale;

            bool visibleAfterRelease = handle.HasEnabledRenderer();
            instance.GetComponent<VfxLifecycleTag>()?.NotifyPoolRelease(visibleAfterRelease);
            instance.SetActive(false);
            RecordRelease(handle.FamilyId, visibleAfterRelease);

            if (Pools.TryGetValue(handle.PoolKey, out Stack<GameObject> pool) == false)
            {
                pool = new Stack<GameObject>();
                Pools.Add(handle.PoolKey, pool);
            }

            if (pool.Count >= MaxPooledPerPrefab)
            {
                DestroyRuntimeObject(instance);
                return;
            }

            pool.Push(instance);
        }

        public static void ReleaseAllActive()
        {
            if (ActiveInstances.Count == 0)
                return;

            GameObject[] snapshot = new GameObject[ActiveInstances.Count];
            ActiveInstances.CopyTo(snapshot);
            for (int i = 0; i < snapshot.Length; i++)
                ReleaseOrDestroy(snapshot[i]);

            ActiveInstances.Clear();
        }
    }
}
