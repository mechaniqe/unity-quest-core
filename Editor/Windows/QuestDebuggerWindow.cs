using UnityEngine;
using UnityEditor;
using DynamicBox.Quest.Core;
using System.Linq;
using System.Collections.Generic;

namespace DynamicBox.Quest.Editor.Windows
{
    public class QuestDebuggerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private QuestManager _questManager;
        private bool _autoRefresh = true;
        private float _lastRefreshTime;
        
        [MenuItem("Tools/DynamicBox Quest/Quest Debugger")]
        public static void ShowWindow()
        {
            GetWindow<QuestDebuggerWindow>("Quest Debugger");
        }

        private void OnEnable()
        {
            FindQuestManager();
        }

        private void OnGUI()
        {
            DrawToolbar();
            
            if (_questManager == null)
            {
                DrawNoQuestManagerWarning();
                return;
            }

            DrawQuestStates();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                FindQuestManager();
            }
            
            GUILayout.FlexibleSpace();
            
            _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNoQuestManagerWarning()
        {
            EditorGUILayout.HelpBox(
                "No QuestManager found in the scene. Make sure you have a QuestManager component active in your scene.",
                MessageType.Warning);
                
            if (GUILayout.Button("Find QuestManager"))
            {
                FindQuestManager();
            }
        }

        private void DrawQuestStates()
        {
            if (_questManager.ActiveQuests == null || _questManager.ActiveQuests.Count == 0)
            {
                EditorGUILayout.HelpBox("No active quests found.", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var questLog in _questManager.ActiveQuests.Values)
            {
                DrawQuestLog(questLog);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawQuestLog(QuestState questState)
        {
            var quest = questState.Definition;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Quest header
            EditorGUILayout.BeginHorizontal();
            var statusColor = GetStatusColor(questState.Status);
            var originalColor = GUI.color;
            GUI.color = statusColor;
            
            EditorGUILayout.LabelField($"● {quest.DisplayName}", EditorStyles.boldLabel);
            GUI.color = originalColor;
            
            EditorGUILayout.LabelField($"[{questState.Status}]", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            
            // Quest info
            EditorGUILayout.LabelField($"ID: {quest.QuestId}", EditorStyles.miniLabel);
            if (!string.IsNullOrEmpty(quest.Description))
            {
                EditorGUILayout.LabelField($"Description: {quest.Description}", EditorStyles.wordWrappedMiniLabel);
            }
            
            EditorGUILayout.Space();
            
            // Objectives
            if (quest.Objectives != null && quest.Objectives.Count > 0)
            {
                EditorGUILayout.LabelField("Objectives:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                
                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    var objective = quest.Objectives[i];
                    if (objective == null) continue;
                    
                    questState.TryGetObjective(objective.ObjectiveId, out var objState);
                    var status = objState?.Status ?? ObjectiveStatus.NotStarted;
                    
                    var objColor = GetObjectiveColor(status);
                    GUI.color = objColor;
                    
                    string statusSymbol = GetObjectiveSymbol(status);
                    EditorGUILayout.LabelField($"{statusSymbol} {objective.DisplayName} [{objState}]");
                    
                    GUI.color = originalColor;
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Debug actions
            EditorGUILayout.BeginHorizontal();
            
            if (questState.Status == QuestStatus.InProgress && GUILayout.Button("Complete", GUILayout.Width(80)))
            {
                _questManager.CompleteQuest(questState);
            }
            
            if (questState.Status == QuestStatus.InProgress && GUILayout.Button("Fail", GUILayout.Width(80)))
            {
                _questManager.FailQuest(questState);
            }
            
            if (GUILayout.Button("Details", GUILayout.Width(80)))
            {
                EditorGUIUtility.PingObject(quest);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private Color GetStatusColor(QuestStatus status)
        {
            switch (status)
            {
                case QuestStatus.NotStarted: return Color.gray;
                case QuestStatus.InProgress: return Color.yellow;
                case QuestStatus.Completed: return Color.green;
                case QuestStatus.Failed: return Color.red;
                default: return Color.white;
            }
        }

        private Color GetObjectiveColor(ObjectiveStatus status)
        {
            switch (status)
            {
                case ObjectiveStatus.NotStarted: return Color.gray;
                case ObjectiveStatus.InProgress: return Color.yellow;
                case ObjectiveStatus.Completed: return Color.green;
                case ObjectiveStatus.Failed: return Color.red;
                default: return Color.white;
            }
        }

        private string GetObjectiveSymbol(ObjectiveStatus status)
        {
            switch (status)
            {
                case ObjectiveStatus.NotStarted: return "○";
                case ObjectiveStatus.InProgress: return "●";
                case ObjectiveStatus.Completed: return "✓";
                case ObjectiveStatus.Failed: return "✗";
                default: return "?";
            }
        }

        private void FindQuestManager()
        {
            _questManager = FindObjectOfType<QuestManager>();
        }

        private void Update()
        {
            if (_autoRefresh && Time.realtimeSinceStartup - _lastRefreshTime > 1f)
            {
                _lastRefreshTime = Time.realtimeSinceStartup;
                Repaint();
            }
        }
    }
}
