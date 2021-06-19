// https://qiita.com/shirasaya0201/items/ee32f35ad3caac428368

using UnityEngine;
using UnityEditor;
using System.IO;

public class EditorWindowSample : EditorWindow
{
    /// <summary>
    /// ScriptableObjectSampleの変数
    /// </summary>
    private ScriptableObjectSample _sample;
    /// <summary>
    /// アセットパス
    /// </summary>
    private const string ASSET_PATH = "Assets/Resourses/ScriptableObjectSample.asset";

    [MenuItem("Editor/Sample")]
    private static void Create()
    {
        // 生成
        GetWindow<EditorWindowSample>("aaaaaa");
        // ウインドウの最小サイズ
        GetWindow<EditorWindowSample>().minSize = new Vector2(320, 320);
    }

    private void OnGUI()
    {
        if (_sample == null)
        {
            // 読み込み
            Import();
        }

        /*
        using (new GUILayout.HorizontalScope()) // 横に並べる
        {
            _sample.SampleIntValue = EditorGUILayout.IntField("sample Horizontal", _sample.SampleIntValue);
            _sample.SampleIntValue = EditorGUILayout.IntField("sample Horizontal", _sample.SampleIntValue);
            _sample.SampleIntValue = EditorGUILayout.IntField("sample Horizontal", _sample.SampleIntValue);
        }

        using (new GUILayout.VerticalScope()) // 縦に並べる
        {
            _sample.SampleIntValue = EditorGUILayout.IntField("sample Vertical", _sample.SampleIntValue);
            _sample.SampleIntValue = EditorGUILayout.IntField("sample Vertical", _sample.SampleIntValue);
            _sample.SampleIntValue = EditorGUILayout.IntField("sample Vertical", _sample.SampleIntValue);
        }
        */

        Color defaultColor = GUI.backgroundColor;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Settings");
            }
            GUI.backgroundColor = defaultColor;

            _sample.SampleIntValue = EditorGUILayout.IntField("sample Int", _sample.SampleIntValue);
        }
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("ファイル操作");
            }
            GUI.backgroundColor = defaultColor;

            GUILayout.Label("パス:" + ASSET_PATH);

            using (new GUILayout.HorizontalScope(GUI.skin.box))
            {
                // 読み込みボタン
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("読み込み"))
                {
                    Import();
                    Debug.Log("<color=green>Import!</color>");
                    Debug.Log("<size=32>Import!</size>");
                    Debug.Log("<color=#00ff00><size=32>Import!</size></color>");
                }
                // 書き込みボタン
                GUI.backgroundColor = Color.magenta;
                if (GUILayout.Button("書き込み"))
                {
                    Export();
                }
                GUI.backgroundColor = defaultColor;
            }
        }
    }

    private void Import()
    {
        if (_sample == null)
        {
            _sample = ScriptableObjectSample.CreateInstance<ScriptableObjectSample>();
        }

        ScriptableObjectSample sample = AssetDatabase.LoadAssetAtPath<ScriptableObjectSample>(ASSET_PATH);
        if (sample == null) return;

        EditorUtility.CopySerialized(sample, _sample);
    }

    private void Export()
    {
        // 読み込み
        ScriptableObjectSample sample = AssetDatabase.LoadAssetAtPath<ScriptableObjectSample>(ASSET_PATH);
        if (sample == null)
        {
            sample = ScriptableObjectSample.CreateInstance<ScriptableObjectSample>();
        }

        // AssetDatabaseに_sampleが登録されていなければ
        if (!AssetDatabase.Contains(sample as UnityEngine.Object))
        {
            string directory = Path.GetDirectoryName(ASSET_PATH);
            // ディレクトリがなければ作成
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // アセット作成
            AssetDatabase.CreateAsset(sample, ASSET_PATH);
        }

        // コピー
        EditorUtility.CopySerialized(_sample, sample);

        // インスペクターから設定できないようにする
        sample.hideFlags = HideFlags.NotEditable;
        // 更新通知
        EditorUtility.SetDirty(sample);
        // 保存
        AssetDatabase.SaveAssets();
        // エディタを最新の状態にする
        AssetDatabase.Refresh();
    }
}

