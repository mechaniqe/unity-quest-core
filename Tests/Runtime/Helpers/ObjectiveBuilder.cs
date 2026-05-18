using System.Collections.Generic;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Tests.Helpers
{
    /// <summary>
    /// Fluent builder for ObjectiveAsset test instances.
    /// </summary>
    public class ObjectiveBuilder
    {
        private string _objectiveId = "test-objective";
        private string _title = "";
        private string _description = "";
        private bool _isOptional;
        private bool _isRetryable;
        private readonly List<ObjectiveAsset> _prerequisites = new();
        private ConditionAsset? _completionCondition;
        private ConditionAsset? _failCondition;

        public ObjectiveBuilder WithObjectiveId(string id) { _objectiveId = id; return this; }
        public ObjectiveBuilder WithTitle(string title) { _title = title; return this; }
        public ObjectiveBuilder AsOptional(bool optional = true) { _isOptional = optional; return this; }
        public ObjectiveBuilder AsRetryable(bool retryable = true) { _isRetryable = retryable; return this; }
        public ObjectiveBuilder AddPrerequisite(ObjectiveAsset prereq) { _prerequisites.Add(prereq); return this; }
        public ObjectiveBuilder WithCompletionCondition(ConditionAsset? cond) { _completionCondition = cond; return this; }
        public ObjectiveBuilder WithFailCondition(ConditionAsset? cond) { _failCondition = cond; return this; }

        public ObjectiveAsset Build() =>
            ObjectiveAsset.CreateForTest(
                _objectiveId, _title, _description, _isOptional,
                _prerequisites, _completionCondition, _failCondition, _isRetryable);
    }
}
