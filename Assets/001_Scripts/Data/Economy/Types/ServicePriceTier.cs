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

        public string TierId => tierId;
        public int VisitFee => visitFee;
        public int RequiredCareUnitPrice => requiredCareUnitPrice;
        public int OptionalCareBonusUnitPrice => optionalCareBonusUnitPrice;
    }
}
