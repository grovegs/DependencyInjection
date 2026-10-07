using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.LowLevel;

namespace GroveGames.DependencyInjection.Unity
{
    internal static class ContainerPlayerLoop
    {
        private struct ContainerUpdate
        {
        }

        private struct ContainerFixedUpdate
        {
        }

        private struct ContainerLateUpdate
        {
        }

        private static IContainer? s_container;

        public static void Register(IContainer container)
        {
            s_container = container;
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystems(ref playerLoop);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.Update), typeof(ContainerUpdate), Update);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.FixedUpdate), typeof(ContainerFixedUpdate), FixedUpdate);
            AddSystem(ref playerLoop, typeof(UnityEngine.PlayerLoop.PreLateUpdate), typeof(ContainerLateUpdate), LateUpdate);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void Unregister()
        {
            s_container = null;
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveSystems(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void Update()
        {
            var container = s_container;

            if (container == null)
            {
                return;
            }

            try
            {
                container.Update(Time.deltaTime);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void FixedUpdate()
        {
            var container = s_container;

            if (container == null)
            {
                return;
            }

            try
            {
                container.FixedUpdate(Time.fixedDeltaTime);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void LateUpdate()
        {
            var container = s_container;

            if (container == null)
            {
                return;
            }

            try
            {
                container.LateUpdate(Time.deltaTime);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
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

                if (system.type == typeof(ContainerUpdate) || system.type == typeof(ContainerFixedUpdate) || system.type == typeof(ContainerLateUpdate))
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
