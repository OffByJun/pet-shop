using System;
using System.Collections.Generic;
using _001_Scripts.Core.Services;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>
    /// 단일 인스턴스(<see cref="SinManagerBase{TManager}"/>)에 "자기 계약을 등록한다"만 더합니다.
    /// 매니저는 <see cref="ProvideServices"/>에 자기 계약을 적기만 하면 해제는 자동입니다.
    /// </summary>
    public abstract class ServiceManagerBase<TManager> : SinManagerBase<TManager>, IService
        where TManager : ServiceManagerBase<TManager>
    {
        private readonly List<Action> unregisters = new List<Action>();

        /// <summary>이 매니저가 제공하는 계약을 <see cref="Provide{T}"/>로 적습니다.</summary>
        protected abstract void ProvideServices();

        protected void Provide<T>() where T : class, IService
        {
            if (!(this is T service)) { Debug.LogError($"{name} does not implement {typeof(T).Name}.", this); return; }
            GameServices.Register(service);
            unregisters.Add(GameServices.Unregister<T>);
        }

        protected override void Awake()
        {
            base.Awake();
            if (IsPrimaryInstance) ProvideServices();
        }

        protected override void OnDestroy()
        {
            if (IsPrimaryInstance)
            {
                foreach (var unregister in unregisters) unregister();
                unregisters.Clear();
            }

            base.OnDestroy();
        }
    }
}
