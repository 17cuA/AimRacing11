/*====================================================*/
// 内容		：トランスミッションの選択シーン用
// ファイル	：ChoiceTransmission.cs
//
// Copyright (C) AimRacing10Team All Rights Reserved.
/*----------------------------------------------------*/
//〔概要〕
// トランスミッションの選択を管理。
// 選択後、GameManagerのisAutomaticに反映。
/*----------------------------------------------------*/
//〔更新履歴〕
// 2018/07/?? 〔刈谷 　司〕 新規作成
// 2018/08/20 〔刈谷 　司〕 GameManagerに選択内容を登録する処理を追加
// 2018/08/22 〔刈谷 　司〕 アニメーション処理を追加
// 2018/08/24 〔古明地 慧〕 サウンド関連の修正
// 2018/10/10 〔刈谷 　司〕 整理、コメント追記
/*====================================================*/
using UnityEngine;
using UnityEngine.UI;

public class ChoiceTransmission : MonoBehaviour
{

	public Image[] transmissionImage;    //[0]ATor[1]MT
	private int choiceTransmissionNum = -1;
	//private GameObject fade;

	public bool canChoice;               // 選択許可
	//public bool selected = false;        // 選択完了
	private const float sizeRate = 1.5f; // 選択後のアイコンサイズ

	// SE関連
	//AudioSource audioSource_SE;
	//[SerializeField] private UnityEngine.Audio.AudioMixerGroup se_mixer;
	//public AudioClip choice_SE;
	//public AudioClip decision_SE;

	private void Start()
	{
		//if (GetComponent<AudioSource>())
		//	audioSource_SE = GetComponent<AudioSource>();
		//else
		//	audioSource_SE = gameObject.AddComponent<AudioSource>();

		//audioSource_SE.outputAudioMixerGroup = se_mixer;
	}

	public void choiceTransmission()
	{
		// 選択完了後/フェード中は処理しない
		//if (selected) { return; }

		// トランスミッションの選択
		if (canChoice)
		{
			// AT
			if (Input.GetAxis("Horizontal") <= -0.8f)
			{
				// 選択、サイズ変更
				choiceTransmissionNum = 0;
				transmissionImage[0].transform.localScale = new Vector3(sizeRate, sizeRate, 1.0f);
				transmissionImage[1].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				//audioSource_SE.PlayOneShot(choice_SE);
			}
			// MT
			else if (Input.GetAxis("Horizontal") >= 0.8f)
			{
				choiceTransmissionNum = 1;
				transmissionImage[0].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				transmissionImage[1].transform.localScale = new Vector3(sizeRate, sizeRate, 1.0f);
				//audioSource_SE.PlayOneShot(choice_SE);
			}

			// トランスミッションの確定
			if (choiceTransmissionNum > -1 && Input.GetAxis("Vertical") >= 0.9f)
			{
				//GameManager.Instance.isAutomatic = choiceTransmissionNum == 0; // ATならTrue MTならFalse
				//audioSource_SE.PlayOneShot(decision_SE);
				canChoice = false;
			}
		}
	}
}
