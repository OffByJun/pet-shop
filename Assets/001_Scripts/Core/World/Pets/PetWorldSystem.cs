using System;
using System.Collections.Generic;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Core.World
{
    /// <summary>펫 케어, 도구 조작과 부산물 획득을 처리합니다.</summary>
    public sealed class PetWorldSystem : IWorldSystem, IPetCareService
    {
        private IWorldContext world;
        private readonly HashSet<PetToolInteractionSession> sessions = new HashSet<PetToolInteractionSession>();
        private readonly List<PetToolInteractionSession> expiredSessions = new List<PetToolInteractionSession>();
        private IPetByproductRandom random = new UnityPetByproductRandom();

        public void Initialize(IWorldContext context)
        {
            if (world != null) throw new InvalidOperationException("The pet system already belongs to a world.");
            world = context ?? throw new ArgumentNullException(nameof(context));
            world.Subscribe<PetCareRequest>(request =>
            {
                if (!world.Contains(request.Pet) || request.Reply == null || !request.Reply.TryClaim()) return;
                var success = TryCare(request.Pet, request.Action, out var result);
                request.Reply.Complete(success, result);
            });
            world.Subscribe<BeginToolRequest>(request =>
            {
                if (!world.Contains(request.Pet) || request.Reply == null || !request.Reply.TryClaim()) return;
                var success = TryBegin(request.Tool, request.Order, request.Pet, request.Request, out var result);
                request.Reply.Complete(success, result);
            });
            world.Subscribe<ToolInputRequest>(request =>
            {
                if (!Owns(request.Session) || request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(ApplyInput(request.Session, request.Mode, request.Amount));
            });
            world.Subscribe<CompleteToolRequest>(request =>
            {
                if (!Owns(request.Session) || request.Reply == null || !request.Reply.TryClaim()) return;
                var success = TryComplete(request.Session, out var result);
                request.Reply.Complete(success, result);
            });
            world.Subscribe<CancelToolRequest>(request => Cancel(request.Session));
            world.Subscribe<EntityUnregistered>(message => CancelFor(message.Entity));
        }

        public void SetRandom(IPetByproductRandom source) => random = source ?? throw new ArgumentNullException(nameof(source));

        public bool TryCare(PetInstance pet, PetCareAction action, out PetCareResult result)
        {
            result = new PetCareResult(pet == null ? null : pet.Variant, action);
            var context = world;
            if (context == null || !context.Contains(pet) || pet.Variant == null || !GamePipe.HasInventory()) return false;

            var rules = pet.Variant.Byproducts;
            for (var i = 0; i < rules.Count; i++)
            {
                if (!context.Contains(pet)) break;
                var rule = rules[i];
                if (!rule.IsValid || rule.CareAction != action || !pet.TryConsumeByproductRule(i)) continue;
                if (random.Value > rule.Chance) continue;
                var amount = random.RangeInclusive(rule.MinAmount, rule.MaxAmount);
                var stack = new ItemStack(rule.Item, amount);
                result.Add(stack, GamePipe.TryGrantItem(rule.Item, amount));
            }

            context.Publish(new PetCareCompleted(result));
            return true;
        }

        public bool TryBegin(
            PetToolDefinition tool,
            ServiceOrder order,
            PetInstance pet,
            ServiceRequestState request,
            out PetToolInteractionSession session)
        {
            session = null;
            if (world == null || !world.Contains(pet) || tool == null || order == null || order.IsFinalized || request == null) return false;
            if (pet.Variant != order.Pet || request.IsResolved || !BelongsToOrder(order, request)) return false;
            if (!tool.CanProcess(request.Condition)) return false;
            foreach (var current in sessions)
                if (ReferenceEquals(current.Pet, pet) || ReferenceEquals(current.Request, request)) return false;
            session = new PetToolInteractionSession(tool, order, pet, request);
            sessions.Add(session);
            world.Publish(new ToolInteractionStarted(session));
            return Owns(session) && !session.IsCancelled && !session.IsCommitted;
        }

        public bool ApplyInput(PetToolInteractionSession session, PetToolInteractionMode mode, float normalizedAmount)
        {
            if (!Owns(session) || !world.Contains(session.Pet) || session.Order.IsFinalized ||
                !session.ApplyInput(mode, normalizedAmount)) return false;
            world.Publish(new ToolInteractionProgressed(session));
            return true;
        }

        public bool TryComplete(PetToolInteractionSession session, out PetToolUseResult result)
        {
            result = null;
            if (!Owns(session) || !world.Contains(session.Pet) || session.Order.IsFinalized || !session.IsReadyToComplete) return false;
            if (!session.Order.ResolveRequest(session.Request)) return false;

            var context = world;
            // 보상 알림에서 같은 세션이 재진입하지 않도록 먼저 완료 상태로 만듭니다.
            sessions.Remove(session);
            session.Commit();
            TryCare(session.Pet, session.Tool.RewardAction, out var careReward);
            result = new PetToolUseResult(session.Tool, session.Request, careReward);
            context.Publish(new ToolInteractionCompleted(result));
            return true;
        }

        public void Cancel(PetToolInteractionSession session)
        {
            if (session == null || !sessions.Remove(session)) return;
            session.Cancel();
            world.Publish(new ToolInteractionCancelled(session));
        }

        private bool Owns(PetToolInteractionSession session)
            => world != null && world.IsActive && session != null && sessions.Contains(session);

        private void CancelFor(GameEntity entity)
        {
            foreach (var session in sessions)
            {
                if (!ReferenceEquals(session.Pet, entity)) continue;
                Cancel(session);
                break;
            }
        }

        public void Tick(float deltaTime)
        {
            expiredSessions.Clear();
            foreach (var session in sessions)
                if (!world.Contains(session.Pet) || session.Order.IsFinalized || session.IsCancelled || session.IsCommitted)
                    expiredSessions.Add(session);
            for (var i = 0; i < expiredSessions.Count; i++) Cancel(expiredSessions[i]);
        }

        public void Shutdown()
        {
            // 구독은 WorldContext가 일괄 해제합니다.
            try
            {
                while (sessions.Count > 0)
                {
                    using var iterator = sessions.GetEnumerator();
                    iterator.MoveNext();
                    Cancel(iterator.Current);
                }
            }
            finally
            {
                sessions.Clear();
                expiredSessions.Clear();
                world = null;
            }
        }

        private static bool BelongsToOrder(ServiceOrder order, ServiceRequestState request)
        {
            var requests = order.Requests;
            for (var i = 0; i < requests.Count; i++) if (ReferenceEquals(requests[i], request)) return true;
            return false;
        }
    }
}
