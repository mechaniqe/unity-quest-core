using System.Reflection;
using DynamicBox.Quest.Core;
using UnityEngine.UIElements;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Node for ItemCollectedConditionAsset.
    /// Displays item ID and quantity requirements.
    /// </summary>
    public class ItemCollectedConditionNode : BaseConditionNode
    {
        public ItemCollectedConditionNode(ConditionAsset asset) : base(asset)
        {
            title = "📦 ITEM COLLECTED";
            AddToClassList("item-condition-node");

            BuildContent();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void BuildContent()
        {
            if (Asset == null)
            {
                var placeholderLabel = CreateLabel("New Condition (Not Saved)", "node-placeholder");
                mainContainer.Add(placeholderLabel);
                return;
            }

            // Use reflection to read private fields
            var itemIdField = Asset.GetType().GetField("itemId",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var quantityField = Asset.GetType().GetField("requiredQuantity",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var itemId = itemIdField?.GetValue(Asset) as string ?? "unknown";
            var quantity = (int)(quantityField?.GetValue(Asset) ?? 1);

            mainContainer.Add(CreatePropertyDisplay("Item ID", itemId));
            mainContainer.Add(CreatePropertyDisplay("Quantity", quantity.ToString()));
            mainContainer.Add(CreateServiceBadge("Inventory"));

            var typeLabel = CreateLabel("📊 Event-Driven", "condition-type-label");
            mainContainer.Add(typeLabel);
        }

        public override void RefreshNode()
        {
            mainContainer.Clear();
            BuildContent();
            RefreshExpandedState();
        }

        public override UnityEngine.Object GetAsset()
        {
            return Asset;
        }
    }

    /// <summary>
    /// Node for AreaEnteredConditionAsset.
    /// Displays area ID requirement.
    /// </summary>
    public class AreaEnteredConditionNode : BaseConditionNode
    {
        public AreaEnteredConditionNode(ConditionAsset asset) : base(asset)
        {
            title = "📍 AREA ENTERED";
            AddToClassList("area-condition-node");

            BuildContent();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void BuildContent()
        {
            if (Asset == null)
            {
                var placeholderLabel = CreateLabel("New Condition (Not Saved)", "node-placeholder");
                mainContainer.Add(placeholderLabel);
                return;
            }

            var areaIdField = Asset.GetType().GetField("_areaId",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var areaId = areaIdField?.GetValue(Asset) as string ?? "unknown";

            mainContainer.Add(CreatePropertyDisplay("Area ID", areaId));
            mainContainer.Add(CreateServiceBadge("Area"));

            var typeLabel = CreateLabel("📊 Event-Driven", "condition-type-label");
            mainContainer.Add(typeLabel);
        }

        public override void RefreshNode()
        {
            mainContainer.Clear();
            BuildContent();
            RefreshExpandedState();
        }

        public override UnityEngine.Object GetAsset()
        {
            return Asset;
        }
    }

    /// <summary>
    /// Node for TimeElapsedConditionAsset.
    /// Displays time requirement.
    /// </summary>
    public class TimeElapsedConditionNode : BaseConditionNode
    {
        public TimeElapsedConditionNode(ConditionAsset asset) : base(asset)
        {
            title = "⏱️ TIME ELAPSED";
            AddToClassList("time-condition-node");

            BuildContent();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void BuildContent()
        {
            if (Asset == null)
            {
                var placeholderLabel = CreateLabel("New Condition (Not Saved)", "node-placeholder");
                mainContainer.Add(placeholderLabel);
                return;
            }

            var requiredSecondsField = Asset.GetType().GetField("requiredSeconds",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var seconds = (float)(requiredSecondsField?.GetValue(Asset) ?? 0f);

            mainContainer.Add(CreatePropertyDisplay("Required Time", $"{seconds}s"));
            mainContainer.Add(CreateServiceBadge("Time"));

            var typeLabel = CreateLabel("⏱️ Time-Based", "condition-type-label");
            mainContainer.Add(typeLabel);
        }

        public override void RefreshNode()
        {
            mainContainer.Clear();
            BuildContent();
            RefreshExpandedState();
        }

        public override UnityEngine.Object GetAsset()
        {
            return Asset;
        }
    }

    /// <summary>
    /// Node for CustomFlagConditionAsset.
    /// Displays flag ID and expected value.
    /// </summary>
    public class CustomFlagConditionNode : BaseConditionNode
    {
        public CustomFlagConditionNode(ConditionAsset asset) : base(asset)
        {
            title = "🏁 CUSTOM FLAG";
            AddToClassList("flag-condition-node");

            BuildContent();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void BuildContent()
        {
            if (Asset == null)
            {
                var placeholderLabel = CreateLabel("New Condition (Not Saved)", "node-placeholder");
                mainContainer.Add(placeholderLabel);
                return;
            }

            var flagIdField = Asset.GetType().GetField("_flagId",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var expectedValueField = Asset.GetType().GetField("expectedValue",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var flagId = flagIdField?.GetValue(Asset) as string ?? "unknown";
            var expectedValue = (bool)(expectedValueField?.GetValue(Asset) ?? true);

            mainContainer.Add(CreatePropertyDisplay("Flag ID", flagId));
            mainContainer.Add(CreatePropertyDisplay("Expected", expectedValue.ToString()));
            mainContainer.Add(CreateServiceBadge("Flag"));

            var typeLabel = CreateLabel("🎯 Custom Logic", "condition-type-label");
            mainContainer.Add(typeLabel);
        }

        public override void RefreshNode()
        {
            mainContainer.Clear();
            BuildContent();
            RefreshExpandedState();
        }

        public override UnityEngine.Object GetAsset()
        {
            return Asset;
        }
    }

    /// <summary>
    /// Node for ConditionGroupAsset.
    /// Displays AND/OR logic and child count.
    /// </summary>
    public class ConditionGroupConditionNode : BaseConditionNode
    {
        public ConditionGroupConditionNode(ConditionAsset asset) : base(asset)
        {
            title = "🔀 CONDITION GROUP";
            AddToClassList("group-condition-node");

            BuildContent();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void BuildContent()
        {
            if (Asset == null)
            {
                var placeholderLabel = CreateLabel("New Condition (Not Saved)", "node-placeholder");
                mainContainer.Add(placeholderLabel);
                return;
            }

            var operatorField = Asset.GetType().GetField("@operator",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var childrenField = Asset.GetType().GetField("children",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var operatorValue = operatorField?.GetValue(Asset);
            var operatorName = operatorValue?.ToString() ?? "And";
            
            var children = childrenField?.GetValue(Asset) as System.Collections.IList;
            var childCount = children?.Count ?? 0;

            mainContainer.Add(CreatePropertyDisplay("Logic", operatorName));
            mainContainer.Add(CreatePropertyDisplay("Children", childCount.ToString()));

            var typeLabel = CreateLabel("🔗 Composite", "condition-type-label");
            mainContainer.Add(typeLabel);
        }

        public override void RefreshNode()
        {
            mainContainer.Clear();
            BuildContent();
            RefreshExpandedState();
        }

        public override UnityEngine.Object GetAsset()
        {
            return Asset;
        }
    }
}
