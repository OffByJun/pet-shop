using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data.Tools
{
    [CreateAssetMenu(fileName = "PetToolCatalog", menuName = "PetShop/Tools/Pet Tool Catalog")]
    public sealed class PetToolCatalog : ScriptableObject
    {
        [SerializeField] private PetToolDefinition[] tools = new PetToolDefinition[0];
        public IReadOnlyList<PetToolDefinition> Tools => tools;

        public PetToolDefinition Find(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId)) return null;
            for (var i = 0; i < tools.Length; i++)
                if (tools[i] != null && tools[i].ToolId == toolId) return tools[i];
            return null;
        }
    }
}
