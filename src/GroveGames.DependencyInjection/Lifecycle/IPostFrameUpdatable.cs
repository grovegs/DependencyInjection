namespace GroveGames.DependencyInjection;

public interface IPostFrameUpdatable
{
    public void PostFrameUpdate(DeltaTime deltaTime);
}
