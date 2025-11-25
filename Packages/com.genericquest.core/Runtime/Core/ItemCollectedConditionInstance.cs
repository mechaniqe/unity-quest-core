using System;

namespace GenericQuest.Core.Conditions
{
    /// <summary>
    /// Event class that conditions can listen to when an item is collected.
    /// Games will publish this event from their item pickup systems.
    /// </summary>
    public sealed class ItemCollectedEvent
    {
        public string ItemId { get; }
        public int Amount { get; }

        public ItemCollectedEvent(string itemId, int amount = 1)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }

    public sealed class ItemCollectedConditionInstance : IConditionInstance
    {
        private readonly string _itemId;
        private readonly int _requiredCount;
        private int _currentCount;
        private Action? _onChanged;

        public bool IsMet => _currentCount >= _requiredCount;

        public ItemCollectedConditionInstance(string itemId, int requiredCount)
        {
            _itemId = itemId;
            _requiredCount = requiredCount;
            _currentCount = 0;
        }

        public void Bind(IQuestEventBus eventBus, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;
            eventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
        }

        public void Unbind(IQuestEventBus eventBus, QuestContext context)
        {
            eventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
            _onChanged = null;
        }

        private void OnItemCollected(ItemCollectedEvent evt)
        {
            if (evt.ItemId == _itemId)
            {
                int oldCount = _currentCount;
                _currentCount += evt.Amount;

                if (oldCount < _requiredCount && _currentCount >= _requiredCount)
                    _onChanged?.Invoke();
                else if (oldCount >= _requiredCount && _currentCount >= _requiredCount)
                    return; // Already met, no change
                else
                    _onChanged?.Invoke(); // Count changed but not met yet
            }
        }
    }
}
