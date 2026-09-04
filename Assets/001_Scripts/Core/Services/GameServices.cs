using System;
using System.Collections.Generic;

namespace _001_Scripts.Core.Services
{
    /// <summary>
    /// 계약과 구현을 이어주는 단 하나의 지점입니다.
    /// 싱글톤은 여기 하나로 모으고, 매니저는 "등록된 구현체"가 됩니다.
    /// </summary>
    public static class GameServices
    {
        private static readonly Dictionary<Type, IService> services = new Dictionary<Type, IService>();

        public static void Register<T>(T service) where T : class, IService
            => services[typeof(T)] = service ?? throw new ArgumentNullException(nameof(service));

        public static void Unregister<T>() where T : class, IService => services.Remove(typeof(T));

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            if (services.TryGetValue(typeof(T), out var found)) { service = (T)found; return true; }
            service = null;
            return false;
        }

        public static T Get<T>() where T : class, IService
            => TryGet(out T service) ? service : throw new InvalidOperationException($"Service '{typeof(T).Name}' is not registered.");

        /// <summary>테스트에서 상태를 비웁니다.</summary>
        public static void Clear() => services.Clear();
    }
}
