//===================================================
// ファイル名	：Car.cs
// 概要			：ギアの制御
// 作成者		：謝 敬鴻
// 作成日		：2019.03.29
//===================================================
using UnityEngine;

namespace AIM
{
	public class Gearbox : MonoBehaviour
	{
	    [Header("パラメータ")]
	    
	    public float[] gearRatio;
	    public float finalGear;
	    public int currentGear;
	
	}
}

