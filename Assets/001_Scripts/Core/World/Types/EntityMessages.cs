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
    public readonly struct EntityRegisterRequest : IPipeMsg
    {
        public readonly GameEntity Entity;

        public EntityRegisterRequest(GameEntity entity)
        {
            Entity = entity;
        }
    }

    public readonly struct EntityUnregisterRequest : IPipeMsg
    {
        public readonly GameEntity Entity;

        public EntityUnregisterRequest(GameEntity entity)
        {
            Entity = entity;
        }
    }

    public readonly struct EntityRegistered : IPipeMsg
    {
        public readonly GameEntity Entity;

        public EntityRegistered(GameEntity entity)
        {
            Entity = entity;
        }
    }

    public readonly struct EntityUnregistered : IPipeMsg
    {
        public readonly GameEntity Entity;

        public EntityUnregistered(GameEntity entity)
        {
            Entity = entity;
        }
    }
}
