using UnityEditor.Experimental.GraphView;

public class LogNode : ProcessNode
{
    public LogNode() : base()
    {
        title = "LogNodeTitle";

        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(string));
        inputContainer.Add(inputPort);
    }
}