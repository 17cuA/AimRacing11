//===============================================================
// ファイル名：StateBase.cs
// 作成者    ：村上一真
// 作成日　　：20190516
// ステートの基底クラス及びデフォルトのステートクラス
//===============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public enum ActionArgMode
{
	NoArg,
	Arg1,
    Arg2,
}

[System.Serializable]
public abstract class StateBase
{
	//使用するデリゲートに必要な情報（主に初期化、CreateDelegate）
	[System.Serializable]
	public class ActionDelegate
	{
		public DelegateBase action;
		public string actionName = null;
		public UnityEngine.Object instance = null;
		public void Init()
		{
			if(argMode == ActionArgMode.NoArg)
			{
				action = Activator.CreateInstance(typeof(ActionVoid)) as DelegateBase;
				typeof(ActionVoid).GetMethod("Init").Invoke(action, new object[] { instance, actionName });
			}
			else if(argMode == ActionArgMode.Arg1)
			{
				//一つ目の引数のタイプ（Typeがシリアル化不可のため）
				Type type1 = typeof(object);
				switch (argType1)
				{
					case ArgType.UnityObject:
						type1 = arg1.GetTypeArg(ArgType.UnityObject).GetType();
						break;
					case ArgType.Bool:
						type1 = typeof(bool);
						break;
					case ArgType.Float:
						type1 = typeof(float);
						break;
					case ArgType.Int:
						type1 = typeof(int);
						break;
					case ArgType.String:
						type1 = typeof(string);
						break;
				}

				Type type = typeof(ActionVoid<>).MakeGenericType(type1);
				action = Activator.CreateInstance(type) as DelegateBase;
				//delegate作成
				type.GetMethod("Init").Invoke(action, new object[] { instance, actionName, arg1.GetTypeArg(argType1) });

			}
            else if (argMode == ActionArgMode.Arg2)
            {
                //一つ目の引数のタイプ（Typeがシリアル化不可のため）
                Type type1 = typeof(object);
                switch (argType1)
                {
                    case ArgType.UnityObject:
                        type1 = arg1.GetTypeArg(ArgType.UnityObject).GetType();
                        break;
                    case ArgType.Bool:
                        type1 = typeof(bool);
                        break;
                    case ArgType.Float:
                        type1 = typeof(float);
                        break;
                    case ArgType.Int:
                        type1 = typeof(int);
                        break;
                    case ArgType.String:
                        type1 = typeof(string);
                        break;
                }
                Type type2 = typeof(object);
                switch (argType2)
                {
                    case ArgType.UnityObject:
                        type2 = arg2.GetTypeArg(ArgType.UnityObject).GetType();
                        break;
                    case ArgType.Bool:
                        type2 = typeof(bool);
                        break;
                    case ArgType.Float:
                        type2 = typeof(float);
                        break;
                    case ArgType.Int:
                        type2 = typeof(int);
                        break;
                    case ArgType.String:
                        type2 = typeof(string);
                        break;
                }

                Type type = typeof(ActionVoid<,>).MakeGenericType(type1,type2);
                action = Activator.CreateInstance(type) as DelegateBase;
                //delegate作成
                type.GetMethod("Init").Invoke(action, new object[] { instance, actionName, arg1.GetTypeArg(argType1),arg2.GetTypeArg(argType2) });

            }

        }

        public Argument arg1 = new Argument();
        public Argument arg2 = new Argument();

		public ActionArgMode argMode = ActionArgMode.NoArg;

		public ArgType argType1;
        public ArgType argType2;

#if UNITY_EDITOR
		public string comment;
#endif
    }
    //ステートの名前
    public string stateName = null;
	public InstanceID ID = new InstanceID();
	//次のステートの情報群
	public List<NextStateJudge> nextStateJudge = new List<NextStateJudge>();
	//実行するデリゲートと実行処理
    //public List<Action> execDelegate;               //ステート移行時に再生するデリゲート
	public List<ActionDelegate> execDelegate = new List<ActionDelegate>();

	public void Init()
	{
		if (execDelegate.Count > 0)
		{
			foreach (ActionDelegate actionDelegate in execDelegate)
			{
				actionDelegate.Init();
			}
		}
		if (playUpdateDelegate.Count > 0)
		{
			foreach (ActionDelegate actionDelegate in playUpdateDelegate)
			{
				actionDelegate.Init();
			}
		}
	}

    public virtual void Execute()
    {
        if (execDelegate != null)
        {
			foreach (ActionDelegate action in execDelegate)
			{
				action.action.Invoke();
			}
        }
    }
	public Action updateJudgeDelegate;		//常時判定処理
    public List<ActionDelegate> playUpdateDelegate = new List<ActionDelegate>();		//ステート移行時に常時プレイするデリゲートリスト
	public virtual void PlayUpdate()//常時実行処理まとめ
    {
		if(updateJudgeDelegate != null)
        {
            updateJudgeDelegate();
        }
        if(playUpdateDelegate != null)
        {
			foreach (ActionDelegate action in playUpdateDelegate)
			{
				action.action.Invoke();
			}
        }
    }
	/*ノードエディターで使用するプロパティ*/
#if UNITY_EDITOR
    public Vector2 nodePosition = new Vector2(0, 0);
    public bool toggleWindow = true;
#endif

	//ステート名を取得するメソッド
    public abstract string getStateName();
}
//通常ステート
[System.Serializable]
public class NomalState : StateBase
{
    public override string getStateName()
    {
        return stateName;
    }
}

//IDを参照渡ししたいのでクラス化
[System.Serializable]
public class InstanceID
{
	public int number;
    //演算子オーバーロードしてもよいが現状問題ないのでそのまま
}

