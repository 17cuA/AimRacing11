//===============================================================
// ファイル名：StateProcessor.cs
// 作成者    ：村上一真
// 作成日　　：20190516
// 現在のステートを格納、管理するクラス
//===============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StateProcessor
{
    //ステート本体
    private StateBase _State = null;
    public StateBase State
    {
        set { _State = value; }
        get { return _State; }
    }
    // ステート移行時に一度だけ
    public void Execute()
    {
        State.Execute();
    }
    // ステート中常時
    public void PlayUpdate()
    {
        State.PlayUpdate();
    }

}
