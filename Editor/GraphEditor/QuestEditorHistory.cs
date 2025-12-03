using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Manages recently opened quests for quick access in the graph editor.
    /// Stores quest paths in EditorPrefs for persistence across Unity sessions.
    /// </summary>
    public static class QuestEditorHistory
    {
        private const string HistoryKey = "QuestEditor.RecentQuests";
        private const int MaxRecentQuests = 10;

        [System.Serializable]
        private class RecentQuestData
        {
            public string path;
            public string displayName;
            public string questId;
            public long timestamp;
        }

        [System.Serializable]
        private class RecentQuestsList
        {
            public List<RecentQuestData> quests = new List<RecentQuestData>();
        }

        /// <summary>
        /// Adds a quest to the recent history.
        /// </summary>
        public static void AddToHistory(QuestAsset quest)
        {
            if (quest == null)
                return;

            string path = AssetDatabase.GetAssetPath(quest);
            if (string.IsNullOrEmpty(path))
                return;

            var history = LoadHistory();

            // Remove existing entry if present
            history.quests.RemoveAll(q => q.path == path);

            // Add new entry at the front
            history.quests.Insert(0, new RecentQuestData
            {
                path = path,
                displayName = quest.DisplayName,
                questId = quest.QuestId,
                timestamp = System.DateTime.Now.Ticks
            });

            // Limit to max count
            if (history.quests.Count > MaxRecentQuests)
            {
                history.quests.RemoveRange(MaxRecentQuests, history.quests.Count - MaxRecentQuests);
            }

            SaveHistory(history);
        }

        /// <summary>
        /// Gets the list of recent quests.
        /// </summary>
        public static List<(string path, string displayName, string questId)> GetRecentQuests(int maxCount = 5)
        {
            var history = LoadHistory();
            var result = new List<(string path, string displayName, string questId)>();

            foreach (var quest in history.quests.Take(maxCount))
            {
                // Verify the asset still exists
                var asset = AssetDatabase.LoadAssetAtPath<QuestAsset>(quest.path);
                if (asset != null)
                {
                    result.Add((quest.path, quest.displayName, quest.questId));
                }
            }

            return result;
        }

        /// <summary>
        /// Clears the recent quest history.
        /// </summary>
        public static void ClearHistory()
        {
            EditorPrefs.DeleteKey(HistoryKey);
        }

        private static RecentQuestsList LoadHistory()
        {
            string json = EditorPrefs.GetString(HistoryKey, "");
            if (string.IsNullOrEmpty(json))
            {
                return new RecentQuestsList();
            }

            try
            {
                return JsonUtility.FromJson<RecentQuestsList>(json);
            }
            catch
            {
                return new RecentQuestsList();
            }
        }

        private static void SaveHistory(RecentQuestsList history)
        {
            string json = JsonUtility.ToJson(history);
            EditorPrefs.SetString(HistoryKey, json);
        }
    }
}
