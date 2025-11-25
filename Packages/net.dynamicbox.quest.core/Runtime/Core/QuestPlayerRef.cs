using UnityEngine;

namespace DynamicBox.Quest.Core
{
    public class QuestPlayerRef : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour areaServiceProvider;      // implements IQuestAreaService
        [SerializeField] private MonoBehaviour inventoryServiceProvider; // implements IQuestInventoryService
        [SerializeField] private MonoBehaviour timeServiceProvider;      // implements IQuestTimeService

        public QuestContext BuildContext()
        {
            var area = areaServiceProvider as IQuestAreaService;
            var inv  = inventoryServiceProvider as IQuestInventoryService;
            var time = timeServiceProvider as IQuestTimeService;

            return new QuestContext(area, inv, time);
        }
    }
}
