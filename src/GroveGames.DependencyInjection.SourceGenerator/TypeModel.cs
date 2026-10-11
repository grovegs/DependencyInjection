namespace GroveGames.DependencyInjection.SourceGenerator;

internal sealed class TypeModel : IEquatable<TypeModel>
{
    private readonly string _name;
    private readonly EquatableArray<string>? _constructorParameterTypes;
    private readonly EquatableArray<MethodModel> _injectMethods;

    public string Name => _name;
    public EquatableArray<string>? ConstructorParameterTypes => _constructorParameterTypes;
    public EquatableArray<MethodModel> InjectMethods => _injectMethods;

    public TypeModel(string name, EquatableArray<string>? constructorParameterTypes, EquatableArray<MethodModel> injectMethods)
    {
        _name = name;
        _constructorParameterTypes = constructorParameterTypes;
        _injectMethods = injectMethods;
    }

    public bool Equals(TypeModel? other)
    {
        return other is not null
            && _name == other._name
            && _constructorParameterTypes.Equals(other._constructorParameterTypes)
            && _injectMethods.Equals(other._injectMethods);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TypeModel);
    }

    public override int GetHashCode()
    {
        return unchecked((((_name.GetHashCode() * 31) + _constructorParameterTypes.GetHashCode()) * 31) + _injectMethods.GetHashCode());
    }
}
