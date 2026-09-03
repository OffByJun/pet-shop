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
    public readonly struct PetCareRequest : IPipeMsg
    {
        public readonly PetInstance Pet;
        public readonly PetCareAction Action;
        public readonly PipeReply<PetCareResult> Reply;

        public PetCareRequest(PetInstance pet, PetCareAction action, PipeReply<PetCareResult> reply)
        {
            Pet = pet;
            Action = action;
            Reply = reply;
        }
    }

    public readonly struct BeginToolRequest : IPipeMsg
    {
        public readonly PetToolDefinition Tool;
        public readonly ServiceOrder Order;
        public readonly PetInstance Pet;
        public readonly ServiceRequestState Request;
        public readonly PipeReply<PetToolInteractionSession> Reply;

        public BeginToolRequest(PetToolDefinition tool, ServiceOrder order, PetInstance pet, ServiceRequestState request, PipeReply<PetToolInteractionSession> reply)
        {
            Tool = tool;
            Order = order;
            Pet = pet;
            Request = request;
            Reply = reply;
        }
    }

    public readonly struct ToolInputRequest : IPipeMsg
    {
        public readonly PetToolInteractionSession Session;
        public readonly PetToolInteractionMode Mode;
        public readonly float Amount;
        public readonly PipeReply<bool> Reply;

        public ToolInputRequest(PetToolInteractionSession session, PetToolInteractionMode mode, float amount, PipeReply<bool> reply)
        {
            Session = session;
            Mode = mode;
            Amount = amount;
            Reply = reply;
        }
    }

    public readonly struct CompleteToolRequest : IPipeMsg
    {
        public readonly PetToolInteractionSession Session;
        public readonly PipeReply<PetToolUseResult> Reply;

        public CompleteToolRequest(PetToolInteractionSession session, PipeReply<PetToolUseResult> reply)
        {
            Session = session;
            Reply = reply;
        }
    }

    public readonly struct CancelToolRequest : IPipeMsg
    {
        public readonly PetToolInteractionSession Session;

        public CancelToolRequest(PetToolInteractionSession session)
        {
            Session = session;
        }
    }

    public readonly struct PetCareCompleted : IPipeMsg
    {
        public readonly PetCareResult Result;

        public PetCareCompleted(PetCareResult result)
        {
            Result = result;
        }
    }

    public readonly struct ToolInteractionStarted : IPipeMsg
    {
        public readonly PetToolInteractionSession Session;

        public ToolInteractionStarted(PetToolInteractionSession session)
        {
            Session = session;
        }
    }

    public readonly struct ToolInteractionProgressed : IPipeMsg
    {
        public readonly PetToolInteractionSession Session;

        public ToolInteractionProgressed(PetToolInteractionSession session)
        {
            Session = session;
        }
    }

    public readonly struct ToolInteractionCancelled : IPipeMsg
    {
        public readonly PetToolInteractionSession Session;

        public ToolInteractionCancelled(PetToolInteractionSession session)
        {
            Session = session;
        }
    }

    public readonly struct ToolInteractionCompleted : IPipeMsg
    {
        public readonly PetToolUseResult Result;

        public ToolInteractionCompleted(PetToolUseResult result)
        {
            Result = result;
        }
    }
}
