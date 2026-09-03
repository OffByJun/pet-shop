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
    public readonly struct ContentUnlockedQuery : IPipeMsg
    {
        public readonly string ContentId;
        public readonly PipeReply<bool> Reply;

        public ContentUnlockedQuery(string contentId, PipeReply<bool> reply)
        {
            ContentId = contentId;
            Reply = reply;
        }
    }

    public readonly struct UnlockProgressionRequest : IPipeMsg
    {
        public readonly ProgressionUnlockDefinition Definition;
        public readonly PipeReply<bool> Reply;

        public UnlockProgressionRequest(ProgressionUnlockDefinition definition, PipeReply<bool> reply)
        {
            Definition = definition;
            Reply = reply;
        }
    }

    public readonly struct CompleteEndingRequest : IPipeMsg
    {
        public readonly SettlementGoalDefinition Goal;
        public readonly PipeReply<bool> Reply;

        public CompleteEndingRequest(SettlementGoalDefinition goal, PipeReply<bool> reply)
        {
            Goal = goal;
            Reply = reply;
        }
    }

    public readonly struct ProgressionUnlocked : IPipeMsg
    {
        public readonly ProgressionUnlockDefinition Definition;

        public ProgressionUnlocked(ProgressionUnlockDefinition definition)
        {
            Definition = definition;
        }
    }

    public readonly struct EndingReached : IPipeMsg
    {
        public readonly SettlementGoalDefinition Goal;

        public EndingReached(SettlementGoalDefinition goal)
        {
            Goal = goal;
        }
    }
}
