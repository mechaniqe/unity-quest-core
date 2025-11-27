#nullable enable

namespace DynamicBox.Quest.Core
{
    public sealed class ObjectiveState
    {
        public ObjectiveAsset Definition { get; }
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.NotStarted;

        internal IConditionInstance? CompletionInstance { get; }
        internal IConditionInstance? FailInstance { get; }

        // Public accessors for testing
        public IConditionInstance? GetCompletionInstance() => CompletionInstance;
        public IConditionInstance? GetFailInstance() => FailInstance;

        public ObjectiveState(ObjectiveAsset definition)
        {
            Definition = definition;

            if (definition.CompletionCondition != null)
                CompletionInstance = definition.CompletionCondition.CreateInstance();

            if (definition.FailCondition != null)
                FailInstance = definition.FailCondition.CreateInstance();
        }

        public void SetStatus(ObjectiveStatus status)
        {
            Status = status;
        }
    }
}
