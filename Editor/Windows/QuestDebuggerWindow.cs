using DynamicBox.Quest.Core;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DynamicBox.Quest.Editor.Windows
{
    public class QuestDebuggerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private QuestManager _questManager;
        private bool _autoRefresh = true;
        private float _lastRefreshTime;
        
        #region Unity Methods

        void OnEnable()
        {
            FindQuestManager();
        }

        void OnGUI()
        {
            DrawToolbar();
            
            if (_questManager == null)
            {
                DrawNoQuestManagerWarning();
                return;
            }

            DrawQuestStates();
        }

        #endregion

        [MenuItem("Tools/DynamicBox/Quest System/Quest Debugger")]
        public static void ShowWindow()
        {
            GetWindow<QuestDebuggerWindow>("Quest Debugger");
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

            foreach (var questState in _questManager.ActiveQuests)
            {
                DrawQuestLog(questState);
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
                    EditorGUILayout.LabelField($"{statusSymbol} {objective.DisplayName} [{status}]");
                    
                    GUI.color = originalColor;
                    
                    // Show objective details
                    if (objState != null && status == ObjectiveStatus.InProgress)
                    {
                        EditorGUI.indentLevel++;
                        DrawObjectiveDetails(objective, objState);
                        EditorGUI.indentLevel--;
                    }
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

        private void DrawObjectiveDetails(ObjectiveAsset objective, ObjectiveState objState)
        {
            // Show objective ID and description
            if (!string.IsNullOrEmpty(objective.ObjectiveId))
            {
                EditorGUILayout.LabelField($"ID: {objective.ObjectiveId}", EditorStyles.miniLabel);
            }
            
            if (!string.IsNullOrEmpty(objective.Description))
            {
                EditorGUILayout.LabelField($"Description: {objective.Description}", EditorStyles.wordWrappedMiniLabel);
            }
            
            // Show completion condition
            if (objective.CompletionCondition != null)
            {
                var completionInstance = objState.CompletionInstance;
                var isMet = completionInstance?.IsMet ?? false;
                var conditionColor = isMet ? Color.green : Color.white;
                
                GUI.color = conditionColor;
                EditorGUILayout.LabelField($"Completion: {objective.CompletionCondition.name} {(isMet ? "✓" : "○")}", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            
            // Show fail condition if any
            if (objective.FailCondition != null)
            {
                var failInstance = objState.FailInstance;
                var isMet = failInstance?.IsMet ?? false;
                var conditionColor = isMet ? Color.red : Color.white;
                
                GUI.color = conditionColor;
                EditorGUILayout.LabelField($"Fail: {objective.FailCondition.name} {(isMet ? "✗" : "○")}", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            
            // Show prerequisites
            if (objective.Prerequisites != null && objective.Prerequisites.Count > 0)
            {
                EditorGUILayout.LabelField($"Prerequisites: {objective.Prerequisites.Count}", EditorStyles.miniLabel);
            }
            
            // Show if optional
            if (objective.IsOptional)
            {
                EditorGUILayout.LabelField("(Optional)", EditorStyles.miniLabel);
            }
        }

        private void FindQuestManager()
        {
            _questManager = FindFirstObjectByType<QuestManager>();
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
