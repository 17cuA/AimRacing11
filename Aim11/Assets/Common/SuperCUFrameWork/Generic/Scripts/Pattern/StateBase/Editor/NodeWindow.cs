//===============================================================
// ファイル名	：NodeWindow.cs
// 作成者	：村上一真
// 作成日	：20190516
// 描画するノード内のウィンドウ
// Enum型は対応してません(エラー)
//===============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;


public partial class Node
{
	//変更に使う保存用変数
	string nowStateName = "";

	//コンポーネント、メソッド取得に使う
	private Component[] compornents; //取得するコンポーネント
	private Component[] compornentsBool; //取得するコンポーネント
	private List<NowMethods> nowMethods;
	private List<NowMethods> nowMethodsBool; //現在のメソッド集
	private string[] nowOptions; //現在のメソッド名集
	private string[] nowOptionsBool;

    private bool IDChangeFlag = false;


	private void WindowFunc(int id)
	{
        EditorGUILayout.BeginHorizontal();
        //このノードをアクティブにする
        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
		{
            IDChangeFlag = true;
		}
        if(Event.current.type == EventType.Layout && IDChangeFlag)
        {
            nodeActiveID = id;
            IDChangeFlag = false;
        }
        myState.toggleWindow = EditorGUILayout.Toggle(myState.toggleWindow);//このノードをポップアップして表示するか否か
        NodeColorChange();
        if ((nodeActiveID != ID && NodeBaseEditor.optionAllView == false  )|| myState.toggleWindow == false)
        {
            rect.height = 40;
            EditorGUILayout.EndHorizontal();
            return;
        }
        nowStateName = myState.stateName;
		//ステートの名前を変更する
		myState.stateName = EditorGUILayout.TextField(myState.stateName, GUILayout.Width(rect.width - 25));
        EditorGUILayout.EndHorizontal();
		//名前が変更されたら更新
		if(nowStateName!=myState.stateName)
		{
			NodeBaseEditor.initFlag = true;
			nowStateName = myState.stateName;
		}
		//メソッド取得
		GetDrawMethod(ref compornents, typeof(void), ref nowMethods, ref nowOptions);
		GetDrawMethod(ref compornentsBool, typeof(bool), ref nowMethodsBool, ref nowOptionsBool);

		//規定の高さ
		rect.height = 45+57;

		EditorGUILayout.LabelField("移行時処理");
		PopupNonReturnStartMethod();
		EditorGUILayout.LabelField("常時処理");
		PopupNonReturnUpdateMethod();
		EditorGUILayout.LabelField("条件処理");
		PopupBoolMethod();

	}
	private void PopupNonReturnStartMethod()
	{
		for (int i = 0; i < startFuncs.Count; i++)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);
			EditorGUILayout.BeginHorizontal();
			//オーバーフローしてたら0に
			if (startFuncs[i].nowStringNumber >= nowMethods.Count)
			{
				startFuncs[i].nowStringNumber = 0;
			}
			//メソッドの同期
			for (int j = 0; j < nowMethods.Count; j++)
			{
				if (nowMethods[j].nowMethod.Name == startFuncs[i].actionDelegate.actionName && nowMethods[j].compornent == startFuncs[i].actionDelegate.instance)
				{
					startFuncs[i].nowStringNumber = j;
				}
			}
			//ポップアップでメソッド取得
			startFuncs[i].nowStringNumber = EditorGUILayout.Popup(startFuncs[i].nowStringNumber, nowOptions, GUILayout.Width(rect.width - 83));
            if(GUILayout.Button("⇧", GUILayout.Height(15), GUILayout.Width(18)))
            {
                if (i > 0)
                {
                    StateBase.ActionDelegate actionDel = startFuncs[i].actionDelegate;
                    myState.execDelegate[i] = myState.execDelegate[i-1];
                    myState.execDelegate[i - 1] = actionDel;
                    NodeBaseEditor.initFlag = true;
                }
            }
            if (GUILayout.Button("⇩", GUILayout.Height(15), GUILayout.Width(18)))
            {
                if (i < myState.execDelegate.Count-1)
                {
                    StateBase.ActionDelegate actionDel = startFuncs[i].actionDelegate;
                    myState.execDelegate[i] = myState.execDelegate[i + 1];
                    myState.execDelegate[i + 1] = actionDel;
                    NodeBaseEditor.initFlag = true;
                }
            }
            if (GUILayout.Button("×", GUILayout.Height(15), GUILayout.Width(19)))
			{
				myState.execDelegate.Remove(startFuncs[i].actionDelegate);
				NodeBaseEditor.initFlag = true;
			}
			//現在のメソッドがあれば
			if (nowMethods.Count > 0)
			{
				startFuncs[i].actionDelegate.instance = nowMethods[startFuncs[i].nowStringNumber].compornent;
				startFuncs[i].actionDelegate.actionName = nowMethods[startFuncs[i].nowStringNumber].nowMethod.Name;
				//GetCompornentは同じクラスがあった場合にも対応
				//startFuncs[i].actionDelegate.action = (Action)nowMethods[startFuncs[i].nowStringNumber].nowMethod.CreateDelegate(typeof(Action), nowMethods[startFuncs[i].nowStringNumber].compornent);
			}

