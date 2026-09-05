using _001_Scripts.Data.Economy;
using UnityEngine;

namespace _001_Scripts.Data
{
    [CreateAssetMenu(menuName = "PetShop/Routine/Decoration")]
    public sealed class ShopDecorationDefinition : ScriptableObject
    {
        [SerializeField] private string decorationId;
        [SerializeField] private string displayName;
        [SerializeField, Min(0)] private int cost;
        [SerializeField] private Color accentColor = Color.white;
        [SerializeField] private Sprite artwork;
        public string DecorationId => decorationId;
        public string DisplayName => displayName;
        public int Cost => cost;
        public Color AccentColor => accentColor;
        public Sprite Artwork => artwork;
        public ExpenseQuote Quote => new ExpenseQuote(decorationId, ExpenseCategory.StoreEquipment, cost);
    }
}
