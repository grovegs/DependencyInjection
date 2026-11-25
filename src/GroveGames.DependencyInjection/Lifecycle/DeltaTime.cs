namespace GroveGames.DependencyInjection;

public readonly struct DeltaTime
{
    private readonly float _scaled;
    private readonly float _unscaled;

    public DeltaTime(float scaled, float unscaled)
    {
        _scaled = scaled;
        _unscaled = unscaled;
    }

    public float Scaled => _scaled;
    public float Unscaled => _unscaled;
}
