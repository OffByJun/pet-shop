using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.Core;
using _001_Scripts.Core.Composition;
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
        [SerializeField] private string serviceId;

        [Tooltip("숨김 상태에서는 GameObject를 꺼둡니다. 시작 시 숨김이면 등록을 마친 뒤 스스로 꺼지고, Show 요청이 오면 다시 켜집니다.")]
        [SerializeField] private bool deactivateWhenHidden;

        private readonly ModuleSet<IUIAnimator> animators = new ModuleSet<IUIAnimator>();
        private readonly ModuleSet<IUIAction> actions = new ModuleSet<IUIAction>();
        private CancellationTokenSource lifetimeCancellation;
        private CancellationTokenSource transitionCancellation;
        private UIVisibilityState settledState;

        public UIVisibilityState State { get; private set; }

        public bool IsTransitioning => State == UIVisibilityState.Showing || State == UIVisibilityState.Hiding;

        public UIAnimationPreset AnimationPreset => animationPreset;

        public string ServiceId => string.IsNullOrWhiteSpace(serviceId) ? gameObject.name : serviceId;

        protected virtual void Awake()
        {
            RefreshComponents();
            EnsureLifetimeToken();
            SetInstant(initialState == UIVisibilityState.Visible);

            // 모든 UI는 시작할 때 스스로를 등록합니다. UIManager는 이 메시지 외의 경로로 UI를 찾지 않습니다.
            UIPipe.Register(this);
        }

        /// <summary>
        /// 등록이 끝난 뒤에 꺼야 하므로 Awake가 아니라 Start에서 비활성화합니다.
        /// </summary>
        protected virtual void Start()
        {
            if (deactivateWhenHidden && State == UIVisibilityState.Hidden)
            {
                gameObject.SetActive(false);
            }
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

        /// <summary>
        /// 비활성화가 아니라 파괴될 때 해제합니다.
        /// 꺼져 있는 패널도 ID로 다시 열 수 있어야 하므로 등록은 유지되어야 합니다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UIPipe.Unregister(this);
        }

        /// <summary>자기 소유의 애니메이터/액션을 다시 모읍니다. 수집과 정렬은 공용 ModuleSet이 담당합니다.</summary>
        public void RefreshComponents()
        {
            animators.Collect(this, includeChildren);
            actions.Collect(this, includeChildren);
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
            // 꺼져 있는 패널을 여는 것이 기본 사용 흐름이므로 여기서 되살립니다.
            // Awake/OnEnable이 이 시점에 동기로 실행되어 초기 상태가 먼저 적용됩니다.
            if (transition == UITransition.Show && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (!isActiveAndEnabled)
            {
                if (transition == UITransition.Hide)
                {
                    // 이미 꺼져 있으면 숨김 요청은 그대로 달성된 상태입니다.
                    settledState = UIVisibilityState.Hidden;
                    State = settledState;
                    return;
                }

                throw new InvalidOperationException(
                    $"{name} could not be activated for a UI transition. Check its parent hierarchy and enabled state.");
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

                if (transition == UITransition.Hide && deactivateWhenHidden)
                {
                    gameObject.SetActive(false);
                }
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
