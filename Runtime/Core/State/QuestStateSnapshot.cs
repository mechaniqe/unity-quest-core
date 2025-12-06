using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBox.Quest.Core.State
{
    /// <summary>
    /// Serializable snapshot of quest state for save/load systems.
    /// Contains only the data needed to restore quest progress.
    /// Use QuestStateManager to create snapshots and restore from them.
    /// </summary>
    [Serializable]
    public class QuestStateSnapshot
    {
        [Tooltip("Unique identifier for the quest")]
        public string QuestId;
        
        [Tooltip("Current status of the quest")]
        public QuestStatus Status;
        
        [Tooltip("Status of each objective (using list for Unity JsonUtility compatibility)")]
        public List<ObjectiveStatusEntry> ObjectiveStatuses = new List<ObjectiveStatusEntry>();

        /// <summary>
        /// Helper method to get objective statuses as a dictionary for easy lookup.
        /// </summary>
        public Dictionary<string, ObjectiveStatus> GetObjectiveStatusesDict()
        {
            var dict = new Dictionary<string, ObjectiveStatus>();
            foreach (var entry in ObjectiveStatuses)
            {
                dict[entry.ObjectiveId] = entry.Status;
            }
            return dict;
        }

        /// <summary>
        /// Validates that the snapshot contains required data.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(QuestId) && ObjectiveStatuses != null;
        }
    }

    /// <summary>
    /// Serializable entry for objective status.
    /// Unity's JsonUtility doesn't support Dictionary, so we use a list of entries.
    /// </summary>
    [Serializable]
    public class ObjectiveStatusEntry
    {
        [Tooltip("Unique identifier for the objective")]
        public string ObjectiveId;
        
        [Tooltip("Current status of the objective")]
        public ObjectiveStatus Status;
    }

    /// <summary>
    /// Container for multiple quest snapshots (represents a full save file).
    /// Can be serialized to JSON using Unity's JsonUtility or other serializers.
    /// </summary>
    [Serializable]
    public class QuestSaveData
    {
        [Tooltip("All active and completed quests")]
        public List<QuestStateSnapshot> Quests = new List<QuestStateSnapshot>();

        [Tooltip("Timestamp when the save was created")]
        public string SaveTimestamp;

        [Tooltip("Optional metadata (save slot, player name, etc.)")]
        public string Metadata;

        /// <summary>
        /// Creates a save data object with current timestamp.
        /// </summary>
        public static QuestSaveData Create()
        {
            return new QuestSaveData
            {
                SaveTimestamp = DateTime.UtcNow.ToString("o"), // ISO 8601 format
                Quests = new List<QuestStateSnapshot>()
            };
        }

        /// <summary>
        /// Finds a quest snapshot by ID.
        /// </summary>
        public QuestStateSnapshot FindQuest(string questId)
        {
            return Quests.Find(q => q.QuestId == questId);
        }

        /// <summary>
        /// Validates that all snapshots in the save data are valid.
        /// </summary>
        public bool IsValid()
        {
            if (Quests == null) return false;
            foreach (var quest in Quests)
            {
                if (quest == null || !quest.IsValid())
                    return false;
            }
            return true;
        }
    }
}
