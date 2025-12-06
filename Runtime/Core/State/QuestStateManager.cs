using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DynamicBox.Quest.Core.State
{
    /// <summary>
    /// Production-ready manager for quest state serialization and persistence.
    /// Provides save/load functionality for individual quests or entire quest collections.
    /// </summary>
    public static class QuestStateManager
    {
        /// <summary>
        /// Creates a serializable snapshot from a QuestState.
        /// </summary>
        public static QuestStateSnapshot CaptureSnapshot(QuestState questState)
        {
            if (questState == null)
                throw new ArgumentNullException(nameof(questState));

            var snapshot = new QuestStateSnapshot
            {
                QuestId = questState.Definition.QuestId,
                Status = questState.Status,
                ObjectiveStatuses = new List<ObjectiveStatusEntry>()
            };

            foreach (var objectiveState in questState.GetObjectiveStates())
            {
                snapshot.ObjectiveStatuses.Add(new ObjectiveStatusEntry
                {
                    ObjectiveId = objectiveState.Definition.ObjectiveId,
                    Status = objectiveState.Status
                });
            }

            return snapshot;
        }

        /// <summary>
        /// Creates snapshots for multiple quests and packages them into QuestSaveData.
        /// </summary>
        public static QuestSaveData CaptureAllSnapshots(IEnumerable<QuestState> questStates, string metadata = null)
        {
            if (questStates == null)
                throw new ArgumentNullException(nameof(questStates));

            var saveData = QuestSaveData.Create();
            saveData.Metadata = metadata;

            foreach (var questState in questStates)
            {
                if (questState != null)
                {
                    saveData.Quests.Add(CaptureSnapshot(questState));
                }
            }

            return saveData;
        }

        /// <summary>
        /// Restores a QuestState from a snapshot.
        /// Note: This creates a new QuestState - you need to add it to your quest manager.
        /// </summary>
        public static QuestState RestoreFromSnapshot(QuestStateSnapshot snapshot, QuestAsset questAsset, QuestContext context)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));
            if (questAsset == null)
                throw new ArgumentNullException(nameof(questAsset));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!snapshot.IsValid())
                throw new ArgumentException("Invalid quest snapshot", nameof(snapshot));

            if (questAsset.QuestId != snapshot.QuestId)
                throw new ArgumentException($"Quest asset ID '{questAsset.QuestId}' doesn't match snapshot ID '{snapshot.QuestId}'");

            // Create new quest state
            var questState = new QuestState(questAsset);
            questState.SetStatus(snapshot.Status);

            // Restore objective statuses
            var objectiveStatusMap = snapshot.GetObjectiveStatusesDict();
            foreach (var objectiveState in questState.GetObjectiveStates())
            {
                if (objectiveStatusMap.TryGetValue(objectiveState.Definition.ObjectiveId, out var status))
                {
                    objectiveState.SetStatus(status);
                }
            }

            // Note: Condition binding happens when quest is added to QuestManager
            // The context parameter is kept for API consistency and future use
            return questState;
        }

        /// <summary>
        /// Restores multiple quests from save data.
        /// </summary>
        public static List<QuestState> RestoreAllFromSnapshots(
            QuestSaveData saveData, 
            Dictionary<string, QuestAsset> questAssetMap, 
            QuestContext context)
        {
            if (saveData == null)
                throw new ArgumentNullException(nameof(saveData));
            if (questAssetMap == null)
                throw new ArgumentNullException(nameof(questAssetMap));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!saveData.IsValid())
                throw new ArgumentException("Invalid save data", nameof(saveData));

            var restoredQuests = new List<QuestState>();

            foreach (var snapshot in saveData.Quests)
            {
                if (questAssetMap.TryGetValue(snapshot.QuestId, out var questAsset))
                {
                    try
                    {
                        var questState = RestoreFromSnapshot(snapshot, questAsset, context);
                        restoredQuests.Add(questState);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to restore quest '{snapshot.QuestId}': {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Quest asset '{snapshot.QuestId}' not found - skipping");
                }
            }

            return restoredQuests;
        }

        #region File I/O Helpers

        /// <summary>
        /// Saves a single quest to a JSON file.
        /// </summary>
        public static void SaveQuestToFile(QuestState questState, string filePath)
        {
            if (questState == null)
                throw new ArgumentNullException(nameof(questState));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var snapshot = CaptureSnapshot(questState);
            var json = JsonUtility.ToJson(snapshot, true);
            
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Saves multiple quests to a JSON file.
        /// </summary>
        public static void SaveAllQuestsToFile(IEnumerable<QuestState> questStates, string filePath, string metadata = null)
        {
            if (questStates == null)
                throw new ArgumentNullException(nameof(questStates));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var saveData = CaptureAllSnapshots(questStates, metadata);
            var json = JsonUtility.ToJson(saveData, true);
            
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a single quest from a JSON file.
        /// </summary>
        public static QuestState LoadQuestFromFile(string filePath, QuestAsset questAsset, QuestContext context)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Quest save file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            var snapshot = JsonUtility.FromJson<QuestStateSnapshot>(json);
            
            return RestoreFromSnapshot(snapshot, questAsset, context);
        }

        /// <summary>
        /// Loads multiple quests from a JSON file.
        /// </summary>
        public static List<QuestState> LoadAllQuestsFromFile(
            string filePath, 
            Dictionary<string, QuestAsset> questAssetMap, 
            QuestContext context)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Quest save file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            var saveData = JsonUtility.FromJson<QuestSaveData>(json);
            
            return RestoreAllFromSnapshots(saveData, questAssetMap, context);
        }

        #endregion
    }
}
