using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Stores graph layout information (node positions, zoom, pan) for a quest.
    /// Serialized to JSON and saved alongside the quest asset.
    /// </summary>
    [Serializable]
    public class QuestGraphLayout
    {
        [Serializable]
        public class NodePosition
        {
            public string nodeId;  // Asset GUID or instance ID
            public float x;
            public float y;
            public float width;
            public float height;
        }

        public List<NodePosition> nodePositions = new List<NodePosition>();
        public float zoom = 1.0f;
        public float panX = 0f;
        public float panY = 0f;

        /// <summary>
        /// Gets the file path for the layout file associated with a quest asset.
        /// </summary>
        public static string GetLayoutPath(string questAssetPath)
        {
            if (string.IsNullOrEmpty(questAssetPath))
                return null;

            // Replace .asset with .questgraph.json
            var directory = System.IO.Path.GetDirectoryName(questAssetPath);
            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(questAssetPath);
            return System.IO.Path.Combine(directory, fileNameWithoutExtension + ".questgraph.json");
        }

        /// <summary>
        /// Saves the layout to a JSON file.
        /// </summary>
        public void Save(string path)
        {
            try
            {
                // Round all values before serialization for cleaner JSON
                RoundAllValues();
                
                var json = JsonUtility.ToJson(this, true);
                System.IO.File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save quest graph layout: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the layout from a JSON file.
        /// </summary>
        public static QuestGraphLayout Load(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    return JsonUtility.FromJson<QuestGraphLayout>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load quest graph layout: {e.Message}");
            }

            return new QuestGraphLayout();
        }

        /// <summary>
        /// Gets a unique identifier for a node based on its asset.
        /// </summary>
        public static string GetNodeId(UnityEngine.Object asset)
        {
            if (asset == null)
                return null;

            // Use asset GUID if available
            var path = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(path))
            {
                return UnityEditor.AssetDatabase.AssetPathToGUID(path);
            }

            // Fall back to instance ID for unsaved assets
            return $"instance_{asset.GetInstanceID()}";
        }

        /// <summary>
        /// Sets the position for a node.
        /// </summary>
        public void SetNodePosition(string nodeId, Rect position)
        {
            if (string.IsNullOrEmpty(nodeId))
                return;

            // Round to 2 decimal places for cleaner JSON
            var roundedX = Mathf.Round(position.x * 100f) / 100f;
            var roundedY = Mathf.Round(position.y * 100f) / 100f;
            var roundedWidth = Mathf.Round(position.width * 100f) / 100f;
            var roundedHeight = Mathf.Round(position.height * 100f) / 100f;

            var existing = nodePositions.Find(np => np.nodeId == nodeId);
            if (existing != null)
            {
                existing.x = roundedX;
                existing.y = roundedY;
                existing.width = roundedWidth;
                existing.height = roundedHeight;
            }
            else
            {
                nodePositions.Add(new NodePosition
                {
                    nodeId = nodeId,
                    x = roundedX,
                    y = roundedY,
                    width = roundedWidth,
                    height = roundedHeight
                });
            }
        }

        /// <summary>
        /// Gets the position for a node.
        /// </summary>
        public Rect? GetNodePosition(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return null;

            var position = nodePositions.Find(np => np.nodeId == nodeId);
            if (position != null)
            {
                return new Rect(position.x, position.y, position.width, position.height);
            }

            return null;
        }

        /// <summary>
        /// Rounds all float values to 2 decimal places for cleaner JSON output.
        /// </summary>
        private void RoundAllValues()
        {
            foreach (var nodePos in nodePositions)
            {
                nodePos.x = Mathf.Round(nodePos.x * 100f) / 100f;
                nodePos.y = Mathf.Round(nodePos.y * 100f) / 100f;
                nodePos.width = Mathf.Round(nodePos.width * 100f) / 100f;
                nodePos.height = Mathf.Round(nodePos.height * 100f) / 100f;
            }
            
            zoom = Mathf.Round(zoom * 100f) / 100f;
            panX = Mathf.Round(panX * 100f) / 100f;
            panY = Mathf.Round(panY * 100f) / 100f;
        }
    }
}
