namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Represents the current status of a quest.
    /// </summary>
    public enum QuestStatus
    {
        /// <summary>Quest has not been started yet.</summary>
        NotStarted,
        
        /// <summary>Quest is actively being pursued.</summary>
        InProgress,
        
        /// <summary>Quest has been successfully completed.</summary>
        Completed,
        
        /// <summary>Quest has failed and cannot be completed.</summary>
        Failed
    }

    /// <summary>
    /// Represents the current status of an objective within a quest.
    /// </summary>
    public enum ObjectiveStatus
    {
        /// <summary>Objective has not been started (prerequisites not met).</summary>
        NotStarted,
        
        /// <summary>Objective is actively being pursued.</summary>
        InProgress,
        
        /// <summary>Objective has been successfully completed.</summary>
        Completed,
        
        /// <summary>Objective has failed.</summary>
        Failed
    }
}
