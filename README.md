# BehaviorTree
A simple behavior tree implementation with an editor made for Unity in C#.

## FAQ
* Where is the editor window for the behavior tree?
You can find it under Window/Behavior Tree/Behavior Tree Editor.

## Optimizations
* Create an assembly definition file for the Behavior Tree code. Then, in BehaviorTreeNodeAttribute.cs : Line 37, change the string constant 'BEHAVIOR_TREE_NODE_ASSEMBLY_NAME' to match whatever you have named the assembly definition file.
