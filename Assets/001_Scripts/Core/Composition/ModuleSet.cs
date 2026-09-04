using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Core.Composition
{
    /// <summary>
    /// 호스트가 소유한 조각들을 모아 순서대로 들고 있습니다.
    /// 호스트는 "누가 참여하는지"만 알고, "무엇을 하는지"는 조각이 정합니다.
    /// </summary>
    public sealed class ModuleSet<T> : IReadOnlyList<T> where T : class, IModule
    {
        private readonly List<T> modules = new List<T>();

        public int Count => modules.Count;
        public T this[int index] => modules[index];

        /// <summary>
        /// 호스트가 소유한 조각을 다시 모읍니다.
        /// 같은 타입의 호스트가 중첩되어 있으면 더 가까운 호스트의 것은 건너뜁니다.
        /// </summary>
        public void Collect<THost>(THost host, bool includeChildren = true) where THost : Component
        {
            modules.Clear();
            if (host == null) return;

            var behaviours = includeChildren
                ? host.GetComponentsInChildren<MonoBehaviour>(true)
                : host.GetComponents<MonoBehaviour>();

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null || ReferenceEquals(behaviour, host) || !behaviour.enabled) continue;
                if (!(behaviour is T module)) continue;
                if (includeChildren && behaviour.GetComponentInParent<THost>(true) != host) continue;
                modules.Add(module);
            }

            Sort();
        }

        public bool Add(T module)
        {
            if (module == null || modules.Contains(module)) return false;
            modules.Add(module);
            Sort();
            return true;
        }

        public bool Remove(T module) => module != null && modules.Remove(module);

        public void Clear() => modules.Clear();

        /// <summary>한 조각의 예외가 나머지를 막지 않도록 격리해서 실행합니다.</summary>
        public void Each(Action<T> action)
        {
            for (var i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module == null) continue;
                try { action(module); }
                catch (Exception exception) { Debug.LogException(exception, module as UnityEngine.Object); }
            }
        }

        // 순서를 정의한 조각이 없으면 배치 순서를 그대로 둡니다.
        private void Sort()
        {
            foreach (var module in modules)
                if (module is IOrderedModule)
                {
                    modules.Sort((left, right) => Order(left).CompareTo(Order(right)));
                    return;
                }
        }

        private static int Order(T module) => module is IOrderedModule ordered ? ordered.Order : 0;

        public List<T>.Enumerator GetEnumerator() => modules.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => modules.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => modules.GetEnumerator();
    }
}
