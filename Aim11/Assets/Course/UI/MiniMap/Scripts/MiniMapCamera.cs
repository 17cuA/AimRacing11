/*====================================================*/
// 内容		：ミニマップ用のカメラの処理
// ファイル	：MiniMapCamera.cs
//
// Copyright (C) 刈谷　司 All Rights Reserved.
/*----------------------------------------------------*/
//〔更新履歴〕
// 2018/07/05 新規作成
/*====================================================*/
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
	Quaternion initRotation;					//カメラの初期角度
	float playerInitYEulerAngle = 0;				//プレイヤーの初期Yオイラー角度
	private const float offsetHeight = 10f;		//プレイヤーから離す高さ
	private Transform player;       //最終的には関数内で取得できるようにする予定(07/05)
	private bool initialized = false;

	private void Start()
	{
		player = GameObject.Find("test2").transform;
	}
	void Update()
	{
		if(player == null) { return; }
		if(!initialized)
		{
			initialized = true;
			playerInitYEulerAngle = player.eulerAngles.y;
			//カメラのy軸回転をプレイヤーに合わせる
			Vector3 newAngle = transform.rotation.eulerAngles;
			newAngle.y = playerInitYEulerAngle;
			transform.rotation = Quaternion.Euler(newAngle);
			initRotation = transform.rotation;
		}

		//プレイヤーのy軸(左右)回転に合わせて回る。
		//プレイヤーのy軸回転 * カメラの初期回転
		transform.rotation = Quaternion.AngleAxis((player.eulerAngles.y-playerInitYEulerAngle), Vector3.up) * initRotation;

		//プレイヤーの追跡
		Vector3 newPos;
		newPos = player.position;
		newPos.y += offsetHeight;	//高さ調整
		transform.position = newPos;
	}
}
