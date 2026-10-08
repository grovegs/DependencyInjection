using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.LowLevel;

namespace GroveGames.DependencyInjection.Unity
{
    internal static class ContainerPlayerLoop
    {
        private struct ContainerInitialization
        {
        }

        private struct ContainerFrameUpdate
        {
        }

        private struct ContainerPhysicsUpdate
        {
        }

        private struct ContainerPostFrameUpdate
        {
        }

        private sealed class Entry
        {
            public readonly IContainer Container;
            public readonly IReadOnlyList<IPreFrameUpdatable> PreFrameUpdatables;
            public readonly IReadOnlyList<IFrameUpdatable> FrameUpdatables;
            public readonly IReadOnlyList<IPostFrameUpdatable> PostFrameUpdatables;
            public readonly IReadOnlyList<IPhysicsUpdatable> PhysicsUpdatables;

            public Entry(IContainer container)
            {
                Container = container;
                PreFrameUpdatables = container.ResolveAll<IPreFrameUpdatable>();
                FrameUpdatables = container.ResolveAll<IFrameUpdatable>();
                PostFrameUpdatables = container.ResolveAll<IPostFrameUpdatable>();
                PhysicsUpdatables = container.ResolveAll<IPhysicsUpdatable>();
            }
        }

        private static Entry[] s_entries = Array.Empty<Entry>();

        public static void Install()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystems(ref playerLoop);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.Initialization), typeof(ContainerInitialization), Initialization);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.Update), typeof(ContainerFrameUpdate), FrameUpdate);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.FixedUpdate), typeof(ContainerPhysicsUpdate), PhysicsUpdate);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.PreLateUpdate), typeof(ContainerPostFrameUpdate), PostFrameUpdate);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void Uninstall()
        {
            s_entries = Array.Empty<Entry>();
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystems(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
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
            try
            {
                ContainerBootstrapper.StartPendingScenes();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void FrameUpdate()
        {
            var deltaTime = Time.deltaTime;
            var entries = s_entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var preFrameUpdatables = entry.PreFrameUpdatables;

                for (var j = 0; j < preFrameUpdatables.Count && !entry.Container.IsDisposed; j++)
                {
                    try
                    {
                        preFrameUpdatables[j].PreFrameUpdate(deltaTime);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            entries = s_entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var frameUpdatables = entry.FrameUpdatables;

                for (var j = 0; j < frameUpdatables.Count && !entry.Container.IsDisposed; j++)
                {
                    try
                    {
                        frameUpdatables[j].FrameUpdate(deltaTime);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }
        }

        private static void PhysicsUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            var entries = s_entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var physicsUpdatables = entry.PhysicsUpdatables;

                for (var j = 0; j < physicsUpdatables.Count && !entry.Container.IsDisposed; j++)
                {
                    try
                    {
                        physicsUpdatables[j].PhysicsUpdate(deltaTime);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }
        }

        private static void PostFrameUpdate()
        {
            var deltaTime = Time.deltaTime;
            var entries = s_entries;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var postFrameUpdatables = entry.PostFrameUpdatables;

                for (var j = 0; j < postFrameUpdatables.Count && !entry.Container.IsDisposed; j++)
                {
                    try
                    {
                        postFrameUpdatables[j].PostFrameUpdate(deltaTime);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
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

                if (system.type == typeof(ContainerInitialization) || system.type == typeof(ContainerFrameUpdate) || system.type == typeof(ContainerPhysicsUpdate) || system.type == typeof(ContainerPostFrameUpdate))
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
