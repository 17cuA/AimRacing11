//────────────────────────────────────────────
// ファイル名	：Entry.cs
// 概要		：Entryシーン全体の管理クラス
// 作成者	：杉山 雅哉
// 作成日	：2018.3.8
// 
//────────────────────────────────────────────
// 更新履歴：
// 2019/05/31 [杉山 雅哉] コメント付け
// 
// TODO：
//
//────────────────────────────────────────────
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Entry : StateBaseScriptMonoBehaviour
{
	public float time = 0;
	Fade fade;
	E_UIMove e_UIMove;

	//初期化──────────────────────────────────────────────────────────────
	private void Start()
	{
		fade = GameObject.Find("Fade").GetComponent<Fade>();
	}
	//更新処理─────────────────────────────────────────────────────────────
	private void Update()
	{
		time += Time.deltaTime;
	}
	//外部呼出しメソッド─────────────────────────────────────────────────────────
	//フェード処理群ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	/// <summary>
	/// 全体のフェードアウト処理
	/// </summary>
	/// <param name="sec">処理終了時間数</param>
	/// <returns></returns>
	public void StartFadeOut(float sec)
	{
		Debug.Log(fade);
		StartCoroutine(fade.FadeOut(sec));
	}
	/// <summary>
	/// 全体のフェードイン処理
	/// </summary>
	/// <param name="sec">処理終了時間数</param>
	/// <returns></returns>
	public void StartFadeIn(float sec)
	{
		Debug.Log(fade);
		StartCoroutine(fade.FadeIn(sec));
	}
	/// <summary>
	/// 画像のフェードイン処理
	/// </summary>
	/// <param name="image">処理対象画像</param>
	/// <param name="sec">処理終了時間数</param>
	/// <returns></returns>
	public void StartImageFadeIn(Image image, float sec)
	{
		StartCoroutine(fade.ImageFadeIn(image, sec));
	}
	/// <summary>
	/// 画像のフェードアウト処理
	/// </summary>
	/// <param name="image">処理対象画像</param>
	/// <param name="sec">処理終了時間数</param>
	/// <returns></returns>
	public void StartImageFadeOut(Image image, float sec)
	{
		StartCoroutine(fade.ImageFadeOut(image, sec));
	}
	public bool flowFade0(float sec)
	{
		if (time >= sec)
		{
			time = 0;
			return true;
		}
		return false;
	}
	//移動処理群ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
}
