//===================================================
// ファイル名	：EventArea.cs
// 概要			：イベントを発生させるエリア
// 作成者		：杉山 雅哉
// 作成日		：2018.1.22
// 
//---------------------------------------------------
// 更新履歴：
// 2019/01/22 [杉山 雅哉] クラス作成。車がエリア内に侵入したら消す
// 2019/01/22 [杉山 雅哉] 削除するのではなく、接触フラグを上げることで侵入を知らせる
//
// TODO：
// 2019/01/22 [杉山 雅哉] 判別を行うためにCarTagを追加。
//
//===================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventArea : MonoBehaviour
{
	[SerializeField]bool isIn = false;
	/// <summary>
	/// 侵入したかの判定
	/// 読み取り専用
	/// </summary>
	public bool IsIn
	{
		get
		{
			if (isIn)
			{
				isIn = false;
				return true;
			}
			return false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		isIn = true;
	}
}
