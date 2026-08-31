using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.UI.UILib;
using DG.Tweening;
using UnityEngine;

namespace _001_Scripts.UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIFadeAnimator : UIAnimatorComponent
    {
        [SerializeField, Min(0f)] private float duration = 0.2f;
        [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool manageInteraction = true;

        private CanvasGroup canvasGroup;
        private Tween activeTween;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public override async Task PlayAsync(UIAnimationContext context, CancellationToken cancellationToken)
        {
            EnsureCanvasGroup();
            KillActiveTween();
            UIAnimationSettings settings = context.Settings;
            float animationDuration = settings == null ? duration : settings.Duration;
            AnimationCurve animationEasing = settings == null ? easing : settings.Easing;
            bool animationUsesUnscaledTime = settings == null ? useUnscaledTime : settings.UseUnscaledTime;
            float targetAlpha = settings == null
                ? (context.Transition == UITransition.Show ? 1f : 0f)
                : settings.Alpha;

            if (manageInteraction)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (animationDuration <= 0f || Mathf.Approximately(canvasGroup.alpha, targetAlpha))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplyInstant(context);
                return;
            }

            Tween tween = canvasGroup
                .DOFade(targetAlpha, animationDuration)
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
            ApplyInstant(context);
        }

        public override void ApplyInstant(UIAnimationContext context)
        {
            EnsureCanvasGroup();
            KillActiveTween();
            UIAnimationSettings settings = context.Settings;
            bool visible = context.Transition == UITransition.Show;
            canvasGroup.alpha = settings == null ? (visible ? 1f : 0f) : settings.Alpha;

            if (manageInteraction)
            {
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
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
