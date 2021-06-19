using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviorTree
{

    public class BehaviorTreeExecutor : MonoBehaviour
    {
        private TreeContainer _treeContainer;
        private RootNode _root;

        private List<ActFuncPair> _actFuncPairs = new List<ActFuncPair>();
        private List<CondFuncPair> _condFuncPairs = new List<CondFuncPair>();

        private List<ActFunction> _actFunctions = new List<ActFunction>();
        private bool _exeFinished = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="treeContainer"></param>
        public BehaviorTreeExecutor()
        {

        }

        /// <summary>
        /// Initialize excecutor.
        /// </summary>
        /// <param name="actFuncPairs"></param>
        /// <param name="condFuncPairs"></param>
        public void Init(TreeContainer treeContainer, List<ActFuncPair> actFuncPairs, List<CondFuncPair> condFuncPairs)
        {
            _treeContainer = treeContainer;
            _root = new RootNode
            {
                name = nameof(RootNode),
                GUID = _treeContainer.NodeDatas.First(data => data.nodeType == nameof(RootNode)).GUID
            };

            _actFuncPairs = actFuncPairs;
            _condFuncPairs = condFuncPairs;

            ConstructTree(_root);

            WakeUp(_root);
        }

        /// <summary>
        /// Recursively set a parent node and child nodes to the nodes on data.
        /// </summary>
        /// <param name="node"></param>
        private void ConstructTree(BaseNode node)
        {
            var connections = _treeContainer.LinkDatas.Where(data => data.SourceNodeGuid == node.GUID).ToList();
            node.ChildNodes = new List<BaseNode>();

            LinkData connection = null;
            string nodeType = _treeContainer.NodeDatas.First(data => data.GUID == node.GUID).nodeType;

            
            for (int i = 0; i < connections.Count; i++)
            {

                if(nodeType == nameof(SelectorNode) || nodeType == nameof(SequenceNode)) // if node is composite
                {
                    connection = connections.First(con => con.PortName == (i + 1).ToString()); // to make sure to choose a correct port
                }
                else
                {
                    connection = connections.First();
                }
                var childNodeData = _treeContainer.NodeDatas.First(data => data.GUID == connection.TargetNodeGuid);
                if (childNodeData == null) continue;

                BaseNode childNode = null;

                switch (childNodeData.nodeType)
                {
                    case nameof(ActionNode):
                        childNode = new ActionNode
                        {
                            name = nameof(ActionNode),
                            act = Enum.GetValues(typeof(Act)).Cast<Act>().ToList()
                                .First(act => act.ToString() == childNodeData.actionNodeData.act),
                            targetExecutor = this,
                        };
                        if ((childNode as ActionNode).act != Act.None)
                        {
                            (childNode as ActionNode).action = _actFuncPairs.First(pair => pair.act == (childNode as ActionNode).act).action;
                        }
                        break;
                    case nameof(ConditionalNode):
                        childNode = new ConditionalNode
                        {
                            name = nameof(ConditionalNode),
                            condition = Enum.GetValues(typeof(Condition)).Cast<Condition>().ToList()
                                .First(cond => cond.ToString() == childNodeData.conditionalNodeData.condition),
                            value = childNodeData.conditionalNodeData.value
                        };
                        if ((childNode as ConditionalNode).condition != Condition.None
                            && (childNode as ConditionalNode).condition != Condition.True
                            && (childNode as ConditionalNode).condition != Condition.False)
                        {
                            (childNode as ConditionalNode).judgeFunc = _condFuncPairs.First(pair => pair.condition == (childNode as ConditionalNode).condition).judgeFunc;
                        }
                        break;
                    case nameof(SelectorNode):
                        childNode = new SelectorNode
                        {
                            name = nameof(SelectorNode),
                        };
                        break;
                    case nameof(SequenceNode):
                        childNode = new SequenceNode
                        {
                            name = nameof(SequenceNode),
                        };
                        break;
                    case nameof(RepeaterNode):
                        childNode = new RepeaterNode
                        {
                            name = nameof(RepeaterNode),
                            repeatCount = childNodeData.repeaterNodeData.repeatCount
                        };
                        break;
                    case nameof(InverterNode):
                        childNode = new InverterNode
                        {
                            name = nameof(InverterNode),
                        };
                        break;
                    default:
                        break;
                }
                if (childNode != null)
                {
                    childNode.GUID = childNodeData.GUID;
                    node.ChildNodes.Add(childNode);
                    childNode.ParentNode = node;
                }
            }

            foreach (var childNode in node.ChildNodes)
            {
                ConstructTree(childNode);
            }
        }

        /// <summary>
        /// Make all nodes awake.
        /// </summary>
        /// <param name="node"></param>
        private void WakeUp(BaseNode node)
        {
            node.OnAwake();
            foreach (var child in node.ChildNodes)
            {
                WakeUp(child);
            }
        }

        /// <summary>
        /// Make all nodes inactive.
        /// </summary>
        /// <param name="node"></param>
        private void End(BaseNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                child.OnEnd();
            }
            node.OnEnd();
        }

        /// <summary>
        /// Execute behavior tree.
        /// </summary>
        public void Execute()
        {
            if (_exeFinished)
            {
                _root.OnUpdate(); // set functions in _actFunctions
                StartCoroutine(ExecuteCoroutine()); // execute coroutines in _actFunctions
                End(_root);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteCoroutine()
        {
            _exeFinished = false;

            foreach (var func in _actFunctions)
            {
                yield return StartCoroutine(func());
            }

            _exeFinished = true;
        }

        /// <summary>
        /// Setter for actFunctions. Only implementation use.
        /// </summary>
        /// <param name="actFunction"></param>
        public void SetActFunction(ActFunction actFunction)
        {
            _actFunctions.Add(actFunction);
        }

        private void TreeWalk(BaseNode node)
        {
            Debug.Log(node.name);
            foreach (var child in node.ChildNodes)
            {
                TreeWalk(child);
            }
        }

        public void Test()
        {
            Debug.Log("---TreeWalk---");
            TreeWalk(_root);
        }
    }

}