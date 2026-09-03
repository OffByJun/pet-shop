using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.UI.UILib;
using DG.Tweening;
using UnityEngine;

namespace _001_Scripts.UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIScaleAnimator : UIAnimatorComponent
    {
        [SerializeField] private Vector3 hiddenScale = new Vector3(0.9f, 0.9f, 1f);
        [SerializeField] private Vector3 visibleScale = Vector3.one;
        [SerializeField, Min(0f)] private float duration = 0.2f;
        [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool useUnscaledTime = true;

        private RectTransform rectTransform;
        private Tween activeTween;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public override async Task PlayAsync(UIAnimationContext context, CancellationToken cancellationToken)
        {
            EnsureRectTransform();
            KillActiveTween();
            UIAnimationSettings settings = context.Settings;
            float animationDuration = settings == null ? duration : settings.Duration;
            AnimationCurve animationEasing = settings == null ? easing : settings.Easing;
            bool animationUsesUnscaledTime = settings == null ? useUnscaledTime : settings.UseUnscaledTime;
            Vector3 target = settings == null
                ? (context.Transition == UITransition.Show ? visibleScale : hiddenScale)
                : settings.Scale;

            if (animationDuration <= 0f || rectTransform.localScale == target)
            {
                cancellationToken.ThrowIfCancellationRequested();
                rectTransform.localScale = target;
                return;
            }

            Tween tween = rectTransform
                .DOScale(target, animationDuration)
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
            rectTransform.localScale = target;
        }

        public override void ApplyInstant(UIAnimationContext context)
        {
            EnsureRectTransform();
            KillActiveTween();
            UIAnimationSettings settings = context.Settings;
            rectTransform.localScale = settings == null
                ? (context.Transition == UITransition.Show ? visibleScale : hiddenScale)
                : settings.Scale;
        }

        private void EnsureRectTransform()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
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
