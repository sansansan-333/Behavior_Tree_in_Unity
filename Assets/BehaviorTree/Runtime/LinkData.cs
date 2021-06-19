namespace BehaviorTree
{
    /// <summary>
    /// Holding connection data between two nodes.
    /// </summary>
    [System.Serializable]
    public class LinkData
    {
        public string SourceNodeGuid;
        public string PortName;
        public string TargetNodeGuid;
    }
}