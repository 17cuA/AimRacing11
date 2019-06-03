//===============================================================
// ファイル名：StateProcessor.cs
// 作成者    ：村上一真
// 作成日　　：20190516
// 現在のステートの移行、ステートの挙動を担うクラス。
// MonoBehaviourを継承、このコンポーネントが軸。
//
// ※ステートの中身をプレイ中動的に変えることは出来ないので注意
// 20190531 更新・Debugで読み込んでるバグの修正および名前のないステートでも動作するように
//===============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperCU;

public class StateMonobehavior : MonoBehaviour,IEventable
{
	//名前
	public string stateName;

	//変更前のステート名
    private string _beforeStateName;

	protected bool stateMove = false;
	private bool startFlag = true;

    //ステート本体（プロセッサ）
    public StateProcessor stateProcessor = new StateProcessor();
    //使用するすべてのステート
    public List<NomalState> states = new List<NomalState>();

    //TODO:DictionaryではなくIDで管理する//OK
	//ステートを名前で管理する辞書
    private Dictionary<int, NomalState> stateDictionary = new Dictionary<int, NomalState>();

	protected virtual void Awake()
	{
		foreach (NomalState state in states)
		{
			foreach (NextStateJudge judge in state.nextStateJudge)
			{
				judge.Init();
			}
		}
		foreach (NomalState nomalState in states)
		{
			nomalState.Init();
		}
	}
    //初期化処理
    protected virtual void Start()
    {
		UpdateManager.Instance.AddUpdate(this);//アップデートリストに追加
		//ステートがあれば処理
        if (states.Count > 0)
        {
            //0がスタートステート
            stateProcessor.State = states[0];//ステート0をスタートステートに
			//ステートの中身のアップデートデリゲートに登録
            foreach (NomalState ns in states)
            {
                ns.updateJudgeDelegate = NomalStateJudge;	//判定
                stateDictionary.Add(ns.ID.number, ns);//辞書追加
                ns.nextStateJudge.Sort((a, b) => b.priority - a.priority);//優先度順にソート(高ければ高い、降順)
            }
        }
		//ステートの格納
		foreach(NomalState state in states)
        {
			if (state.nextStateJudge.Count > 0)
			{
				foreach (NextStateJudge judge in state.nextStateJudge)
				{
					//TODO:同じ名前のステートでもできるようにする
					if (judge.nextStateName == null)
					{
						judge.nextState = null;
						continue;
					}
					judge.nextState = stateDictionary[judge.nextID.number];
				}
			}
        }
	}
	//常時処理
	public virtual void UpdateGame()
    {
		if (startFlag)
		{
			stateProcessor.Execute();
			startFlag = false;
		}

		stateMove = false;
		//ステートがあれば実行
        if (states.Count > 0)
        {
			//現在のステートの常時実行処理
            stateProcessor.PlayUpdate();
            //ステートの値が変更されたら実行処理を行う
            if (stateProcessor.State == null)
            {
                return;
            }
            if (stateMove)
            {
                _beforeStateName = stateProcessor.State.getStateName();
				stateProcessor.Execute();
			}
		}
    }
	/// <summary>
	/// 判定処理、現在のステートの中身から判定処理を実行
	/// </summary>
    public void NomalStateJudge()
    {
        //優先度順にジャッジ(ソートは別)
        foreach (NextStateJudge judge in stateProcessor.State.nextStateJudge)
        {
			//次のステートが登録されていなければ移行しない
			if (judge.nextState == null)
			{
				continue;
			}
            if (judge.judgeFunc.Invoke())
            {
                stateProcessor.State = judge.nextState;
				stateMove = true;
				break;
            }
        }
    }
	//インターフェースを継承しているのでおいておくだけ
	public virtual void LateUpdateGame()
    {
    }
    public virtual void FixedUpdateGame()
    {
    }
}
