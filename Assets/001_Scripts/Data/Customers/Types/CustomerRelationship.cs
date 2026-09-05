using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    /// <summary>손님 한 명과 쌓인 관계입니다. 방문 횟수와 만족이 여기 남습니다.</summary>
    public sealed class CustomerRelationship
    {
        public string CustomerTypeId { get; }
        public int Visits { get; private set; }
        public int HappyVisits { get; private set; }
        public int DisappointedVisits { get; private set; }
        /// <summary>마지막 방문에서 무슨 일이 있었는지입니다. 다음 인사에 쓰입니다.</summary>
        public ServiceOrderStatus LastResult { get; private set; } = ServiceOrderStatus.Active;

        public CustomerRelationship(string customerTypeId) => CustomerTypeId = customerTypeId;

        public void RecordArrival() => Visits++;

        public void RecordResult(ServiceOrderStatus result)
        {
            LastResult = result;
            if (result == ServiceOrderStatus.Perfect || result == ServiceOrderStatus.Completed) HappyVisits++;
            else if (result == ServiceOrderStatus.Failed) DisappointedVisits++;
        }
    }

    /// <summary>손님별 관계 구간입니다. 단골이 될수록 팁이 붙습니다.</summary>
    [Serializable]
    public struct CustomerBondTier
    {
        [SerializeField] private string label;
        [Tooltip("이 방문 횟수 이상이면 이 구간입니다.")]
        [SerializeField, Min(0)] private int minimumVisits;
        [Tooltip("이 만족 횟수 이상이어야 합니다.")]
        [SerializeField, Min(0)] private int minimumHappyVisits;
        [Tooltip("기본 요금에 얹히는 팁 비율입니다. .15면 15%입니다.")]
        [SerializeField, Min(0f)] private float tipRatio;

        public string Label => string.IsNullOrWhiteSpace(label) ? "처음 뵙네요" : label;
        public int MinimumVisits => minimumVisits;
        public int MinimumHappyVisits => minimumHappyVisits;
        public float TipRatio => tipRatio;
    }

    /// <summary>가게가 기억하는 모든 손님입니다.</summary>
    public sealed class CustomerRelationshipBook
    {
        private readonly Dictionary<string, CustomerRelationship> entries =
            new Dictionary<string, CustomerRelationship>(StringComparer.Ordinal);

        public IReadOnlyCollection<CustomerRelationship> All => entries.Values;

        public CustomerRelationship For(CustomerBase customer)
        {
            if (customer == null) return null;
            var id = customer.CustomerTypeId;
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (entries.TryGetValue(id, out var found)) return found;
            var created = new CustomerRelationship(id);
            entries[id] = created;
            return created;
        }

        public void Clear() => entries.Clear();
    }
}
