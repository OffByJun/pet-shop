using System.Collections.Generic;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Services;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Managers.Interfaces
{
    /// <summary>하루 영업의 진행 상태와 흐름입니다. 손님 응대 화면은 이 계약만 봅니다.</summary>
    public interface IDayService : IService
    {
        DayStatus Status { get; }
        int DayNumber { get; }
        int TotalCustomers { get; }
        int CurrentCustomerNumber { get; }
        ServiceOrder CurrentOrder { get; }
        PetInstance CurrentPet { get; }
        PetToolInteractionSession ActiveToolSession { get; }
        bool CanSellByproducts { get; }
        IReadOnlyList<ServiceOrder> Orders { get; }

        bool StartBusiness();
        bool AcceptCurrentPet(PetInstance pet);
        bool TryApplyCare(PetCareAction action, out PetCareResult result);
        bool TryBeginTool(PetToolDefinition tool, ServiceRequestState request, out PetToolInteractionSession session);
        bool ApplyToolInput(PetToolInteractionMode mode, float normalizedAmount);
        bool TryCompleteTool(out PetToolUseResult result);
        void CancelActiveTool();
        bool TryCompleteCurrentService(out ServiceOrderCompletion completion);
        bool ContinueAfterCustomerSettlement();
        bool TrySellByproduct(ItemBase item, int amount, out ItemSaleResult result);
        bool FinishDay(out DaySummary summary);
        DaySummary BuildSummary();
    }
}
