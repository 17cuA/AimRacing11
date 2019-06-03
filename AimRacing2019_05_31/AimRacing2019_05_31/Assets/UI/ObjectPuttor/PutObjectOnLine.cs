//──────────────────────────────────────────────
// ファイル名	：PutObjectOnLine.cs
// 概要			：引かれた線上にオブジェクトを配置する
// 作成者		：杉山 雅哉
// 作成日		：2019.05.13
// 
//──────────────────────────────────────────────
// 更新履歴：
// 2019/05/11 [杉山 雅哉] 線上にオブジェクトを配置する（配置間隔などは考慮せず）
// 2019/05/12 [杉山 雅哉] 配置するオブジェクトを一か所にまとめる
// 2019/05/12 [杉山 雅哉] アタッチされているはずのものが存在しない場合、警告文を出す
// 2019/05/13 [杉山 雅哉] 等間隔に配置を行う
//──────────────────────────────────────────────
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutObjectOnLine : MonoBehaviour
{
	//プロパティ───────────────────────────────────────
	[SerializeField] private LineRenderer lineRenderer;
	[Header("配置するオブジェクトプレハブ")]
	[SerializeField] GameObject PutedObject;
	[Header("オブジェクト名(半角)")]
	[SerializeField] string parentName;
	//────────────────────────────────────────────

	//初期化─────────────────────────────────────────
	void Start()
	{
		PutObject();
	}
	//────────────────────────────────────────────

	//外部呼出しメソッド───────────────────────────────────
	public void PutObject()
	{
		print(lineRenderer);
		if (!CheckError()) return;

		GameObject parent = new GameObject(parentName);
		for (int i = 0; i < lineRenderer.positionCount; ++i)
		{
			GameObject obj = Instantiate(PutedObject, lineRenderer.GetPosition(i), Quaternion.identity);
			if (i < lineRenderer.positionCount - 1)
			{
				obj.transform.LookAt(lineRenderer.GetPosition(i + 1));
			}
			else
			{
				obj.transform.LookAt(lineRenderer.GetPosition(i - 1));
			}

			obj.transform.parent = parent.transform;
		}
	}
	//内部メソッド──────────────────────────────────────

	//ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	/// <summary>
	/// 必要なものがアタッチされているか確認し、存在しないのであれば警告を出す
	/// </summary>
	/// <returns></returns>
	bool CheckError()
	{
		if (!PutedObject){ Debug.LogError("置くオブジェクトのプレハブが存在しません"); return false; }
		if(parentName.Length == 0) { Debug.LogError("名前を入力してください");return false; }
		else { return true; }
	}

}
