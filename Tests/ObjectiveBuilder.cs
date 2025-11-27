using DynamicBox.Quest.Core;
using System.Collections.Generic;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Helper class to programmatically build objective assets for testing.
    /// </summary>
    public sealed class ObjectiveBuilder
    {
        private string _objectiveId = "test_objective";
        private string _title = "Test Objective";
        private string _description = "A test objective";
        private bool _isOptional = false;
        private List<ObjectiveAsset> _prerequisites = new();
        private ConditionAsset _completionCondition;
        private ConditionAsset _failCondition;

        public ObjectiveBuilder WithObjectiveId(string objectiveId)
        {
            _objectiveId = objectiveId;
            return this;
        }

        public ObjectiveBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public ObjectiveBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ObjectiveBuilder AsOptional(bool isOptional = true)
        {
            _isOptional = isOptional;
            return this;
        }

        public ObjectiveBuilder AddPrerequisite(ObjectiveAsset prerequisite)
        {
            if (prerequisite != null)
                _prerequisites.Add(prerequisite);
            return this;
        }

        public ObjectiveBuilder WithCompletionCondition(ConditionAsset condition)
        {
            _completionCondition = condition;
            return this;
        }

        public ObjectiveBuilder WithFailCondition(ConditionAsset condition)
        {
            _failCondition = condition;
            return this;
        }

        public ObjectiveAsset Build()
        {
            var objective = UnityEngine.ScriptableObject.CreateInstance<ObjectiveAsset>();

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
            var completionConditionField = typeof(ObjectiveAsset).GetField("completionCondition",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var failConditionField = typeof(ObjectiveAsset).GetField("failCondition",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            objectiveIdField?.SetValue(objective, _objectiveId);
            titleField?.SetValue(objective, _title);
            descriptionField?.SetValue(objective, _description);
            isOptionalField?.SetValue(objective, _isOptional);
            prerequisitesField?.SetValue(objective, _prerequisites);
            completionConditionField?.SetValue(objective, _completionCondition);
            failConditionField?.SetValue(objective, _failCondition);

            return objective;
        }
    }
}
