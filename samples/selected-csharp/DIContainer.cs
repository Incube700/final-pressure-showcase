// Curated FINAL PRESSURE portfolio sample. Public namespace adapted for presentation.
using System;
using System.Collections.Generic;

namespace FinalPressure.Infrastructure.DI
{
    public class DIContainer
    {
        private readonly Dictionary<Type, Registration> _registrations = new();

        // Tracks in-flight resolves to detect circular dependencies.
        private readonly List<Type> _resolveStack = new();

        private readonly DIContainer _parent;

        public DIContainer() : this(null)
        {
        }

        public DIContainer(DIContainer parent) => _parent = parent;

        public IRegistrationOptions RegisterAsSingle<T>(Func<DIContainer, T> creator)
        {
            if (IsRegistered<T>())
                throw new InvalidOperationException($"{typeof(T)} is already registered");

            Registration registration = new Registration(container => creator.Invoke(container));
            _registrations.Add(typeof(T), registration);
            return registration;
        }

        public bool IsRegistered<T>()
        {
            if (_registrations.ContainsKey(typeof(T)))
                return true;

            if (_parent != null)
                return _parent.IsRegistered<T>();

            return false;
        }

        public T Resolve<T>()
        {
            if (_resolveStack.Contains(typeof(T)))
                throw new InvalidOperationException($"Circular dependency while resolving {typeof(T)}");

            _resolveStack.Add(typeof(T));

            try
            {
                if (_registrations.TryGetValue(typeof(T), out Registration registration))
                    return (T)registration.GetInstance(this);

                if (_parent != null)
                    return _parent.Resolve<T>();
            }
            finally
            {
                _resolveStack.Remove(typeof(T));
            }

            throw new InvalidOperationException($"Registration for {typeof(T)} does not exist");
        }

        public void Initialize()
        {
            foreach (Registration registration in _registrations.Values)
            {
                if (registration.IsNonLazy)
                    registration.GetInstance(this);

                registration.OnInitialize();
            }
        }

        public void Dispose()
        {
            foreach (Registration registration in _registrations.Values)
                registration.OnDispose();
        }
    }
}
