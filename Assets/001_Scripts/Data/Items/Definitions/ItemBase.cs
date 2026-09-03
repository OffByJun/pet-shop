using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data.Items
{
    /// <summary>게임에서 거래/보관되는 아이템의 변하지 않는 정의입니다.</summary>
    public abstract class ItemBase : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [TextArea(2, 5)] [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [Header("Economy")]
        [SerializeField] private ItemCategory category = ItemCategory.Material;
        [SerializeField, Min(0)] private int baseSellPrice = 1;
        [SerializeField, Min(1)] private int maxStackSize = 99;
        [Header("Metadata")]
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        [SerializeField] private string[] tags;

        public string ItemId => itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemCategory Category => category;
        public ItemRarity Rarity => rarity;
        public int BaseSellPrice => baseSellPrice;
        public int MaxStackSize => maxStackSize;
        public IReadOnlyCollection<string> Tags => tags;

        public bool HasTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || tags == null) return false;
            for (var i = 0; i < tags.Length; i++)
                if (string.Equals(tags[i], tag, System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId)) itemId = name;
            maxStackSize = Mathf.Max(1, maxStackSize);
            baseSellPrice = Mathf.Max(0, baseSellPrice);
        }
    }
}
