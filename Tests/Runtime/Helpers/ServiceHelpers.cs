using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Services;
using UnityEngine;

namespace DynamicBox.Quest.Tests.Helpers
{
    /// <summary>
    /// Factory helpers for creating MonoBehaviour service instances on temporary GameObjects.
    /// Always call Object.DestroyImmediate(service.gameObject) in [TearDown].
    /// </summary>
    public static class ServiceHelpers
    {
        public static DefaultFlagService CreateFlagService()
        {
            var go = new GameObject("TestFlagService");
            return go.AddComponent<DefaultFlagService>();
        }

        public static SimpleAreaService CreateAreaService()
        {
            var go = new GameObject("TestAreaService");
            return go.AddComponent<SimpleAreaService>();
        }

        public static SimpleInventoryService CreateInventoryService()
        {
            var go = new GameObject("TestInventoryService");
            return go.AddComponent<SimpleInventoryService>();
        }

        public static DefaultTimeService CreateTimeService()
        {
            var go = new GameObject("TestTimeService");
            return go.AddComponent<DefaultTimeService>();
        }

        public static QuestContext CreateContextWithAllServices()
        {
            return new QuestContext(
                areaService: CreateAreaService(),
                inventoryService: CreateInventoryService(),
                timeService: CreateTimeService(),
                flagService: CreateFlagService()
            );
        }
    }
}