			EditorGUILayout.EndHorizontal();
            if (startFuncs.Count > i)
            {
				ParameterInfo[] parameters = { };
				if (nowMethods.Count > 0)
				{
					//引数があればそれに応じたフィールドの生成
					parameters = nowMethods[startFuncs[i].nowStringNumber].nowMethod.GetParameters();
				}
				//引数1
				if (parameters.Length == 1)
				{
					EditorGUILayout.BeginHorizontal();
                    SetArgument(ref startFuncs[i].actionDelegate.arg1, ref startFuncs[i].actionDelegate.argType1, parameters, 0, 180);
					EditorGUILayout.EndHorizontal();
					//引数が一つであることを渡す
					startFuncs[i].actionDelegate.argMode = ActionArgMode.Arg1;
				}
                if (parameters.Length == 2)
                {
                    EditorGUILayout.BeginHorizontal();
                    SetArgument(ref startFuncs[i].actionDelegate.arg1, ref startFuncs[i].actionDelegate.argType1, parameters, 0, 90);
                    SetArgument(ref startFuncs[i].actionDelegate.arg2, ref startFuncs[i].actionDelegate.argType2, parameters, 1, 90);
                    EditorGUILayout.EndHorizontal();
                    //引数が一つであることを渡す
                    startFuncs[i].actionDelegate.argMode = ActionArgMode.Arg2;
                }

                //引数なし
                else if (parameters.Length == 0)
				{
					startFuncs[i].actionDelegate.argMode = ActionArgMode.NoArg;
					GUILayout.Label("引数なし");
				}

				startFuncs[i].actionDelegate.comment = EditorGUILayout.TextField(startFuncs[i].actionDelegate.comment);
            }
            EditorGUILayout.EndVertical();
			//高さのプラス
			rect.height += 44;
			rect.height += 18;
		}
	}
	private void PopupNonReturnUpdateMethod()
	{
		for (int i = 0; i < updateFuncs.Count; i++)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);
			EditorGUILayout.BeginHorizontal();
			//オーバーフローしてたら0に
			if (updateFuncs[i].nowStringNumber >= nowMethods.Count)
			{
				updateFuncs[i].nowStringNumber = 0;
			}
			//メソッドの同期
			for (int j = 0; j < nowMethods.Count; j++)
			{
				if (nowMethods[j].nowMethod.Name == updateFuncs[i].actionDelegate.actionName && nowMethods[j].compornent == updateFuncs[i].actionDelegate.instance)
				{
					updateFuncs[i].nowStringNumber = j;
				}
			}
			//ポップアップでメソッド取得
			updateFuncs[i].nowStringNumber = EditorGUILayout.Popup(updateFuncs[i].nowStringNumber, nowOptions, GUILayout.Width(rect.width - 83));
            if (GUILayout.Button("⇧", GUILayout.Height(15), GUILayout.Width(18)))
            {
                if (i > 0)
                {
                    StateBase.ActionDelegate actionDel = updateFuncs[i].actionDelegate;
                    myState.playUpdateDelegate[i] = myState.playUpdateDelegate[i - 1];
                    myState.playUpdateDelegate[i - 1] = actionDel;
                    NodeBaseEditor.initFlag = true;
                }
            }
            if (GUILayout.Button("⇩", GUILayout.Height(15), GUILayout.Width(18)))
            {
                if (i < myState.playUpdateDelegate.Count - 1)
                {
                    StateBase.ActionDelegate actionDel = updateFuncs[i].actionDelegate;
                    myState.playUpdateDelegate[i] = myState.playUpdateDelegate[i + 1];
                    myState.playUpdateDelegate[i + 1] = actionDel;
                    NodeBaseEditor.initFlag = true;
                }
            }
            if (GUILayout.Button("×", GUILayout.Height(15), GUILayout.Width(19)))
			{
				myState.playUpdateDelegate.Remove(updateFuncs[i].actionDelegate);
				NodeBaseEditor.initFlag = true;
			}
			//現在のメソッドがあれば
			if (nowMethods.Count > 0)
			{
				//Type t = nowMethods[updateFuncs[i].nowStringNumber].nowMethod.DeclaringType;
				updateFuncs[i].actionDelegate.instance = nowMethods[updateFuncs[i].nowStringNumber].compornent;
				//gameObject.GetComponent(t);
				updateFuncs[i].actionDelegate.actionName = nowMethods[updateFuncs[i].nowStringNumber].nowMethod.Name;
				//GetCompornentは同じクラスがあった場合にも対応
				//updateFuncs[i].actionDelegate.action = (Action)nowMethods[updateFuncs[i].nowStringNumber].nowMethod.CreateDelegate(typeof(Action), nowMethods[updateFuncs[i].nowStringNumber].compornent);
			}

			EditorGUILayout.EndHorizontal();
            if (updateFuncs.Count > i)
            {
				ParameterInfo[] parameters = { };
				if (nowMethods.Count > 0)
				{
					//引数があればそれに応じたフィールドの生成
					parameters = nowMethods[updateFuncs[i].nowStringNumber].nowMethod.GetParameters();
				}
				//引数1
				if (parameters.Length == 1)
				{
					EditorGUILayout.BeginHorizontal();
                    //インスタンスがObject型に変換可能であれば引数に代入し、引数のTypeを決める
                    SetArgument(ref updateFuncs[i].actionDelegate.arg1, ref updateFuncs[i].actionDelegate.argType1, parameters,0, 180);
					EditorGUILayout.EndHorizontal();
					//引数が一つであることを渡す
					updateFuncs[i].actionDelegate.argMode = ActionArgMode.Arg1;
				}
                //引数1
                if (parameters.Length == 2)
                {
                    EditorGUILayout.BeginHorizontal();
                    //インスタンスがObject型に変換可能であれば引数に代入し、引数のTypeを決める
                    SetArgument(ref updateFuncs[i].actionDelegate.arg1, ref updateFuncs[i].actionDelegate.argType1, parameters, 0, 90);
                    SetArgument(ref updateFuncs[i].actionDelegate.arg2, ref updateFuncs[i].actionDelegate.argType2, parameters, 1, 90);
                    EditorGUILayout.EndHorizontal();
                    //引数が二つであることを渡す
                    updateFuncs[i].actionDelegate.argMode = ActionArgMode.Arg2;
                }
                //引数なし
                else if (parameters.Length == 0)
				{
					updateFuncs[i].actionDelegate.argMode = ActionArgMode.NoArg;
					GUILayout.Label("引数なし");
				}

				updateFuncs[i].actionDelegate.comment = EditorGUILayout.TextField(updateFuncs[i].actionDelegate.comment);
            }
            EditorGUILayout.EndVertical();
			//高さのプラス
			rect.height += 44;
			rect.height += 18;

		}
	}
	//戻り値booolのメソッドをポップアップで表示
	private void PopupBoolMethod()
	{
        //boolメソッド欄表示
        for (int i = 0; i < outPoints.Count; i++)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);
			EditorGUILayout.BeginHorizontal();
			//オーバーフローしてたら0に
			if (outPoints[i].nowStringNumber >= nowMethodsBool.Count)
			{
				outPoints[i].nowStringNumber = 0;
			}
			//メソッドの同期
			for (int j = 0; j < nowMethodsBool.Count; j++)
			{
				if (nowMethodsBool[j].nowMethod.Name == outPoints[i].stateJudge.judgeFuncName && nowMethodsBool[j].compornent == outPoints[i].stateJudge.judgeFuncInstance)
				{
					outPoints[i].nowStringNumber = j;
				}
			}

			//ポップアップでメソッド取得
			outPoints[i].nowStringNumber = EditorGUILayout.Popup(outPoints[i].nowStringNumber, nowOptionsBool,GUILayout.Width(rect.width - 83));
            if (GUILayout.Button("⇧", GUILayout.Height(15), GUILayout.Width(18)))
            {
                if (i > 0)
                {
                    NextStateJudge actionDel = outPoints[i].stateJudge;
                    myState.nextStateJudge[i] = myState.nextStateJudge[i - 1];
                    myState.nextStateJudge[i - 1] = actionDel;
                    NodeBaseEditor.initFlag = true;
                }
            }
            if (GUILayout.Button("⇩", GUILayout.Height(15), GUILayout.Width(18)))
            {
                if (i < myState.nextStateJudge.Count - 1)
                {
                    NextStateJudge actionDel = outPoints[i].stateJudge;
                    myState.nextStateJudge[i] = myState.nextStateJudge[i + 1];
                    myState.nextStateJudge[i + 1] = actionDel;
                    NodeBaseEditor.initFlag = true;
                }
            }
            if (GUILayout.Button("×",GUILayout.Height(15),GUILayout.Width(19)))
			{
				myState.nextStateJudge.Remove(outPoints[i].stateJudge);
				NodeBaseEditor.initFlag = true;
			}

			//現在のメソッドがあれば
			if (nowMethodsBool.Count > 0)
			{
				outPoints[i].stateJudge.judgeFuncInstance = nowMethodsBool[outPoints[i].nowStringNumber].compornent;
				outPoints[i].stateJudge.judgeFuncName = nowMethodsBool[outPoints[i].nowStringNumber].nowMethod.Name;


				//GetCompornentは同じクラスがあった場合にも対応
				//outPoints[i].stateJudge.judgeFunc = (Func<bool>)nowMethodsBool[outPoints[i].nowStringNumber].nowMethod.CreateDelegate(typeof(Func<bool>), nowMethodsBool[outPoints[i].nowStringNumber].compornent);
			}

			EditorGUILayout.EndHorizontal();
            if (outPoints.Count > i)
            {
				ParameterInfo[] parameters = { };
				if (nowMethodsBool.Count > 0)
				{
					//引数があればそれに応じたフィールドの生成
					parameters = nowMethodsBool[outPoints[i].nowStringNumber].nowMethod.GetParameters();
				}
				//引数1
				if (parameters.Length == 1)
				{
					EditorGUILayout.BeginHorizontal();
                    //引数のセットと表示
                    SetArgument(ref outPoints[i].stateJudge.arg1, ref outPoints[i].stateJudge.argType1, parameters, 0,180);
					EditorGUILayout.EndHorizontal();
					//引数が一つであることを渡す
					outPoints[i].stateJudge.argMode = JudgeFuncArgMode.Arg1;
				}
                else if (parameters.Length == 2)
                {
                    EditorGUILayout.BeginHorizontal();
                    SetArgument(ref outPoints[i].stateJudge.arg1, ref outPoints[i].stateJudge.argType1, parameters, 0,90);
                    SetArgument(ref outPoints[i].stateJudge.arg2, ref outPoints[i].stateJudge.argType2, parameters, 1,90);
                    EditorGUILayout.EndHorizontal();
                    //引数が2つであることを渡す
                    outPoints[i].stateJudge.argMode = JudgeFuncArgMode.Arg2;
                }

                //引数なし
                else if (parameters.Length == 0)
				{
					outPoints[i].stateJudge.argMode = JudgeFuncArgMode.NoArg;
					GUILayout.Label("引数なし");
				}
                outPoints[i].stateJudge.comment = EditorGUILayout.TextField(outPoints[i].stateJudge.comment);
            }
			EditorGUILayout.EndVertical();
			//高さのプラス
			rect.height += 44;
			rect.height += 18;
		}
	}

    private void SetArgument(ref Argument sop,ref ArgType argType, ParameterInfo[] infos,int paramNum,int wid)
    {
        //インスタンスがObject型に変換可能であれば引数に代入し、引数のTypeを決める
        if (infos[paramNum].ParameterType.IsSubclassOf(typeof(UnityEngine.Object)))
        {
            GUILayout.Label("Object");
            sop.object_Arg = (UnityEngine.Object)EditorGUILayout.ObjectField(sop.object_Arg, infos[paramNum].ParameterType, true, GUILayout.Width(wid));
            argType = ArgType.UnityObject;
        }
        //int型
        else if (infos[paramNum].ParameterType == typeof(int))
        {
            GUILayout.Label("int");
            sop.int_Arg = EditorGUILayout.IntField(sop.int_Arg, GUILayout.Width(wid));
            argType = ArgType.Int;
        }
        //string型
        else if (infos[paramNum].ParameterType == typeof(string))
        {
            GUILayout.Label("string");
            sop.string_Arg = EditorGUILayout.TextField(sop.string_Arg, GUILayout.Width(wid-20));
            argType = ArgType.String;
        }
        //float型
        else if (infos[paramNum].ParameterType == typeof(float))
        {
            GUILayout.Label("float");
            sop.float_Arg = EditorGUILayout.FloatField(sop.float_Arg, GUILayout.Width(wid));
            argType = ArgType.Float;
        }
        //bool型
        else if (infos[paramNum].ParameterType == typeof(bool))
        {
            GUILayout.Label("bool");
            sop.bool_Arg = EditorGUILayout.Toggle(sop.bool_Arg, GUILayout.Width(wid));
            argType = ArgType.Bool;
        }
        else
        {
            GUILayout.Label("未対応の引数です、エラーが起こります");
        }
    }

    private void GetDrawMethod(ref Component[] comp, Type tp, ref List<NowMethods> nowm,ref string[] op)
	{
		bool compornentsFlag = false;
		//アタッチされているすべてのコンポーネントを取得(コンポーネントが違えば取得)
		if (comp != gameObject.GetComponents<StateBaseScriptMonoBehaviour>())
		{
			comp = gameObject.GetComponents<StateBaseScriptMonoBehaviour>();
			compornentsFlag = true;
		}
		//コンポーネントが変更されたときの処理
		if (compornentsFlag)
		{
			//取得するメソッドの条件
			var atr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly;
			List<NowMethods> methods = new List<NowMethods>();//取得するメソッドリスト
			//コンポーネントごとに回して格納(transform,Animatorは含まない)
			for (int i = 0; i < comp.Length; i++)
			{
				if (comp[i].GetType() == typeof(Animator) || comp[i].GetType() == typeof(StateMonobehavior))
				{
					continue;
				}
				Type type = comp[i].GetType();//コンポーネントのタイプの取得
				//メソッド入れてます
				NowMethods[] nows = { };
				MethodInfo[] methodInfos = type.GetMethods(atr);
				for (int j = 0; j < methodInfos.Length; j++)
				{
					Array.Resize(ref nows, nows.Length + 1);
					nows[j] = new NowMethods();
					//コンポーネントを入れる
					nows[j].compornent = comp[i];
					nows[j].nowMethod = methodInfos[j];
				}

				methods.AddRange(nows);//メソッドに追加

			}
			//引数のあるメソッドを削除
			//Countで中で消すとその分減るため削除用リストを用意
			List<NowMethods> removeMethods = new List<NowMethods>();
			if (methods.Count > 0)
			{
				for (int j = 0; j < methods.Count; j++)
				{
					if (methods[j].nowMethod.GetParameters().Length > 2)
					{
						removeMethods.Add(methods[j]);
						continue;
					}
					//bool取得
					if (methods[j].nowMethod.ReturnType != tp)
					{
						removeMethods.Add(methods[j]);
						continue;
					}
				}
				if (removeMethods.Count > 0)
				{
					foreach (NowMethods now in removeMethods)
					{
						methods.Remove(now);
					}
				}
			}
			//使えるメソッドがあれば名前とクラスの名前取得
			string[] options = { };
			int count = 0;
			if (methods.Count > 0)
			{
				foreach (NowMethods method in methods)
				{
					Array.Resize(ref options, options.Length + 1);
					options[count] = method.nowMethod.Name + ":Class " + (method.compornent as StateBaseScriptMonoBehaviour).name;
					count++;
				}
			}
			nowm = methods;
			op = options;
			compornentsFlag = false;//一応
		}
	}
	//再生中ノードの色を変える
	private void NodeColorChange()
	{
		color = new Color(1,1,1,0.8f);
		if (EditorApplication.isPlaying == true)
		{
			if (stateMonobehavior.stateProcessor.State == myState)
			{
				color = new Color(1,0,0,0.8f);
			}
			else
			{
				color = new Color(1, 1, 1, 0.8f);
			}
		}
	}
}
