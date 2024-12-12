namespace Game
{
    public interface ISystem
    {
        void Initialize() { }
        void Update(Entity entity) { }
        void Cleanup() { }
    }
}
