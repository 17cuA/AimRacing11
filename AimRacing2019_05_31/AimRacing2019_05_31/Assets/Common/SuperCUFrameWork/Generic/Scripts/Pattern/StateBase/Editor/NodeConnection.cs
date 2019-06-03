using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeConnection
{
	public NodeConnectionPoint inPoint;//終点
	public NodeConnectionPoint outPoint;//始点
	public Action<NodeConnection> OnClickRemoveConnection;

	//初期化
	public NodeConnection(NodeConnectionPoint inPoint, NodeConnectionPoint outPoint, Action<NodeConnection> onClickRemove)
	{
		this.inPoint = inPoint;
		this.outPoint = outPoint;
		OnClickRemoveConnection = onClickRemove;
	}

	//描画
	public void Draw()
	{
        //ベジェ曲線の描画（今は直線）
        Handles.DrawBezier(
            inPoint.rect.center,//終点
            outPoint.rect.center, //始点
            inPoint.rect.center + new Vector2(-150, 0),
            outPoint.rect.center + new Vector2(150, 0),
            new Color(1, 1, 1, 0.5f), //色
            null, //テクスチャ
            4f//太さ
            );
		//真ん中の矩形ハンドル
		if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
		{
			if (OnClickRemoveConnection != null)
			{
				OnClickRemoveConnection(this);
			}
		}
	}
}
