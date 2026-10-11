namespace GroveGames.DependencyInjection.Generator;

internal sealed class MethodModel : IEquatable<MethodModel>
{
    private readonly string _name;
    private readonly EquatableArray<string> _parameterTypes;

    public string Name => _name;
    public EquatableArray<string> ParameterTypes => _parameterTypes;

    public MethodModel(string name, EquatableArray<string> parameterTypes)
    {
        _name = name;
        _parameterTypes = parameterTypes;
    }

    public bool Equals(MethodModel? other)
    {
        return other is not null && _name == other._name && _parameterTypes.Equals(other._parameterTypes);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MethodModel);
    }

    public override int GetHashCode()
    {
        return unchecked((_name.GetHashCode() * 31) + _parameterTypes.GetHashCode());
    }
}
