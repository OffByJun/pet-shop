using _001_Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI
{
    /// <summary>Applies purchased interior colors to explicitly authored scene surfaces.</summary>
    public sealed class ShopDecorationView : MonoBehaviour
    {
        [SerializeField] private Image[] surfaces;
        private _001_Scripts.Data.ShopDecorationDefinition applied;
        private void Update()
        {
            if (!ShopRoutineManager.HasInstance) return;
            var decoration = ShopRoutineManager.Instance.Decoration;
            if (decoration == null || decoration == applied) return;
            applied = decoration;
            foreach (var surface in surfaces) if (surface != null) surface.color = decoration.AccentColor;
        }
    }
}
