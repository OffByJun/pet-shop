using _001_Scripts.Data.Customers;
using UnityEngine;

namespace _001_Scripts.Core.Entity
{
    /// <summary>공유 손님 정의를 참조하는 씬 개체입니다.</summary>
    public class CustomerInstance : GameEntity
    {
        [SerializeField] private CustomerBase definition;

        public CustomerBase Definition => definition;
        public override string DefinitionId => definition == null ? string.Empty : definition.CustomerTypeId;
        public override string DisplayName => definition == null ? name : definition.DisplayName;

        public void Initialize(CustomerBase customer) => definition = customer;
    }
}
