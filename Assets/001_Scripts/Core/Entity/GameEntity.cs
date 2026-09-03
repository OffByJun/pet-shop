using _001_Scripts.Core;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.World;
using UnityEngine;

namespace _001_Scripts.Core.Entity
{
    /// <summary>씬에 존재하는 게임 개체의 공통 식별 정보와 활성 수명입니다.</summary>
    [DisallowMultipleComponent]
    public abstract class GameEntity : GameBehaviour
    {
        /// <summary>현재 실행에서만 유효합니다. 저장 데이터의 ID로 사용하지 않습니다.</summary>
        public EntityId EntityId => GetEntityId();
        public IWorldContext World { get; internal set; }
        public abstract string DefinitionId { get; }
        public abstract string DisplayName { get; }

        protected virtual void OnEnable()
        {
            GamePipe.Publish(new EntityRegisterRequest(this));
        }

        protected virtual void OnDisable()
        {
            World?.Unregister(this);
        }

        protected virtual void OnDestroy()
        {
            World?.Unregister(this);
        }
    }
}
