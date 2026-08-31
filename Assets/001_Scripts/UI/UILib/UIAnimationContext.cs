using _001_Scripts.UI.Components;

namespace _001_Scripts.UI.UILib
{
    public readonly struct UIAnimationContext
    {
        public UIAnimationContext(
            UIComponent owner,
            UITransition transition,
            UIAnimationPreset preset)
        {
            Owner = owner;
            Transition = transition;
            Preset = preset;
            Timing = transition == UITransition.Show
                ? UIAnimationTiming.Show
                : UIAnimationTiming.Hide;
        }

        public UIComponent Owner { get; }

        public UITransition Transition { get; }

        public UIAnimationTiming Timing { get; }

        public UIAnimationPreset Preset { get; }

        public UIAnimationSettings Settings => Preset == null ? null : Preset.Get(Timing);
    }
}
