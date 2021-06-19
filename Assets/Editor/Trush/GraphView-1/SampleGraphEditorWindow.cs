// https://qiita.com/ma_sh/items/7627a6151e849f5a0ede

using UnityEditor;
using UnityEngine;

public class SampleGraphEditorWindow : EditorWindow
{
    [MenuItem("Window/Open SampleGraphView #z")]
    public static void Open()
    {
        GetWindow<SampleGraphEditorWindow>("SampleGraphView");
    }

    void OnEnable()
    {
        var graphView = new SampleGraphView()
        {
            style = {flexGrow = 1}
            // style = new IStyle{ flexGrow = 1 } の略？
        };
        rootVisualElement.Add(graphView);
    }
}