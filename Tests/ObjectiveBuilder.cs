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
            // Use factory method instead of reflection for type safety and performance
            return ObjectiveAsset.CreateForTest(
                _objectiveId,
                _title,
                _description,
                _isOptional,
                _prerequisites,
                _completionCondition,
                _failCondition
            );
        }
    }
}
