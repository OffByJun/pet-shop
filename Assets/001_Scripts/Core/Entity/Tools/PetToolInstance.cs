using _001_Scripts.Data.Tools;
using UnityEngine;

namespace _001_Scripts.Core.Entity
{
    /// <summary>공유 도구 정의를 참조하는 씬 개체입니다.</summary>
    public sealed class PetToolInstance : GameEntity
    {
        [SerializeField] private PetToolDefinition definition;

        public PetToolDefinition Definition => definition;
        public override string DefinitionId => definition == null ? string.Empty : definition.ToolId;
        public override string DisplayName => definition == null ? name : definition.DisplayName;

        public void Initialize(PetToolDefinition tool) => definition = tool;
    }
}
