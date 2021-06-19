using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTree
{
    public class TreeSaveUtility
    {
        private BehaviorTreeGraphView _targetGraphView;
        private TreeContainer _containerCache;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<BaseNode> BaseNodes => _targetGraphView.nodes.ToList().Cast<BaseNode>().ToList();

        public static TreeSaveUtility GetInstance(BehaviorTreeGraphView behaviorTreeGraphView)
        {
            return new TreeSaveUtility
            {
                _targetGraphView = behaviorTreeGraphView
            };
        }

        /// <summary>
        /// Save tree.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveTree(string fileName)
        {
            var treeContainer = ScriptableObject.CreateInstance<TreeContainer>();
            if (!SaveNodes(treeContainer)) return;

            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/BehaviorTree")) AssetDatabase.CreateFolder("Assets/Resources", "BehaviorTree");

            AssetDatabase.CreateAsset(treeContainer, $"Assets/Resources/BehaviorTree/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Save each node and edge.
        /// </summary>
        /// <param name="treeContainer"></param>
        /// <returns></returns>
        private bool SaveNodes(TreeContainer treeContainer)
        {
            if (!Edges.Any())
            {
                EditorUtility.DisplayDialog("Save failed", "Graph isn't saved since threr is no connection.", "OK");
                return false;
            }

            // remove unconnected output ports from Sequence and Selector nodes
            // then rename
            foreach(var node in BaseNodes)
            {
                if (node.name != nameof(SequenceNode) && node.name != nameof(SelectorNode)) continue;

                foreach (var port in node.outputContainer.Query(className:"port").ToList().Cast<Port>())
                {
                    if(!port.connected) node.outputContainer.Remove(port);
                }

                _targetGraphView.RenamePortsWithOrder(node);
                node.RefreshExpandedState();
                node.RefreshPorts();
            }

            // save container.LinkData
            var connectedEdges = Edges.Where(edge => edge.input.node != null).ToArray();
            for (int i = 0; i < connectedEdges.Length; i++)
            {
                var outputNode = connectedEdges[i].output.node as BaseNode;
                var inputNode = connectedEdges[i].input.node as BaseNode;

                treeContainer.LinkDatas.Add(new LinkData
                {
                    SourceNodeGuid = outputNode.GUID,
                    PortName = connectedEdges[i].output.portName,
                    TargetNodeGuid = inputNode.GUID
                });
            }

            // save container.NodeData
            foreach (var baseNode in BaseNodes)
            {
                var tmpNodeData = new NodeData
                {
                    GUID = baseNode.GUID,
                    nodeType = baseNode.name,
                    position = baseNode.GetPosition().position
                };
                switch (baseNode.name)
                {
                    case nameof(RootNode):
                        break;
                    case nameof(ActionNode):
                        tmpNodeData.actionNodeData = new NodeData.ActionNodeData
                        {
                            act = baseNode.mainContainer.Q<EnumField>(className:"unity-enum-field").value.ToString()
                        };
                        break;
                    case nameof(ConditionalNode):
                        tmpNodeData.conditionalNodeData = new NodeData.ConditionalNodeData
                        {
                            condition = baseNode.mainContainer.Query<EnumField>(className: "unity-enum-field").First().value.ToString(),
                            value = (TruthValue)baseNode.mainContainer.Query<EnumField>(className: "unity-enum-field").ToList()[1].value == TruthValue.True
                                        ? true : false
                        };
                        break;
                    case nameof(SequenceNode):
                        break;
                    case nameof(SelectorNode):
                        break;
                    case nameof(RepeaterNode):
                        tmpNodeData.repeaterNodeData = new NodeData.RepeaterNodeData
                        {
                            repeatCount = int.Parse(baseNode.mainContainer.Q<TextField>(className:"unity-text-field").value)
                        };
                        break;
                    case nameof(InverterNode):
                        break;
                    default:
                        Debug.LogError("Something is wrong.");
                        break;
                }

                treeContainer.NodeDatas.Add(tmpNodeData);
            }

            return true;
        }

        /// <summary>
        /// Load graph.
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadTree(string fileName)
        {
            _containerCache = Resources.Load<TreeContainer>($"BehaviorTree/{fileName}");
            if (_containerCache == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target tree graph file does not exist!", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
            SetNodePosition();
        }

        /// <summary>
        /// Clean up all nodes and edges except for root node.
        /// </summary>
        private void ClearGraph()
        {
            BaseNodes.Find(node => node.name == nameof(RootNode)).GUID = _containerCache.NodeDatas.Find(data => data.nodeType == nameof(RootNode)).GUID;

            foreach(var node in BaseNodes.Where(node => node.name != nameof(RootNode)))
            {
                Edges.Where(edge => edge.input.node == node).ToList()
                    .ForEach(edge => _targetGraphView.RemoveElement(edge));

                _targetGraphView.RemoveElement(node);
            }
        }

        /// <summary>
        /// Create edges from cashe.
        /// </summary>
        private void CreateNodes()
        {
            Node createdNode = null;
            Vector2 position = Vector2.zero;
            List<LinkData> nodePorts;
            foreach (var nodeData in _containerCache.NodeDatas)
            {
                switch (nodeData.nodeType.ToString())
                {
                    case nameof(RootNode):
                        break;
                    case nameof(ActionNode):
                        var selectedAct = Enum.GetValues(typeof(Act)).Cast<Act>().ToList()
                            .Where(act => act.ToString() == nodeData.actionNodeData.act).First();
                        createdNode = _targetGraphView.CreateActionNode(position, selectedAct);
                        break;
                    case nameof(ConditionalNode):
                        var selectedCondition = Enum.GetValues(typeof(Condition)).Cast<Condition>().ToList()
                            .Where(cond => cond.ToString() == nodeData.conditionalNodeData.condition).First();
                        var selectedValue = nodeData.conditionalNodeData.value;
                        createdNode = _targetGraphView.CreateConditionalNode(position, selectedCondition, selectedValue);
                        break;
                    case nameof(SequenceNode):
                        createdNode = _targetGraphView.CreateSequenceNode(position);
                        nodePorts = _containerCache.LinkDatas.Where(data => data.SourceNodeGuid == nodeData.GUID).ToList();
                        nodePorts.ForEach(data => _targetGraphView.AddOutputPort((BaseNode)createdNode, data.PortName));
                        break;
                    case nameof(SelectorNode):
                        createdNode = _targetGraphView.CreateSelectorNode(position);
                        nodePorts = _containerCache.LinkDatas.Where(data => data.SourceNodeGuid == nodeData.GUID).ToList();
                        nodePorts.ForEach(data => _targetGraphView.AddOutputPort((BaseNode)createdNode, data.PortName));
                        break;
                    case nameof(RepeaterNode):
                        createdNode = _targetGraphView.CreateRepeaterNode(position, nodeData.repeaterNodeData.repeatCount);
                        break;
                    case nameof(InverterNode):
                        createdNode = _targetGraphView.CreateInverterNode(position);
                        break;
                    default:
                        Debug.LogError("Something is wrong.");
                        break;
                }

                if(createdNode != null) (createdNode as BaseNode).GUID = nodeData.GUID;
            }
        }

        /// <summary>
        /// Connect edges and nodes based on cashe.
        /// </summary>
        private void ConnectNodes()
        {
            for (int i = 0; i < BaseNodes.Count; i++)
            {
                var connections = _containerCache.LinkDatas.Where(data => data.SourceNodeGuid == BaseNodes[i].GUID).ToList();
                var sourcePorts = BaseNodes[i].outputContainer.Query(className: "port").ToList().Cast<Port>();
                for (int j = 0; j < connections.Count; j++)
                {
                    var targetNode = BaseNodes.First(node => node.GUID == connections[j].TargetNodeGuid);

                    LinkNodes(sourcePorts.First(port => port.portName == connections[j].PortName), targetNode.inputContainer.Q<Port>());
                    /*
                    targetNode.SetPosition(new Rect
                    (
                        _containerCache.NodeDatas.First(data => data.GUID == targetNode.GUID).position,
                        _targetGraphView.defaultNodeSize
                    ));
                    */
                }
            }
        }

        /// <summary>
        /// Put node at a position from cashe.
        /// </summary>
        private void SetNodePosition()
        {
            foreach (var node in BaseNodes)
            {
                node.SetPosition(new Rect
                (
                    _containerCache.NodeDatas.First(data => data.GUID == node.GUID).position,
                    _targetGraphView.defaultNodeSize
                ));
            }
        }

        /// <summary>
        /// Create and connect an edge between two ports.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input"></param>
        private void LinkNodes(Port output, Port input)
        {
            var tmpEdge = new Edge
            {
                output = output,
                input = input
            };

            tmpEdge.output.Connect(tmpEdge);
            tmpEdge.input.Connect(tmpEdge);
            _targetGraphView.Add(tmpEdge);
        }
    }
}