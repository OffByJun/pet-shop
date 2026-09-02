using System;
using _001_Scripts.Core;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Pets;
using UnityEngine;

namespace _001_Scripts.Data.Tools
{
    /// <summary>도구 호환성, 직접 조작 세션, 주문 상태 해결을 연결합니다.</summary>
    public sealed class PetToolService : GameBehaviour
    {
        [SerializeField] private PetCareService petCareService;

        public event Action<PetToolInteractionSession> InteractionStarted;
        public event Action<PetToolInteractionSession> InteractionProgressed;
        public event Action<PetToolInteractionSession> InteractionCancelled;
        public event Action<PetToolUseResult> InteractionCompleted;

        public bool TryBegin(
            PetToolDefinition tool,
            ServiceOrder order,
            PetInstance pet,
            ServiceRequestState request,
            out PetToolInteractionSession session)
        {
            session = null;
            if (tool == null || order == null || order.IsFinalized || pet == null || request == null) return false;
            if (pet.Variant != order.Pet || request.IsResolved || !BelongsToOrder(order, request)) return false;
            if (!tool.CanProcess(request.Condition)) return false;
            session = new PetToolInteractionSession(tool, order, pet, request);
            InteractionStarted?.Invoke(session);
            return true;
        }

        public bool ApplyInput(PetToolInteractionSession session, PetToolInteractionMode mode, float normalizedAmount)
        {
            if (session == null || !session.ApplyInput(mode, normalizedAmount)) return false;
            InteractionProgressed?.Invoke(session);
            return true;
        }

        public bool TryComplete(PetToolInteractionSession session, out PetToolUseResult result)
        {
            result = null;
            if (session == null || !session.IsReadyToComplete) return false;
            if (!session.Order.ResolveRequest(session.Request)) return false;

            PetCareResult careReward = null;
            if (petCareService != null)
                petCareService.TryCare(session.Pet, session.Tool.RewardAction, out careReward);
            session.Commit();
            result = new PetToolUseResult(session.Tool, session.Request, careReward);
            InteractionCompleted?.Invoke(result);
            return true;
        }

        public void Cancel(PetToolInteractionSession session)
        {
            if (session == null || session.IsCommitted || session.IsCancelled) return;
            session.Cancel();
            InteractionCancelled?.Invoke(session);
        }

        public void SetPetCareService(PetCareService service) => petCareService = service;

        private static bool BelongsToOrder(ServiceOrder order, ServiceRequestState request)
        {
            var requests = order.Requests;
            for (var i = 0; i < requests.Count; i++) if (ReferenceEquals(requests[i], request)) return true;
            return false;
        }
    }
}
