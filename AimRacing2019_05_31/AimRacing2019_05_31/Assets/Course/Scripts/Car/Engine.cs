//===================================================
// ファイル名	：Engine.cs
// 概要			：車のエンジンの制御
// 作成者		：謝 敬鴻
// 作成日		：2019.03.29
//===================================================
using UnityEngine;

namespace AIM
{
	public class Engine : MonoBehaviour
	{
	    [Header("パラメータ")]
	
	    public int bhp;
		public float RPM;
		public float torque;
	
	    [Tooltip("X軸 = RPM, y軸 = トルク 一番右のキーは最大RPM")]
	    public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0, 0, 8000, 400);
	}
}

