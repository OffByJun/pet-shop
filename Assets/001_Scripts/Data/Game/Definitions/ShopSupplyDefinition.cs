using _001_Scripts.Data.Economy;
using UnityEngine;

namespace _001_Scripts.Data
{
    /// <summary>케어 한 건마다 소모되는 보급품입니다. 떨어지면 그 케어를 할 수 없습니다.</summary>
    [CreateAssetMenu(fileName = "ShopSupply", menuName = "PetShop/Routine/Shop Supply")]
    public sealed class ShopSupplyDefinition : ScriptableObject
    {
        [SerializeField] private string supplyId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [Tooltip("한 묶음의 가격입니다.")]
        [SerializeField, Min(0)] private int packCost = 40;
        [Tooltip("한 번 구매할 때 들어오는 개수입니다.")]
        [SerializeField, Min(1)] private int packSize = 5;
        [Tooltip("새 게임을 시작할 때 들고 있는 개수입니다.")]
        [SerializeField, Min(0)] private int startingStock = 6;

        public string SupplyId => string.IsNullOrWhiteSpace(supplyId) ? name : supplyId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public Sprite Icon => icon;
        public int PackCost => packCost;
        public int PackSize => packSize;
        public int StartingStock => startingStock;

        public ExpenseQuote Quote => new ExpenseQuote(SupplyId, ExpenseCategory.StoreEquipment, packCost);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(supplyId)) supplyId = name;
            packCost = Mathf.Max(0, packCost);
            packSize = Mathf.Max(1, packSize);
            startingStock = Mathf.Max(0, startingStock);
        }
    }
}
