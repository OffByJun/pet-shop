using System;
using UnityEngine;

namespace _001_Scripts.Data
{
    /// <summary>평판 구간입니다. 구간이 오르면 하루에 오는 손님이 늘어납니다.</summary>
    [Serializable]
    public struct ShopReputationTier
    {
        [SerializeField] private string title;
        [Tooltip("이 점수 이상이면 이 구간입니다.")]
        [SerializeField] private int minimumPoints;
        [Tooltip("하루 손님 수에 더해지는 값입니다.")]
        [SerializeField] private int extraCustomers;

        public string Title => string.IsNullOrWhiteSpace(title) ? "이름 없는 가게" : title;
        public int MinimumPoints => minimumPoints;
        public int ExtraCustomers => extraCustomers;
    }

    /// <summary>하루 결과가 쌓이는 가게 평판입니다.</summary>
    public sealed class ShopReputation
    {
        public int Points { get; private set; }
        public int LastGain { get; private set; }

        public void Reset()
        {
            Points = 0;
            LastGain = 0;
        }

        /// <summary>목표 달성처럼 하루 요약 밖에서 오는 평판을 더합니다.</summary>
        public void Add(int delta)
        {
            if (delta == 0) return;
            LastGain += delta;
            Points = Mathf.Max(0, Points + delta);
        }

        /// <summary>하루 결과를 평판 점수로 환산해 더합니다. 점수는 0 아래로 내려가지 않습니다.</summary>
        public int Apply(DaySummary summary, ShopRoutineSettings settings)
        {
            if (summary == null || settings == null) return 0;
            var gain = summary.PerfectOrders * settings.ReputationPerPerfectOrder
                     + summary.CompletedOrders * settings.ReputationPerCompletedOrder
                     + summary.FailedOrders * settings.ReputationPerFailedOrder
                     + summary.UnservedOrders * settings.ReputationPerUnservedOrder;
            LastGain = gain;
            Points = Mathf.Max(0, Points + gain);
            return gain;
        }
    }
}
