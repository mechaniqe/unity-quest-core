using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Base class for all quest graph nodes.
    /// Provides common styling and port management functionality.
    /// </summary>
    public abstract class BaseQuestNode : Node
    {
        protected Port _inputPort;
        protected Port _outputPort;

        protected BaseQuestNode()
        {
            // Apply base styling
            AddToClassList("base-quest-node");
        }

        /// <summary>
        /// Gets the asset associated with this node (for layout persistence).
        /// </summary>
        public abstract UnityEngine.Object GetAsset();

        /// <summary>
        /// Refreshes the node display after property changes.
        /// </summary>
        public virtual void RefreshNode()
        {
            // Default implementation - subclasses can override
        }

        /// <summary>
        /// Creates an input port for this node.
        /// </summary>
        protected Port CreateInputPort(string portName = "Input", Port.Capacity capacity = Port.Capacity.Multi)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }

        /// <summary>
        /// Creates an output port for this node.
        /// </summary>
        protected Port CreateOutputPort(string portName = "Output", Port.Capacity capacity = Port.Capacity.Multi)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }

        /// <summary>
        /// Adds a styled label to the node.
        /// </summary>
        protected Label CreateLabel(string text, string className = "node-label")
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        /// <summary>
        /// Adds a property field to the node.
        /// </summary>
        protected TextField CreateTextField(string label, string value, bool multiline = false)
        {
            var textField = new TextField(label)
            {
                value = value,
                multiline = multiline
            };
            textField.AddToClassList("node-textfield");
            return textField;
        }

        /// <summary>
        /// Creates a colored badge/icon element.
        /// </summary>
        protected VisualElement CreateBadge(string text, Color color)
        {
            var badge = new Label(text);
            badge.AddToClassList("node-badge");
            badge.style.backgroundColor = color;
            return badge;
        }
    }
}
