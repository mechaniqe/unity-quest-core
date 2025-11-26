#nullable enable
using System;
using DynamicBox.Quest.GameEvents;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core.Conditions
{
    public sealed class ItemCollectedConditionInstance : IConditionInstance
    {
        private readonly string _itemId;
        private readonly int _requiredCount;
        private int _currentCount;
        private Action? _onChanged;
        private EventManager? _eventManager;
        private EventManager.EventDelegate<ItemCollectedEvent>? _eventHandler;

        public bool IsMet => _currentCount >= _requiredCount;

        public ItemCollectedConditionInstance(string itemId, int requiredCount)
        {
            _itemId = itemId;
            _requiredCount = requiredCount;
            _currentCount = 0;
        }

        public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
        {
            _eventManager = eventManager;
            _onChanged = onChanged;
            _eventHandler = OnItemCollected;
            eventManager.AddListener<ItemCollectedEvent>(_eventHandler);
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            if (_eventManager != null && _eventHandler != null)
            {
                eventManager.RemoveListener<ItemCollectedEvent>(_eventHandler);
                _eventManager = null;
                _eventHandler = null;
            }
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
