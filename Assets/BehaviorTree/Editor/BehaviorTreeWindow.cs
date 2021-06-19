using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static BehaviorTree.BehaviorTreeExecutor;

namespace BehaviorTree
{
    public class BehaviorTreeWindow : EditorWindow
    {
        private BehaviorTreeGraphView _graphView;

        private string _saveFileName = "New Narrative";

        [MenuItem("Editor/Behavior Tree #B")]
        public static void OpenWindow()
        {
            var window = GetWindow<BehaviorTreeWindow>();
            window.titleContent = new GUIContent("Behavior Tree");
            window.minSize = new Vector2(600, 500);
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            GenerateMiniMap();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        /// <summary>
        /// Add GraphView to this window.
        /// </summary>
        public void ConstructGraphView()
        {
            _graphView = new BehaviorTreeGraphView(this) { name = "Bihavior Tree Graph" };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        /// <summary>
        /// Generates toolbar on this window.
        /// </summary>
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var fileNameTextField = new TextField("File Name");
            fileNameTextField.SetValueWithoutNotify(_saveFileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback((evt) => _saveFileName = evt.newValue);
            toolbar.Add(fileNameTextField);

            toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Tree" });
            toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Tree" });
            //toolbar.Add(new Button(() => TestFunc()) { text = "Test" });

            rootVisualElement.Add(toolbar);
        }

        /// <summary>
        /// Generates minimap on graph view.
        /// </summary>
        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap() { anchored = false };
            var cords = _graphView.contentContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
            miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            _graphView.Add(miniMap);
        }

        /// <summary>
        /// Save or Load tree data with TreeSaveUtility.
        /// </summary>
        /// <param name="save"></param>
        private void RequestDataOperation(bool save)
        {
            if (string.IsNullOrEmpty(_saveFileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
                return;
            }

            var saveUtility = TreeSaveUtility.GetInstance(_graphView);
            if (save)
            {
                saveUtility.SaveTree(_saveFileName);
            }
            else
            {
                saveUtility.LoadTree(_saveFileName);
            }
        }

        private void TestFunc()
        {
            /*
            var nodes = _graphView.nodes.ToList().Cast<Node>().ToList();
            var targetNode = nodes.Where(node => node.name == nameof(SequenceNode)).Cast<SequenceNode>().First();
            Debug.Log(actionNode.mainContainer.Query<EnumField>(className: "unity-enum-field").ToList()[1].value);
            _graphView.RenamePortsWithOrder(targetNode);
            */

            /*
            BehaviorTreeExecutor bte = new BehaviorTreeExecutor(Resources.Load<TreeContainer>("BehaviorTree/New Narrative"));
            List<ActFuncPair> actFuncPairs = new List<ActFuncPair>
            {

            };
            List<CondFuncPair> condFuncPairs = new List<CondFuncPair>
            {

            };
            bte.Init(actFuncPairs, condFuncPairs);
            bte.Test();
            */
        }
    }
}