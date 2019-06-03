//===================================================
// ファイル名	：EntryManager.cs
// 概要			：EntrySceneの管理
// 作成者		：藤森 悠輝
// 作成日		：2019.05.09
//===================================================
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EntryManager : MonoBehaviour
{
	Fade fade;
	Image welcomeTitle;
	GameObject choiceTransmission;

	[SerializeField] List<bool> flowFlag = new List<bool>();
	// Start is called before the first frame update
	void Start()
    {
		fade = GameObject.Find("Fade").GetComponent<Fade>();
		welcomeTitle = GameObject.Find("WelcomeToAimRacing2018_Image").GetComponent<Image>();
		choiceTransmission = GameObject.Find("ChoiceTransmission");
		for (int i = 0; i < 5; i++)
		{
			flowFlag.Add(true);
		}
    }

    // Update is called once per frame
    void Update()
    {
		StartCoroutine(EntryFrow());
		choiceTransmission.GetComponent<ChoiceTransmission>().choiceTransmission();
    }

	//==============================================================================================================================
	//　処理の流れ
	//==============================================================================================================================
	IEnumerator EntryFrow()
	{
		//最初のフェード
		if (flowFlag[0])
		{
			yield return StartCoroutine(fade.FadeOut(3.0f));
			flowFlag[0] = false;
		}
		//AimRacing2019の世界へようこそ、を表示
		else if (flowFlag[1])
		{
			yield return StartCoroutine(fade.ImageFadeIn(welcomeTitle, 3.0f));
			flowFlag[1] = false;
		}
		else if (flowFlag[2])
		{
			yield return StartCoroutine(fade.ImageFadeOut(welcomeTitle, 3.0f));
			flowFlag[2] = false;
			if(!flowFlag[2])
			{
				choiceTransmission.transform.Find("Parent").gameObject.SetActive(true);
			}
		}
		//表示終了後、名前を入力してもらう（基本的に左から右へメニューを流すイメージ）
		else if (flowFlag[3])
		{
			yield return StartCoroutine(fade.ImageFadeOut(welcomeTitle, 3.0f));
			flowFlag[3] = false;
		}
		//入力後、オートマ、マニュアルを入力（ハンドルと同じInputで入力、角度をとって画像の角度も変化する）
		//入力後、それではお楽しみください。を表示→LODE
	}
}
