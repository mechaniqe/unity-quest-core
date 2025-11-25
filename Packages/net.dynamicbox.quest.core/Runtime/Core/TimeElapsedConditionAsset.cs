using UnityEngine;

namespace DynamicBox.Quest.Core.Conditions
{
    [CreateAssetMenu(menuName = "DynamicBox Quest/Conditions/Time Elapsed", fileName = "NewTimeElapsedCondition")]
    public class TimeElapsedConditionAsset : ConditionAsset
    {
        [SerializeField] private float requiredSeconds = 10f;

        public float RequiredSeconds => requiredSeconds;

        public override IConditionInstance CreateInstance()
        {
            return new TimeElapsedConditionInstance(requiredSeconds);
        }
    }
}
