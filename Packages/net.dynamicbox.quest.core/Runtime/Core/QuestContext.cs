namespace DynamicBox.Quest.Core
{
    // Marker holder for whatever services the game offers to conditions.
    // For v0.1, this is minimal and can be expanded per project.
    public sealed class QuestContext
    {
        // Example: inventory, areas, time, etc.
        // These are interfaces implemented by game-side systems.

        public IQuestAreaService? AreaService { get; }
        public IQuestInventoryService? InventoryService { get; }
        public IQuestTimeService? TimeService { get; }

        public QuestContext(
            IQuestAreaService? areaService,
            IQuestInventoryService? inventoryService,
            IQuestTimeService? timeService)
        {
            AreaService = areaService;
            InventoryService = inventoryService;
            TimeService = timeService;
        }
    }

    // For v0.1, we only define minimal interfaces; actual implementations live in game code.
    public interface IQuestAreaService { /* optional for now */ }
    public interface IQuestInventoryService { /* optional for now */ }
    public interface IQuestTimeService { /* optional for now */ }
}
