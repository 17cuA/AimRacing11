/*======================================================================*/
//内容			：揺れる葉の遅延時間をセットするスクリプト
//ファイル		：TreeDelaySet.cs
//
// Copyright (C) 製作者 (根本 勇斗) All Rights Reserved.
/*----------------------------------------------------------------------*/
// [更新履歴]
// 2019/02/06 [根本 勇斗] 新規作成
/*======================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeDelaySet : MonoBehaviour
{
	[SerializeField] Vector3 worldSize;
    [SerializeField] float maxDelayTime;

	void Start()
	{
		//木が配置されている範囲をBoxColliderから取得
		BoxCollider box = GetComponent<BoxCollider>();
		if(box!=null)
		{
			worldSize = box.size;
			Destroy(box);
		}

		//子オブジェクトをすべて取得し、MeshRendererを持つオブジェクトにマテリアルプロパティブロックでDelayTime,WaveAmountを設定
        Transform[] child = transform.GetComponentsInChildren<Transform>();
		var delay = new MaterialPropertyBlock();

		for (int i=0;i<child.Length;i++)
		{
            Renderer render = child[i].GetComponent<Renderer>();
            if (render!=null)
            {
				float delayTime = child[i].position.magnitude / worldSize.magnitude * maxDelayTime;
				delay.SetInt("_DelayTime", (int)delayTime);
				delay.SetFloat("_WaveAmountX", Random.Range(0.1f, 0.5f));
				delay.SetFloat("_WaveAmountY", Random.Range(0.1f, 0.5f));
				delay.SetFloat("_WaveAmountZ", Random.Range(0.3f, 0.5f));
				for (int j = 0; j < render.sharedMaterials.Length; j++)
				{
					if (render.sharedMaterials[j].name == "TrembleLeaf"||render.sharedMaterials[j].name=="TrembleBranch")
					{
						render.SetPropertyBlock(delay);
					}
				}
            }
		}
	}
}
