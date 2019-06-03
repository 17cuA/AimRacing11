//──────────────────────────────────────────────────────────────────────────
// ファイル名	：Anker.cs
// 概要			：ベジェ曲線のアンカーの役割をするオブジェクトのクラス
// 作成者		：杉山 雅哉
// 作成日		：2018/04/16
// 
//──────────────────────────────────────────────────────────────────────────
// 更新履歴：
// 2019/04/16 [杉山 雅哉] 新たに点が置かれたときに、ハンドルを生成させる
// 2019/04/17 [杉山 雅哉] 一方のハンドルの座標が変わったとき、角度だけ対角線上に合わせるようにする。
// 2019/04/19 [杉山 雅哉] 使用を変更し、座標も対角線上に置く。
// 2019/05/09 [杉山 雅哉] 次のアンカーが生成されたとき、自動的にいい感じの角度に変える
// 2019/05/30 [杉山 雅哉] ハンドルの片方の長さが変わったとき、もう片方は距離を維持して対角線上になるように修正する
//
// TODO：
//
//──────────────────────────────────────────────────────────────────────────

using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
//SceneViewを取得するために宣言、エディタ外では使えないのでUNITY_EDITORで囲む
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class Anker : MonoBehaviour
{
	//変数宣言──────────────────────────────────────────────────────────────────────
	[SerializeField] GameObject prevHandle;		//!< 前の曲線に影響するハンドル
	[SerializeField] GameObject nextHandle;		//!< 次の曲線に影響するハンドル
	[SerializeField] LineRenderer lineRenderer;	//!< ラインレンダラー情報

	Vector3 prevPrevPosition;
	Vector3 prevNextPosition;
	//──────────────────────────────────────────────────────────────────────────
	//法線の向き
	public Vector3 Normal { get; set; }
	//前ハンドル
	public Vector3 PrevHandlePos
	{
		get { return prevHandle.transform.position; }
		set { prevHandle.transform.position = value; }
	}
	//次ハンドル
	public Vector3 NextHandlePos
	{
		get { return nextHandle.transform.position; }
		set { nextHandle.transform.position = value; }
	}
	//アンカー
	public Vector3 AnkerPos
	{
		get { return transform.position; }
		set { transform.position = value; }
	}
	//角度調整処理群──────────────────────────────────────────────────────────────
	/// <summary>
	/// 直線になるように最初のアンカーを調整する（2点の状態のみ作動）
	/// </summary>
	/// <param name="next">一つ手前のアンカー</param>
	/// <returns></returns>
	public void AdjustFirstAnkerAngle(Anker next)
	{
		transform.LookAt(next.AnkerPos,Vector3.up);
	}
	/// <summary>
	/// 直線になるように2点目のアンカーを調整する（2点の状態のみ作動）
	/// </summary>
	/// <param name="prev">一つ手前のアンカー</param>
	/// <returns></returns>
	public void AdjustSecondAnkerAngle(Anker prev)
	{
		//!< 見る地点を指定する
		Vector3 target = AnkerPos + (AnkerPos - prev.AnkerPos);

		transform.LookAt(target,Vector3.up);
	}
	/// <summary>
	/// 前後のアンカーとの角度と位置をいい感じにする
	/// </summary>
	/// <param name="prev">一つ手前のアンカー</param>
	/// <param name="next">一つ先のアンカー</param>
	/// <returns></returns>
	public void AdjustCenterAnkerAngle(Anker prev,Anker next)
	{
		GameObject obj = new GameObject();
		obj.transform.position = prev.AnkerPos;

		obj.transform.LookAt(next.AnkerPos);

		transform.rotation = obj.transform.rotation;

		DestroyImmediate(obj);
	}
	/// <summary>
	/// 前のアンカーとの角度と位置をいい感じにする
	/// </summary>
	/// <param name="prev">一つ手前のアンカー</param>
	/// <returns></returns>
	public void AdjustPrevAnkerAngle(Anker prev)
	{
		//!< 見る地点を指定する
		Vector3 target = AnkerPos + (AnkerPos - prev.NextHandlePos);
		transform.LookAt(target, Vector3.up);
	}
	/// <summary>
	/// 次のアンカーとの角度と位置をいい感じにする
	/// </summary>
	/// <param name="next">一つ先のアンカー</param>
	/// <returns></returns>
	public void AdjustNextAnkerAngle(Anker next)
	{
		transform.LookAt(next.PrevHandlePos);
	}
	//ハンドルの長さ調整処理群────────────────────────────────────────────────────
	/// <summary>
	/// 対角線上に座標を配置する
	/// </summary>
	/// <param name="symmetric">対位置の対称となる座標</param>
	/// <param name="target">変更の対象となる座標</param>
	/// <returns></returns>
	Vector3 VersusPosition(Vector3 symmetric, Vector3 target)
	{
		//!< 変更した座標の距離を計算
		float distanceFromSymmetric = Vector3.Distance(symmetric, Vector3.zero);
		//!< 変更を加える座標の距離を計算
		float distanceFromTarget = Vector3.Distance(target, Vector3.zero);
		//!< 割合を算出
		float percentage = distanceFromTarget / distanceFromSymmetric;
		//!< 変更した座標の値に割合の値をかけ、符号を反転
		return symmetric * percentage * -1;
	}
	/// <summary>
	/// 次のアンカーに影響するハンドルの長さを調整する
	/// </summary>
	/// <param name="next">一つ先のアンカー</param>
	/// <returns></returns>
	public void AdjustNextHandleRange(Anker next)
	{
		//!< ハンドルの影響値は自分の座標が2、次のアンカー座標が1
		nextHandle.transform.localPosition = new Vector3(0, 0, Vector3.Distance(AnkerPos, next.AnkerPos) / 3);
	}
	/// <summary>
	/// 前のアンカーに影響するハンドルの長さを調整する
	/// </summary>
	/// <param name="prev">一つ前のアンカー</param>
	/// <returns></returns>
	public void AdjustPrevHandleRange(Anker prev)
	{
		//!< ハンドルの影響値は自分の座標が2、次のアンカー座標が1
		prevHandle.transform.localPosition = new Vector3(0, 0, -Vector3.Distance(AnkerPos, prev.AnkerPos) / 3);
	}
	//内部メソッド───────────────────────────────────────────────────────────────
	void Update()
	{
		gameObject.hideFlags = HideFlags.None;


		if (NextHandlePos != prevNextPosition)
		{
			prevHandle.transform.localPosition = VersusPosition(nextHandle.transform.localPosition, prevHandle.transform.localPosition);
		}
		if (PrevHandlePos != prevPrevPosition)
		{
			nextHandle.transform.localPosition = VersusPosition(prevHandle.transform.localPosition, nextHandle.transform.localPosition);
		}

		//!< LineRendererのポジションを更新
		UpdateLinePosition();

		prevPrevPosition = PrevHandlePos;
		prevNextPosition = NextHandlePos;
	}
	//ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
	/// <summary>
	/// LineRendererのポジションを更新する
	/// </summary>
	/// <returns></returns>
	void UpdateLinePosition()
	{
		lineRenderer.SetPosition(0, PrevHandlePos);
		lineRenderer.SetPosition(1, transform.position);
		lineRenderer.SetPosition(2, NextHandlePos);
	}
	//──────────────────────────────────────────────────────────────────────────
}
