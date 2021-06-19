using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public DialogueNode entryPoint;

    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

    public Blackboard Blackboard;
    public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
    private NodeSearchWindow _searchWindow;

    public DialogueGraphView(EditorWindow editorWindow)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph")); // スタイルシートを読み込む(罫線を設定してるuss)
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0 ,grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryPoint());
        AddSearchWindow(editorWindow);
    }

    private void AddSearchWindow(EditorWindow editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach( (port) =>
        {
            if (startPort != port && startPort.node != port.node)
                compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    /// <summary>
    /// Portを生成する
    /// </summary>
    /// <param name="node"></param>
    /// <param name="portDirection"></param>
    /// <param name="capacity"></param>
    /// <returns>生成したPort</returns>
    private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    /// <summary>
    /// EntryPointのNodeを生成する
    /// </summary>
    /// <returns>生成したNode</returns>
    private DialogueNode GenerateEntryPoint()
    {
        var node = new DialogueNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = "ENTRYPOINT",
            EntryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        // make the entryPoint unmovable and undeletable
        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        // containerに何かした時にはリフレッシュする
        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));

        entryPoint = node;

        return node;
    }

    /// <summary>
    /// DialogueNodeを追加する
    /// </summary>
    /// <param name="nodeName"></param>
    public void CreateNode(string nodeName, Vector2 position)
    {
        AddElement(CreateDialogueNode(nodeName, position));
    }

    /// <summary>
    /// DialogueNodeを生成する
    /// </summary>
    /// <param name="nodeName"></param>
    /// <returns>生成したNode</returns>
    internal DialogueNode CreateDialogueNode(string nodeName, Vector2 position)
    {
        var dialogueNode = new DialogueNode
        {
            title = nodeName,
            DialogueText = nodeName,
            GUID = Guid.NewGuid().ToString(),
        };

        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";

        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        var button = new Button( () => AddChoicePort(dialogueNode) );
        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);

        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueText = evt.newValue;
            dialogueNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField);

        dialogueNode.inputContainer.Add(inputPort);
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position, defaultNodeSize));

        return dialogueNode;
    }

    public void AddChoicePort(DialogueNode dialogueNode, string overriddenPortName = "")
    {
        var generatedPort = GeneratePort(dialogueNode, Direction.Output);

        // デフォルトでついてくるいらないlabelを削除する
        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);

        var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;

        var choicePortName = string.IsNullOrEmpty(overriddenPortName)
            ? $"Choice {outputPortCount + 1}"
            : overriddenPortName;

        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        textField.MarkDirtyRepaint();
        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);
        var deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort))
        {
            text = "X"
        };
        generatedPort.contentContainer.Add(deleteButton);

        generatedPort.portName = choicePortName;
        dialogueNode.outputContainer.Add(generatedPort);
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
    }

    /// <summary>
    /// Delete a port.
    /// </summary>
    /// <param name="dialogueNode"></param>
    /// <param name="generatedPort"></param>
    private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
    { 
        var targetEdge = edges.ToList().Where(x =>
            x.output.portName == generatedPort.portName && x.output.node == generatedPort.node); // 同じ名前のポートかつ同じノードについているポート

        if (targetEdge.Any())
        {
            // edgeを削除
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

        // ポートを削除
        dialogueNode.outputContainer.Remove(generatedPort);

        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    public void AddPropertyToBlackBoard(ExposedProperty exposedProperty)
    {
        var localPropertyName = exposedProperty.PropertyName;
        var localPropertyValue = exposedProperty.PropertyValue;
        while (ExposedProperties.Any(x => x.PropertyName == localPropertyName))
            localPropertyName = $"{localPropertyName}(1)"; // USERNAME(1) || USERNAME(1)(1) etc... // プロパティの名前の重複避け


        var property = new ExposedProperty();
        property.PropertyName = localPropertyName;
        property.PropertyValue = localPropertyValue;
        ExposedProperties.Add(property);

        var container = new VisualElement();
        var blackBoardField = new BlackboardField { text = property.PropertyName, typeText = "string property" };
        container.Add(blackBoardField);

        var propertyValueTextField = new TextField("Value")
        {
            value = localPropertyValue
        };
        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = ExposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);
            ExposedProperties[changingPropertyIndex].PropertyValue = evt.newValue;
        });
        var blackBoardValueRow = new BlackboardRow(blackBoardField, propertyValueTextField);
        container.Add(blackBoardValueRow);

        Blackboard.Add(container);
    }

    public void ClearBlackBoardAndExposedProperties()
    {
        ExposedProperties.Clear();
        Blackboard.Clear();
    }

    
    ///<remarks>My code</remarks>
    ///<summary>深さ優先でノードの名前を出力する</summary>
    public void OutputAllDialogues(DialogueNode dialogueNode)
    {
        Debug.Log(dialogueNode.DialogueText);
        foreach (var node in GetChildren(dialogueNode))
        {
            OutputAllDialogues(node);
        }
    }

    /// <summary>
    /// 全ての子ノードを接続順に取得する
    /// </summary>
    /// <param name="dialogueNode">親ノード</param>
    /// <returns></returns>
    /// <remarks>My code</remarks>
    private List<DialogueNode> GetChildren(DialogueNode dialogueNode)
    {
        var childNodes = new List<DialogueNode>();
        var outputPorts = new List<Port>();
        var edgesFrom = new List<Edge>();

        for (int i = 0; i < dialogueNode.outputContainer.Query("connector").ToList().Count; i++)
        {
            outputPorts.Add(dialogueNode.outputContainer[i].Query<Port>());
        }
        foreach (var port in outputPorts)
        {
            edgesFrom.AddRange(port.connections);
        }
        foreach (var edge in edgesFrom)
        {
            childNodes.Add((DialogueNode)edge.input.node);
        }
        
        return childNodes;
    }
}
