//===================================================
// ファイル名	：CameraControl.cs
// 概要			：カメラのスクリプト
// 作成者		：謝 敬鴻
// 作成日		：2018.11.30
// 
//---------------------------------------------------
// 更新履歴：
// 2018/11/30 [謝 敬鴻] スクリプト作成
// 2018/11/30 [謝 敬鴻] 三人称視点作成
// 2018/01/10 [杉山 雅哉] 一人称視点追加
// 2018/01/22 [杉山 雅哉] 固定三人称視点追加
// 2018/01/30 [杉山 雅哉] 視点切り替え処理修正
// 2018/01/30 [杉山 雅哉] コメント追加
// 2019/04/25 [謝 敬鴻]  車に追跡視点(SmoothFollowView())
//===================================================
using System.Collections;
using UnityEngine;

namespace AIM
{
	public class CameraControl : MonoBehaviour
	{
		public enum CameraStatus
		{
			FIRSTPERSONVIEW,
			SMOOTHFOLLOWVIEW,

		}

		//カメラの移動上限値
		[SerializeField]
		private const float Y_ANGLE_MIN = 0.0f;
		private const float Y_ANGLE_MAX = 50.0f;

		[SerializeField]
		private CameraStatus cameraStatus = CameraStatus.SMOOTHFOLLOWVIEW;
		[SerializeField]
		private Transform TargetObject;
		[SerializeField]
		private Transform camTransform;
		//視点切り替え時のカメラの位置
		[SerializeField]
		private Transform farstPersonViewObject = null;
		[SerializeField]
		private Transform smoothFollowViewObject = null;
		[SerializeField]
		private Camera Camera;

		// Use this for initialization
		private void Start()
		{
			Camera = GetComponent<Camera>();
			camTransform = transform;
			//nowfov = FieldOfView;
			//nowPos = TargetObject.transform.position;
		}

		//LateUpdateだとVector3.Laepでがたつきが発生するため、FixedUpdateに変更
		private void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.O))
			{
				if (cameraStatus == CameraStatus.SMOOTHFOLLOWVIEW)
				{
					cameraStatus = CameraStatus.FIRSTPERSONVIEW;
					transform.position = farstPersonViewObject.position;
				}
				else
				{
					cameraStatus++;
					transform.position = smoothFollowViewObject.position;
				}
			}
			if (!farstPersonViewObject || !smoothFollowViewObject)
			{
				Debug.Log("targetにPlayerが設定されていません。");
				return;
			}
			switch (cameraStatus)
			{
				case CameraStatus.FIRSTPERSONVIEW:
					TargetObject = farstPersonViewObject;
					FirstPersonView();
					break;
				case CameraStatus.SMOOTHFOLLOWVIEW:
					TargetObject = smoothFollowViewObject;
					SmoothFollowView();
					break;
			}
		}

		//マウスのポジションに向かってカメラの向きが動く
		//private void MouseView()
		//{
		//	currentX += Input.GetAxis("Mouse X");
		//	currentY += Input.GetAxis("Mouse Y");

		//	currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);

		//	Vector3 dir = new Vector3(0, 0, -distance);
		//	Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
		//	camTransform.position = target.position + rotation * dir;
		//	camTransform.LookAt(target.position);
		//}

		//一人称視点カメラ
		private void FirstPersonView()
		{
			float distance = 0.0f;  //車との距離
			float height = 0.0f;    //カメラの高さ

			float attenRate = 30.0f;//減衰率

			float FoVAttenRate = 3.0f;//FoV減衰率
			float movedFoV = 65.0f; //プレイヤーが移動しているときのFoV
			float stopedFoV = 50.0f;//プレイヤーが止まっているときのFoV

			Vector3 prevTargetPos = TargetObject.position;  //前フレームのターゲットの位置

			//減衰処理
			Vector3 pos = TargetObject.position + new Vector3(0.0f, height, distance);
			transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * attenRate);
			transform.rotation = Quaternion.Slerp(transform.rotation, TargetObject.transform.rotation, Time.deltaTime * 3.0f);

			//FoV処理
			bool moved = TargetObject.position != prevTargetPos;
			prevTargetPos = TargetObject.position;

			float fov = moved ? movedFoV : stopedFoV;
			Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, fov, Time.deltaTime * FoVAttenRate);
		}

		//三人称視点カメラ
		private void SmoothFollowView()
		{
			float distance = 0.0f;  //車との距離
			float height = 0.0f;    //カメラの高さ

			float attenRate = 5.0f;//減衰率

			float FoVAttenRate = 3.0f;//FoV減衰率
			float movedFoV = 65.0f; //プレイヤーが移動しているときのFoV
			float stopedFoV = 50.0f;//プレイヤーが止まっているときのFoV

			Vector3 prevTargetPos = TargetObject.position;  //前フレームのターゲットの位置

			//減衰処理
			Vector3 pos = TargetObject.position + new Vector3(0.0f, height, distance);
			transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * attenRate);
			transform.rotation = Quaternion.Slerp(transform.rotation, TargetObject.transform.rotation, Time.deltaTime * 2.0f);

			//FoV処理
			bool moved = TargetObject.position != prevTargetPos;
			prevTargetPos = TargetObject.position;

			float fov = moved ? movedFoV : stopedFoV;
			Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, fov, Time.deltaTime * FoVAttenRate);
		}
	}
}
