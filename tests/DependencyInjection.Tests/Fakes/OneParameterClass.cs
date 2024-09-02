namespace DependencyInjection.Tests.Fakes;

internal sealed class OneParameterClass : IOneParameterClass
{
    private readonly IZeroParameterClass? _zeroParameterClass;

    public OneParameterClass()
    {
        _zeroParameterClass = null;
    }

    public OneParameterClass(IZeroParameterClass zeroParameterClass)
    {
        _zeroParameterClass = zeroParameterClass;
    }

    public IZeroParameterClass? GetZeroParameterClass()
    {
        return _zeroParameterClass;
    }
}
