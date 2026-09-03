using System;
using _001_Scripts.Data.Items;
using UnityEngine;

namespace _001_Scripts.Core.Entity
{
    /// <summary>씬에 놓인 아이템과 그 수량입니다. 인벤토리 안에서는 ItemStack을 사용합니다.</summary>
    public sealed class ItemInstance : GameEntity
    {
        [SerializeField] private ItemBase definition;
        [SerializeField, Min(1)] private int amount = 1;

        public ItemBase Definition => definition;
        public int Amount => amount;
        public override string DefinitionId => definition == null ? string.Empty : definition.ItemId;
        public override string DisplayName => definition == null ? name : definition.DisplayName;

        public void Initialize(ItemBase item, int quantity = 1)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            definition = item;
            amount = quantity;
        }
    }
}
