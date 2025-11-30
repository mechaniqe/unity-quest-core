using DynamicBox.Quest.Core;
using System.Collections.Generic;

namespace DynamicBox.Quest.Tests
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
            // Use factory method instead of reflection for type safety and performance
            return QuestAsset.CreateForTest(
                _questId,
                _displayName,
                _description,
                _objectives
            );
        }
    }
}
