using System.Collections.Generic;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Tests.Helpers
{
    /// <summary>
    /// Fluent builder for QuestAsset test instances.
    /// </summary>
    public class QuestBuilder
    {
        private string _questId = "test-quest";
        private string _displayName = "Test Quest";
        private string _description = "";
        private readonly List<ObjectiveAsset> _objectives = new();

        public QuestBuilder WithQuestId(string id) { _questId = id; return this; }
        public QuestBuilder WithDisplayName(string name) { _displayName = name; return this; }
        public QuestBuilder WithDescription(string desc) { _description = desc; return this; }
        public QuestBuilder AddObjective(ObjectiveAsset obj) { _objectives.Add(obj); return this; }

        public QuestAsset Build() =>
            QuestAsset.CreateForTest(_questId, _displayName, _description, _objectives);
    }
}
