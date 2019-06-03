//───────────────────────────────────
// ファイル名	：OkutamaSceneManager.cs
// 概要		：奥多摩シーンの全体の管理を行う
// 作成者	：杉山 雅哉
// 作成日	：2019.05.07
// 
//───────────────────────────────────
// 更新履歴：
// 2019/05/07 [杉山 雅哉] 
//───────────────────────────────────
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OkutamaSceneManager : MonoBehaviour
{
	//プロパティ────────────────────────────
	[SerializeField] private Meter_Onbord meter = null;
	//─────────────────────────────────

	//初期化──────────────────────────────
	private void Start()
	{
		if (GameObject.Find("Canvas_Meter_Onbord_new") != null)
		{
			meter = GameObject.Find("Canvas_Meter_Onbord_new").GetComponent<Meter_Onbord>();
		}
		else
		{
			Debug.Log("Meter_Onbordが取得できません。");
		}
	}
	//─────────────────────────────────

	//更新処理─────────────────────────────
	private void Update()
	{
		if (meter != null)
		{
			meter.MetarUpdate();
		}
	}
	//─────────────────────────────────

	//物理系更新処理──────────────────────────
	private void FixedUpdate()
	{
		//  if(userControl != null)
		//  {
		//userControl = GameObject.Find("alfaromeo").GetComponent<UserControl>();
		//userControl.KeyboardCtrl();
		//  }
		//  if(carAI != null)
		//  {
		//carAI = GameObject.Find("CarAI").GetComponent<CarAI>();
		//carAI.AICtrl();
		//  }
	}
	//─────────────────────────────────
}
