using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.Pipes.Pipes;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Core.World
{
    /// <summary>IWorldContext를 통해 MessagePipe로 월드 연산을 요청합니다.</summary>
    public static class WorldAPI
    {
        private static bool Send<TRequest, TResult>(IWorldContext world, in TRequest request,
            PipeReply<TResult> reply, out TResult result) where TRequest : struct, IPipeMsg
        {
            result = default;
            if (world == null || !world.IsActive) return false;
            world.Publish(in request);
            result = reply.Value;
            return reply.Completed && reply.Succeeded;
        }

        public static bool TryCarePet(this IWorldContext world, PetInstance pet, PetCareAction action, out PetCareResult result)
        {
            result = null;
            if (world == null || !world.Contains(pet)) return false;
            var reply = new PipeReply<PetCareResult>();
            return Send(world, new PetCareRequest(pet, action, reply), reply, out result);
        }

        public static bool TryBeginTool(this IWorldContext world, PetToolDefinition tool, ServiceOrder order, PetInstance pet, ServiceRequestState request, out PetToolInteractionSession result)
        {
            result = null;
            if (world == null || !world.Contains(pet)) return false;
            var reply = new PipeReply<PetToolInteractionSession>();
            return Send(world, new BeginToolRequest(tool, order, pet, request, reply), reply, out result);
        }

        public static bool TryApplyToolInput(this IWorldContext world, PetToolInteractionSession session, PetToolInteractionMode mode, float amount)
        {
            if (world == null || !world.Contains(session?.Pet)) return false;
            var reply = new PipeReply<bool>();
            return Send(world, new ToolInputRequest(session, mode, amount, reply), reply, out _);
        }

        public static bool TryCompleteTool(this IWorldContext world, PetToolInteractionSession session, out PetToolUseResult result)
        {
            result = null;
            if (world == null || !world.Contains(session?.Pet)) return false;
            var reply = new PipeReply<PetToolUseResult>();
            return Send(world, new CompleteToolRequest(session, reply), reply, out result);
        }

        public static void CancelTool(this IWorldContext world, PetToolInteractionSession session)
        {
            if (world != null && world.Contains(session?.Pet)) world.Publish(new CancelToolRequest(session));
        }
    }
}
