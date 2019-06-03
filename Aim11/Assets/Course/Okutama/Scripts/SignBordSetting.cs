/*====================================================*/
// 内容		：標識のマテリアル設定スクリプト
// ファイル	：SignBordSetting
//
// Copyright (C) 根本 勇斗 All Rights Reserved.
/*----------------------------------------------------*/
//〔更新履歴〕
// 2019/1/21 〔根本 勇斗〕新規作成
// 2019/1/22 〔根本 勇斗〕標識の種類をenumで設定するように変更
// 2019/1/23 〔根本 勇斗〕複数の標識を持つオブジェクト用に種類を配列で持ち、要素分だけ処理を行うように変更
/*====================================================*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignBordSetting : MonoBehaviour
{
	public enum SignType
	{
		Tuukoudome,
		Syaryoutuukoudome,
		Saikousokudo_40,
		Tenkaikinsi,
		OikosiKinsi,
		Tyuuteisyakinsi,
		Oudanhodou,
		Ura_Circle,
		Ura_Square,
		Ura_Triangle,
		Dourokousatenari,
		Rakusekityuui,
		Kudarikyuukoubai,
		Agarikyuukoubai,
		DoubutuTobidasi,
		Migituduraori,
		ya1,
		ya2,
		ya3,
		ya4
	};

	[SerializeField]
	SignType[] type;

	Color[] signUV = new Color[]
	{
		new Color(0.485f,0.485f, 0, 0.8f), new Color(0.485f,0.485f, 0.485f, 0.8f), new Color(0.485f,0.485f, 0.6655f, 0.8f), new Color(0.485f,0.485f, 0.7982f, 0.8f),new Color(0.485f,0.485f, 0.9062f,0.8f),
		new Color(0.485f,0.485f, 0, 0.6f),
		new Color(0.485f,0.485f, 0, 0.4f), new Color(0.485f,0.485f, 0.485f, 0.4f), new Color(0.485f,0.485f, 0.6655f, 0.4f),new Color(0.485f,0.485f, 0.7982f, 0.4f),
		new Color(0.485f,0.485f, 0, 0.2f), new Color(0.485f,0.485f, 0.485f, 0.2f), new Color(0.485f,0.485f, 0.6655f, 0.2f), new Color(0.485f,0.485f, 0.7982f,0.2f),new Color(0.485f,0.485f, 0.9062f, 0.2f),
		new Color(0.485f,0.485f, 0, 0), new Color(0.485f,0.485f, 0.485f, 0), new Color(0.485f,0.485f, 0.6655f, 0), new Color(0.485f,0.485f, 0.7982f, 0),new Color(0.485f,0.485f, 0.9062f, 0),
	};

    void Start()
    {
		for(int i=0;i<type.Length;i++)
		{
			var block = new MaterialPropertyBlock();
			var backBlock = new MaterialPropertyBlock();
			var renderer = GetComponent<MeshRenderer>();

			var backType = 0;

			if ((int)type[i] <= 5)
				backType = 7;
			else if ((int)type[i] == 6)
				backType = 9;
			else
				backType = 8;

			block.SetColor("_UVColorInfo", signUV[(int)type[i]]);
			backBlock.SetColor("_UVColorInfo", signUV[backType]);

			var setCnt = Vector2Int.zero;

			for (int j = 0; j < renderer.sharedMaterials.Length; j++)
			{
				if (renderer.sharedMaterials[j].name == "SignBord")
				{
					if (i == setCnt.x)
						renderer.SetPropertyBlock(block, j);
					else
						setCnt.x++;
				}
				if (renderer.sharedMaterials[j].name == "SignBord_Back")
				{
					if (i == setCnt.y)
						renderer.SetPropertyBlock(backBlock, j);
					else
						setCnt.y++;
				}
			}
		}
        
    }
}
