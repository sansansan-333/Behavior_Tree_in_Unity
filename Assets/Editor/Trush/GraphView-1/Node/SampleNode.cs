using UnityEditor.Experimental.GraphView;

// 自作ノードの基底クラス
public abstract class SampleNode : Node
{

}

/*
public class SampleNode : Node
{
    public SampleNode()
    {
        title = "Node Title";

        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Port));
        inputContainer.Add(inputPort);

        var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
        outputContainer.Add(outputPort);
    }
}
*/