namespace YVR.Utilities
{
    /// <summary>
    /// Used for adding priority information as log prefix
    /// </summary>
    public class PriorityLogPrefix : LogPrefixBase
    {
        /// <summary>
        /// Constructor for PriorityLogPrefix
        /// </summary>
        /// <param name="prefix"> wrapped log prefix </param>
        /// <returns></returns>
        public PriorityLogPrefix(LogPrefixBase prefix = null) : base(prefix) { }

        /// <summary>
        /// Get priority info prefix for log
        /// </summary>
        /// <param name="context"> The context where output the log </param>
        /// <param name="log"> The log message </param>
        /// <param name="priority"> Log Priority </param>
        /// <returns> The priority info prefix in format 'Priority: {priorityInfo}' </returns>
        protected override string GetPrefix(object context, string log, LogPriority priority)
        {
            return $"Priority: {priority.ToString()}";
        }
    }
}