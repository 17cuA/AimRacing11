using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData 
{
	//　位置のリスト
	public List<GhostPos> Listspos = new List<GhostPos>();
	//　角度リスト
	public List<GhostRot> Listsrot = new List<GhostRot>();
	public List<float> time = new List<float>();
}

[Serializable]
public class GhostData 
{
	// データの長さ
	public int length;

	//　位置配列
	public Vector3[] arrayPos;
	//　角度配列
	public Quaternion[] arrayQua;
	
	/// <summary>
	/// 要素数設定
	/// </summary>
	/// <param name="dataLength">要素数</param>
	/// <returns></returns>
	public void SetDataLength(int dataLength)
	{
		length = dataLength;
		arrayPos = new Vector3[dataLength];
		arrayQua = new Quaternion[dataLength];
	}
}

[Serializable]
public class GhostPos
{
	public Vector3 body;
}

[Serializable]
public class GhostRot
{
	public Quaternion body;
	public Quaternion WheelFL;
	public Vector2 WheelRL;
}