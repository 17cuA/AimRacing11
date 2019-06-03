//===============================================================
// ファイル名	：Node.cs
// 作成者	：村上一真
// 作成日	：20190516
// 描画するノードのクラス
//===============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public partial class Node
{
	//現行ステート側からのポイント
	public class StateOutPoint
	{
		public NodeConnectionPoint outPoint;
		public NextStateJudge stateJudge;
		public int nowStringNumber;
	}
	public class FuncList
	{
		public StateBase.ActionDelegate actionDelegate;
		public int nowStringNumber;
	}
	public List<FuncList> updateFuncs = new List<FuncList>();
    public List<FuncList> startFuncs = new List<FuncList>();
	public class NowMethods
	{
		public MethodInfo nowMethod;
		public Component compornent;
	}
	public static int nodeActiveID;//現在アクティブになっているウィンドウのID
	public Rect rect; //ノードの位置
	public string title;//ノードの名前
	public NomalState myState;//このノードの管理するステート
	public GameObject gameObject;//基底のゲームオブジェクト
	public int ID;//自分のＩＤ

	public StateMonobehavior stateMonobehavior;//このステートのある基底クラス

	public NodeConnectionPoint inPoint;//ノードの入ってくるポイント
	public List<StateOutPoint> outPoints = new List<StateOutPoint>();//出るポイント

	public Action<Node> OnRemoveNode;//ノード削除処理

	public bool isDragged;//ドラッグ中かどうか
	public bool isSelect;//選択中かどうか

	public Color color = Color.white;

	/// <summary>
	/// 初期化
	/// </summary>
	public Node(
		Rect recPos,
		GUIStyle inPointStyle,
		GUIStyle outPointStyle,
		Action<NodeConnectionPoint> OnClickInPoint,
		Action<NodeConnectionPoint> OnClickOutPoint,
		Action<Node> OnClickRemoveNode,
		string title,
		int id,
		NomalState nomalS,
		GameObject game,
		StateMonobehavior monobehavior
		)
	{
		rect = recPos;//ウィンドウの大きさ設定
		inPoint = new NodeConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);//入力ポイント
		OnRemoveNode = OnClickRemoveNode;
		this.title = title;
		ID = id;
		myState = nomalS;
		gameObject = game;
		stateMonobehavior = monobehavior;
		//出力ポイント
		for (int i = 0; i < myState.nextStateJudge.Count; i++)
		{
			StateOutPoint rectOut = new StateOutPoint();
			rectOut.stateJudge = myState.nextStateJudge[i];
			rectOut.outPoint = new NodeConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint, myState.nextStateJudge[i]);
			outPoints.Add(rectOut);
		}
		//アップデート中の処理
		for (int i = 0; i < myState.playUpdateDelegate.Count; i++)
		{
			FuncList list = new FuncList();
			list.actionDelegate = myState.playUpdateDelegate[i];
			updateFuncs.Add(list);
		}
		//ステートになった時の処理 
		for (int i = 0; i < myState.execDelegate.Count; i++)
		{
			FuncList list = new FuncList();
			list.actionDelegate = myState.execDelegate[i];
			startFuncs.Add(list);
		}
	}
	public void Draw()
	{
		DrawPoints();
		GUI.backgroundColor = color;
        if (ID == 0)
        {
            rect = GUI.Window(ID, rect,  WindowFunc, "START::" + title);//windowベースで描画
        }
        else
        {
            rect = GUI.Window(ID, rect, WindowFunc, title);//windowベースで描画
        }
		GUI.backgroundColor = Color.white;
		myState.nodePosition = rect.position;//ポジションのセーブ
	}
	/// <summary>
	/// ドラッグ処理
	/// </summary>
	public void Drag(Vector2 vec2)
	{
		rect.position += vec2;
	}
	/// <summary>
	/// 左右のポイントの描画処理
	/// </summary>
	private void DrawPoints()
	{
		inPoint.Draw();
        if ((nodeActiveID == ID||NodeBaseEditor.optionAllView)&&myState.toggleWindow)
        {
            //右のポイント群の描画
            float ypos = -((rect.height / 2)) + 53f;
            ypos += updateFuncs.Count * 62;
            ypos += startFuncs.Count * 62;
            ypos += 57;//ラベル分
            for (int i = 0; i < outPoints.Count; i++)
            {
                outPoints[i].outPoint.Draw(new Rect(rect.position + new Vector2(0, ypos), rect.size));
                ypos += 62;
            }
        }
        else
        {
            float ypos = 0;
            for (int i = 0; i < outPoints.Count; i++)
            {
                outPoints[i].outPoint.Draw(new Rect(rect.position + new Vector2(0, ypos), rect.size));
                ypos += 0;
            }
        }
    }
	/// <summary>
	/// 何かイベントがあった時
	/// </summary>
	public bool ProcessEvents(Event e)
	{
		switch (e.type)
		{
			//マウスが押されたとき
			case EventType.MouseDown:
				//左クリック
				if (e.button == 0)
				{
					//ノード内にマウスがあるかどうか
					if (rect.Contains(e.mousePosition))
					{
						isDragged = true;
						GUI.changed = true;
						isSelect = true;
					}
					else
					{
						GUI.changed = true;
						isSelect = false;
					}
				}
				//選択中で右クリック
				if (e.button == 1 && isSelect && rect.Contains(e.mousePosition))
				{
					//ノード削除のコンテキストメニュー追加
					ProcessContextMenu();
					e.Use();
				}

				break;

			//マウスを離した時
			case EventType.MouseUp:
				//ドラッグ中止
				isDragged = false;
				break;

			//ドラッグ中
			case EventType.MouseDrag:
				//左クリック
				if (e.button == 0 && isDragged)
				{
					//ドラッグ処理
					Drag(e.delta);
					//他のイベント処理を抑制
					e.Use();
					return true;
				}
				break;
		}
		return false;
	}
	/// <summary>
	/// ノード削除のコンテキストメニュー
	/// </summary>
	private void ProcessContextMenu()
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
		genericMenu.AddItem(new GUIContent("条件式追加"), false, OnClickPlusUpdateJudge);
		genericMenu.AddItem(new GUIContent("常時実行処理追加"), false, OnClickPlusUpdate);
		genericMenu.AddItem(new GUIContent("移行時処理追加"), false, OnClickPlusStart);
        genericMenu.AddItem(new GUIContent("コピー"), false, OnClickCopy);
		genericMenu.ShowAsContext();
	}
	/// <summary>
	/// ノード削除
	/// </summary>
	private void OnClickRemoveNode()
	{
		if (OnRemoveNode != null)
		{
            NodeBaseEditor.initFlag = true;
            OnRemoveNode(this);
		}
	}
    private void OnClickPlusUpdateJudge()
    {
        myState.nextStateJudge.Add(new NextStateJudge());
        NodeBaseEditor.initFlag = true;

    }
	private void OnClickPlusUpdate()
	{
		myState.playUpdateDelegate.Add(new StateBase.ActionDelegate());
		NodeBaseEditor.initFlag = true;
	}
	private void OnClickPlusStart()
	{
		myState.execDelegate.Add(new StateBase.ActionDelegate());
		NodeBaseEditor.initFlag = true;
	}
    private void OnClickCopy()
    {
        NodeBaseEditor.copyNode = this;
    }
}
