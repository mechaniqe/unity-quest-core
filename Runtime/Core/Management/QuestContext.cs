#nullable enable
using System;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Context object that provides game services to condition instances.
    /// Acts as a service locator for quest-related game systems.
    /// </summary>
    public sealed class QuestContext
    {
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
        }

        /// <summary>
        /// Gets a service of the specified type, throwing if not available.
        /// </summary>
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
        /// </summary>
        public T? GetService<T>() where T : class
        {
            return typeof(T).Name switch
            {
                nameof(IQuestAreaService) => AreaService as T,
                nameof(IQuestInventoryService) => InventoryService as T,
                nameof(IQuestTimeService) => TimeService as T,
                nameof(IQuestFlagService) => FlagService as T,
                _ => null
            };
        }

        /// <summary>
        /// Checks if a service of the specified type is available.
        /// </summary>
        public bool HasService<T>() where T : class
        {
            return GetService<T>() != null;
        }
    }
}
