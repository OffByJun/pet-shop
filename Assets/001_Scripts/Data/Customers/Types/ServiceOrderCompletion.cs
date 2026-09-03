namespace _001_Scripts.Data.Customers
{
    public sealed class ServiceOrderCompletion
    {
        public ServiceOrderStatus Result { get; }
        public ServiceReward Reward { get; }

        public ServiceOrderCompletion(ServiceOrderStatus result, ServiceReward reward)
        {
            Result = result;
            Reward = reward;
        }
    }
}
