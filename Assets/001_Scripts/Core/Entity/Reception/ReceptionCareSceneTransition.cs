using System;
using _001_Scripts.Core;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _001_Scripts.Core.Entity
{
    /// <summary>Infrastructure adapter for visit persistence and Unity scene navigation.</summary>
    public sealed class ReceptionCareSceneTransition : GameBehaviour
    {
        [SerializeField] private string careSceneName = "CarePlayScene";
        private ServiceOrder preparedOrder;

        public void Prepare(ServiceOrder order)
        {
            preparedOrder = order ?? throw new ArgumentNullException(nameof(order));
            CareHandoffContext.Set(order);
        }

        public void EnterCareScene()
        {
            if (preparedOrder == null) return;
            if (_001_Scripts.Managers.ShopRoutineManager.HasInstance)
                _001_Scripts.Managers.ShopRoutineManager.Instance.EnterCare();
            else SceneManager.LoadScene(careSceneName);
        }

        public void ResetState()
        {
            preparedOrder = null;
        }
    }
}
