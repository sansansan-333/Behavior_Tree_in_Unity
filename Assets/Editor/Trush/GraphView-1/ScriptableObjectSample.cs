// https://qiita.com/shirasaya0201/items/ee32f35ad3caac428368

using System;
using UnityEngine;

[Serializable]
public class ScriptableObjectSample : ScriptableObject
{
    [SerializeField]
    private int _sampleIntValue;

    public int SampleIntValue
    {
        get { return _sampleIntValue; }
#if UNITY_EDITOR
        set { _sampleIntValue = Mathf.Clamp(value, 0, int.MaxValue); }
    }
#endif
}
