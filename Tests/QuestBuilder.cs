using GenericQuest.Core;
using System.Collections.Generic;

namespace GenericQuest.Tests
{
    /// <summary>
    /// Helper class to programmatically build quest assets for testing.
    /// </summary>
    public sealed class QuestBuilder
    {
        private string _questId = "test_quest";
        private string _displayName = "Test Quest";
        private string _description = "A test quest";
        private readonly List<ObjectiveAsset> _objectives = new();

        public QuestBuilder WithQuestId(string questId)
        {
            _questId = questId;
            return this;
        }

        public QuestBuilder WithDisplayName(string displayName)
        {
            _displayName = displayName;
            return this;
        }

        public QuestBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public QuestBuilder AddObjective(ObjectiveAsset objective)
        {
            if (objective != null)
                _objectives.Add(objective);
            return this;
        }

        public QuestAsset Build()
        {
            var quest = new QuestAsset();
            
            // Use reflection to set private fields since these are Unity ScriptableObjects
            var questIdField = typeof(QuestAsset).GetField("questId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var displayNameField = typeof(QuestAsset).GetField("displayName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(QuestAsset).GetField("description",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var objectivesField = typeof(QuestAsset).GetField("objectives",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            questIdField?.SetValue(quest, _questId);
            displayNameField?.SetValue(quest, _displayName);
            descriptionField?.SetValue(quest, _description);
            objectivesField?.SetValue(quest, _objectives);

            return quest;
        }
    }
}
