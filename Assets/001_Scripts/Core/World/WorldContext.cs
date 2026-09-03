using System;
using System.Collections.Generic;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.Pipes.Pipes;
using UnityEngine;

namespace _001_Scripts.Core.World
{
    /// <summary>월드의 엔티티, 시스템 실행과 MessagePipe 구독 수명을 소유합니다.</summary>
    public sealed class WorldContext : IWorldContext, IDisposable
    {
        private readonly Dictionary<EntityId, GameEntity> entities = new Dictionary<EntityId, GameEntity>();
        private readonly List<IDisposable> subscriptions = new List<IDisposable>();
        private readonly IWorldSystem[] systems;
        private bool active;
        private bool stopping;
        private bool disposed;
        private int initializedSystems;

        public WorldContext(params IWorldSystem[] systems)
        {
            if (systems == null) throw new ArgumentNullException(nameof(systems));
            this.systems = (IWorldSystem[])systems.Clone();
            for (var i = 0; i < systems.Length; i++)
            {
                if (systems[i] == null) throw new ArgumentException("World systems cannot be null.", nameof(systems));
                for (var j = 0; j < i; j++)
                    if (systems[j].GetType() == systems[i].GetType())
                        throw new ArgumentException("A world system type can only be registered once.", nameof(systems));
            }
        }

        public bool IsActive => active && !stopping;
        public int Count => entities.Count;
        public IReadOnlyCollection<GameEntity> Entities => entities.Values;

        public void Activate()
        {
            if (disposed) throw new ObjectDisposedException(nameof(WorldContext));
            if (active) return;
            active = true;
            try
            {
                Subscribe<EntityRegisterRequest>(request => Register(request.Entity));
                Subscribe<EntityUnregisterRequest>(request => Unregister(request.Entity));
                for (var i = 0; i < systems.Length; i++)
                {
                    initializedSystems++;
                    systems[i].Initialize(this);
                }
                // 월드보다 먼저 활성화된 엔티티도 연결합니다.
                var existing = UnityEngine.Object.FindObjectsByType<GameEntity>(FindObjectsInactive.Exclude);
                for (var i = 0; i < existing.Length; i++) Register(existing[i]);
            }
            catch
            {
                Deactivate();
                throw;
            }
        }

        public void Tick(float deltaTime)
        {
            for (var i = 0; i < systems.Length && IsActive; i++) systems[i].Tick(deltaTime);
        }

        public void Deactivate()
        {
            if (!active || stopping) return;
            stopping = true;
            try
            {
                for (var i = initializedSystems - 1; i >= 0; i--)
                {
                    try { systems[i].Shutdown(); }
                    catch (Exception exception) { Debug.LogException(exception); }
                }
            }
            finally
            {
                initializedSystems = 0;
                foreach (var entity in entities.Values)
                    if (entity != null && ReferenceEquals(entity.World, this)) entity.World = null;
                entities.Clear();
                for (var i = subscriptions.Count - 1; i >= 0; i--) subscriptions[i]?.Dispose();
                subscriptions.Clear();
                active = false;
                stopping = false;
            }
        }

        public bool Register(GameEntity entity)
        {
            if (!IsActive || entity == null || !entity.isActiveAndEnabled) return false;
            if (entity.World != null && !ReferenceEquals(entity.World, this)) return false;
            if (!entities.TryAdd(entity.EntityId, entity)) return false;
            entity.World = this;
            Publish(new EntityRegistered(entity));
            return true;
        }

        public bool Unregister(GameEntity entity)
        {
            if (ReferenceEquals(entity, null) || !ReferenceEquals(entity.World, this)) return false;
            if (!entities.Remove(entity.EntityId)) return false;
            entity.World = null;
            Publish(new EntityUnregistered(entity));
            return true;
        }

        public bool Contains(GameEntity entity)
            => IsActive && entity != null && entity.isActiveAndEnabled && ReferenceEquals(entity.World, this);

        public bool TryGet(EntityId entityId, out GameEntity entity)
        {
            if (entities.TryGetValue(entityId, out entity) && Contains(entity)) return true;
            entity = null;
            return false;
        }

        public void GetEntities<T>(List<T> results) where T : GameEntity
        {
            if (results == null) throw new ArgumentNullException(nameof(results));
            results.Clear();
            foreach (var entity in entities.Values)
                if (Contains(entity) && entity is T typed) results.Add(typed);
        }

        public T GetSystem<T>() where T : class, IWorldSystem
        {
            for (var i = 0; i < systems.Length; i++)
                if (systems[i] is T system) return system;
            return null;
        }

        // 기존 허브를 재사용합니다. 종료 중에도 시스템의 취소 알림은 발행할 수 있습니다.
        public bool Publish<T>(in T message) where T : struct, IPipeMsg
            => active && GamePipe.Publish(in message);

        public IDisposable Subscribe<T>(Action<T> handler) where T : struct, IPipeMsg
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (!IsActive) throw new InvalidOperationException("The world is not active.");
            var subscription = GamePipe.Subscribe<T>(message =>
            {
                if (IsActive) handler(message);
            });
            subscriptions.Add(subscription);
            return subscription;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Deactivate();
        }
    }
}
