using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STBehaviorTree;

public class BehaviorTreeMonoBehavior : MonoBehaviour
{
    [SerializeField] BehaviorTree _tree;
    [SerializeField] Blackboard _blackboard;

	void Update ()
    {
        _tree.TreeUpdate(_blackboard);	
	}
}