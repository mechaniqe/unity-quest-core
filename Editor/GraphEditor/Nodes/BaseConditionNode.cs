using DynamicBox.Quest.Core;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Base class for all condition nodes in the graph.
    /// Condition nodes represent the logic for completing or failing objectives.
    /// </summary>
    public abstract class BaseConditionNode : BaseQuestNode
    {
        public ConditionAsset Asset { get; protected set; }
        public Port InputPort => _inputPort;

        protected BaseConditionNode(ConditionAsset asset)
        {
            Asset = asset;
            AddToClassList("condition-node");

            // All condition nodes have an input port
            _inputPort = CreateInputPort("Input", Port.Capacity.Single);
            inputContainer.Add(_inputPort);
        }

        /// <summary>
        /// Creates a property display for condition-specific data.
        /// </summary>
        protected VisualElement CreatePropertyDisplay(string label, string value)
        {
            var container = new VisualElement();
            container.AddToClassList("property-container");

            var propertyLabel = new Label($"{label}: {value}");
            propertyLabel.AddToClassList("property-label");
            container.Add(propertyLabel);

            return container;
        }

        /// <summary>
        /// Creates a service dependency indicator.
        /// </summary>
        protected VisualElement CreateServiceBadge(string serviceName)
        {
            var badge = CreateLabel($"🔍 {serviceName}", "service-badge");
            return badge;
        }
    }

    /// <summary>
    /// Generic condition node for unknown or unsupported condition types.
    /// </summary>
    public class GenericConditionNode : BaseConditionNode
    {
        public GenericConditionNode(ConditionAsset asset) : base(asset)
        {
            title = "❓ CONDITION";
            
            if (asset != null)
            {
                var typeLabel = CreateLabel($"Type: {asset.GetType().Name}", "node-name-label");
                mainContainer.Add(typeLabel);

                var assetLabel = CreateLabel($"Asset: {asset.name}", "node-description-label");
                mainContainer.Add(assetLabel);
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        public override UnityEngine.Object GetAsset()
        {
            return Asset;
        }
    }
}
