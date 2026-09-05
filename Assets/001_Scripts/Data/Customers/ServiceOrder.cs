using System;
using System.Collections.Generic;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Data.Customers
{
    /// <summary>손님, 펫, 요청 진행도, 최종 보상을 묶는 한 건의 서비스 주문입니다.</summary>
    public sealed class ServiceOrder
    {
        private readonly List<ServiceRequestState> requests;
        private readonly List<ServiceRequestState> requiredRequests;
        private readonly List<ServiceRequestState> optionalRequests;
        private readonly float perfectOptionalRatio;

        public string OrderId { get; }
        public CustomerTypeDefinition Customer { get; }
        public PetVariantDefinition Pet { get; }
        public IReadOnlyList<ServiceRequestState> Requests => requests;
        public IReadOnlyList<ServiceRequestState> RequiredRequests => requiredRequests;
        public IReadOnlyList<ServiceRequestState> OptionalRequests => optionalRequests;
        public ServiceOrderStatus Status { get; private set; } = ServiceOrderStatus.Active;
        /// <summary>케어 솜씨 배율입니다. 1이 기본이고 잘할수록 커집니다.</summary>
        public float CareQuality { get; private set; } = 1f;
        /// <summary>손님과의 관계로 붙는 팁 배율입니다. 0이면 팁 없음입니다.</summary>
        public float RelationshipTip { get; private set; }
        /// <summary>정산 화면에 보여 줄 케어 등급 이름입니다.</summary>
        public string CareGrade { get; private set; } = string.Empty;
        public ServiceOrderCompletion Completion { get; private set; }
        public ServiceReward? Reward => Completion == null ? null : Completion.Reward;
        public bool IsFinalized => Status != ServiceOrderStatus.Active;

        public ServiceOrder(
            CustomerTypeDefinition customer,
            PetVariantDefinition pet,
            IEnumerable<PetConditionDefinition> required,
            IEnumerable<PetConditionDefinition> optional,
            float perfectOptionalRatio = 1f,
            string orderId = null)
        {
            Customer = customer ?? throw new ArgumentNullException(nameof(customer));
            Pet = pet ?? throw new ArgumentNullException(nameof(pet));
            OrderId = string.IsNullOrWhiteSpace(orderId) ? Guid.NewGuid().ToString("N") : orderId;
            this.perfectOptionalRatio = Math.Clamp(perfectOptionalRatio, 0f, 1f);
            requests = new List<ServiceRequestState>();
            requiredRequests = new List<ServiceRequestState>();
            optionalRequests = new List<ServiceRequestState>();
            AddRequests(required, ServiceRequestKind.Required);
            AddRequests(optional, ServiceRequestKind.Optional);
            if (RequiredCount == 0) throw new ArgumentException("A service order requires at least one required condition.");
        }

        /// <summary>케어가 끝난 뒤 그 결과를 주문에 기록합니다. 정산 전에만 반영됩니다.</summary>
        public void RecordCareResult(float quality, string grade)
        {
            if (IsFinalized) return;
            CareQuality = Math.Clamp(quality, 0f, 4f);
            CareGrade = grade ?? string.Empty;
        }

        public void RecordRelationshipTip(float multiplier)
        {
            if (IsFinalized) return;
            RelationshipTip = Math.Clamp(multiplier, 0f, 2f);
        }

        public int RequiredCount => Count(ServiceRequestKind.Required, false);
        public int ResolvedRequiredCount => Count(ServiceRequestKind.Required, true);
        public int OptionalCount => Count(ServiceRequestKind.Optional, false);
        public int ResolvedOptionalCount => Count(ServiceRequestKind.Optional, true);

        public int ApplyCare(PetCareAction action)
        {
            if (IsFinalized) return 0;
            var resolved = 0;
            for (var i = 0; i < requests.Count; i++)
                if (!requests[i].IsResolved &&
                    requests[i].Condition.RequiredCapabilities == PetToolCapability.None &&
                    requests[i].Condition.ResolvedBy == action &&
                    requests[i].Resolve()) resolved++;
            return resolved;
        }

        internal bool ResolveRequest(ServiceRequestState request)
        {
            if (request == null || request.IsResolved || IsFinalized) return false;
            for (var i = 0; i < requests.Count; i++)
                if (ReferenceEquals(requests[i], request)) return requests[i].Resolve();
            return false;
        }

        public ServiceOrderStatus PreviewResult()
        {
            if (ResolvedRequiredCount < RequiredCount) return ServiceOrderStatus.Failed;
            if (OptionalCount == 0) return ServiceOrderStatus.Completed;
            var optionalRatio = ResolvedOptionalCount / (float)OptionalCount;
            return optionalRatio >= perfectOptionalRatio ? ServiceOrderStatus.Perfect : ServiceOrderStatus.Completed;
        }

        public ServiceOrderCompletion Finalize(IServiceOrderEconomy economy)
        {
            if (Completion != null) return Completion;
            if (economy == null) throw new ArgumentNullException(nameof(economy));
            Status = PreviewResult();
            Completion = new ServiceOrderCompletion(Status, economy.CalculateReward(this, Status));
            return Completion;
        }

        private void AddRequests(IEnumerable<PetConditionDefinition> conditions, ServiceRequestKind kind)
        {
            if (conditions == null) return;
            foreach (var condition in conditions)
            {
                if (condition == null || Contains(condition)) continue;
                var request = new ServiceRequestState(condition, kind);
                requests.Add(request);
                if (kind == ServiceRequestKind.Required) requiredRequests.Add(request);
                else optionalRequests.Add(request);
            }
        }

        private bool Contains(PetConditionDefinition condition)
        {
            for (var i = 0; i < requests.Count; i++) if (requests[i].Condition == condition) return true;
            return false;
        }

        private int Count(ServiceRequestKind kind, bool resolvedOnly)
        {
            var count = 0;
            for (var i = 0; i < requests.Count; i++)
                if (requests[i].Kind == kind && (!resolvedOnly || requests[i].IsResolved)) count++;
            return count;
        }
    }
}
