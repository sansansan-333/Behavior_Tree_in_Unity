using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using Random = UnityEngine.Random;
using System;

public class BehaviorTreeTest : MonoBehaviour
{
    [SerializeField]
    private TreeContainer _treeContainer;
    private BehaviorTreeExecutor _treeExecutor;
    private List<ActFuncPair> _actFuncPairs;
    private List<CondFuncPair> _condFuncPairs;

    private Renderer renderer;

    void Start()
    {
        if(_treeContainer != null)
        {
            _actFuncPairs = new List<ActFuncPair>
            {
                new ActFuncPair(Act.OutputLog, ()=>OutputLog()),
                new ActFuncPair(Act.Wait, ()=>Wait()),
                new ActFuncPair(Act.MoveLeft, ()=>MoveRight()),
                new ActFuncPair(Act.MoveRight, ()=>MoveLeft()),
            };
            _condFuncPairs = new List<CondFuncPair>
            {
                new CondFuncPair(Condition.FiftyFifty, ()=>FiftyFifty()),
                new CondFuncPair(Condition.IsInRange, ()=>IsInRange(new Rect(new Vector2(0, 0), new Vector2(1, 1))))
            };

            _treeExecutor = gameObject.AddComponent<BehaviorTreeExecutor>();
            _treeExecutor.Init(_treeContainer, _actFuncPairs, _condFuncPairs);
        }

        renderer = gameObject.GetComponent<Renderer>();
    }

    void Update()
    {
        if (IsInRange(new Rect(new Vector2(0, 0), new Vector2(1, 1))))
        {
            renderer.material.color = Color.red;
        }
        else
        {
            renderer.material.color = Color.blue;
        }
        _treeExecutor.Execute();
    }

    private IEnumerator OutputLog()
    {
        Debug.Log("Output log");
        yield return null;
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("Wait");
        yield return new WaitForSeconds(1);
    }

    private IEnumerator MoveLeft()
    {
        gameObject.transform.position += new Vector3(-0.1f, 0);
        yield return null;
    }

    private IEnumerator MoveRight()
    {
        gameObject.transform.position += new Vector3(0.1f, 0);
        yield return null;
    }

    private bool FiftyFifty()
    {
        return Random.Range(0, 2) == 0;
    }

    private bool IsInRange(Rect range)
    {
        Vector2 pos = gameObject.transform.position;
        if (range.position.x <= pos.x && pos.x <= range.position.x+range.width
            && range.position.y-range.height <= pos.y && pos.y <= range.position.y)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }
}
