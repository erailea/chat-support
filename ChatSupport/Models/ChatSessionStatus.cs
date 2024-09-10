namespace ChatSupport.Models
{
    public enum ChatSessionStatus
    {
        /// <summary>
        /// Client is inactive
        /// </summary>
        InActive,
        /// <summary>
        /// The chat session is pending in queue
        /// </summary>
        Pending,
        /// <summary>
        /// The chat session is in progress. agent
        /// </summary>
        InProgress,
        /// <summary>
        /// The chat session is completed
        /// </summary>
        Completed
    }
}
