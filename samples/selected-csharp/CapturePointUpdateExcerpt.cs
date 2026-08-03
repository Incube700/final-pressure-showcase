// Curated FINAL PRESSURE portfolio excerpt.
// Surrounding fields, constructor, reward helpers, views, and project types are omitted.
// This file demonstrates production capture-state flow; it is not standalone compilable code.

using System;
using UnityEngine;

namespace FinalPressure.Gameplay.Features.Progression
{
    public sealed partial class CapturePointService
    {
        public void Update(float deltaTime)
        {
            if (_context == null || _playerPosition == null || deltaTime <= 0f)
                return;

            Vector3 playerPosition = _playerPosition.Position;
            _capturedCount = 0;
            string activePointName = null;
            string activePointDisplayName = null;
            float activeRemainingSeconds = 0f;
            float activeProgress01 = 0f;
            int activeSquadAssistCount = 0;

            for (int i = 0; i < _context.Points.Count; i++)
            {
                CapturePointState point = _context.Points[i];
                if (point.IsCaptured)
                {
                    UpdateMarker(point, disabled: false, capturing: false);
                    _capturedCount++;
                    UpdateCapturedReward(point, deltaTime);
                    continue;
                }

                Vector3 toPoint = playerPosition - point.Position;
                toPoint.y = 0f;

                bool isInside = toPoint.sqrMagnitude <= point.CaptureRadius * point.CaptureRadius;
                bool disabled = CountLivingSiteGuardians(point.Name) > 0;
                if (isInside == false)
                {
                    point.Progress = Mathf.Max(0f, point.Progress - deltaTime * 0.6f);
                    UpdateMarker(point, disabled, capturing: false);
                    continue;
                }

                TryRequestSiteGuardian(point);
                disabled = CountLivingSiteGuardians(point.Name) > 0;
                if (disabled)
                {
                    point.Progress = Mathf.Max(0f, point.Progress - deltaTime * 0.6f);
                    UpdateMarker(point, disabled: true, capturing: false);
                    continue;
                }

                int squadAssistCount = GetSquadAssistCount(point);
                float captureRate = GetCaptureRate(squadAssistCount);
                bool wasCaptureStarted = point.Progress <= 0f;
                point.Progress = Mathf.Min(point.CaptureSeconds, point.Progress + deltaTime * captureRate);
                if (wasCaptureStarted && point.Progress > 0f)
                    _captureStarted?.Invoke(point.Name);

                if (point.Progress < point.CaptureSeconds)
                {
                    UpdateMarker(point, disabled: false, capturing: true);
                    activePointName = point.Name;
                    activePointDisplayName = CapturePointDisplayNameService.Format(point.Name);
                    activeRemainingSeconds = captureRate > 0f
                        ? Mathf.Max(0f, point.CaptureSeconds - point.Progress) / captureRate
                        : Mathf.Max(0f, point.CaptureSeconds - point.Progress);
                    activeProgress01 = point.CaptureSeconds <= 0f ? 1f : point.Progress / point.CaptureSeconds;
                    activeSquadAssistCount = squadAssistCount;
                    continue;
                }

                point.IsCaptured = true;
                point.RewardCooldown = FirstRewardDelaySeconds;
                _capturedCount++;
                _challengeRating?.RegisterCaptureProgress(_capturedCount, _context.Points.Count);
                _wallet?.AddScrap((int)DefaultRewardCoins);
                _experienceCollected?.Invoke(RunExperienceSource.Objective, DefaultRewardXp);
                _progression?.AddExperience(DefaultRewardXp);
                ApplyCaptureBonus(point);
                RescueSurvivorAtPoint(point);
                UpdatePointVisual(point, captured: true);
                ApplyCapturePulse(point);

                if (_captureCompleted != null)
                    _captureCompleted.Invoke(point.Name);
                else
                    _announce?.Invoke(
                        $"{CapturePointDisplayNameService.Format(point.Name)} CONNECTED — {GetPointRewardLabel(point.Kind)}",
                        Color.cyan);
            }

            if (_capturedCount == _context.Points.Count &&
                _hasAnnouncedAllCaptured == false &&
                _context.Points.Count > 0)
            {
                _hasAnnouncedAllCaptured = true;
                ApplyGridMasteryBonus();
            }

            CapturePointHudState hudState = new(
                _capturedCount,
                _context.Points.Count,
                activePointName,
                activeRemainingSeconds,
                activeProgress01,
                activeSquadAssistCount,
                activePointDisplayName);
            _updateHud?.Invoke(hudState);
        }
    }
}
