using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.UI.UILib;
using DG.Tweening;
using UnityEngine;

namespace _001_Scripts.UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIPositionAnimator : UIAnimatorComponent
    {
        [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -40f);
        [SerializeField] private Vector2 visibleOffset = Vector2.zero;
        [SerializeField, Min(0f)] private float duration = 0.2f;
        [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool useUnscaledTime = true;

        private RectTransform rectTransform;
        private Vector2 baseAnchoredPosition;
        private Tween activeTween;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            baseAnchoredPosition = rectTransform.anchoredPosition;
        }

        public override async Task PlayAsync(UIAnimationContext context, CancellationToken cancellationToken)
        {
            EnsureRectTransform();
            KillActiveTween();

            UIAnimationSettings settings = context.Settings;
            float animationDuration = settings == null ? duration : settings.Duration;
            AnimationCurve animationEasing = settings == null ? easing : settings.Easing;
            bool animationUsesUnscaledTime = settings == null ? useUnscaledTime : settings.UseUnscaledTime;
            Vector2 offset = settings == null
                ? (context.Transition == UITransition.Show ? visibleOffset : hiddenOffset)
                : settings.AnchoredPositionOffset;
            Vector2 target = baseAnchoredPosition + offset;

            if (animationDuration <= 0f || rectTransform.anchoredPosition == target)
            {
                cancellationToken.ThrowIfCancellationRequested();
                rectTransform.anchoredPosition = target;
                return;
            }

            Tween tween = rectTransform
                .DOAnchorPos(target, animationDuration)
                .SetEase(animationEasing)
                .SetUpdate(animationUsesUnscaledTime)
                .SetTarget(this);
            activeTween = tween;

            try
            {
                await tween.AwaitCompletionAsync(cancellationToken);
            }
            finally
            {
                if (ReferenceEquals(activeTween, tween))
                {
                    activeTween = null;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            rectTransform.anchoredPosition = target;
        }

        public override void ApplyInstant(UIAnimationContext context)
        {
            EnsureRectTransform();
            KillActiveTween();
            UIAnimationSettings settings = context.Settings;
            Vector2 offset = settings == null
                ? (context.Transition == UITransition.Show ? visibleOffset : hiddenOffset)
                : settings.AnchoredPositionOffset;
            rectTransform.anchoredPosition = baseAnchoredPosition + offset;
        }

        private void EnsureRectTransform()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                baseAnchoredPosition = rectTransform.anchoredPosition;
            }
        }

        private void OnDisable()
        {
            KillActiveTween();
        }

        private void KillActiveTween()
        {
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill(false);
            }

            activeTween = null;
        }
    }
}
