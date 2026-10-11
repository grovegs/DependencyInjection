using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection.Generated;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GeneratedActivatorsAttribute : Attribute
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    private readonly Type _providerType;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type ProviderType => _providerType;

    public GeneratedActivatorsAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type providerType)
    {
        _providerType = providerType;
    }
}
