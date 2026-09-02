using _001_Scripts.Core;
using UnityEngine;

namespace _001_Scripts.Data.Items
{
    [RequireComponent(typeof(ItemInventory))]
    public sealed class ItemInventoryAcquisitionService : GameBehaviour, IItemAcquisitionService
    {
        public ItemInventory Inventory { get; private set; }
        private void Awake() => Inventory = GetComponent<ItemInventory>();
        public bool TryGrant(ItemBase item, int amount) => Inventory != null && Inventory.TryAdd(item, amount);
    }
}
