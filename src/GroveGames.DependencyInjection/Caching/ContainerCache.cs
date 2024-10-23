namespace GroveGames.DependencyInjection.Caching;

internal sealed class ContainerCache : IContainerCache
{
    public static readonly ContainerCache Shared = new();

    private readonly List<IContainer> _containers = [];

    public IContainer? Find(in ReadOnlySpan<char> path)
    {
        if (path.SequenceEqual("/"))
        {
            return _containers.FirstOrDefault(c => string.Equals(c.Name, string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var container in _containers)
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

    public void Add(IContainer container)
    {
        _containers.Add(container);
    }

    public void Remove(IContainer container)
    {
        _containers.Remove(container);
    }

    public void Clear()
    {
        _containers.Clear();
    }
}