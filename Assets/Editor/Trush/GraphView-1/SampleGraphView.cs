using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

public class SampleGraphView : GraphView
{
    public RootNode root;

    public SampleGraphView() : base()
    {
        // ズームできるようにする
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        // 背景を挿入
        Insert(0, new GridBackground());

        // rootNodeをデフォルトで追加
        root = new RootNode();
        AddElement(root);

        // Nodeをドラッグできるようにする
        this.AddManipulator(new SelectionDragger());

        var sampleSearchWindowProvider = ScriptableObject.CreateInstance<SampleSearchWindowProvider>();
        sampleSearchWindowProvider.Initialize(this);

        // コンストラクタの時点でCreationRequestをした時に呼ばれるイベントを登録しとく
        nodeCreationRequest += context =>
        {
            //AddElement(new SampleNode());
            if (SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), sampleSearchWindowProvider))
            {
                Debug.Log("Window Opened");
            }
            else
            {
                Debug.Log("Failed to open window");
            }
        };
    }

    /// <summary>
    /// Get all ports compatible with given port.
    /// </summary>
    /// <param name="startAnchor">Start port to validate against.</param>
    /// <param name="nodeAdapter">Node adapter.</param>
    /// <returns>List of compatible ports.</returns>
    public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
    {
        return ports.ToList();
    }
}
