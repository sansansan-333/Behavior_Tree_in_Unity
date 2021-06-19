using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class SampleSearchWindowProvider : ScriptableObject, ISearchWindowProvider
{
    private SampleGraphView sampleGraphView;

    public void Initialize(SampleGraphView sampleGraphView)
    {
        this.sampleGraphView = sampleGraphView;
    }

    List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
    {
        Debug.Log("ISearchWindowProvider.CreateSearchTree");
        var entries = new List<SearchTreeEntry>();
        entries.Add(new SearchTreeEntry(new GUIContent("Create Node")));

        // ２つのforeachで、定義されている全ての型を１つ１つtypeに入れて回している
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && (type.IsSubclassOf(typeof(SampleNode)))
                    && type != typeof(RootNode))
                {
                    entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
                }
            }
        }

        Debug.Log("Entries in list: " + entries.Count);
        Debug.Log("ISearchWindowProvider.CreateSearchTree ends");
        return entries;
    }

    bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        // ???
        Debug.Log("ISearchWindowProvider.OnSelectEntry");
        var type = SearchTreeEntry.userData as System.Type;
        var node = Activator.CreateInstance(type) as SampleNode;

        sampleGraphView.AddElement(node);
        return true;
    }
}
