using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.State;
using UnityEngine;

namespace DynamicBox.Quest.Samples
{
    /// <summary>
    /// Minimal example of capturing and restoring quest state.
    /// Shows the core serialization API in ~30 lines of code.
    /// </summary>
    public class BasicSerializationSample : MonoBehaviour
    {
        [SerializeField] private QuestAsset questAsset;

        [ContextMenu("Capture Snapshot")]
        public void CaptureSnapshot()
        {
            // Create a quest state
            var questState = new QuestState(questAsset);
            
            // Capture snapshot
            var snapshot = QuestStateManager.CaptureSnapshot(questState);
            
            // Serialize to JSON
            string json = JsonUtility.ToJson(snapshot, prettyPrint: true);
            
            Debug.Log($"Captured snapshot:\n{json}");
        }

        [ContextMenu("Restore Snapshot")]
        public void RestoreSnapshot()
        {
            // Example JSON (normally loaded from file/PlayerPrefs/etc)
            string json = @"{
                ""QuestId"": ""example_quest"",
                ""Status"": 1,
                ""ObjectiveStatuses"": []
            }";
            
            // Deserialize
            var snapshot = JsonUtility.FromJson<QuestStateSnapshot>(json);
            
            // Restore quest state
            var context = new QuestContext();
            var restored = QuestStateManager.RestoreFromSnapshot(snapshot, questAsset, context);
            
            Debug.Log($"Restored quest: {restored.Definition.DisplayName} - Status: {restored.Status}");
        }
    }
}
