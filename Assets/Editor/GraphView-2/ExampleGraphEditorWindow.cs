// 参考: https://light11.hatenadiary.com/entry/2020/06/16/200750

using UnityEditor;

public class ExampleGraphEditorWindow : EditorWindow
{
    [MenuItem("Window/ExampleGraphEditorWindow")]
    public static void Open()
    {
        GetWindow<ExampleGraphEditorWindow>(ObjectNames.NicifyVariableName(nameof(ExampleGraphEditorWindow)));
    }

    void OnEnable()
    {
        var graphView = new ExampleGraphView(this);
        rootVisualElement.Add(graphView);
    }
}
