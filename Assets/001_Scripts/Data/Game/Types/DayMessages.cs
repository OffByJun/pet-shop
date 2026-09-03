using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes.Pipes;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Progression;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Core.Pipes.Msgs
{

    // 요청은 담당 매니저 하나가 처리하며, 같은 Reply의 중복 실행을 막습니다.

    // 확정된 상태 변경과 반환값이 없는 명령입니다.

    public readonly struct DayStarted : IPipeMsg
    {
        public readonly int DayNumber;
        public readonly int TotalCustomers;

        public DayStarted(int dayNumber, int totalCustomers)
        {
            DayNumber = dayNumber;
            TotalCustomers = totalCustomers;
        }
    }

    public readonly struct CustomerArrived : IPipeMsg
    {
        public readonly ServiceOrder Order;
        public readonly int Number;
        public readonly int Total;

        public CustomerArrived(ServiceOrder order, int number, int total)
        {
            Order = order;
            Number = number;
            Total = total;
        }
    }

    public readonly struct PetAccepted : IPipeMsg
    {
        public readonly ServiceOrder Order;
        public readonly PetInstance Pet;

        public PetAccepted(ServiceOrder order, PetInstance pet)
        {
            Order = order;
            Pet = pet;
        }
    }

    public readonly struct CustomerServiceCompleted : IPipeMsg
    {
        public readonly ServiceOrder Order;
        public readonly ServiceOrderCompletion Completion;
        public readonly PetInstance Pet;

        public CustomerServiceCompleted(ServiceOrder order, ServiceOrderCompletion completion, PetInstance pet)
        {
            Order = order;
            Completion = completion;
            Pet = pet;
        }
    }

    public readonly struct DaySettlementStarted : IPipeMsg
    {
        public readonly DaySummary Summary;

        public DaySettlementStarted(DaySummary summary)
        {
            Summary = summary;
        }
    }

    public readonly struct ByproductSold : IPipeMsg
    {
        public readonly ItemSaleResult Result;

        public ByproductSold(ItemSaleResult result)
        {
            Result = result;
        }
    }

    public readonly struct DayEnded : IPipeMsg
    {
        public readonly DaySummary Summary;

        public DayEnded(DaySummary summary)
        {
            Summary = summary;
        }
    }

}

