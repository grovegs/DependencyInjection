namespace GroveGames.DependencyInjection.Unity
{
    public interface IPreFrameUpdatable
    {
        void PreFrameUpdate(float deltaTime);
    }
}
