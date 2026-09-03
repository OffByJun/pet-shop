using _001_Scripts.UI.Components;

namespace _001_Scripts.UI.UILib
{
    public readonly struct UIActionContext
    {
        public UIActionContext(UIComponent owner, UITransition transition, UIActionTiming timing)
        {
            Owner = owner;
            Transition = transition;
            Timing = timing;
        }

        public UIComponent Owner { get; }

        public UITransition Transition { get; }

        public UIActionTiming Timing { get; }
    }
}
