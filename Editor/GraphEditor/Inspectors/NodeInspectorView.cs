using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Inspector panel for editing node properties.
    /// Displays on the right side of the graph editor.
    /// </summary>
    public class NodeInspectorView : VisualElement
    {
        private Label _titleLabel;
        private VisualElement _contentContainer;
        private BaseQuestNode _currentNode;
        private QuestGraphEditorWindow _editorWindow;

        public NodeInspectorView(QuestGraphEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
            
            // Setup inspector styling
            style.width = 300;
            style.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
            style.borderLeftWidth = 1;
            style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            style.paddingTop = 10;
            style.paddingBottom = 10;
            style.paddingLeft = 10;
            style.paddingRight = 10;

            // Title
            _titleLabel = new Label("Inspector");
            _titleLabel.style.fontSize = 16;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.marginBottom = 10;
            _titleLabel.style.color = Color.white;
            Add(_titleLabel);

            // Content container
            _contentContainer = new VisualElement();
            Add(_contentContainer);

            ShowNoSelection();
        }

        /// <summary>
        /// Updates the inspector to show properties for the selected node.
        /// </summary>
        public void UpdateSelection(BaseQuestNode node)
        {
            _currentNode = node;
            _contentContainer.Clear();

            if (node == null)
            {
                ShowNoSelection();
                return;
            }

            if (node is QuestNode questNode)
            {
                ShowQuestInspector(questNode);
            }
            else if (node is ObjectiveNode objectiveNode)
            {
                ShowObjectiveInspector(objectiveNode);
            }
            else if (node is BaseConditionNode conditionNode)
            {
                ShowConditionInspector(conditionNode);
            }
            else
            {
                ShowNoSelection();
            }
        }

        private void ShowNoSelection()
        {
            _titleLabel.text = "Inspector";
            _contentContainer.Clear();
            
            var helpLabel = new Label("Select a node to edit its properties");
            helpLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            helpLabel.style.marginTop = 20;
            helpLabel.style.whiteSpace = WhiteSpace.Normal;
            _contentContainer.Add(helpLabel);
        }

        private void ShowQuestInspector(QuestNode node)
        {
            _titleLabel.text = "Quest Properties";

            if (node.Asset == null)
            {
                ShowCreateAssetPrompt("Quest");
                return;
            }

            AddSectionHeader("Basic Information");
            
            // Quest ID
            AddPropertyField("Quest ID:", node.Asset.QuestId, false);
            
            // Display Name
            AddEditableField("Display Name:", node.Asset.DisplayName, (newValue) =>
            {
                UpdateQuestField(node.Asset, "displayName", newValue);
                node.RefreshNode();
            });

            // Description
            AddEditableTextArea("Description:", node.Asset.Description, (newValue) =>
            {
                UpdateQuestField(node.Asset, "description", newValue);
            });

            AddSectionHeader("Statistics");
            AddPropertyField("Objectives:", (node.Asset.Objectives?.Count ?? 0).ToString(), false);

            AddAssetReferenceButton(node.Asset);
        }

        private void ShowObjectiveInspector(ObjectiveNode node)
        {
            _titleLabel.text = "Objective Properties";

            if (node.Asset == null)
            {
                ShowCreateAssetPrompt("Objective");
                return;
            }

            AddSectionHeader("Basic Information");
            
            // Objective ID
            AddPropertyField("Objective ID:", node.Asset.ObjectiveId, false);
            
            // Title
            AddEditableField("Title:", node.Asset.Title, (newValue) =>
            {
                UpdateObjectiveField(node.Asset, "title", newValue);
                node.RefreshNode();
            });

            // Description
            AddEditableTextArea("Description:", node.Asset.Description, (newValue) =>
            {
                UpdateObjectiveField(node.Asset, "description", newValue);
            });

            AddSectionHeader("Configuration");
            
            // Is Optional toggle
            AddEditableToggle("Is Optional:", node.Asset.IsOptional, (newValue) =>
            {
                UpdateObjectiveField(node.Asset, "isOptional", newValue);
                node.RefreshNode();
            });

            AddSectionHeader("Conditions");
            AddPropertyField("Completion:", node.Asset.CompletionCondition?.name ?? "None", true);
            AddPropertyField("Failure:", node.Asset.FailCondition?.name ?? "None", true);

            AddSectionHeader("Prerequisites");
            var prereqCount = node.Asset.Prerequisites?.Count ?? 0;
            AddPropertyField("Count:", prereqCount.ToString(), false);

            AddAssetReferenceButton(node.Asset);
        }

        private void ShowConditionInspector(BaseConditionNode node)
        {
            _titleLabel.text = "Condition Properties";

            if (node.Asset == null)
            {
                ShowCreateAssetPrompt("Condition");
                return;
            }

            AddSectionHeader("Condition Type");
            AddPropertyField("Type:", node.Asset.GetType().Name.Replace("ConditionAsset", ""), false);
            AddPropertyField("Asset Name:", node.Asset.name, false);

            AddSectionHeader("Configuration");
            
            // Show condition-specific editable properties
            if (node is ItemCollectedConditionNode)
            {
                ShowItemCollectedConditionFields(node.Asset);
            }
            else if (node is AreaEnteredConditionNode)
            {
                ShowAreaEnteredConditionFields(node.Asset);
            }
            else if (node is TimeElapsedConditionNode)
            {
                ShowTimeElapsedConditionFields(node.Asset);
            }
            else if (node is CustomFlagConditionNode)
            {
                ShowCustomFlagConditionFields(node.Asset);
            }
            else if (node is ConditionGroupConditionNode)
            {
                ShowConditionGroupFields(node.Asset);
            }
            else
            {
                // Fallback for unknown types - show read-only
                var fields = node.Asset.GetType().GetFields(
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (field.Name.StartsWith("<") || field.Name == "hideFlags")
                        continue;

                    var value = field.GetValue(node.Asset);
                    var displayValue = value?.ToString() ?? "null";
                    
                    // Format field name
                    var fieldName = field.Name;
                    if (fieldName.StartsWith("_"))
                        fieldName = fieldName.Substring(1);
                    
                    // Capitalize first letter
                    fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
                    
                    AddPropertyField(fieldName + ":", displayValue, false);
                }
            }

            AddAssetReferenceButton(node.Asset);
        }

        private void ShowItemCollectedConditionFields(ConditionAsset asset)
        {
            var conditionIdField = typeof(ConditionAsset).GetField("conditionId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requiredCountField = asset.GetType().GetField("requiredCount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var itemId = conditionIdField?.GetValue(asset) as string ?? "";
            var requiredCount = (int)(requiredCountField?.GetValue(asset) ?? 1);

            AddEditableField("Item ID:", itemId, (newValue) =>
            {
                UpdateConditionField(asset, conditionIdField, newValue);
            });

            AddEditableIntField("Required Count:", requiredCount, (newValue) =>
            {
                UpdateConditionField(asset, requiredCountField, newValue);
            });
        }

        private void ShowAreaEnteredConditionFields(ConditionAsset asset)
        {
            var conditionIdField = typeof(ConditionAsset).GetField("conditionId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var areaDescField = asset.GetType().GetField("_areaDescription",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var areaId = conditionIdField?.GetValue(asset) as string ?? "";
            var areaDesc = areaDescField?.GetValue(asset) as string ?? "";

            AddEditableField("Area ID:", areaId, (newValue) =>
            {
                UpdateConditionField(asset, conditionIdField, newValue);
            });

            AddEditableTextArea("Area Description:", areaDesc, (newValue) =>
            {
                UpdateConditionField(asset, areaDescField, newValue);
            });
        }

        private void ShowTimeElapsedConditionFields(ConditionAsset asset)
        {
            var requiredSecondsField = asset.GetType().GetField("requiredSeconds",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var requiredSeconds = (float)(requiredSecondsField?.GetValue(asset) ?? 10f);

            AddEditableFloatField("Required Seconds:", requiredSeconds, (newValue) =>
            {
                UpdateConditionField(asset, requiredSecondsField, newValue);
            });
        }

        private void ShowCustomFlagConditionFields(ConditionAsset asset)
        {
            var conditionIdField = typeof(ConditionAsset).GetField("conditionId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var expectedValueField = asset.GetType().GetField("_expectedValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = asset.GetType().GetField("_description",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var flagId = conditionIdField?.GetValue(asset) as string ?? "";
            var expectedValue = (bool)(expectedValueField?.GetValue(asset) ?? true);
            var description = descriptionField?.GetValue(asset) as string ?? "";

            AddEditableField("Flag ID:", flagId, (newValue) =>
            {
                UpdateConditionField(asset, conditionIdField, newValue);
            });

            AddEditableToggle("Expected Value:", expectedValue, (newValue) =>
            {
                UpdateConditionField(asset, expectedValueField, newValue);
            });

            AddEditableTextArea("Description:", description, (newValue) =>
            {
                UpdateConditionField(asset, descriptionField, newValue);
            });
        }

        private void ShowConditionGroupFields(ConditionAsset asset)
        {
            var operatorField = asset.GetType().GetField("@operator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var childrenField = asset.GetType().GetField("children",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var operatorValue = operatorField?.GetValue(asset);
            var operatorName = operatorValue?.ToString() ?? "And";
            
            var children = childrenField?.GetValue(asset) as System.Collections.IList;
            var childCount = children?.Count ?? 0;

            AddPropertyField("Logic Operator:", operatorName, false);
            AddPropertyField("Children Count:", childCount.ToString(), false);
            
            var helpLabel = new Label("Tip: Edit children in the Unity Inspector");
            helpLabel.style.fontSize = 11;
            helpLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            helpLabel.style.marginTop = 5;
            _contentContainer.Add(helpLabel);
        }

        private void UpdateConditionField(ConditionAsset asset, System.Reflection.FieldInfo field, object newValue)
        {
            if (field == null || asset == null)
                return;

            Undo.RecordObject(asset, $"Modify {field.Name}");
            field.SetValue(asset, newValue);
            EditorUtility.SetDirty(asset);
            
            // Refresh the node to show updated values
            if (_currentNode != null)
            {
                _currentNode.RefreshNode();
            }
        }

        private void ShowCreateAssetPrompt(string assetType)
        {
            var helpLabel = new Label($"This {assetType.ToLower()} node is not yet saved as an asset.");
            helpLabel.style.color = new Color(0.9f, 0.7f, 0.3f, 1f);
            helpLabel.style.marginTop = 10;
            helpLabel.style.whiteSpace = WhiteSpace.Normal;
            _contentContainer.Add(helpLabel);

            var createButton = new Button(() => CreateAssetFromNode())
            {
                text = $"Create {assetType} Asset"
            };
            createButton.style.marginTop = 10;
            _contentContainer.Add(createButton);
        }

        private void CreateAssetFromNode()
        {
            if (_currentNode == null)
                return;

            string assetType = "";
            string defaultName = "New";
            ScriptableObject newAsset = null;

            if (_currentNode is QuestNode questNode)
            {
                assetType = "Quest";
                defaultName = "NewQuest";
                var quest = ScriptableObject.CreateInstance<QuestAsset>();
                
                // Initialize with reflection
                var questIdField = typeof(QuestAsset).GetField("questId", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var displayNameField = typeof(QuestAsset).GetField("displayName", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var descriptionField = typeof(QuestAsset).GetField("description", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var objectivesField = typeof(QuestAsset).GetField("objectives", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                questIdField?.SetValue(quest, System.Guid.NewGuid().ToString());
                displayNameField?.SetValue(quest, "New Quest");
                descriptionField?.SetValue(quest, "Quest description here");
                objectivesField?.SetValue(quest, new System.Collections.Generic.List<ObjectiveAsset>());

                newAsset = quest;
            }
            else if (_currentNode is ObjectiveNode objectiveNode)
            {
                assetType = "Objective";
                defaultName = "NewObjective";
                var objective = ScriptableObject.CreateInstance<ObjectiveAsset>();
                
                // Initialize with reflection
                var objectiveIdField = typeof(ObjectiveAsset).GetField("objectiveId", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var titleField = typeof(ObjectiveAsset).GetField("title", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var descriptionField = typeof(ObjectiveAsset).GetField("description", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var isOptionalField = typeof(ObjectiveAsset).GetField("isOptional", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var prerequisitesField = typeof(ObjectiveAsset).GetField("prerequisites", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                objectiveIdField?.SetValue(objective, System.Guid.NewGuid().ToString());
                titleField?.SetValue(objective, "New Objective");
                descriptionField?.SetValue(objective, "Objective description here");
                isOptionalField?.SetValue(objective, false);
                prerequisitesField?.SetValue(objective, new System.Collections.Generic.List<ObjectiveAsset>());

                newAsset = objective;
            }
            else if (_currentNode is BaseConditionNode conditionNode)
            {
                // Determine condition type and create appropriate asset
                if (_currentNode is ItemCollectedConditionNode)
                {
                    assetType = "ItemCollected";
                    defaultName = "NewItemCollectedCondition";
                    var condition = ScriptableObject.CreateInstance<ItemCollectedConditionAsset>();
                    
                    var conditionIdField = typeof(ConditionAsset).GetField("conditionId",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var requiredCountField = typeof(ItemCollectedConditionAsset).GetField("requiredCount",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    conditionIdField?.SetValue(condition, "item_id_here");
                    requiredCountField?.SetValue(condition, 1);
                    
                    newAsset = condition;
                }
                else if (_currentNode is AreaEnteredConditionNode)
                {
                    assetType = "AreaEntered";
                    defaultName = "NewAreaEnteredCondition";
                    var condition = ScriptableObject.CreateInstance<AreaEnteredConditionAsset>();
                    
                    var conditionIdField = typeof(ConditionAsset).GetField("conditionId",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var areaDescField = typeof(AreaEnteredConditionAsset).GetField("_areaDescription",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    conditionIdField?.SetValue(condition, "area_id_here");
                    areaDescField?.SetValue(condition, "Area description");
                    
                    newAsset = condition;
                }
                else if (_currentNode is TimeElapsedConditionNode)
                {
                    assetType = "TimeElapsed";
                    defaultName = "NewTimeElapsedCondition";
                    var condition = ScriptableObject.CreateInstance<TimeElapsedConditionAsset>();
                    
                    var requiredSecondsField = typeof(TimeElapsedConditionAsset).GetField("requiredSeconds",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    requiredSecondsField?.SetValue(condition, 10f);
                    
                    newAsset = condition;
                }
                else if (_currentNode is CustomFlagConditionNode)
                {
                    assetType = "CustomFlag";
                    defaultName = "NewCustomFlagCondition";
                    var condition = ScriptableObject.CreateInstance<CustomFlagConditionAsset>();
                    
                    var conditionIdField = typeof(ConditionAsset).GetField("conditionId",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var expectedValueField = typeof(CustomFlagConditionAsset).GetField("_expectedValue",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var descriptionField = typeof(CustomFlagConditionAsset).GetField("_description",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    conditionIdField?.SetValue(condition, "flag_id_here");
                    expectedValueField?.SetValue(condition, true);
                    descriptionField?.SetValue(condition, "Flag condition description");
                    
                    newAsset = condition;
                }
                else if (_currentNode is ConditionGroupConditionNode)
                {
                    assetType = "ConditionGroup";
                    defaultName = "NewConditionGroup";
                    var condition = ScriptableObject.CreateInstance<ConditionGroupAsset>();
                    
                    var operatorField = typeof(ConditionGroupAsset).GetField("@operator",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var childrenField = typeof(ConditionGroupAsset).GetField("children",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    // Set default to And operator (0)
                    operatorField?.SetValue(condition, 0);
                    childrenField?.SetValue(condition, new System.Collections.Generic.List<ConditionAsset>());
                    
                    newAsset = condition;
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Unknown Type",
                        "Unknown condition type. Cannot create asset.",
                        "OK"
                    );
                    return;
                }
            }

            if (newAsset == null)
                return;

            // Save asset to disk
            string path = EditorUtility.SaveFilePanelInProject(
                $"Save {assetType} Asset",
                defaultName,
                "asset",
                $"Choose a location to save the {assetType} asset");

            if (string.IsNullOrEmpty(path))
                return;

            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Update the node to reference the new asset
            if (_currentNode is QuestNode qNode)
            {
                qNode.Asset = newAsset as QuestAsset;
                qNode.RefreshNode();
            }
            else if (_currentNode is ObjectiveNode oNode)
            {
                oNode.Asset = newAsset as ObjectiveAsset;
                oNode.RefreshNode();
            }
            else if (_currentNode is BaseConditionNode cNode)
            {
                // Update the Asset property via reflection since it's in BaseConditionNode
                var assetProperty = typeof(BaseConditionNode).GetProperty("Asset",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                assetProperty?.SetValue(cNode, newAsset as ConditionAsset);
                cNode.RefreshNode();
            }

            // Refresh the inspector
            UpdateSelection(_currentNode);

            EditorUtility.DisplayDialog(
                "Asset Created",
                $"{assetType} asset created successfully at:\n{path}",
                "OK"
            );
        }

        private void AddSectionHeader(string title)
        {
            var header = new Label(title);
            header.style.fontSize = 13;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginTop = 15;
            header.style.marginBottom = 5;
            header.style.color = new Color(0.8f, 0.9f, 1f, 1f);
            _contentContainer.Add(header);

            var separator = new VisualElement();
            separator.style.height = 1;
            separator.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            separator.style.marginBottom = 8;
            _contentContainer.Add(separator);
        }

        private void AddPropertyField(string label, string value, bool isClickable)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginBottom = 5;

            var labelElement = new Label(label);
            labelElement.style.minWidth = 120;
            labelElement.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            labelElement.style.fontSize = 11;
            container.Add(labelElement);

            var valueElement = new Label(value);
            valueElement.style.color = Color.white;
            valueElement.style.fontSize = 11;
            valueElement.style.flexGrow = 1;
            
            if (isClickable)
            {
                valueElement.style.color = new Color(0.4f, 0.7f, 1f, 1f);
            }
            
            container.Add(valueElement);
            _contentContainer.Add(container);
        }

        private void AddEditableField(string label, string value, Action<string> onValueChanged)
        {
            var textField = new TextField(label)
            {
                value = value ?? ""
            };
            textField.style.marginBottom = 8;
            textField.labelElement.style.minWidth = 100;
            textField.labelElement.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            textField.RegisterValueChangedCallback(evt =>
            {
                onValueChanged?.Invoke(evt.newValue);
            });
            
            _contentContainer.Add(textField);
        }

        private void AddEditableTextArea(string label, string value, Action<string> onValueChanged)
        {
            var labelElement = new Label(label);
            labelElement.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            labelElement.style.marginBottom = 3;
            _contentContainer.Add(labelElement);

            var textField = new TextField()
            {
                value = value ?? "",
                multiline = true
            };
            textField.style.height = 80;
            textField.style.marginBottom = 8;
            
            textField.RegisterValueChangedCallback(evt =>
            {
                onValueChanged?.Invoke(evt.newValue);
            });
            
            _contentContainer.Add(textField);
        }

        private void AddEditableIntField(string label, int value, Action<int> onValueChanged)
        {
            var intField = new IntegerField(label)
            {
                value = value
            };
            intField.style.marginBottom = 8;
            intField.labelElement.style.minWidth = 100;
            intField.labelElement.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            intField.RegisterValueChangedCallback(evt =>
            {
                onValueChanged?.Invoke(evt.newValue);
            });
            
            _contentContainer.Add(intField);
        }

        private void AddEditableFloatField(string label, float value, Action<float> onValueChanged)
        {
            var floatField = new FloatField(label)
            {
                value = value
            };
            floatField.style.marginBottom = 8;
            floatField.labelElement.style.minWidth = 100;
            floatField.labelElement.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            floatField.RegisterValueChangedCallback(evt =>
            {
                onValueChanged?.Invoke(evt.newValue);
            });
            
            _contentContainer.Add(floatField);
        }

        private void AddEditableToggle(string label, bool value, Action<bool> onValueChanged)
        {
            var toggle = new Toggle(label)
            {
                value = value
            };
            toggle.style.marginBottom = 8;
            toggle.labelElement.style.minWidth = 100;
            toggle.labelElement.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            toggle.RegisterValueChangedCallback(evt =>
            {
                onValueChanged?.Invoke(evt.newValue);
            });
            
            _contentContainer.Add(toggle);
        }

        private void AddAssetReferenceButton(ScriptableObject asset)
        {
            var button = new Button(() =>
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            })
            {
                text = "Select in Project"
            };
            button.style.marginTop = 15;
            _contentContainer.Add(button);
        }

        private void UpdateQuestField(QuestAsset asset, string fieldName, object value)
        {
            var field = typeof(QuestAsset).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                Undo.RecordObject(asset, $"Modified Quest {fieldName}");
                field.SetValue(asset, value);
                EditorUtility.SetDirty(asset);
            }
        }

        private void UpdateObjectiveField(ObjectiveAsset asset, string fieldName, object value)
        {
            var field = typeof(ObjectiveAsset).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                Undo.RecordObject(asset, $"Modified Objective {fieldName}");
                field.SetValue(asset, value);
                EditorUtility.SetDirty(asset);
            }
        }
    }
}
