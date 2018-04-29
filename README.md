# STBehaviorTree
A simple behavior tree implementation with an editor made for Unity in C#. The editor stuff is pretty hacky though, so you'll have to forgive me.

## IMPORTANT!
Be careful when making changes to BehaviorTree.cs. When it gets compiled, it will wipe all the behavior trees clean and leave them quite bare.

## FAQ
***Where is the editor window for the behavior tree?***
  
  You can find it under Window/Behavior Tree/Behavior Tree Editor. (Make sure to have a behavior tree asset selected too!)

## Optimizations
* Create an assembly definition file for the Behavior Tree code. Then, in BehaviorTreeNodeAttribute.cs : Line 37, change the string constant 'BEHAVIOR_TREE_NODE_ASSEMBLY_NAME' to match whatever you have named the assembly definition file.
