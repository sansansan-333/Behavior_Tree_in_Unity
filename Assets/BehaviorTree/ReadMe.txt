I referred to the pages below.

https://www.slideshare.net/torisoup/unity-behavior-treeai
https://edom18.hateblo.jp/entry/2017/08/28/104547

<Usage>
0. Put "BehaviorTree" folder anywhere you like.
1. Create Act-function pairs and Condition-judgeFunction pairs. Act and Condition is defined in ActFuncPair.cs and CondFuncPair.cs respectively.
    These two enums are used just to display on the behavior tree window. You can add members of enums.
    Then you need to create functions correspond to enums(see ActFuncPair.cs and CondFuncPair.cs). 
2. Let's construct a tree on behavior tree window(shortcut: shift+B). Right click to create a node, connect them and change paramiters of nodes.
3. If you have done, set the name of tree data(default "New Narrative") and click "Save Tree". Saved data will be in "Assets/Resources/BehaviorTree" folder.
4. It's time to execute the behavior tree. Create TreeExecuter instance by using "AddComponent()", not "new" keyword(because it uses coroutines).
    You should "AddComponent()" to the gameobject you want to apply your behavior tree.
    After that, call "Init()" of the instance with appropreate arguments you may have created.
5. Call "Execute()" of the TreeExecuter in Update() or Start()(or Awake()). Your behavior tree will work there.

There is an example code in BehaviorTreeTest.cs. Please see it.

<Notice>
-Changing enum names such as "Act.OutputLog -> Act.LogOut" may cause problems when a tree is loaded on editor window
 because saved tree data uses the string of enums. If you want to do, please change the name of enum member before you save the tree.
-IEnumeratorを返さなきゃいけない理由はコルーチンにするため。n秒待つとかその関数内でいらないんだったらその関数の最後でyield return nullすればとりあえずok。

<Remarks(for my own)>
-Node.name is used to represent its type of node. For example, if Node.name == "RootNode", then this node is a RootNode.

-Behavior tree data you save will be put on "Assets/Resources/BehaviorTree". You can change this location by altering TreeSaveUtility/SaveTree().
