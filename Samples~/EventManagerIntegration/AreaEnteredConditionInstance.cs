#nullable enable
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Samples.EventManagerIntegration
{
    public sealed class AreaEnteredConditionInstance : EventDrivenConditionBase<AreaEnteredEvent>
    {
        private readonly string _areaId;
        private readonly string? _areaDescription;
        private bool _isCompleted;

        public override bool IsMet => _isCompleted;

        public AreaEnteredConditionInstance(string areaId, string? areaDescription = null)
        {
            _areaId = areaId;
            _areaDescription = areaDescription;
        }

        protected override void HandleEvent(AreaEnteredEvent evt)
        {
            if (evt.AreaId == _areaId && !_isCompleted)
            {
                _isCompleted = true;
                NotifyChanged();
            }
        }

        public override string ToString() => $"Enter area: {_areaDescription ?? _areaId}";
    }
}
