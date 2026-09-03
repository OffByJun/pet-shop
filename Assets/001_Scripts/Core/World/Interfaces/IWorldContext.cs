using System;
using System.Collections.Generic;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes.Pipes;
using UnityEngine;

namespace _001_Scripts.Core.World
{
    /// <summary>엔티티와 시스템이 참조하는 월드 API입니다. Unity 메인 스레드에서 사용합니다.</summary>
    public interface IWorldContext
    {
        bool IsActive { get; }
        int Count { get; }
        IReadOnlyCollection<GameEntity> Entities { get; }
        bool Register(GameEntity entity);
        bool Unregister(GameEntity entity);
        bool Contains(GameEntity entity);
        bool TryGet(EntityId entityId, out GameEntity entity);
        void GetEntities<T>(List<T> results) where T : GameEntity;
        T GetSystem<T>() where T : class, IWorldSystem;
        bool Publish<T>(in T message) where T : struct, IPipeMsg;
        IDisposable Subscribe<T>(Action<T> handler) where T : struct, IPipeMsg;
    }
}
