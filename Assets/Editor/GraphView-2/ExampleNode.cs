using UnityEditor.Experimental.GraphView;

public class ExampleNode : Node
{
    public ExampleNode()
    {
        title = "Title_ExampleNode";

        // 入力用のポート
        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float)); // 第三引数をPort.Capacity.Multipleにすると複数のポートへの接続が可能になる
        inputPort.portName = "Name_Input";
        inputContainer.Add(inputPort); // 入力用ポートはinputContainerに追加する

        var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        outputPort.portName = "Name_Output";
        outputContainer.Add(outputPort); // 出力ようポートはoutputContainerに追加する
    }
}
