//===============================================================
// ファイル名	：NodeBaseEditor.cs
// 作成者	：村上一真
// 作成日	：20190516
// StateMonobehaviorのノードをビジュアル化する
//===============================================================
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeBaseEditor : EditorWindow
{
	private GameObject gameObject = null;//ステート管理オブジェクト
	private GameObject nowGameObject = null;//判定用

	private List<Node> nodes;   //現在管理している描画ノード,ステートの分存在
	private List<NomalState> states = new List<NomalState>();//現在管理しているステート

	private StateMonobehavior[] stateMonobehaviors;//管理しているobjectのクラス
	private string[] stateMonoOption = { };//表示するビヘイビア文字列
	private int nowStateMonoNuber = 0;
	private int _beforeMonoNumber = 0;

	private List<NodeConnection> connections;//描画する接続線

	private int nowId = 0;//次のノードウィンドウのID
    public static bool optionAllView;//全て表示するか否か
	public static bool initFlag = false;//初期化用のフラグ

	private Vector2 offset;//
	private Vector2 drag;//キャンバスのドラッグ
    private EventType beforeEvent;

	//選択中（ドラッグ中）のハンドル
	private NodeConnectionPoint selectedInPoint;
	private NodeConnectionPoint selectedOutPoint;

	//ノードの左右矩形ハンドルのスタイル
	private GUIStyle inPointStyle;
	private GUIStyle outPointStyle;

    public static Node copyNode = null;

	[MenuItem("SuperCU/NodeEditor")]
	private static void Open()
	{
		NodeBaseEditor window = GetWindow<NodeBaseEditor>();
		window.autoRepaintOnSceneChange = true;//シーンが変わるたびに自動で再描画
		window.titleContent = new GUIContent("のーどえでたー");
	}
	//オブジェクトがロードされた時のコールバック
	private void OnEnable()
	{
		initFlag = true;
		//接続ボタンハンドルスタイルの設定
		inPointStyle = new GUIStyle();
		inPointStyle.normal.background = EditorGUIUtility.Load("btn left.png") as Texture2D;
		inPointStyle.active.background = EditorGUIUtility.Load("btn left on.png") as Texture2D;
		inPointStyle.border = new RectOffset(4, 4, 12, 12);
		outPointStyle = new GUIStyle();
		outPointStyle.normal.background = EditorGUIUtility.Load("btn right.png") as Texture2D;
		outPointStyle.active.background = EditorGUIUtility.Load("btn right on.png") as Texture2D;
		outPointStyle.border = new RectOffset(4, 4, 12, 12);
	}
	//描画処理
	private void OnGUI()
	{
		DrawGrid(20, 0.2f, Color.gray);//グリッド描画
		DrawGrid(100, 0.4f, Color.gray);//4マスごとに濃く描画
		DrawConnections();//ノード接続線描画
		DrawConnectionLine(Event.current);//選択した接続点からマウスの位置までベジェの描画
		BeginWindows();
        DrawNodes();//ノード描画
        EndWindows();
        GameObjField();
        if (Event.current.type == EventType.Repaint)
        {
            InitState();
        }
		//同じクラスがあった場合の基底クラス選択
		if (gameObject != null)
		{
			nowStateMonoNuber = EditorGUILayout.Popup(nowStateMonoNuber, stateMonoOption);
		}
        optionAllView = EditorGUILayout.Toggle("AllView", optionAllView);//全て表示させるか否か
		//変わったら更新
		if (nowStateMonoNuber != _beforeMonoNumber)
		{
			initFlag = true;
			_beforeMonoNumber = nowStateMonoNuber;
		}

										  //イベント処理
		ProcessNodeEvents(Event.current);
		ProcessEvents(Event.current);
		//OnGUIを呼び出す（なんらかのコントロールの入力データが変更された場合）
		if (GUI.changed) Repaint();
	}
	//ステート管理オブジェクトのフィールド
	private void GameObjField()
	{
		gameObject = EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true) as GameObject;
		//ゲームオブジェクトが変わったときに初期化
		if (gameObject != null)
		{
			if (nowGameObject != gameObject)
			{
				initFlag = true;
				nowGameObject = gameObject;
                nowStateMonoNuber = 0;
			}
		}
		//ゲームオブジェクトが入っていなければすべて消す
		else
		{
			if (nodes != null)
			{
				nodes.Clear();
				nodes = null;
			}
			if (connections != null)
			{
				connections.Clear();
				connections = null;
			}
			initFlag = false;
			nowGameObject = null;
		}
	}
	/// <summary>
	/// 初期化処理
	/// </summary>
	private void InitState()
	{
		EditorApplication.playModeStateChanged += OnChangedPlayMode;//プレイモードの変更時の処理
																	//フラグがtrueなら起動
		if (initFlag == false)
		{
			return;
		}


		nowId = 0;//windowIDを初期化
				  //ノードがあればノードを一度初期化する
		if (nodes != null)
		{
			nodes.Clear();
			nodes = null;
		}
		if (connections != null)
		{
			connections.Clear();
			connections = null;
		}
		//クラスを格納してそのクラスのステートを現在管理するステートに
		var cla = gameObject.GetComponent<StateMonobehavior>();
		Array.Resize(ref stateMonoOption, 0);
		stateMonobehaviors = gameObject.GetComponents<StateMonobehavior>();
		for (int i = 0; i < stateMonobehaviors.Length; i++)
		{
			Array.Resize(ref stateMonoOption, stateMonoOption.Length + 1);
			stateMonoOption[i] = stateMonobehaviors[i].stateName;
		}
		states = stateMonobehaviors[nowStateMonoNuber].states;
		if (nodes == null)
		{
			nodes = new List<Node>();
		}
		//ステートの分ノードを追加
		foreach (NomalState st in states)
		{
			//ノードの追加
			nodes.Add(new Node(new Rect(st.nodePosition.x, st.nodePosition.y, 250, 150), inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, st.getStateName(), nowId, st, gameObject , stateMonobehaviors[nowStateMonoNuber]));
			nowId++;
		}
		//nextStateの線を描く
		if (nodes.Count > 0)
		{
			foreach (Node n in nodes)
			{
				if (n.outPoints.Count <= 0)
				{
					continue;
				}
				foreach (Node.StateOutPoint point in n.outPoints)
				{
					selectedOutPoint = point.outPoint;
					foreach (Node a in nodes)
					{
						if (point.stateJudge.nextID.number == 0)
						{
							break;
						}

						if (a.myState.ID.number == point.stateJudge.nextID.number)
						{
							selectedInPoint = a.inPoint;
							break;
						}
					}
					if (selectedOutPoint != null && selectedInPoint != null)
					{
						if (selectedOutPoint.node != selectedInPoint.node)
						{
							CreateConnection();
						}
					}
					ClearConnectionSelection();
				}
			}
		}
		//IDを振り分け
		int countOne = 1;
		foreach (NomalState nomal in states)
		{
			nomal.ID.number = countOne;
			countOne++;
		}
		initFlag = false;
	}
	//グリッド描画
	private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
	{
		int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
		int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

		Handles.BeginGUI();
		Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

		offset += drag * 0.5f;
		Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

		for (int i = 0; i < widthDivs; i++)
		{
			Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
		}

		for (int j = 0; j < heightDivs; j++)
		{
			Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
		}

		Handles.color = Color.white;
		Handles.EndGUI();
	}
	//ノード描画
	private void DrawNodes()
	{
		if (nodes != null)
		{
			foreach (Node node in nodes)
			{
				node.Draw();
			}
		}
	}
	//ノード接続線描画
	private void DrawConnections()
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Count; i++)
			{
				connections[i].Draw();
			}
		}
	}
	//選択した接続点からマウスの位置までベジェの描画
	private void DrawConnectionLine(Event e)
	{
		//どちらかが入力されていれば登録
		if (selectedInPoint != null && selectedOutPoint == null)
		{
			Handles.DrawBezier(
				selectedInPoint.rect.center,
				e.mousePosition,
				selectedInPoint.rect.center + Vector2.left * 50f,
				e.mousePosition - Vector2.left * 50f,
				Color.white,
				null,
				2f
			);

			GUI.changed = true;
		}

		if (selectedOutPoint != null && selectedInPoint == null)
		{
			Handles.DrawBezier(
				selectedOutPoint.rect.center,
				e.mousePosition,
				selectedOutPoint.rect.center - Vector2.left * 50f,
				e.mousePosition + Vector2.left * 50f,
				Color.white,
				null,
				2f
			);

			GUI.changed = true;
		}
	}
	//何かイベントが起こった時の挙動
	private void ProcessEvents(Event e)
	{
		drag = Vector2.zero;

		switch (e.type)
		{
			//マウスが押されたとき
			case EventType.MouseDown:
				//右クリックメニューの表示
				if (e.button == 1)
				{
					ProcessContextMenu(e.mousePosition);
                    beforeEvent = EventType.MouseDown;
                    Event.current.Use();
				}
				break;
			case EventType.MouseDrag:
				if (e.button == 2)
				{
                    if (beforeEvent != EventType.MouseDown)
                    {
                        OnDrag(e.delta);
                    }
                    beforeEvent = EventType.MouseDrag;
				}
				break;
		}
	}
	//何かイベントが起こった時の挙動（ノード内のもの）
	private void ProcessNodeEvents(Event e)
	{
		if (nodes != null)
		{
			//最後のノードが一番上に描画されるように逆順
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				bool guiChanged = nodes[i].ProcessEvents(e);

				if (guiChanged)
				{
					GUI.changed = true;
				}
			}
		}
	}
	//キャンバス全体のドラッグ処理（ノード全て）
	private void OnDrag(Vector2 delta)
	{
		drag = delta;

		if (nodes != null)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].Drag(delta);
			}
		}

		GUI.changed = true;
	}
	//右クリックメニューの作成
	private void ProcessContextMenu(Vector2 mousePosition)
	{
		GenericMenu genericMenu = new GenericMenu();
		//右クリックメニュー、ノード追加
		genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Paste(バグったらHierarchyのこのGameObject押して)"), false, () => OnClickPasteNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Paste All"), false, () => OnClickPasteNodeAll(mousePosition));
		//メニュー表示
		genericMenu.ShowAsContext();
	}
	//ノードの追加
	private void OnClickAddNode(Vector2 mousePosition)
	{
		//ノード、ステート追加
		NomalState statePlus = new NomalState();
		states.Add(statePlus);

		if (nodes == null)
		{
			nodes = new List<Node>();
		}
		//マウスの場所に追加
		nodes.Add(new Node(new Rect( mousePosition.x,mousePosition.y, 200, 50),inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, " ", nowId, statePlus, gameObject,stateMonobehaviors[nowStateMonoNuber]));
		nowId++;
	}
    //コピーしたステートのペースト
    private void OnClickPasteNode(Vector2 mousePosition)
    {
        if(copyNode==null)
        {
            return;
        }
        NomalState statePlus = new NomalState();
        statePlus.execDelegate = new List<StateBase.ActionDelegate>(copyNode.myState.execDelegate);
        statePlus.stateName = copyNode.myState.stateName+"(Clone)";
        statePlus.nextStateJudge = new List<NextStateJudge>(copyNode.myState.nextStateJudge);
        for(int i = 0;i< statePlus.nextStateJudge.Count;i++)
        {
            statePlus.nextStateJudge[i].nextID = null;
            statePlus.nextStateJudge[i].nextState = null;
            statePlus.nextStateJudge[i].nextStateName = null;
        }
        statePlus.playUpdateDelegate = new List<StateBase.ActionDelegate>(copyNode.myState.playUpdateDelegate);
        states.Add(statePlus);
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        //マウスの場所に追加
        nodes.Add(new Node(new Rect(mousePosition.x, mousePosition.y, 200, 50), inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, " ", nowId, statePlus, gameObject, stateMonobehaviors[nowStateMonoNuber]));
        nowId++;
        initFlag = true;
    }
    //コピーしたステートのペースト(ネクストステート含め)
    private void OnClickPasteNodeAll(Vector2 mousePosition)
    {
        if (copyNode == null)
        {
            return;
        }
        NomalState statePlus = new NomalState();
        statePlus.execDelegate = new List<StateBase.ActionDelegate>(copyNode.myState.execDelegate);
        statePlus.stateName = copyNode.myState.stateName+"(Clone)";
        statePlus.nextStateJudge = new List<NextStateJudge>();
        for (int i = 0; i < copyNode.myState.nextStateJudge.Count; i++)
        {
            NextStateJudge next = new NextStateJudge();
            next.nextID = new InstanceID();
            next.nextID.number = copyNode.myState.nextStateJudge[i].nextID.number;
            next.nextState = copyNode.myState.nextStateJudge[i].nextState;
            next.nextStateName = copyNode.myState.nextStateJudge[i].nextStateName;
            next.priority = copyNode.myState.nextStateJudge[i].priority;
            next.comment = copyNode.myState.nextStateJudge[i].comment;
            next.judgeFunc = copyNode.myState.nextStateJudge[i].judgeFunc;
            next.judgeFuncName = copyNode.myState.nextStateJudge[i].judgeFuncName;
            next.judgeFuncInstance = copyNode.myState.nextStateJudge[i].judgeFuncInstance;
            statePlus.nextStateJudge.Add(next);
        }
        statePlus.playUpdateDelegate = new List<StateBase.ActionDelegate>(copyNode.myState.playUpdateDelegate);
        states.Add(statePlus);
        if (nodes == null)
        {
            nodes = new List<Node>();
        }
        //マウスの場所に追加
        nodes.Add(new Node(new Rect(mousePosition.x, mousePosition.y, 200, 50), inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, " ", nowId, statePlus, gameObject, stateMonobehaviors[nowStateMonoNuber]));
        nowId++;
        initFlag = true;
    }
    //入る点が押されたとき登録
    private void OnClickInPoint(NodeConnectionPoint inPoint)
	{
		selectedInPoint = inPoint;

		if (selectedOutPoint != null)
		{
			//現在選択中の始点終点ノードがあればノード線を作成
			if (selectedOutPoint.node != selectedInPoint.node)
			{
				CreateConnection();
				ClearConnectionSelection();
			}
			else
			{
				ClearConnectionSelection();
			}
		}
        else
        {
            selectedInPoint = null;
        }
	}
	//出る点が押されたとき登録
	private void OnClickOutPoint(NodeConnectionPoint outPoint)
	{
        if(outPoint.node.ID != Node.nodeActiveID&&optionAllView==false)
        {
            return;
        }
        if(selectedOutPoint == outPoint)
        {
            ClearConnectionSelection();
            return;
        }
        selectedOutPoint = outPoint;

		if (selectedInPoint != null)
		{
			//現在選択中の始点終点ノードがあればノード線を作成
			if (selectedOutPoint.node != selectedInPoint.node)
			{
				CreateConnection();
				ClearConnectionSelection();
			}
			else
			{
				ClearConnectionSelection();
			}
		}
	}
	//真ん中の矩形
	private void OnClickRemoveConnection(NodeConnection connection)
	{
        if (selectedOutPoint == null && selectedInPoint == null)
        {
            connection.outPoint.myNextStateJudge.nextState = null;
            connection.outPoint.myNextStateJudge.nextStateName = null;
            connection.outPoint.myNextStateJudge.nextID = null;
            connections.Remove(connection);
        }
	}
	//ノード線の作成
	private void CreateConnection()
	{
		if (connections == null)
		{
			connections = new List<NodeConnection>();
		}
		//元ある線を削除
		List<NodeConnection> removeConnection = new List<NodeConnection>();
		foreach (NodeConnection node in connections)
		{
			if (node.outPoint == selectedOutPoint)
			{
				removeConnection.Add(node);
			}
		}
		if (removeConnection.Count > 0)
		{
			foreach (NodeConnection node in removeConnection)
			{
				connections.Remove(node);
			}
		}
		//OutPointのステートに名前を登録する
		//IDに変えたい
		selectedOutPoint.myNextStateJudge.nextState = selectedInPoint.node.myState;//一応
		selectedOutPoint.myNextStateJudge.nextStateName = selectedInPoint.node.myState.getStateName();
		selectedOutPoint.myNextStateJudge.nextID = selectedInPoint.node.myState.ID;
		connections.Add(new NodeConnection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
	}
	//選択中のノード矩形のリセット
	private void ClearConnectionSelection()
	{
		selectedInPoint = null;
		selectedOutPoint = null;
	}
	private void OnClickRemoveNode(Node node)
	{
		if (connections != null)
		{
			List<NodeConnection> connectionsToRemove = new List<NodeConnection>();
			//他ノードへの接続線の削除
			for (int i = 0; i < connections.Count; i++)
			{
				for (int j = 0; j < node.outPoints.Count; j++)
				{
					if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoints[j].outPoint)
					{
						connectionsToRemove.Add(connections[i]);
					}
				}
			}
			for (int i = 0; i < connectionsToRemove.Count; i++)
			{
				connections.Remove(connectionsToRemove[i]);
			}

			connectionsToRemove = null;
		}
		states.Remove(node.myState);
		//ノードの削除
		nodes.Remove(node);
	}
	//プレイモードが変更された時の処理
    void OnChangedPlayMode(PlayModeStateChange isNowPlayng)
    {
		if(isNowPlayng == PlayModeStateChange.EnteredPlayMode)
		{
			initFlag = true;
		}
        if (isNowPlayng == PlayModeStateChange.ExitingPlayMode)
        {
			initFlag = true;
        }
		if (isNowPlayng == PlayModeStateChange.EnteredEditMode)
		{
			initFlag = true;
		}
    }
}
