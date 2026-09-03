namespace _001_Scripts.Core.World
{
    public interface IWorldSystem
    {
        void Initialize(IWorldContext world);
        void Tick(float deltaTime);
        void Shutdown();
    }
}
