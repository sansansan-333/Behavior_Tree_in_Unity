using System;

/// <summary>
/// Holding connection data between two nodes.
/// </summary>
[Serializable]
public class NodeLinkData
{
    public string BaseNodeGuid;
    public string PortName;
    public string TargetNodeGuid;
}
