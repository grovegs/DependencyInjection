namespace DependencyInjection.Core;

internal static class ContainerCache
{
    private static readonly List<IContainer> s_containers = [];

    public static IContainer? Find(ReadOnlySpan<char> path)
    {
        foreach (var container in s_containers)
        {
            var currentContainer = container;
            var currentPath = path;

            while (currentContainer != null)
            {
                var separatorIndex = currentPath.LastIndexOf('/');
                var segment = separatorIndex == -1 ? currentPath : currentPath[(separatorIndex + 1)..];

                if (!segment.Equals(currentContainer.Name, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (separatorIndex == -1)
                {
                    return container;
                }

                currentContainer = currentContainer.Parent;
                currentPath = currentPath[..separatorIndex];
            }
        }

        return null;
    }

    public static void Add(IContainer container)
    {
        s_containers.Add(container);
    }

    public static void Remove(IContainer container)
    {
        s_containers.Remove(container);
    }

    public static void Clear()
    {
        s_containers.Clear();
    }
}
