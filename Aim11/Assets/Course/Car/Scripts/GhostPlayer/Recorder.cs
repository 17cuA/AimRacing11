//────────────────────────────────────────────
// ファイル名	：Recorder.cs
// 概要			：ゴーストデータを保存する
// 作成者		：杉山 雅哉
// 作成日		：2018.5.16
// 
//────────────────────────────────────────────
// 更新履歴：
// 2019/05/17 [杉山 雅哉] 計測地点を定め、そこまでの座標情報を配列に込める
// 2019/05/17 [杉山 雅哉] めちゃくちゃ重いので計測間隔を設ける
// 2019/05/17 [杉山 雅哉] 別に重くなかった。
// 2019/05/17 [杉山 雅哉] 外部出力処理を追加。
// 2019/05/20 [杉山 雅哉] jsonコード書き出し処理追加
// 2019/05/20 [杉山 雅哉] 計測間隔を毎フレームに修正
// TODO：
//
//────────────────────────────────────────────
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Recorder : MonoBehaviour
{
	//プロパティ───────────────────────────────────────
	[Header("車のデータ")]
	[SerializeField] private Transform original;
	[Header("計測終了地点")]
	[SerializeField] private EventArea endArea;
	[Header("計測の限界量")]
	[SerializeField] private int dataLength;

	private GhostData currentlyData;	// データの一時保存先
	private bool isExecuted;			// 実行済みかの判定フラグ
	//定数値ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	private const string saveDataFolder = "/Projects/Ghost";	// 保存先の情報
	private const string makerDataFolder = "/Projects/Maker";	// 初期化用パス情報
	private const string saveFileName = "/ghostdata.json";		// 保存名
	//────────────────────────────────────────────

	//記録処理────────────────────────────────────────
	/// <summary>
	/// 計測開始（今後の呼び出しは不要）
	/// </summary>
	/// <returns></returns>
	public void StartRecord()
	{
		//!< エラー確認
		if (!CheckError()) return;

		//!< 保存フラグを立てる
		isExecuted = true;

		//!< ファイル作成
		Directory.CreateDirectory(Application.dataPath + saveDataFolder);
		Directory.CreateDirectory(Application.dataPath + makerDataFolder);

		//!< データの長さ合わせ
		currentlyData = new GhostData();
		currentlyData.SetDataLength(dataLength);

		//!< 処理開始
		StartCoroutine(Recording());
	}
	//ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	/// <summary>
	/// 計測を行う
	/// </summary>
	/// <returns></returns>
	IEnumerator Recording()
	{
		int dataNum;
		for (dataNum = 0; dataNum < dataLength; ++dataNum)
		{
			//!< 数秒待機
			yield return new WaitForFixedUpdate();

			//!< 計測地点
			currentlyData.arrayPos[dataNum] = original.position;
			currentlyData.arrayQua[dataNum] = original.rotation;
			if (endArea.IsIn) { break; }
		}
		//!< データの長さが限界量を超えていなければセーブ処理を実行する
		if(dataNum < dataLength)
		{
			Debug.Log("データを保存します");

			currentlyData.length = dataNum;
			Save();
		}
		else
		{
			Debug.Log("計測上限に到達しましたため強制終了します");
		}

		// すべての役目を終えたので消滅
		Destroy(gameObject);
		yield break;
	}
	//ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	/// <summary>
	/// データをセーブする
	/// </summary>
	/// <returns></returns>
	void Save()
	{
		//　GhostDataクラスをJSONデータに書き換え
		string data = JsonUtility.ToJson(currentlyData);
		//　ゲームフォルダにファイルを作成
		File.WriteAllText(Application.dataPath + saveDataFolder + saveFileName, data);
	}
	//内部呼び出しメソッド──────────────────────────────────
	/// <summary>
	/// 必要なものがアタッチされているか確認し、存在しないのであれば警告を出す
	/// </summary>
	/// <returns></returns>
	private bool CheckError()
	{
		if (!original) { Debug.LogError("観測するオブジェクトがありません"); return false; }
		if (!endArea) { Debug.LogError("観測終了地点を設定してください"); return false; }
		if (0 >= dataLength) { Debug.LogError("データの長さを指定してください"); return false; }
		if (isExecuted) { Debug.LogError("現在保存中です。2回以上の呼び出しは行わないでください。");return false; }
		return true;
	}
	//────────────────────────────────────────────
}
