//===================================================
// ファイル名	：TitleManager.cs
// 概要		：タイトルシーン管理
// 作成者	：藤森 悠輝
// 作成日	：2019.05.08
//===================================================
using UnityEngine;

public class TitleManager : MonoBehaviour
{
	// Update is called once per frame
	void Update()
	{
		pushButton();
	}

	void pushButton()
	{
		if(Input.GetAxis("Vertical") >= 0.8f)
		{
			gameObject.GetComponent<NextScene>().nextScene();
		}
	}
}
