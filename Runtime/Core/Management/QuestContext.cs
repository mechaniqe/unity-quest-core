#nullable enable
using System;
using System.Collections.Generic;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Context object that provides game services to condition instances.
    /// Acts as a type-safe service locator for quest-related game systems.
    /// </summary>
    public sealed class QuestContext
    {
        private readonly Dictionary<Type, object> _services = new();

        public IQuestAreaService? AreaService { get; }
        public IQuestInventoryService? InventoryService { get; }
        public IQuestTimeService? TimeService { get; }
        public IQuestFlagService? FlagService { get; }

        public QuestContext(
            IQuestAreaService? areaService = null,
            IQuestInventoryService? inventoryService = null,
            IQuestTimeService? timeService = null,
            IQuestFlagService? flagService = null)
        {
            AreaService = areaService;
            InventoryService = inventoryService;
            TimeService = timeService;
            FlagService = flagService;

            // Register services in type-safe dictionary
            if (areaService != null)
                _services[typeof(IQuestAreaService)] = areaService;
            if (inventoryService != null)
                _services[typeof(IQuestInventoryService)] = inventoryService;
            if (timeService != null)
                _services[typeof(IQuestTimeService)] = timeService;
            if (flagService != null)
                _services[typeof(IQuestFlagService)] = flagService;
        }

        /// <summary>
        /// Gets a service of the specified type, throwing if not available.
        /// Use this when the service is mandatory for the condition to function.
        /// </summary>
        /// <typeparam name="T">The service interface type to retrieve.</typeparam>
        /// <returns>The service instance if registered.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service is not registered.</exception>
        public T GetRequiredService<T>() where T : class
        {
            var service = GetService<T>();
            if (service == null)
            {
                throw new InvalidOperationException(
                    $"Required service {typeof(T).Name} is not available in QuestContext. " +
                    $"Ensure the service is registered in QuestPlayerRef.");
            }
            return service;
        }

        /// <summary>
        /// Gets a service of the specified type, or null if not available.
        /// Type-safe lookup using generic type parameter.
        /// </summary>
        /// <typeparam name="T">The service interface type to retrieve.</typeparam>
        /// <returns>The service instance if registered, otherwise null.</returns>
        public T? GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }
            return null;
        }

        /// <summary>
        /// Checks if a service of the specified type is available.
        /// </summary>
        /// <typeparam name="T">The service interface type to check for.</typeparam>
        /// <returns>True if the service is registered; false otherwise.</returns>
        public bool HasService<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}
