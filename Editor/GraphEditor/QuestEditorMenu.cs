using UnityEditor;
using UnityEngine;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Utility methods for quest editor history.
    /// No menu items - recent quests are accessed via the toolbar dropdown in the graph editor.
    /// </summary>
    public static class QuestEditorMenu
    {
        public static void OpenQuestByPath(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<QuestAsset>(path);
            if (asset != null)
            {
                QuestGraphEditorWindow.OpenWindow();
                var window = EditorWindow.GetWindow<QuestGraphEditorWindow>();
                
                // Use reflection to call LoadQuest
                var method = typeof(QuestGraphEditorWindow).GetMethod("LoadQuest",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(window, new object[] { asset });
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Quest Not Found",
                    $"Could not load quest at path: {path}\n\nThe asset may have been moved or deleted.",
                    "OK"
                );
            }
        }
    }
}
