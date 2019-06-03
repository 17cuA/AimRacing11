//────────────────────────────────────────────
// ファイル名	：GhostPlayer.cs
// 概要			：ゴーストデータを再生する
// 作成者		：杉山 雅哉
// 作成日		：2018.5.16
// 
//────────────────────────────────────────────
// 更新履歴：
// 2019/05/20 [杉山 雅哉] 保存した情報から再生を行う
// 
// TODO：
//
//────────────────────────────────────────────
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class GhostPlayer : MonoBehaviour
{
	//プロパティ───────────────────────────────────────
	[Header("車のデータ(プレハブ)")]
	[SerializeField] private GameObject copyObjectPrefab;

	private GhostData loadData;		// 読み込んだデータの保存先
	private bool isExecuted;		// 実行済みかの判定フラグ
	//定数値ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	private const string saveDataFolder = "/Projects/Ghost";	// 保存先の情報
	private const string makerDataFolder = "/Projects/Maker";	// 初期化用パス情報
	private const string saveFileName = "/ghostdata.json";		// 保存名
	//────────────────────────────────────────────
	//ゴースト再生処理────────────────────────────────────
	/// <summary>
	/// ゴーストの再生を行う(呼び出しは一回のみ)
	/// </summary>
	/// <returns></returns>
	public void Replay()
	{
		//!< エラー確認
		if (!CheckError()) return;

		//!< 実行済みフラグを立てる
		isExecuted = true;

		//!< jsonデータ読み込み
		Load();

		//!< 再生処理開始
		StartCoroutine(Replaying());
	}
	//ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	/// <summary>
	/// 再生を行う
	/// </summary>
	/// <returns></returns>
	IEnumerator Replaying()
	{
		GameObject ghostObject = Instantiate(copyObjectPrefab, loadData.arrayPos[0], loadData.arrayQua[0]);
		for (int dataNum = 0; dataNum < loadData.length; ++dataNum)
		{
			//!< 1フレーム待つ
			yield return new WaitForFixedUpdate();

			//!< 位置更新
			ghostObject.transform.position = loadData.arrayPos[dataNum];
			ghostObject.transform.rotation = loadData.arrayQua[dataNum];
		}
		Debug.Log("再生終了");
		yield break;

	}
	//ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	/// <summary>
	/// データを読み込む
	/// </summary>
	/// <returns></returns>
	void Load()
	{
		//!< jsonデータ読み込み
		string readAllText = File.ReadAllText(Application.dataPath + saveDataFolder + saveFileName);

		//!< ゴーストデータ入れ込み
		loadData = new GhostData();
		JsonUtility.FromJsonOverwrite(readAllText, loadData);
	}
	//────────────────────────────────────────────
	//内部呼び出しメソッド──────────────────────────────────
	/// <summary>
	/// 必要なものがアタッチされているか確認し、存在しないのであれば警告を出す
	/// </summary>
	/// <returns></returns>
	private bool CheckError()
	{
		if (!copyObjectPrefab) { Debug.LogError("ゴーストを反映させるオブジェクトがありません");return false; }
		if (!File.Exists(Application.dataPath + saveDataFolder + saveFileName))
		{ Debug.LogError("データが存在しません");return false; }
		if (isExecuted) { Debug.LogError("現在再生中です。2回以上の呼び出しは行わないでください。");return false; }
		return true;
	}
}