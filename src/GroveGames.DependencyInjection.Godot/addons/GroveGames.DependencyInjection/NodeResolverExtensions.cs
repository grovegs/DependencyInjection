using Godot;

namespace GroveGames.DependencyInjection;

public static class NodeResolverExtensions
{
    public static void InjectTree(this IObjectResolver resolver, Node node)
    {
        if (node == null)
        {
            GD.PushError("Node cannot be null.");
            return;
        }

        resolver.Inject(node);
        var children = node.GetChildren(true);

        for (var i = 0; i < children.Count; i++)
        {
            resolver.InjectTree(children[i]);
        }
    }

    public static T Instantiate<T>(this IObjectResolver resolver, PackedScene scene)
        where T : Node
    {
        if (scene == null)
        {
            GD.PushError("PackedScene cannot be null.");
            return null;
        }

        var instance = scene.Instantiate<T>();
        resolver.InjectTree(instance);
        return instance;
    }
}
