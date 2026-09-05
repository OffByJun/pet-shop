using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Items;
using UnityEngine;

namespace _001_Scripts.Data.Economy
{
    [Serializable]
    public struct ServicePriceTier
    {
        [SerializeField] private string tierId;
        [SerializeField, Min(0)] private int visitFee;
        [SerializeField, Min(0)] private int requiredCareUnitPrice;
        [SerializeField, Min(0)] private int optionalCareBonusUnitPrice;
        [Tooltip("선택 케어까지 모두 끝낸 완벽 주문에 얹어 주는 금액입니다.")]
        [SerializeField, Min(0)] private int perfectBonus;

        public string TierId => tierId;
        public int PerfectBonus => perfectBonus;
        public int VisitFee => visitFee;
        public int RequiredCareUnitPrice => requiredCareUnitPrice;
        public int OptionalCareBonusUnitPrice => optionalCareBonusUnitPrice;
    }
}
