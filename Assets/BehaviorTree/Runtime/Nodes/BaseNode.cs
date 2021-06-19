using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// Node's state.
    /// </summary>
    public enum BehaviourState
    {
        Inactive,
        Success,
        Failure,
        Running,
        Completed
    }

    public abstract class BaseNode : Node
    {
        protected string _guid;
        public string GUID
        {
            get { return _guid; }
            set { _guid = value; }
        }

        protected BaseNode _parentNode;
        public BaseNode ParentNode
        {
            get { return _parentNode; }
            set { _parentNode = value; }
        }

        protected List<BaseNode> _childNodes;
        public List<BaseNode> ChildNodes
        {
            get { return _childNodes; }
            set { _childNodes = value; }
        }

        protected BehaviourState _state;
        public BehaviourState State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// Called once when the tree is on.
        /// </summary>
        public virtual void OnAwake()
        {
            _state = BehaviourState.Inactive;
        }

        /// <summary>
        /// Called once when this node is started.
        /// </summary>
        public virtual void OnStart()
        {
            _state = BehaviourState.Running;
        }

        /// <summary>
        /// Called every frame when running
        /// </summary>
        /// <returns>State when this method ends.</returns>
        public virtual BehaviourState OnUpdate()
        {
            return _state;
        }

        /// <summary>
        /// Called when this node is finished.
        /// </summary>
        public virtual void OnEnd()
        {
            _state = BehaviourState.Inactive;
        }
    }
}