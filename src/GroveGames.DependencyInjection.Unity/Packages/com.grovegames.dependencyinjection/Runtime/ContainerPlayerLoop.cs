using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.LowLevel;

namespace GroveGames.DependencyInjection.Unity
{
    internal static class ContainerPlayerLoop
    {
        private struct ContainerInitialization
        {
        }

        private struct ContainerUpdate
        {
        }

        private struct ContainerFixedUpdate
        {
        }

        private struct ContainerLateUpdate
        {
        }

        private sealed class Entry
        {
            public readonly IContainer Container;
            public readonly IReadOnlyList<IUpdatable> Updatables;
            public readonly IReadOnlyList<IFixedUpdatable> FixedUpdatables;
            public readonly IReadOnlyList<ILateUpdatable> LateUpdatables;

            public Entry(IContainer container)
            {
                Container = container;
                Updatables = container.ResolveAll<IUpdatable>();
                FixedUpdatables = container.ResolveAll<IFixedUpdatable>();
                LateUpdatables = container.ResolveAll<ILateUpdatable>();
            }
        }

        private static Entry[] s_entries = Array.Empty<Entry>();
        private static List<TaskCompletionSource<bool>> s_frameWaiters = new();
        private static List<TaskCompletionSource<bool>> s_completingWaiters = new();

        public static void Install()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystems(ref playerLoop);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.Initialization), typeof(ContainerInitialization), Initialization);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.Update), typeof(ContainerUpdate), Update);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.FixedUpdate), typeof(ContainerFixedUpdate), FixedUpdate);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.PreLateUpdate), typeof(ContainerLateUpdate), LateUpdate);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void Uninstall()
        {
            s_entries = Array.Empty<Entry>();
            var waiters = s_frameWaiters;
            s_frameWaiters = new List<TaskCompletionSource<bool>>();

            for (var i = 0; i < waiters.Count; i++)
            {
                waiters[i].TrySetCanceled();
            }

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystems(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static Task NextFrameAsync()
        {
            var waiter = new TaskCompletionSource<bool>();
            s_frameWaiters.Add(waiter);
            return waiter.Task;
        }

        public static void Add(IContainer container)
        {
            if (container.IsDisposed || IndexOf(container) >= 0)
            {
                return;
            }

            var entry = new Entry(container);
            var entries = s_entries;
            var newEntries = new Entry[entries.Length + 1];
            Array.Copy(entries, newEntries, entries.Length);
            newEntries[entries.Length] = entry;
            s_entries = newEntries;
        }

        public static void Remove(IContainer container)
        {
            var index = IndexOf(container);

            if (index < 0)
            {
                return;
            }

            var entries = s_entries;
            var newEntries = new Entry[entries.Length - 1];
            Array.Copy(entries, 0, newEntries, 0, index);
            Array.Copy(entries, index + 1, newEntries, index, entries.Length - index - 1);
            s_entries = newEntries;
        }

        private static int IndexOf(IContainer container)
        {
            var entries = s_entries;

            for (var i = 0; i < entries.Length; i++)
            {
                if (ReferenceEquals(entries[i].Container, container))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void Initialization()
        {
            if (s_frameWaiters.Count == 0)
            {
                return;
            }

            var waiters = s_frameWaiters;
            s_frameWaiters = s_completingWaiters;
            s_completingWaiters = waiters;

            for (var i = 0; i < waiters.Count; i++)
            {
                waiters[i].TrySetResult(true);
            }

            waiters.Clear();
        }

        private static void Update()
        {
            var deltaTime = Time.deltaTime;
            var entries = s_entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var updatables = entry.Updatables;

                try
                {
                    for (var j = 0; j < updatables.Count && !entry.Container.IsDisposed; j++)
                    {
                        updatables[j].Update(deltaTime);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            var entries = s_entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var fixedUpdatables = entry.FixedUpdatables;

                try
                {
                    for (var j = 0; j < fixedUpdatables.Count && !entry.Container.IsDisposed; j++)
                    {
                        fixedUpdatables[j].FixedUpdate(deltaTime);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            var entries = s_entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var lateUpdatables = entry.LateUpdatables;

                try
                {
                    for (var j = 0; j < lateUpdatables.Count && !entry.Container.IsDisposed; j++)
                    {
                        lateUpdatables[j].LateUpdate(deltaTime);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void AddSystem(ref PlayerLoopSystem playerLoop, Type phaseType, Type systemType, PlayerLoopSystem.UpdateFunction update)
        {
            var phases = playerLoop.subSystemList;

            if (phases == null)
            {
                return;
            }

            for (var i = 0; i < phases.Length; i++)
            {
                if (phases[i].type != phaseType)
                {
                    continue;
                }

                var systems = phases[i].subSystemList ?? Array.Empty<PlayerLoopSystem>();
                var newSystems = new PlayerLoopSystem[systems.Length + 1];
                Array.Copy(systems, newSystems, systems.Length);
                newSystems[systems.Length] = new PlayerLoopSystem
                {
                    type = systemType,
                    updateDelegate = update
                };
                phases[i].subSystemList = newSystems;
                return;
            }
        }

        private static void RemoveSystems(ref PlayerLoopSystem playerLoop)
        {
            var systems = playerLoop.subSystemList;

            if (systems == null)
            {
                return;
            }

            var remaining = new List<PlayerLoopSystem>(systems.Length);

            for (var i = 0; i < systems.Length; i++)
            {
                var system = systems[i];

                if (system.type == typeof(ContainerInitialization) || system.type == typeof(ContainerUpdate) || system.type == typeof(ContainerFixedUpdate) || system.type == typeof(ContainerLateUpdate))
                {
                    continue;
                }

                RemoveSystems(ref system);
                remaining.Add(system);
            }

            playerLoop.subSystemList = remaining.ToArray();
        }
    }
}
