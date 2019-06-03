/*====================================================*/
// 内容		：リザルトの表示、アニメーション
// ファイル	：Result.cs
//
// Copyright (C) 刈谷 司 All Rights Reserved.
/*----------------------------------------------------*/
//〔更新履歴〕
// 2018/08/19 〔刈谷 司〕 新規作成 SpriteAnimationのAnimationコルーチンを使用して、ロード画面のアニメーションを再生する。
// 2018/08/20 〔刈谷 司〕 自己完結ではなく、メソッドを外部から呼び出して動作開始するように変更
// 2018/12/07 〔刈谷 司〕 ロード完了メソッド追加
/*====================================================*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingImage : MonoBehaviour
{

	private bool isLoading;
	private Image loadingImage;
	SpriteAnimation spriteAnim;
	IEnumerator loadingAnim;

	public bool IsLoading { get { return isLoading; } }
	void Start ()
	{
		loadingImage = GetComponent<Image>();
		spriteAnim = GetComponent<SpriteAnimation>();
		loadingAnim = spriteAnim.Animation();
	}


	/// <summary>
	/// ロード開始
	/// </summary>
	public void StartLoad()
	{
		isLoading = true;
		loadingImage.color = Color.white;
		StartCoroutine(loadingAnim);
	}
	/// <summary>
	/// ロード完了
	/// </summary>
	public void FinishLoad()
	{
		loadingImage.color = new Color(1, 1, 1, 0);
		StopCoroutine(loadingAnim);
		spriteAnim.spriteNumber = 0;
	}
}
