using UnityEngine;

namespace _001_Scripts.UI.UILib
{
    [CreateAssetMenu(fileName = "UIAnimationPreset", menuName = "PetShop/UI/Animation Preset")]
    public sealed class UIAnimationPreset : ScriptableObject
    {
        [SerializeField] private UIAnimationSettings show = new UIAnimationSettings();
        [SerializeField] private UIAnimationSettings hide = new UIAnimationSettings();

        public UIAnimationSettings Get(UIAnimationTiming timing)
        {
            return timing == UIAnimationTiming.Show ? show : hide;
        }

        public void SetDefaultValues()
        {
            show.SetDefaults(0.2f, 1f, Vector3.one, Vector2.zero);
            hide.SetDefaults(
                0.15f,
                0f,
                new Vector3(0.95f, 0.95f, 1f),
                new Vector2(0f, -40f));
        }
    }
}
