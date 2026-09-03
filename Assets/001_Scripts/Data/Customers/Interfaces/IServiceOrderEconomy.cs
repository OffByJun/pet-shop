using System;
using System.Collections.Generic;
using _001_Scripts.Data.Items;

namespace _001_Scripts.Data.Customers
{
    /// <summary>가격표와 보상 수치는 경제 시스템이 구현합니다.</summary>
    public interface IServiceOrderEconomy
    {
        ServiceReward CalculateReward(ServiceOrder order, ServiceOrderStatus result);
    }
}
