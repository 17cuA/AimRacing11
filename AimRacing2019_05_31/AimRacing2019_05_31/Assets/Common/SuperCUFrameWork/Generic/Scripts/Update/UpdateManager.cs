//===============================================================
// ファイル名：UpdateManager.cs
// 作成者    ：村上一真
// 作成日　　：20190516
// Update()を一括で管理
//===============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperCU;
using SuperCU.Pattern;

public class UpdateManager : SingletonMono<UpdateManager>
{
    [SerializeField]
    private List<IEventable> updateList = new List<IEventable>();
    protected override void Awake()
    {
		dontDs = false;		//そのシーンだけしか残らないようにする
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
	/// <summary>
	/// このメソッドを呼び出してアップデートするものを追加
	/// </summary>
	/// <param name="ev"></param>
    public void AddUpdate(IEventable ev)
    {
        updateList.Add(ev);
    }
    private void Update()
    {
		//アップデートリストにあるすべてを呼び出し
        foreach (IEventable temp in updateList)
        {
            temp.UpdateGame();
        }
    }
}
