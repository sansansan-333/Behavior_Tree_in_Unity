using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCashe;

    private List<Edge> Edges => _targetGraphView.edges.ToList(); // この書き方で、いつでも最新のエッジのリストをこの変数で取得できる
    private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    ///<remarks>このクラスのインスタンスを一つしか生成しないように、コンストラクタではなくGetInstance()を使う？</remarks> 
    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        if (!SaveNodes(dialogueContainer)) return;
        SaveExposedProperies(dialogueContainer);

        // Auto creates resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");

        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Saves Nodes data.
    /// </summary>
    /// <param name="dialogueContainer"></param>
    /// <returns>If saving process is going wrong, then return false.</returns>
    private bool SaveNodes(DialogueContainer dialogueContainer)
    {
        if (!Edges.Any()) return false; // if there is no edges(no connection) then return

        // save container.NodeLinks
        var connectedPorts = Edges.Where(edge => edge.input.node != null).ToArray(); // only save edges which has node in input side
        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.NodeLinks.Add(new NodeLinkData
            {
                BaseNodeGuid = outputNode.GUID,
                PortName = connectedPorts[i].output.portName, // get output port name because input ports will always have the same name
                TargetNodeGuid = inputNode.GUID
            });
        }

        // save container.DialogueNodeData
        foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
        {
            dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
            {
                Guid = dialogueNode.GUID,
                DialogueText = dialogueNode.DialogueText,
                Position = dialogueNode.GetPosition().position
            });
        }

        return true;
    }

    private void SaveExposedProperies(DialogueContainer dialogueContainer)
    {
        dialogueContainer.ExposedProperties.AddRange(_targetGraphView.ExposedProperties);
    }

    public void LoadGraph(string fileName)
    {
        _containerCashe = Resources.Load<DialogueContainer>(fileName);
        if (_containerCashe == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exist!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateExposedProperties();
    }

    private void ClearGraph()
    {
        // 今あるEntryPointはGUIDだけ置き換えて使う（書き換えるのは、ウインドウを開き直すたびにEntryPointのGUIDが変わるから？）
        Nodes.Find(node => node.EntryPoint).GUID = _containerCashe.NodeLinks[0].BaseNodeGuid;

        foreach (var node in Nodes)
        {
            if (node.EntryPoint) continue;

            // Remove edges connected to this node(このループのノードのinputポートにつながっているedgeをremove)
            Edges.Where(edge => edge.input.node == node).ToList()
                .ForEach(edge => _targetGraphView.RemoveElement(edge));

            // Then remove the node
            _targetGraphView.RemoveElement(node);
        }
    }

    private void CreateNodes()
    {
        foreach (var nodeData in _containerCashe.DialogueNodeData)
        {
            // We pass position later on, so we can just use vec2 zero for now as position while loading nodes.
             var tempNode = _targetGraphView.CreateDialogueNode(nodeData.DialogueText, Vector2.zero);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCashe.NodeLinks.Where(data => data.BaseNodeGuid == nodeData.Guid).ToList(); // Find link data where base node is this node
            nodePorts.ForEach(data => _targetGraphView.AddChoicePort(tempNode, data.PortName));
        }
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < Nodes.Count; i++) // エッジが始まる側のノード
        {
            var connections = _containerCashe.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
            for (int j = 0; j < connections.Count; j++) // エッジが始まる側のノードのoutputポート
            {
                var targetNodeGuid = connections[j].TargetNodeGuid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect
                (
                    _containerCashe.DialogueNodeData.First(x => x.Guid == targetNodeGuid).Position,
                    _targetGraphView.defaultNodeSize
                ));
            }
        }
    }

    /// <summary>
    /// Create and connect an edge between two ports.
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }

    private void CreateExposedProperties()
    {
        // Clear existing properties
        _targetGraphView.ClearBlackBoardAndExposedProperties();
        // Add properties from data
        foreach (var exposedProperty in _containerCashe.ExposedProperties)
        {
            _targetGraphView.AddPropertyToBlackBoard(exposedProperty);
        }
    }
}
