//────────────────────────────────────────────
// ファイル名	：E_UIMove.cs
// 概要		：EntryシーンのUIの動きを管理するクラス
// 作成者	：杉山 雅哉
// 作成日	：2018.3.8
// 
//────────────────────────────────────────────
// 更新履歴：
// 2019/05/31 [杉山 雅哉] クラス作成。UIを指定した座標に移動させる
// 
// TODO：
//
//────────────────────────────────────────────
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_UIMove : MonoBehaviour
{
	//プロパティ─────────────────────────────────────

	//──────────────────────────────────────────
	//外部呼出しメソッド─────────────────────────────────
	/// <summary>
	/// 前後のアンカーとの角度と位置をいい感じにする
	/// </summary>
	/// <param name="obj">移動するオブジェクト</param>
	/// <param name="target">移動先</param>
	/// <param name="time">移動にかかる時間</param>
	/// <returns></returns>
	public IEnumerator MoveTowards(GameObject obj, Transform target,float time)
	{
		// 移動地点を保存する
		Vector3 start = obj.transform.position;
		Vector3 end = target.position;
		//!< 経過時間をはかる
		for (float progress = 0; progress > time; progress += Time.deltaTime)
		{
			//!< 移動割合を算出
			float percentage = progress / time;
			//!< 移動
			obj.transform.position = start * (1 - percentage) + end * percentage;
			//!< また1フレーム後に～～さよなら～～～～
			yield return null;
		}
		//!< 誤差修正
		obj.transform.position = end;
		//!< さよなら～
		yield break;
	}
}
