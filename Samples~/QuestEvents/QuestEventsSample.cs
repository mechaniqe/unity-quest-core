using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Samples
{
    /// <summary>
    /// Shows how to listen to quest events and respond to progress changes.
    /// Demonstrates OnQuestCompleted, OnQuestFailed, OnObjectiveStatusChanged.
    /// </summary>
    public class QuestEventsSample : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private QuestAsset questToStart;

        void OnEnable()
        {
            // Subscribe to events
            questManager.OnQuestCompleted += HandleQuestCompleted;
            questManager.OnQuestFailed += HandleQuestFailed;
            questManager.OnObjectiveStatusChanged += HandleObjectiveChanged;
        }

        void OnDisable()
        {
            // Unsubscribe from events
            questManager.OnQuestCompleted -= HandleQuestCompleted;
            questManager.OnQuestFailed -= HandleQuestFailed;
            questManager.OnObjectiveStatusChanged -= HandleObjectiveChanged;
        }

        [ContextMenu("Start Quest")]
        public void StartQuest()
        {
            questManager.StartQuest(questToStart);
        }

        private void HandleQuestCompleted(QuestState quest)
        {
            Debug.Log($"🎉 Quest Completed: {quest.Definition.DisplayName}");
        }

        private void HandleQuestFailed(QuestState quest)
        {
            Debug.Log($"❌ Quest Failed: {quest.Definition.DisplayName}");
        }

        private void HandleObjectiveChanged(ObjectiveState objective)
        {
            Debug.Log($"📋 Objective Updated: {objective.Definition.Title} - {objective.Status}");
        }
    }
}
