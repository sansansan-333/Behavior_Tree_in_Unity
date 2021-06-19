using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _filename = "New Narrative";

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueDraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    
    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMiniMap();
        GenerateBlackBoard();
    }

    private void GenerateBlackBoard()
    {
        var blackBoard = new Blackboard(_graphView);
        blackBoard.Add(new BlackboardSection { title = "Exposed Properties"});
        blackBoard.addItemRequested = _blackBoard => { _graphView.AddPropertyToBlackBoard(new ExposedProperty()); };
        blackBoard.editTextRequested = (blackBoard1, element, newValue) => // プロパティの名前変更時の処理
        {
            var oldPropertyName = ((BlackboardField)element).text;
            // 名前が被ったとき
            if(_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exsists, please choose another one.", "OK");
                return;
            }
            // 名前がかぶらなかったとき（この処理はいらないようにも見えるけど、コールバックを何かしら設定すると値の変更が反映されない）
            var propertyIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
            _graphView.ExposedProperties[propertyIndex].PropertyName = newValue;
            ((BlackboardField)element).text = newValue; 
        };
        blackBoard.SetPosition(new Rect(10, 30, 200, 300));

        _graphView.Add(blackBoard);
        _graphView.Blackboard = blackBoard;
    }

    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name");
        fileNameTextField.SetValueWithoutNotify(_filename);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback((evt) => _filename = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });
        toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Data" });
        toolbar.Add(new Button(() => _graphView.OutputAllDialogues(_graphView.entryPoint)) { text = "Output"}); // My code

        rootVisualElement.Add(toolbar);
    }

    private void GenerateMiniMap()
    {
        var miniMap = new MiniMap { anchored = true };
        // This will give 10 px offset from left side
        var cords = _graphView.contentContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
        miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
        _graphView.Add(miniMap);
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(_filename))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (save)
        {
            saveUtility.SaveGraph(_filename);
        }
        else
        {
            saveUtility.LoadGraph(_filename);
        }
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }



}
