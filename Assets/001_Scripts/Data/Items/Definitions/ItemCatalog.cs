using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data.Items
{
    /// <summary>아이템 ID를 저장/검색해야 하는 시스템의 단일 진입점입니다.</summary>
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "PetShop/Items/Item Catalog")]
    public sealed class ItemCatalog : ScriptableObject
    {
        [SerializeField] private ItemBase[] items;

        public IReadOnlyList<ItemBase> Items => items;

        public ItemBase Find(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || items == null) return null;
            for (var i = 0; i < items.Length; i++)
                if (items[i] != null && items[i].ItemId == itemId) return items[i];
            return null;
        }

        private void OnValidate()
        {
            if (items == null) return;
            for (var i = 0; i < items.Length; i++)
                for (var j = i + 1; j < items.Length; j++)
                    if (items[i] != null && items[i] == items[j]) items[j] = null;
        }
    }
}
