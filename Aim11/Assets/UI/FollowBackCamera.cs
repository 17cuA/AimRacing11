using System.Collections;
using System;
using UnityEngine;


namespace FollowBackCamera
{
    [System.Serializable]
    public struct OffsetInfo
    {
        public bool lookAt;
        public float smooth;
        public Vector3 offsetPos;
        public Vector3 offsetAngle;
        public float angleSmooth;
        public float cameraViewAngle;
    }
    [System.Serializable]
    public struct RayInfo
    {
        public bool isPreventionNodding;
        public float distance;
        public float radius;
        public LayerMask hitLayer;
    }


    public class FollowBackCamera : MonoBehaviour
    {
        Transform target;
        Camera cam;
		GameObject player;

        Vector3 OffsetPosition
        {
            get
            {
                return new Vector3(
                    offsetInfo.offsetPos.x * target.localScale.x,
                    offsetInfo.offsetPos.y * target.localScale.y,
                    offsetInfo.offsetPos.z * target.localScale.z
                    );
            }
        }

        public OffsetInfo offsetInfo;
        public RayInfo rayInfo;
        public bool LookAt { get; set; }
        public bool Active { get; set; }

        //int nowCameraNumber = 0;
        Vector3 offsetPos;
        Vector3 smothVelocity;
        Vector3 targetOffsetPos;
        RaycastHit raycastHit;

        private bool didSetTarget = false;

        //targetの設定
        private void SetTarget(Transform _target)
        {
            try
            {
                target = _target;
                didSetTarget = true;
            }
            catch(NullReferenceException ex)
            {
                Debug.Log("target was not set");
                didSetTarget = false;
            }
            
        }

        private void Start()
        {
			player = GameObject.Find("test2");
            cam = GetComponent<Camera>();
			SetTarget(player.transform);
            Active = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, rayInfo.radius);
        }

        private void Update()
        {
            if (LookAt) { transform.LookAt(target); }
            if (!didSetTarget || !Active) { return; }

            //カメラ位置座標更新
            transform.position = target.position + offsetPos;

            //カメラ回転座標更新
            if (offsetInfo.lookAt)
            {
                //対象に向く
                transform.LookAt(target.position);
            }
            else
            {
                Vector3 angle;
                //Memo　: Transform.localEulerAnglesはInspector上のRotationを取得する関数
                //Transform.rotationで値を取得すると返り値はQuaternionであるためVector3には代入できない
                angle.x = Mathf.Lerp(transform.localEulerAngles.x, target.localEulerAngles.x + offsetInfo.offsetAngle.x, offsetInfo.angleSmooth);
                angle.y = Mathf.Lerp(transform.localEulerAngles.y, target.localEulerAngles.y + offsetInfo.offsetAngle.y, offsetInfo.angleSmooth);
                angle.z = Mathf.Lerp(transform.localEulerAngles.z, target.localEulerAngles.z + offsetInfo.offsetAngle.z, offsetInfo.angleSmooth);

                transform.localEulerAngles = angle + Vector3.zero;
            }
            //カメラの視野
            cam.fieldOfView = offsetInfo.cameraViewAngle;

            //カメラターゲットからカメラまでレイを飛ばし壁にさえぎられたらカメラを近づけめり込まないようにする。
            if (rayInfo.isPreventionNodding)
            {
                Vector3 direction = Vector3.Normalize(transform.position - target.position);

                //球形のレイを飛ばして接触判定を行う
                Physics.SphereCast(
                    /*開始地点*/target.position,
                    /*　半径　*/rayInfo.radius,
                    /*　方向　*/direction,
                    /*接触した相手の情報*/
                                out raycastHit,
                    /*　長さ　*/rayInfo.distance,
                    /*レイヤー*/rayInfo.hitLayer);
                Debug.DrawRay(target.position, direction, Color.green);

                if (raycastHit.collider != null)
                {
                    Debug.DrawRay(target.position, direction, Color.red);
                    transform.position = raycastHit.point;
                }
            }
        }

        void FixedUpdate()
        {
            targetOffsetPos = target.rotation * OffsetPosition;

            //オフセット位置の更新
            offsetPos = Vector3.SmoothDamp(offsetPos, targetOffsetPos, ref smothVelocity, offsetInfo.smooth).normalized
                            * OffsetPosition.magnitude;
        }
    }
}
