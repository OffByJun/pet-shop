using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.Core;
using _001_Scripts.UI.UILib;
using UnityEngine;

namespace _001_Scripts.UI.Components
{
    [DisallowMultipleComponent]
    public class UIComponent : GameBehaviour
    {
        [SerializeField] private bool includeChildren = true;
        [SerializeField] private UIVisibilityState initialState = UIVisibilityState.Hidden;
        [SerializeField] private UIAnimationPreset animationPreset;

        private readonly List<IUIAnimator> animators = new List<IUIAnimator>();
        private readonly List<IUIAction> actions = new List<IUIAction>();
        private CancellationTokenSource lifetimeCancellation;
        private CancellationTokenSource transitionCancellation;
        private UIVisibilityState settledState;

        public UIVisibilityState State { get; private set; }

        public bool IsTransitioning => State == UIVisibilityState.Showing || State == UIVisibilityState.Hiding;

        public UIAnimationPreset AnimationPreset => animationPreset;

        protected virtual void Awake()
        {
            RefreshComponents();
            EnsureLifetimeToken();
            SetInstant(initialState == UIVisibilityState.Visible);
        }

        protected virtual void OnEnable()
        {
            EnsureLifetimeToken();
        }

        protected virtual void OnDisable()
        {
            Cancel();
            lifetimeCancellation?.Cancel();
            lifetimeCancellation?.Dispose();
            lifetimeCancellation = null;
        }

        public void RefreshComponents()
        {
            animators.Clear();
            actions.Clear();

            MonoBehaviour[] behaviours = includeChildren
                ? GetComponentsInChildren<MonoBehaviour>(true)
                : GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == this || !behaviour.enabled)
                {
                    continue;
                }

                // A nested UIComponent owns its own animation/action components.
                // This keeps a parent panel transition from driving an independent child panel.
                if (behaviour.GetComponentInParent<UIComponent>(true) != this)
                {
                    continue;
                }

                if (behaviour is IUIAnimator animator)
                {
                    animators.Add(animator);
                }

                if (behaviour is IUIAction action)
                {
                    actions.Add(action);
                }
            }

            actions.Sort((left, right) => left.Order.CompareTo(right.Order));
        }

        public Task ShowAsync(CancellationToken cancellationToken = default)
        {
            return TransitionAsync(UITransition.Show, cancellationToken);
        }

        public Task HideAsync(CancellationToken cancellationToken = default)
        {
            return TransitionAsync(UITransition.Hide, cancellationToken);
        }

        public void Cancel()
        {
            transitionCancellation?.Cancel();
        }

        public void SetInstant(bool visible)
        {
            Cancel();
            UITransition transition = visible ? UITransition.Show : UITransition.Hide;
            UIAnimationContext context = new UIAnimationContext(this, transition, animationPreset);

            foreach (IUIAnimator animator in animators)
            {
                animator.ApplyInstant(context);
            }

            settledState = visible ? UIVisibilityState.Visible : UIVisibilityState.Hidden;
            State = settledState;
        }

        private async Task TransitionAsync(UITransition transition, CancellationToken externalToken)
        {
            if (!isActiveAndEnabled)
            {
                throw new InvalidOperationException($"{name} must be active and enabled before starting a UI transition.");
            }

            EnsureLifetimeToken();
            Cancel();

            CancellationTokenSource localCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                lifetimeCancellation.Token,
                externalToken);
            transitionCancellation = localCancellation;

            UIVisibilityState stableState = settledState;
            State = transition == UITransition.Show ? UIVisibilityState.Showing : UIVisibilityState.Hiding;

            try
            {
                UIActionTiming before = transition == UITransition.Show
                    ? UIActionTiming.BeforeShow
                    : UIActionTiming.BeforeHide;
                UIActionTiming after = transition == UITransition.Show
                    ? UIActionTiming.AfterShow
                    : UIActionTiming.AfterHide;

                await RunActionsAsync(before, transition, localCancellation.Token);
                localCancellation.Token.ThrowIfCancellationRequested();

                UIAnimationContext animationContext = new UIAnimationContext(this, transition, animationPreset);
                Task[] animationTasks = animators
                    .Select(animator => animator.PlayAsync(animationContext, localCancellation.Token))
                    .ToArray();
                await Task.WhenAll(animationTasks);
                localCancellation.Token.ThrowIfCancellationRequested();

                settledState = transition == UITransition.Show
                    ? UIVisibilityState.Visible
                    : UIVisibilityState.Hidden;
                State = settledState;
                await RunActionsAsync(after, transition, localCancellation.Token);
            }
            catch (OperationCanceledException) when (localCancellation.IsCancellationRequested)
            {
                if (ReferenceEquals(transitionCancellation, localCancellation))
                {
                    State = stableState;
                }

                throw;
            }
            finally
            {
                if (ReferenceEquals(transitionCancellation, localCancellation))
                {
                    transitionCancellation = null;
                }

                localCancellation.Dispose();
            }
        }

        private async Task RunActionsAsync(
            UIActionTiming timing,
            UITransition transition,
            CancellationToken cancellationToken)
        {
            UIActionContext context = new UIActionContext(this, transition, timing);
            foreach (IUIAction action in actions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (action.RunsAt(timing))
                {
                    await action.ExecuteAsync(context, cancellationToken);
                }
            }
        }

        private void EnsureLifetimeToken()
        {
            if (lifetimeCancellation == null)
            {
                lifetimeCancellation = new CancellationTokenSource();
            }
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            bool hasAnimator = GetComponents<MonoBehaviour>().Any(behaviour => behaviour is IUIAnimator);
            if (!hasAnimator)
            {
                gameObject.AddComponent<UIFadeAnimator>();
            }

            RefreshComponents();
        }

        protected virtual void OnValidate()
        {
            RefreshComponents();
            if (animators.Count == 0)
            {
                Debug.LogWarning($"{name} has no enabled IUIAnimator component.", this);
            }
        }
#endif
    }
}
