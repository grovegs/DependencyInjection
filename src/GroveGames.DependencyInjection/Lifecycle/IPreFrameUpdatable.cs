namespace GroveGames.DependencyInjection;

public interface IPreFrameUpdatable
{
    public void PreFrameUpdate(DeltaTime deltaTime);
}
