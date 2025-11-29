#nullable enable
using UnityEngine;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Helper component that builds a QuestContext from game-provided service implementations.
    /// Automatically searches for service providers on this GameObject, or manually assign them.
    /// Service providers should inherit from the base classes:
    /// - QuestAreaServiceBase, QuestInventoryServiceBase, QuestTimeServiceBase, QuestFlagServiceBase
    /// </summary>
    public class QuestPlayerRef : MonoBehaviour
    {
        [Header("Automatic Service Discovery")]
        [Tooltip("If true, automatically finds service providers on this GameObject")]
        [SerializeField] private bool autoDiscoverServices = true;

        [Header("Manual Service Providers (Optional)")]
        [Tooltip("Area service (inherit from QuestAreaServiceBase)")]
        [SerializeField] private QuestAreaServiceBase? areaServiceProvider;
        
        [Tooltip("Inventory service (inherit from QuestInventoryServiceBase)")]
        [SerializeField] private QuestInventoryServiceBase? inventoryServiceProvider;
        
        [Tooltip("Time service (inherit from QuestTimeServiceBase)")]
        [SerializeField] private QuestTimeServiceBase? timeServiceProvider;
        
        [Tooltip("Flag service (inherit from QuestFlagServiceBase)")]
        [SerializeField] private QuestFlagServiceBase? flagServiceProvider;

        /// <summary>
        /// Builds a QuestContext by finding or using assigned service providers.
        /// Services are optional - quest system will work with whatever is available.
        /// </summary>
        public QuestContext BuildContext()
        {
            IQuestAreaService? area = areaServiceProvider;
            IQuestInventoryService? inv = inventoryServiceProvider;
            IQuestTimeService? time = timeServiceProvider;
            IQuestFlagService? flag = flagServiceProvider;

            if (autoDiscoverServices)
            {
                // Automatically find services on this GameObject if not manually assigned
                area ??= GetComponent<QuestAreaServiceBase>();
                inv ??= GetComponent<QuestInventoryServiceBase>();
                time ??= GetComponent<QuestTimeServiceBase>();
                flag ??= GetComponent<QuestFlagServiceBase>();
            }

            return new QuestContext(area, inv, time, flag);
        }
    }
}
