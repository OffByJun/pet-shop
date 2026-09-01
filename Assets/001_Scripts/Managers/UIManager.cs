using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _001_Scripts.UI.Components;
using _001_Scripts.UI.UILib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _001_Scripts.Managers
{
    /// <summary>
    /// UIComponent를 등록하고 기존 애니메이션 전환 API를 서비스로 제공합니다.
    /// </summary>
    public sealed class UIManager : SinManagerBase<UIManager>, IUIService
    {
        [SerializeField]
        private List<UIComponent> registeredComponents = new List<UIComponent>();

        [SerializeField]
        private bool discoverSceneComponents = true;

        private readonly Dictionary<string, UIComponent> components =
            new Dictionary<string, UIComponent>(StringComparer.Ordinal);

        public static IUIService Service => Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null)
            {
                return;
            }

            var managerObject = new GameObject("[Manager] UIManager");
            managerObject.AddComponent<UIManager>();
        }

        protected override void OnManagerAwake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            RebuildRegistry();
        }

        protected override void OnManagerDestroying()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            foreach (UIComponent component in components.Values)
            {
                if (component != null)
                {
                    component.Cancel();
                }
            }

            components.Clear();
        }

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
            return true;
        }

        public bool Unregister(UIComponent component)
        {
            if (component == null)
            {
                return false;
            }

            string serviceId = component.ServiceId;
            return components.TryGetValue(serviceId, out UIComponent registered)
                   && registered == component
                   && components.Remove(serviceId);
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

        public Task ShowAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            return GetRequired(serviceId).ShowAsync(cancellationToken);
        }

        public Task HideAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            return GetRequired(serviceId).HideAsync(cancellationToken);
        }

        public void Cancel(string serviceId)
        {
            GetRequired(serviceId).Cancel();
        }

        public void SetInstant(string serviceId, bool visible)
        {
            GetRequired(serviceId).SetInstant(visible);
        }

        public void RebuildRegistry()
        {
            components.Clear();

            foreach (UIComponent component in registeredComponents)
            {
                Register(component);
            }

            if (!discoverSceneComponents)
            {
                return;
            }

            UIComponent[] discovered = UnityEngine.Object.FindObjectsByType<UIComponent>(
                FindObjectsInactive.Include);

            foreach (UIComponent component in discovered)
            {
                Register(component);
            }
        }

        private UIComponent GetRequired(string serviceId)
        {
            if (TryGet(serviceId, out UIComponent component))
            {
                return component;
            }

            throw new KeyNotFoundException($"UI component '{serviceId}' is not registered.");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebuildRegistry();
        }
    }
}
