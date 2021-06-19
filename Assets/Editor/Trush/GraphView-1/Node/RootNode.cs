using UnityEditor.Experimental.GraphView;

public class RootNode : SampleNode
{
    public RootNode() : base()
    {
        title = "RootNodeTitle";

        // このGraphElementへ可能な操作の集合から削除可能を引く
        capabilities -= Capabilities.Deletable;

        var outputNode = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
        outputNode.name = "Out";
        outputContainer.Add(outputNode);
    }
}
