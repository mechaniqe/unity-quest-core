using System;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core
{
    public interface IConditionInstance
    {
        bool IsMet { get; }

        void Bind(EventManager eventManager, QuestContext context, Action onChanged);
        void Unbind(EventManager eventManager, QuestContext context);
    }

    // Optional interface for conditions that also want periodic polling.
    public interface IPollingConditionInstance
    {
        void Refresh(QuestContext context, Action onChanged);
    }
}
