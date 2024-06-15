using System;
using DependencyInjection.Resolution;

namespace DependencyInjection.Core
{
    public interface IContainer : IRegistrationResolver, IDisposable
    {
        void AddChild(IContainer child);
        void RemoveChild(IContainer child);
    }
}
