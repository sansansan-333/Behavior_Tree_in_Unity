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
    public class BehaviorTreeGraphView : GraphView
    {
        public RootNode rootNode;

        private BehaviorNodeSearchWindow _searchWindow;

        public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

        public List<Type> nodeClasses = new List<Type>(); // list of subclasses of BaseNode(assigned when constructor called)

        public BehaviorTreeGraphView(EditorWindow editorWindow)
        {
            nodeClasses = GetSubClassOf(typeof(BaseNode));

            styleSheets.Add(Resources.Load<StyleSheet>("BehaviorTreeGraph"));

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            CreateRootNode();
            AddSearchWindow(editorWindow);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        /// <summary>
        /// Create root node.
        /// </summary>
        /// <returns></returns>
        private RootNode CreateRootNode()
        {
            var rootNode = new RootNode
            {
                title = "ROOT",
                name = nameof(RootNode),
                GUID = Guid.NewGuid().ToString()
            };

            var outputPort = GeneratePort(rootNode, Direction.Output);
            outputPort.portName = "Output";
            rootNode.outputContainer.Add(outputPort);

            rootNode.capabilities &= ~Capabilities.Movable;
            rootNode.capabilities &= ~Capabilities.Deletable;

            rootNode.RefreshExpandedState();
            rootNode.RefreshPorts();

            rootNode.SetPosition(new Rect(100, 100, defaultNodeSize.x, defaultNodeSize.y));

            this.rootNode = rootNode;
            AddElement(rootNode);

            return rootNode;
        }

        /// <summary>
        /// Set search window to this graph view.
        /// </summary>
        /// <param name="editorWindow"></param>
        private void AddSearchWindow(EditorWindow editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<BehaviorNodeSearchWindow>();
            _searchWindow.Init(editorWindow, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        /// <summary>
        /// Create Action node.
        /// </summary>
        /// <param name="position">Position where the node is put.</param>
        /// <returns></returns>
        public ActionNode CreateActionNode(Vector2 position, Act defaultAct = Act.None)
        {
            var actionNode = new ActionNode()
            {
                title = "Action",
                name = nameof(ActionNode),
                GUID = Guid.NewGuid().ToString()
            };

            var inputPort = GeneratePort(actionNode, Direction.Input);
            inputPort.name = "Input";
            actionNode.inputContainer.Add(inputPort);

            var enumField = new EnumField(defaultAct);
            enumField.RegisterValueChangedCallback((evt) =>
            {
                actionNode.act = (Act)evt.newValue;
            });
            actionNode.mainContainer.Add(enumField);

            actionNode.RefreshExpandedState();
            actionNode.RefreshPorts();
            actionNode.SetPosition(new Rect(position, defaultNodeSize));

            actionNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            AddElement(actionNode);

            return actionNode;
        }

        /// <summary>
        /// Create Conditional node.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public ConditionalNode CreateConditionalNode(Vector2 position, Condition defaultCondition = Condition.None, bool defaultValue = true)
        {
            var conditionalNode = new ConditionalNode()
            {
                title = "Conditional",
                name = nameof(ConditionalNode),
                GUID = Guid.NewGuid().ToString()
            };

            var inputPort = GeneratePort(conditionalNode, Direction.Input);
            inputPort.portName = "Input";
            conditionalNode.inputContainer.Add(inputPort);

            var enumField = new EnumField(defaultCondition);
            enumField.RegisterValueChangedCallback((evt) =>
            {
                conditionalNode.condition = (Condition)evt.newValue;
            });
            var truthValueField = new EnumField(defaultValue ? TruthValue.True : TruthValue.False) { label = "is" };
            truthValueField.RegisterValueChangedCallback((evt) =>
            {
                conditionalNode.value = (TruthValue)evt.newValue == TruthValue.True;
            });
            enumField.Insert(1, truthValueField);
            conditionalNode.mainContainer.Add(enumField);

            conditionalNode.RefreshExpandedState();
            conditionalNode.RefreshPorts();
            conditionalNode.SetPosition(new Rect(position, defaultNodeSize));

            conditionalNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            AddElement(conditionalNode);

            return conditionalNode;
        }

        /// <summary>
        /// Create Sequence node.
        /// </summary>
        /// <param name="position">Position where the node is put.</param>
        /// <returns></returns>
        public SequenceNode CreateSequenceNode(Vector2 position)
        {
            var sequenceNode = new SequenceNode()
            {
                title = "Sequence",
                name = nameof(SequenceNode),
                GUID = Guid.NewGuid().ToString()
            };

            var inputPort = GeneratePort(sequenceNode, Direction.Input);
            inputPort.portName = "Input";
            sequenceNode.inputContainer.Add(inputPort);

            var button = new Button(() => AddOutputPort(sequenceNode));
            button.text = "New Port";
            sequenceNode.titleContainer.Add(button);

            sequenceNode.RefreshExpandedState();
            sequenceNode.RefreshPorts();
            sequenceNode.SetPosition(new Rect(position, defaultNodeSize));

            sequenceNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            AddElement(sequenceNode);

            return sequenceNode;
        }

        /// <summary>
        /// Create Selector node.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public SelectorNode CreateSelectorNode(Vector2 position)
        {
            var selectorNode = new SelectorNode()
            {
                title = "Selector",
                name = nameof(SelectorNode),
                GUID = Guid.NewGuid().ToString()
            };

            var inputPort = GeneratePort(selectorNode, Direction.Input);
            inputPort.portName = "Input";
            selectorNode.inputContainer.Add(inputPort);

            var button = new Button(() => AddOutputPort(selectorNode));
            button.text = "New Port";
            selectorNode.titleContainer.Add(button);

            selectorNode.RefreshExpandedState();
            selectorNode.RefreshPorts();
            selectorNode.SetPosition(new Rect(position, defaultNodeSize));

            selectorNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            AddElement(selectorNode);

            return selectorNode;
        }

        /// <summary>
        /// Create Repeater node.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public RepeaterNode CreateRepeaterNode(Vector2 position, int defaultRepeatCount = 0)
        {
            var repeaterNode = new RepeaterNode()
            {
                title = "Repeater",
                name = nameof(RepeaterNode),
                GUID = Guid.NewGuid().ToString()
            };

            var inputPort = GeneratePort(repeaterNode, Direction.Input);
            inputPort.portName = "Input";
            repeaterNode.inputContainer.Add(inputPort);

            var outputPort = GeneratePort(repeaterNode, Direction.Output);
            outputPort.portName = "Output";
            repeaterNode.outputContainer.Add(outputPort);

            // Elements to set the number of repeat times <-
            var textField = new TextField()
            {
                label = "Repeat",
                value = defaultRepeatCount.ToString(),
            };
            textField.RegisterValueChangedCallback((evt) =>
            {
                if (int.TryParse(evt.newValue, out var num) && num >= 0)
                {
                    repeaterNode.repeatCount = num;
                }
                else
                {
                    textField.value = "0";
                    repeaterNode.repeatCount = 0; // maybe unnesesary
                }
            });
            var plusButton = new Button(() => textField.value = (int.Parse(textField.value) + 1).ToString())
            {
                text = "+"
            };
            var minusButton = new Button(() => textField.value = (int.Parse(textField.value) - 1).ToString())
            {
                text = "-"
            };
            textField.Insert(2, minusButton);
            textField.Insert(2, plusButton);
            repeaterNode.mainContainer.Add(textField);
            // ->

            repeaterNode.RefreshExpandedState();
            repeaterNode.RefreshPorts();
            repeaterNode.SetPosition(new Rect(position, defaultNodeSize));

            repeaterNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            AddElement(repeaterNode);

            return repeaterNode;
        }

        /// <summary>
        /// Create Inverter node
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public InverterNode CreateInverterNode(Vector2 position)
        {
            var inverterNode = new InverterNode
            {
                title = "Inverter",
                name = nameof(InverterNode),
                GUID = Guid.NewGuid().ToString()
            };

            var inputPort = GeneratePort(inverterNode, Direction.Input);
            inputPort.portName = "Input";
            inverterNode.inputContainer.Add(inputPort);

            var outputPort = GeneratePort(inverterNode, Direction.Output);
            outputPort.portName = "Output";
            inverterNode.outputContainer.Add(outputPort);

            inverterNode.RefreshExpandedState();
            inverterNode.RefreshPorts();
            inverterNode.SetPosition(new Rect(position, defaultNodeSize));

            inverterNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            AddElement(inverterNode);

            return inverterNode;
        }

        /// <summary>
        /// Generate a port.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portDirection"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public Port GeneratePort(BaseNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); // I won't use port type, so "float" or any type should be ok.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compositeNode"></param>
        /// <param name="overriddenPortName"></param>
        /// <remarks>Port name must be unique inside of one node.</remarks>
        public void AddOutputPort(BaseNode compositeNode, string overriddenPortName = "")
        {
            var generatedPort = GeneratePort(compositeNode, Direction.Output);

            //var oldLabel = generatedPort.contentContainer.Q<Label>("type");
            //generatedPort.contentContainer.Remove(oldLabel);

            /*  overriddenPortName is not used.
             
            int outputPortCount = compositeNode.outputContainer.Query("connector").ToList().Count;
            string outputPortName = string.IsNullOrEmpty(overriddenPortName)
                ? $"Port {outputPortCount + 1}"
                : overriddenPortName;
            generatedPort.portName = outputPortName;
            */

            var deleteButton = new Button(() => RemovePort(compositeNode, generatedPort))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);

            compositeNode.outputContainer.Add(generatedPort);
            RenamePortsWithOrder(compositeNode);
            compositeNode.RefreshExpandedState();
            compositeNode.RefreshPorts();
        }

        /// <summary>
        /// Remove output port from node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="outputPort"></param>
        public void RemovePort(BaseNode node, Port outputPort)
        {
            var targetEdge = edges.ToList().Where(x =>
                x.output.portName == outputPort.portName && x.output.node == outputPort.node);

            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(edge);
            }

            node.outputContainer.Remove(outputPort);
            RenamePortsWithOrder(node);
            node.RefreshExpandedState();
            node.RefreshPorts();
        }

        /// <summary>
        /// Rename output ports the number of order.
        /// </summary>
        /// <param name="node"></param>
        public void RenamePortsWithOrder(Node node)
        {
            if(node == null)
            {
                Debug.LogAssertion("null?");
                return;
            }

            int portCount = 1;

            foreach(var port in node.outputContainer.Query(className:"port").ToList())
            {
                (port as Port).portName = portCount.ToString();
                portCount++;
            }
        }

        /// <summary>
        /// Returns a list of subclasses
        /// </summary>
        /// <param name="t">parent type</param>
        /// <returns></returns>
        private List<Type> GetSubClassOf(Type t)
        {
            var result = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(t))
                    {
                        result.Add(type);
                    }
                }
            }

            return result;
        }
    }
}