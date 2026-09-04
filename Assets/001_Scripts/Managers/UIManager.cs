using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.UI.Components;
using _001_Scripts.UI.UILib;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>
    /// UIComponent가 스스로 보낸 등록 메시지로 레지스트리를 유지하고,
    /// UI 파이프 메시지를 받아 전환을 실행한 뒤 결과를 다시 발행합니다.
    /// 호출자는 UIManager를 직접 참조하지 않고 <see cref="UIPipe"/>로 통신합니다.
    /// </summary>
    [DefaultExecutionOrder(-9_000)]
    public sealed class UIManager : ServiceManagerBase<UIManager>, IUIService
    {
        private readonly Dictionary<string, UIComponent> components =
            new Dictionary<string, UIComponent>(StringComparer.Ordinal);

        private readonly List<IDisposable> subscriptions = new List<IDisposable>();

        public static IUIService Service => Instance;

        /// <summary>
        /// UIComponent가 Awake에서 보내는 등록 메시지를 놓치지 않도록 씬 로드 전에 만듭니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null)
            {
                return;
            }

            var managerObject = new GameObject("[Manager] UIManager");
            managerObject.AddComponent<UIManager>();
        }

        protected override void ProvideServices()
        {
            Provide<IUIService>();
        }

        protected override void OnManagerAwake()
        {
            SubscribePipes();
        }

        protected override void OnManagerDestroying()
        {
            UnsubscribePipes();

            foreach (UIComponent component in components.Values)
            {
                if (component != null)
                {
                    component.Cancel();
                }
            }

            components.Clear();
        }

        // ─────────────────────────────────────────────
        // Pipe wiring
        // ─────────────────────────────────────────────

        private void SubscribePipes()
        {
            AddSubscription(UIPipe.Subscribe<UIShowRequest>(OnShowRequest));
            AddSubscription(UIPipe.Subscribe<UIHideRequest>(OnHideRequest));
            AddSubscription(UIPipe.Subscribe<UICancelRequest>(OnCancelRequest));
            AddSubscription(UIPipe.Subscribe<UISetInstantRequest>(OnSetInstantRequest));
            AddSubscription(UIPipe.Subscribe<UIComponentRegisterRequest>(OnRegisterRequest));
            AddSubscription(UIPipe.Subscribe<UIComponentUnregisterRequest>(OnUnregisterRequest));

            if (subscriptions.Count == 0)
            {
                Debug.LogWarning(
                    "MessagePipeHub is not ready. UIManager runs without pipe messages.",
                    this);
            }
        }

        private void AddSubscription(IDisposable subscription)
        {
            if (subscription != null)
            {
                subscriptions.Add(subscription);
            }
        }

        private void UnsubscribePipes()
        {
            foreach (IDisposable subscription in subscriptions)
            {
                subscription.Dispose();
            }

            subscriptions.Clear();
        }

        private void OnShowRequest(UIShowRequest message)
        {
            RunTransition(message.ServiceId, UITransition.Show);
        }

        private void OnHideRequest(UIHideRequest message)
        {
            RunTransition(message.ServiceId, UITransition.Hide);
        }

        private void OnCancelRequest(UICancelRequest message)
        {
            if (TryGet(message.ServiceId, out UIComponent component))
            {
                component.Cancel();
                return;
            }

            LogMissing(message.ServiceId);
        }

        private void OnSetInstantRequest(UISetInstantRequest message)
        {
            if (!TryGet(message.ServiceId, out UIComponent component))
            {
                LogMissing(message.ServiceId);
                return;
            }

            component.SetInstant(message.Visible);
            UIPipe.Publish(new UIVisibilityChanged(message.ServiceId, component.State));
        }

        private void OnRegisterRequest(UIComponentRegisterRequest message)
        {
            Register(message.Component);
        }

        private void OnUnregisterRequest(UIComponentUnregisterRequest message)
        {
            Unregister(message.Component);
        }

        /// <summary>
        /// 파이프 요청은 결과를 기다리지 않으므로, 실패를 예외 대신 메시지와 로그로 알립니다.
        /// </summary>
        private async void RunTransition(string serviceId, UITransition transition)
        {
            try
            {
                await TransitionAsync(serviceId, transition, CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // 취소는 UITransitionCanceled로 이미 알렸습니다.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        // ─────────────────────────────────────────────
        // Registry
        // ─────────────────────────────────────────────

        public bool Register(UIComponent component)
        {
            if (component == null)
            {
                return false;
            }

            string serviceId = component.ServiceId;
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                Debug.LogError($"{component.name} has an empty UI service id.", component);
                return false;
            }

            if (components.TryGetValue(serviceId, out UIComponent registered))
            {
                if (registered == null)
                {
                    components[serviceId] = component;
                    UIPipe.Publish(new UIComponentRegistered(serviceId, component));
                    return true;
                }

                if (registered == component)
                {
                    return true;
                }

                Debug.LogError(
                    $"UI service id '{serviceId}' is already registered by {registered.name}.",
                    component);
                return false;
            }

            components.Add(serviceId, component);
            UIPipe.Publish(new UIComponentRegistered(serviceId, component));
            return true;
        }

        public bool Unregister(UIComponent component)
        {
            if (component == null)
            {
                return false;
            }

            string serviceId = component.ServiceId;
            if (!components.TryGetValue(serviceId, out UIComponent registered)
                || registered != component
                || !components.Remove(serviceId))
            {
                return false;
            }

            UIPipe.Publish(new UIComponentUnregistered(serviceId, component));
            return true;
        }

        public bool TryGet(string serviceId, out UIComponent component)
        {
            if (string.IsNullOrWhiteSpace(serviceId)
                || !components.TryGetValue(serviceId, out component))
            {
                component = null;
                return false;
            }

            if (component != null)
            {
                return true;
            }

            components.Remove(serviceId);
            return false;
        }

        // ─────────────────────────────────────────────
        // Direct API (결과를 await 해야 하는 흐름용)
        // ─────────────────────────────────────────────

        public Task ShowAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            return TransitionAsync(serviceId, UITransition.Show, cancellationToken);
        }

        public Task HideAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            return TransitionAsync(serviceId, UITransition.Hide, cancellationToken);
        }

        public void Cancel(string serviceId)
        {
            GetRequired(serviceId).Cancel();
        }

        public void SetInstant(string serviceId, bool visible)
        {
            UIComponent component = GetRequired(serviceId);
            component.SetInstant(visible);
            UIPipe.Publish(new UIVisibilityChanged(serviceId, component.State));
        }

        private async Task TransitionAsync(
            string serviceId,
            UITransition transition,
            CancellationToken cancellationToken)
        {
            UIComponent component;
            try
            {
                component = GetRequired(serviceId);
            }
            catch (KeyNotFoundException exception)
            {
                UIPipe.Publish(new UITransitionFailed(serviceId, transition, exception));
                throw;
            }

            UIPipe.Publish(new UITransitionStarted(serviceId, transition));

            try
            {
                await (transition == UITransition.Show
                    ? component.ShowAsync(cancellationToken)
                    : component.HideAsync(cancellationToken));
            }
            catch (OperationCanceledException)
            {
                UIPipe.Publish(new UITransitionCanceled(serviceId, transition));
                throw;
            }
            catch (Exception exception)
            {
                UIPipe.Publish(new UITransitionFailed(serviceId, transition, exception));
                throw;
            }

            UIPipe.Publish(new UITransitionCompleted(serviceId, transition));
            UIPipe.Publish(new UIVisibilityChanged(serviceId, component.State));
        }

        private UIComponent GetRequired(string serviceId)
        {
            if (TryGet(serviceId, out UIComponent component))
            {
                return component;
            }

            throw new KeyNotFoundException($"UI component '{serviceId}' is not registered.");
        }

        private void LogMissing(string serviceId)
        {
            Debug.LogError($"UI component '{serviceId}' is not registered.", this);
        }
    }
}
