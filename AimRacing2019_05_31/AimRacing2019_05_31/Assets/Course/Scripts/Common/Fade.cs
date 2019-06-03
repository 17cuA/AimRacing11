//===================================================
// ファイル名	：Fade.cs
// 概要			：シーンのフェード
// 作成者		：藤森 悠輝
// 作成日		：2018.12.21
// 
//---------------------------------------------------
// 更新履歴：
// 2018/12/21 [藤森 悠輝] スクリプト作成
// 2018/12/21 [藤森 悠輝] フェードに必要なメソッド作成
//
//===================================================
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    //変数宣言-------------------------------------------------------------------------------------------
    [SerializeField] private Image fadeCurtain; //フェードに使う画像用
    [SerializeField] private bool didFaded;     //フェード終了判定

	//static public Fade fadeInstance;
	//---------------------------------------------------------------------------------------------------

	/**
     * @fn void Awake()
     * @brief インスタンス初期化
     * @param	無し
     * @return	無し
     */
	//private void Awake()
 //   {
	//	//インスタンス初期化
	//	if (fadeInstance == null)
	//	{
	//		fadeInstance = this;
	//		DontDestroyOnLoad(gameObject);
	//	}
	//	else
	//	{
	//		Destroy(gameObject);
	//	}
	//}

	/**
     * @fn void Start()
     * @brief 初期化
     * @param	無し
     * @return	無し
     */
	private void Start()
    { 
        fadeCurtain = transform.Find("FadeCurtain").gameObject.GetComponent<Image>();
        //完全に明るくなるor暗くなるとフェードを終了させる
        if (fadeCurtain.color.a == 0f || fadeCurtain.color.a == 1f)
        {
            didFaded = true;
        }
    }

	/**
     * @fn IEnumerator FadeIn()
     * @brief フェードイン
     * @param	fadeTime フェード所要時間
     * @return	無し
     */
	public IEnumerator FadeIn(float fadeTime = 3.0f)
    {
		Debug.Log("FadeIn");
		didFaded = false;
		while (fadeCurtain.color.a < 1f)
		{
			fadeCurtain.color += new Color(0f, 0f, 0f, Time.deltaTime / fadeTime);
			if (fadeCurtain.color.a >= 1f)
			{
				break;
			}
			yield return true;
		}
		didFaded = true;
	}

	/**
     * @fn IEnumerator FadeOut()
     * @brief フェードアウト
     * @param	fadeTime フェード所要時間
     * @return	無し
     */
	public IEnumerator FadeOut(float fadeTime = 3.0f)
    {
		Debug.Log("FadeOut");
		didFaded = false;
		while (fadeCurtain.color.a > 0f)
		{
			fadeCurtain.color -= new Color(0f, 0f, 0f, Time.deltaTime / fadeTime);
			if (fadeCurtain.color.a <= 0)
			{
				break;
			}
			yield return true;
		}
		didFaded = true;
	}

	/**
     * @fn IEnumerator ImageFadeIn()
     * @brief 送られてきた画像をフェードインさせる
     * @param	Image fadeImage 他のオブジェクトで管理している画像情報
	 *			fadeTime フェード所要時間
     * @return	無し
     */
	public IEnumerator ImageFadeIn(Image fadeImage, float fadeTime = 3.0f)
    {
		Debug.Log("FadeIN");
        while(fadeImage.color.a < 1f)
        {
            fadeImage.color += new Color(0f, 0f, 0f, Time.deltaTime / fadeTime);
			if (fadeImage.color.a >= 1)
			{
				break;
			}
			yield return true;
		}
		yield return true;
    }

	/**
     * @fn IEnumerator ImageFadeOut()
     * @brief 送られてきた画像をフェードアウトさせる
     * @param	Image fadeImage 他のオブジェクトで管理している画像情報
	 *			fadeTime フェード所要時間
     * @return	無し
     */
	public IEnumerator ImageFadeOut(Image fadeImage, float fadeTime = 3.0f)
    {
		Debug.Log("FadeOUT");
		while (fadeImage.color.a > 0f)
        {
			Debug.Log(Time.deltaTime / fadeTime);
            fadeImage.color -= new Color(0f, 0f, 0f, Time.deltaTime / fadeTime);
			if (fadeImage.color.a <= 0)
			{
				break;
			}
			yield return true;
		}
		yield return true;
	}

	/**
     * @fn bool getDidFaded()
     * @brief didFadedの値を返す
     * @param	無し
     * @return	無し
     */
	public bool getDidFaded ()
    {
        return didFaded;
    }
}
