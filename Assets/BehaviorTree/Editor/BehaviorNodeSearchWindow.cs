using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTree
{
    public class BehaviorNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow _window;
        private BehaviorTreeGraphView _behaviorTreeGraphView;
        private Texture2D _indentationIcon;

        public void Init(EditorWindow window, BehaviorTreeGraphView behaviourTreeGraphView)
        {
            _window = window;
            _behaviorTreeGraphView = behaviourTreeGraphView;

            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),

            new SearchTreeGroupEntry(new GUIContent("Leaf Node"), 1),
            new SearchTreeEntry(new GUIContent(nameof(ActionNode), _indentationIcon))
            {
                level = 2, userData = new ActionNode()
            },
            new SearchTreeEntry(new GUIContent(nameof(ConditionalNode), _indentationIcon))
            {
                level = 2, userData = new ConditionalNode()
            },

            new SearchTreeGroupEntry(new GUIContent("Composite Node"), 1),
            new SearchTreeEntry(new GUIContent(nameof(SequenceNode), _indentationIcon))
            {
                level = 2, userData = new SequenceNode()
            },
            new SearchTreeEntry(new GUIContent(nameof(SelectorNode), _indentationIcon))
            {
                level = 2, userData = new SelectorNode()
            },
            new SearchTreeEntry(new GUIContent(nameof(RepeaterNode), _indentationIcon))
            {
                level = 2, userData = new RepeaterNode()
            },
            new SearchTreeEntry(new GUIContent(nameof(InverterNode), _indentationIcon))
            {
                level = 2, userData = new InverterNode()
            },
        };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            var localMousePosition = _behaviorTreeGraphView.contentViewContainer.WorldToLocal(worldMousePosition);

            switch (searchTreeEntry.userData)
            {
                case ActionNode actionNode:
                    _behaviorTreeGraphView.CreateActionNode(localMousePosition);
                    return true;
                case ConditionalNode conditionalNode:
                    _behaviorTreeGraphView.CreateConditionalNode(localMousePosition);
                    return true;
                case SequenceNode sequenceNode:
                    _behaviorTreeGraphView.CreateSequenceNode(localMousePosition);
                    return true;
                case SelectorNode selectorNode:
                    _behaviorTreeGraphView.CreateSelectorNode(localMousePosition);
                    return true;
                case RepeaterNode repeaterNode:
                    _behaviorTreeGraphView.CreateRepeaterNode(localMousePosition);
                    return true;
                case InverterNode inverterNode:
                    _behaviorTreeGraphView.CreateInverterNode(localMousePosition);
                    return true;
                default:
                    return false;
            }
        }
    }
}