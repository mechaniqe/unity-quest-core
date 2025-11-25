using System;

namespace DynamicBox.Quest.Core
{
    public interface IConditionInstance
    {
        bool IsMet { get; }

        void Bind(IQuestEventBus eventBus, QuestContext context, Action onChanged);
        void Unbind(IQuestEventBus eventBus, QuestContext context);
    }

    // Optional interface for conditions that also want periodic polling.
    public interface IPollingConditionInstance
    {
        void Refresh(QuestContext context, Action onChanged);
    }
}
