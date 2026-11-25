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

        private static IContainer? s_root;

        public static void Install(IContainer root)
        {
            s_root = root;
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
            s_root = null;
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystems(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
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
            var root = s_root;

            if (root == null)
            {
                return;
            }

            var deltaTime = new DeltaTime(Time.deltaTime, Time.unscaledDeltaTime);

            try
            {
                root.PreFrameUpdate(deltaTime);
            }
            catch (Exception exception)
            {
                Log(exception);
            }

            try
            {
                root.FrameUpdate(deltaTime);
            }
            catch (Exception exception)
            {
                Log(exception);
            }
        }

        private static void PhysicsUpdate()
        {
            var root = s_root;

            if (root == null)
            {
                return;
            }

            try
            {
                root.PhysicsUpdate(new DeltaTime(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime));
            }
            catch (Exception exception)
            {
                Log(exception);
            }
        }

        private static void PostFrameUpdate()
        {
            var root = s_root;

            if (root == null)
            {
                return;
            }

            try
            {
                root.PostFrameUpdate(new DeltaTime(Time.deltaTime, Time.unscaledDeltaTime));
            }
            catch (Exception exception)
            {
                Log(exception);
            }
        }

        private static void Log(Exception exception)
        {
            if (exception is not AggregateException aggregate)
            {
                Debug.LogException(exception);
                return;
            }

            var flattened = aggregate.Flatten();

            for (var i = 0; i < flattened.InnerExceptions.Count; i++)
            {
                Debug.LogException(flattened.InnerExceptions[i]);
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
