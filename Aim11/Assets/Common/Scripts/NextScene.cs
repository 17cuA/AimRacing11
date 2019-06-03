//===================================================
// ファイル名	：NextScene.cs
// 概要		：シーンの切り替え
// 作成者	：藤森 悠輝
// 作成日	：2019.05.08
//===================================================
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
	public string nextSceneName = " ";
	public void nextScene()
	{
		if(nextSceneName == " ")
		{
			Debug.LogWarning("nextSceneNameが設定されていません。");
		}
		else
		{
			SceneManager.LoadScene(nextSceneName);
		}
	}
}
