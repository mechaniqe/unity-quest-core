#nullable enable
using UnityEngine;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition asset that completes after a specified amount of time has elapsed.
    /// Uses the polling system to track time progression.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Conditions/Time Elapsed", fileName = "NewTimeElapsedCondition")]
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
