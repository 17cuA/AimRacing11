//===================================================
// ファイル名	：CarController.cs
// 概要			：車の制御スクリプト
// 作成者		：謝 敬鴻
// 作成日		：2018.11.30
//===================================================
using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;


namespace AIM
{
	//ホイール格納クラス
	[Serializable]
	public class Wheel
	{
		public WheelCollider wheelCollider;
		public Transform wheelTransform;
		public Transform caliperTransform;
		public bool steer = false;
		public bool accele = false;
		public bool brake = true;
		public bool handbrake = false;
	}

	//ホイールのデータ
	public class WheelData
	{
		public Wheel wheel;
		public WheelCollider collider;      //ホイールコライダーコンポーネント格納用
		public Transform transform;         //ホイールコライダーの位置
		public Vector3 wheelCenter;         //ホイールコライダーの中心
		public float forceDistance;
		public float steerAngle = 0.0f;     //曲がる角度

		//ホイール地面接触と摩擦
		public bool grounded = false;
		public WheelHit hit;
		public GroundPhysicMaterial groundPhysicMaterial = null;

		//サスペンション
		public float suspensionCompression = 0.0f;
		public float downforce = 0.0f;

		public Vector3 velocity = Vector3.zero;         //速度
		public Vector2 localVelocity = Vector2.zero;    //ローカル速度
		public Vector2 localRigForce = Vector2.zero;

		public bool isBraking = false;      //ブレーキフラグ
		public float finalInput = 0.0f;
		public Vector2 tireSlip = Vector2.zero;			//ホイール滑る係数
		public Vector2 tireForce = Vector2.zero;		//ホイールの力
		public Vector2 dragForce = Vector2.zero;		//ホイール摩擦抵抗
		public Vector2 rawTireForce = Vector2.zero;

		public float angularVelocity = 0.0f;		//曲がる力
		public float angularPosition = 0.0f;        //曲がる位置

		public PhysicMaterial lastPhysicMaterial = new PhysicMaterial();	//摩擦マテリアルと交換用
		public RaycastHit rayHit;               //精密な位置

		public float positionRatio = 0.0f;
		public bool isWheelChildOfCaliper = false;

		public float combinedTireSlip = 0.0f;
		public float downforceRatio = 0.0f;
	}


	[RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
		public Wheel[] wheels = new Wheel[0];
		
		struct CarFrame
		{
			public float frontPosition;
			public float rearPosition;
			public float baseHeight;
			public float frontWidth;
			public float rearWidth;
			public float middlePoint;
		}


		[Header("Motor")]

		public float maxAcceleForce = 5000.0f;

		[Range(0.0001f, 0.9999f)]
		public float forceCurveShape = 0.435f;
		public float maxAcceleSlip = 2.0f;
		public float driveForceToMaxSlip = 3500.0f;

		[Header("Brakes")]

		public float maxBrakeForce = 4500.0f;
		public float brakeForceToMaxSlip = 2000.0f;
		
		public enum BrakeMode { Slip, Ratio };
		public BrakeMode brakeMode = BrakeMode.Slip;
		public float maxBrakeSlip = 2.0f;
		[Range(0,1)]
		public float maxBrakeRatio = 0.5f;

		public BrakeMode handBrakeMode = BrakeMode.Slip;
		public float maxHandBrakeSlip = 3.0f;
		[Range(0,1)]
		public float maxHandBrakeRatio = 0.481f;

	
		[Header("質量の中心")]

		[Range(0.1f, 0.9f)]
		public float centerOfMassPosition = 0.5f;
		[Range(-1.0f, 1.0f)]
		public float centerOfMassHeightOffset = 0.0f;
		[FormerlySerializedAs("centerOfMass")]
		public Transform centerOfMassTransform;

		//車の設定
		public float maxSpeedForward = 60f;
		public float maxSpeedReverse = 12.0f;
		[Range(0,80)]
		public float maxSteerAngle = 45.0f;
		[Range(0,3)]
		public float tireFriction = 1.53f;
		[Range(0,1)]
		public float antiRoll = 0.2f;
		[Range(0,1)]
		public float rollingResistance = 0.05f;	//抵抗力
		[Range(0,4)]
		public float aeroDrag = 0.0f;
		[Range(0,4)]
		public float aeroDownForce = 1.0f;


		//調整用
		[Range(0,1)]
		public float brakeBalance = 0.5f;
		[Range(0,1)]
		public float acceleBalance = 0.5f;
		[Range(0.3f,1.0f)]
		public float tireFrictionBalance = 0.5f;
		[Range(0,1)]
		public float handlingBias = 0.5f;
		[Range(0,1)]
		public float aeroBalance = 0.5f;

		[Header("補助システム")]


		public bool  tcEnabled = false;     //トラクションコントロール
		[Range(-1, 1)]
		public float tcRatio = 1.0f;

		public bool brakeAssist = false;			//Anti Lock Brake
		[Range(-1, 1)]
		public float brakeAssistRatio = 1.0f;

        public bool espEnabled = false;     //横滑り防止装置(Electronic Stability Program)
        [Range(-1, 1)]
        public float espRatio = 0.5f;

        public bool steeringAssist = false;
        [Range(0, 1)]
        public float steeringAssistRatio = 0.5f;

        //コントロール
        [Range(-1, 1)]
		public float steerInput = 0.0f;
		[Range(-1, 1)]
		public float acceleInput = 0.0f;
		[Range(-1, 1)]
		public float brakeInput = 0.0f;
		[Range(-1, 1)]
		public float handbrakeInput = 0.0f;


		public bool steerAngleCorrect = true;


		[NonSerialized] public bool calculExtendedTireData = false;
		[NonSerialized] public float impactThreeshold = 0.6f;
        [NonSerialized] public float impactMinSpeed = 2.0f;

        WheelData[] m_wheelData = new WheelData[0];
		float m_speed;
        float m_speedAngle;
        float m_steerAngle;
		bool m_usesHandbrake = false;
		
		
		Transform m_transform;
		Rigidbody m_rigidbody;
		Rigidbody m_referenceBody = null;
		Rigidbody m_referenceCand = null;

		PhysicMaterialManager m_physicMaterialManager;
		int m_referenceCandCount = 0;

		[Range(0,0.5f)]
		public float sleepVelocity = 0.2f;

		public float defaultGroundGrip = 1.0f;
		public float defaultGroundDrag = 1.0f;



		public Transform cachTransform { get { return m_transform; } }
		public Rigidbody cachRigidbody { get { return m_rigidbody; } }
		public WheelData[] wheelData { get { return m_wheelData; } }
		public float speed { get { return m_speed; } }
		public float speedAngle { get { return m_speedAngle; } }
		public float steerAngle { get { return m_steerAngle; } }

		public bool invertVisualWheelSpinDirection { get; set; }

		//デバッグ
		public bool disallowCenterOfMass = false;

		CarFrame carFrame;

		Tools.BiasLerpContext m_forceBiasCtx = new Tools.BiasLerpContext();

		WheelFrictionCurve m_colliderFriction = new WheelFrictionCurve();

		Collider[] colliders = new Collider[0];
		int[] colLayers = new int[0];

		//アッカーマン
		Vector3 t1;
		Vector3 t2;
		Vector3 l1;
		Vector3 l2;
		float AckerL;
		float AckerT;

        void OnValidate()
        {
            maxAcceleSlip = Mathf.Max(maxAcceleSlip, 0.0f);
            maxBrakeSlip = Mathf.Max(maxBrakeSlip, 0.0f);
            maxHandBrakeSlip = Mathf.Max(maxHandBrakeSlip, 0.0f);

            maxAcceleForce = Mathf.Max(maxAcceleForce, 0.0f);
            maxBrakeForce = Mathf.Max(maxBrakeForce, 0.0f);
            driveForceToMaxSlip = Mathf.Max(driveForceToMaxSlip, 1.0f);
            brakeForceToMaxSlip = Mathf.Max(brakeForceToMaxSlip, 1.0f);
            maxSpeedForward = Mathf.Max(maxSpeedForward, 0.0f);
            maxSpeedReverse = Mathf.Max(maxSpeedReverse, 0.0f);

            aeroDrag = Mathf.Max(aeroDrag, 0.0f);
        }


        void OnEnable()
		{
			m_transform = GetComponent<Transform>();
			m_rigidbody = GetComponent<Rigidbody>();
			m_physicMaterialManager = FindObjectOfType<PhysicMaterialManager>();
			FindColliders();

			m_rigidbody.maxAngularVelocity = 14.0f;
			m_rigidbody.maxDepenetrationVelocity = 8.0f;

			//ホイールがない場合
			if (wheels.Length == 0)
			{
				Debug.LogWarning("タイヤオブジェクトがない");
				enabled = false;
				return;
			}

			carFrame = CalculCarFrame();
			CenterOfMass();

			m_usesHandbrake = false;

			m_wheelData = new WheelData[wheels.Length];
			for (int i = 0; i < m_wheelData.Length; i++)
			{
				Wheel w = wheels[i];
				WheelData wd = new WheelData();

				if (w.wheelCollider == null)
				{
					Debug.LogError("ホイールコライダーが失われた" + gameObject.name);
					enabled = false;
					return;
				}

				if (w.caliperTransform != null && w.wheelTransform != null && w.caliperTransform.IsChildOf(w.wheelTransform))
				{
					Debug.LogWarning(this.ToString() + "ディスクブレーキ (" + w.caliperTransform.name + ")は子オブジェクトに入れていない");
				}

				wd.isWheelChildOfCaliper = w.caliperTransform != null && w.wheelTransform != null &&w.wheelTransform.IsChildOf(w.caliperTransform);

				wd.collider = w.wheelCollider;
				wd.transform = w.wheelCollider.transform;
				if (w.handbrake) m_usesHandbrake = true;

				UpdateWheelCollider(wd.collider);
				wd.forceDistance = GetWheelForceDistance(wd.collider);

				float zPos = m_transform.InverseTransformPoint(wd.transform.TransformPoint(wd.collider.center)).z;
				wd.positionRatio = zPos >= carFrame.middlePoint? 1.0f : 0.0f;

				wd.wheel = w;
				m_wheelData[i] = wd;
			}

			foreach (Wheel wheel in wheels)
			{
				m_colliderFriction.stiffness = 0.0f;
				wheel.wheelCollider.sidewaysFriction = m_colliderFriction;
				wheel.wheelCollider.forwardFriction = m_colliderFriction;
				wheel.wheelCollider.motorTorque = 0.00001f;

				UpdateWheelCollider(wheel.wheelCollider);

				wheel.wheelCollider.ConfigureVehicleSubsteps(1000.0f, 1, 1);
			}

			m_lastImpactedMaterial = new PhysicMaterial();

			CalculAckerLT();
		}


		void FixedUpdate()
		{
			//質量は常に更新
			CenterOfMass();

			//入力
			acceleInput = Mathf.Clamp (acceleInput, -1.0f, +1.0f);
			brakeInput = Mathf.Clamp01(brakeInput);
			handbrakeInput = Mathf.Clamp01(handbrakeInput);

			//速度計算
			if (m_referenceCandCount > m_wheelData.Length / 2)
			{
				m_referenceBody = m_referenceCand;
			}

			Vector3 currentVelocity = m_rigidbody.velocity;
			if (m_referenceBody != null)
			{
				currentVelocity -= m_referenceBody.velocity;
			}

			m_speed = Vector3.Dot(currentVelocity, m_transform.forward);
			m_speedAngle = Vector3.Angle(currentVelocity, m_transform.forward) * Mathf.Sign(Vector3.Dot(currentVelocity, m_transform.right));

			//共通データ
			float referenceDownforce = calculExtendedTireData? (m_rigidbody.mass * Physics.gravity.magnitude) / m_wheelData.Length : 1.0f;

			//ホイール
			int groundedWheels = 0;
			m_referenceCandCount = 0;

			
			

			//ホイール更新
			foreach (WheelData wd in m_wheelData)
			{
				CalculSteerAngle(wd);
				UpdateWheelCollider(wd.collider);

				Steering(wd,wd.steerAngle);

				Suspension(wd);
				LocalFrame(wd);
				UpdateGroundMaterial(wd);

				CalculTireForces(wd);
				UpdateWheelS(wd);

				UpdateTransform(wd, Time.deltaTime);
				if (wd.grounded) groundedWheels++;
			}

			//空気力学
			float sqrVelocity = m_rigidbody.velocity.sqrMagnitude;
			Vector3 normalizedVelocity = m_rigidbody.velocity.normalized;
			float forwardVelocityFactor = Vector3.Dot(m_transform.forward, normalizedVelocity);

			Vector3 dragForce = -aeroDrag * sqrVelocity * normalizedVelocity;
			Vector3 loadForce = -aeroDownForce * sqrVelocity * forwardVelocityFactor * m_transform.up;

			Vector3 aeroAppPoint = m_transform.TransformPoint(new Vector3(0.0f,m_rigidbody.centerOfMass.y, Mathf.Lerp(carFrame.rearPosition, carFrame.frontPosition, aeroBalance)));

			m_rigidbody.AddForceAtPosition(dragForce, aeroAppPoint);
			if (groundedWheels > 0) m_rigidbody.AddForceAtPosition(loadForce, aeroAppPoint);
		}

		//================================================================================================
		// 車のリセット関数
		// 引数：なし
		//================================================================================================
		public void ResetCar()
		{
			Vector3 angles = transform.localEulerAngles;
			m_rigidbody.MoveRotation(Quaternion.Euler(0, angles.y, 0));
			m_rigidbody.MovePosition(m_rigidbody.position + Vector3.up * 1.6f);
			m_rigidbody.velocity = Vector3.zero;
			m_rigidbody.angularVelocity = Vector3.zero;
		}

		//================================================================================================
		// 位置更新関数
		// 引数：ホイールコライダー、
		//================================================================================================
		void UpdateTransform (WheelData wd, float deltaTime)
		{
			if (wd.wheel.wheelTransform != null || wd.wheel.caliperTransform != null)
			{
				//if (!wd.collider.enabled || wd.collider.gameObject.activeInHierarchy)
				//{
				//	//ホイールは生成していない際、オブジェクトを非アクティブ
				//	if (wd.wheel.wheelTransform) wd.wheel.wheelTransform.gameObject.SetActive(false);
				//	if (wd.wheel.caliperTransform) wd.wheel.caliperTransform.gameObject.SetActive(false);
				//	return;
				//}

				//ホイール回転
				float deltaPos = wd.angularVelocity * deltaTime;
				if (invertVisualWheelSpinDirection) deltaPos = -deltaPos;

				wd.angularPosition = (wd.angularPosition + deltaPos) % (Mathf.PI * 2.0f);

				//ホイールの位置
				float suspen;

				bool collided = Physics.Raycast(wd.wheelCenter, -wd.transform.up, out wd.rayHit, (wd.collider.suspensionDistance + wd.collider.radius), Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
				
				if (collided)
				{
					if (Physics.GetIgnoreLayerCollision(wd.collider.gameObject.layer, wd.rayHit.collider.gameObject.layer))
					{
						suspen = wd.collider.suspensionDistance * (1.0f - wd.suspensionCompression) + wd.collider.radius * 0.05f;
						wd.rayHit.point = wd.hit.point;
						wd.rayHit.normal = wd.hit.normal;
					}
					else
					{
						suspen = wd.rayHit.distance - wd.collider.radius * 0.95f;
					}
				}
				else
				{
					suspen = wd.collider.suspensionDistance + wd.collider.radius * 0.05f;
				}

				Vector3 wheelPosition = wd.transform.position - wd.transform.up * suspen;

				//ディスクブレーキ
				if (wd.wheel.caliperTransform != null)
				{
					wd.wheel.caliperTransform.gameObject.SetActive(true);

					wd.wheel.caliperTransform.position = wheelPosition;

					//方向
					wd.wheel.caliperTransform.rotation = wd.transform.rotation * Quaternion.Euler(0.0f, wd.steerAngle, 0.0f);
				}

				if (wd.wheel.wheelTransform != null)
				{
					wd.wheel.wheelTransform.gameObject.SetActive(true);

					if (wd.isWheelChildOfCaliper)
					{
						wd.wheel.wheelTransform.localRotation = Quaternion.Euler(wd.angularPosition * Mathf.Rad2Deg, 0.0f, 0.0f);
					}
					else
					{
						wd.wheel.wheelTransform.position = wheelPosition;
						wd.wheel.wheelTransform.rotation = wd.transform.rotation * Quaternion.Euler(wd.angularPosition * Mathf.Rad2Deg, wd.steerAngle, 0.0f);
					}
				}

			}
			else
			{
				wd.rayHit.point = wd.hit.point;
				wd.rayHit.normal = wd.hit.normal;
			}
		}

		//================================================================================================
		// ホイール更新関数
		// 引数：ホイールコライダー
		//================================================================================================
		void UpdateWheelCollider (WheelCollider col)
		{
			if (!col.enabled) return;

			JointSpring suspension = col.suspensionSpring;
			float sprungForce = -col.sprungMass * Physics.gravity.y;
			float pos = sprungForce / suspension.spring;
			suspension.targetPosition = Mathf.Clamp01(pos / col.suspensionDistance);

			float minSpringRate = sprungForce / col.suspensionDistance;
			if (suspension.spring < minSpringRate)
			{
				suspension.spring = minSpringRate;
			}
			col.suspensionSpring = suspension;
		}

		//================================================================================================
		// 曲がるときの計算とサポート関数
		//================================================================================================
		void CalculSteerAngle(WheelData wd)
		{
			float inputSteerAngle = maxSteerAngle * steerInput;
			float speedFactor = Mathf.InverseLerp(0.1f, 3.0f, speed);
			float ackermannAngle;

			if (steerInput < 0)
			{
				if (wd.wheel.wheelTransform.localPosition.x < 0)
					ackermannAngle = Ackermann(true);
				else
					ackermannAngle = Ackermann(false);
			}
			else
			{
				if (wd.wheel.wheelTransform.localPosition.x < 0)
					ackermannAngle = Ackermann(false);
				else
					ackermannAngle = Ackermann(true);
			}

			inputSteerAngle = ackermannAngle;

            // ESPの参考資料
            // http://www.mate.tue.nl/mate/pdfs/9985.pdf
            // http://www.hightechwave.com/ver04/data/esp/inex.html
            if (espEnabled)
            {
                float forwardSpeed = speed * espRatio * speedFactor;
                float maxEspAngle = Mathf.Asin(Mathf.Clamp01(3.0f / forwardSpeed)) * Mathf.Rad2Deg;
                float steerAngleLimit = Mathf.Min(maxSteerAngle, Mathf.Max(maxEspAngle, Mathf.Abs(m_speedAngle)));
                inputSteerAngle = Mathf.Clamp(inputSteerAngle, -steerAngleLimit, +steerAngleLimit);
            }

            float assistedSteerAngle = 0.0f;
            if(steeringAssist)
            {
                assistedSteerAngle = speedAngle * steeringAssistRatio * speedFactor * Mathf.InverseLerp(2.0f, 3.0f, Mathf.Abs(speedAngle));
            }

            m_steerAngle = Mathf.Clamp(inputSteerAngle + assistedSteerAngle, -maxSteerAngle, +maxSteerAngle);
		}

		

		float Ackermann(bool IN)
		{
			float AckerAngle = maxSteerAngle * steerInput;

			//アッカーマン計算式
			if (IN)
				return AckerAngle;
			if(steerInput < 0)
			{
				//Debug.Log("アッカーマン" + -Mathf.Atan(1f / (AckerT / AckerL + 1f / Mathf.Tan(-AckerAngle * Mathf.Deg2Rad))) * Mathf.Rad2Deg);
				return -Mathf.Atan(1f / (AckerT / AckerL + 1f / Mathf.Tan(-AckerAngle * Mathf.Deg2Rad))) * Mathf.Rad2Deg;
			}
			else
			{
				//Debug.Log("アッカーマン" + Mathf.Atan(1f / (AckerT / AckerL + 1f / Mathf.Tan(AckerAngle * Mathf.Deg2Rad)))* Mathf.Rad2Deg);
				return Mathf.Atan(1f / (AckerT / AckerL + 1f / Mathf.Tan(AckerAngle * Mathf.Deg2Rad))) * Mathf.Rad2Deg;
			}
		}

		void CalculAckerLT()
		{
			foreach (Wheel w in wheels)
			{
				if (w.wheelCollider != null)
				{
					//前後
					if (w.wheelTransform.localPosition.z > 0)
						l1 = w.wheelCollider.transform.TransformPoint(w.wheelCollider.center);
					else
						l2 = w.wheelCollider.transform.TransformPoint(w.wheelCollider.center);

					//左右
					if (w.wheelTransform.localPosition.x < 0)
						t1 = w.wheelCollider.transform.TransformPoint(w.wheelCollider.center);
					else
						t2 = w.wheelCollider.transform.TransformPoint(w.wheelCollider.center);
				}
			}

			AckerL = Vector3.Distance(l1, l2);
			AckerT = Vector3.Distance(t1, t2);
		}

        //================================================================================================
        // 曲がる処理の関数
        // 引数：ホイールのパラメータ
        //================================================================================================
        void Steering (WheelData wd, float inputSteerAngle)
        {
            if (wd.wheel.steer)
            {
                wd.steerAngle = m_steerAngle;
                if (wd.positionRatio < 0.5f)
                    wd.steerAngle = -wd.steerAngle;
            }
            else
            {
                wd.steerAngle = 0.0f;
            }

			
            if (steerAngleCorrect)
			{
				Quaternion steerRot = Quaternion.AngleAxis(inputSteerAngle, wd.transform.up);
				Vector3 wheelForward = steerRot * wd.transform.forward;

				Vector3 rbWheelForward = wheelForward - Vector3.Project(wheelForward, m_transform.up);

				wd.collider.steerAngle = Vector3.Angle(m_transform.forward, rbWheelForward) * Mathf.Sign(Vector3.Dot(m_transform.right, rbWheelForward));
			}
			else
			{
				wd.collider.steerAngle = wd.steerAngle;
			}
        }

		//================================================================================================
        // サスペンション関数
        // 引数：ホイールのデータ
        //================================================================================================
		void Suspension (WheelData wd)
		{
			wd.grounded = wd.collider.GetGroundHit(out wd.hit);
			wd.wheelCenter = wd.transform.TransformPoint(wd.collider.center);
			wd.hit.point += m_rigidbody.velocity * Time.deltaTime;

			//サスペンションの圧縮と下圧
			if(wd.grounded)
			{
				wd.suspensionCompression = 1.0f - (-wd.transform.InverseTransformPoint(wd.hit.point).y - wd.collider.radius) / wd.collider.suspensionDistance;
				if(wd.hit.force < 0.0f)
				{
					wd.hit.force = 0.0f;
				}
				
				wd.downforce = wd.hit.force;
			}
			else
			{
				wd.suspensionCompression = 0.0f;
				wd.downforce = 0.0f;
			}
		}
		//================================================================================================
        // ホイールの力を計算関数
        // 引数：ホイールのデータ
        //================================================================================================
		void CalculTireForces(WheelData wd)
		{
			//アクセル
			float wheelAcceleInput = wd.wheel.accele? acceleInput : 0.0f;
			float wheelMaxDriveSlip = maxAcceleSlip;
			
			if(Mathf.Sign(wheelAcceleInput) != Mathf.Sign(wd.localVelocity.y))
			{
				wheelMaxDriveSlip -= wd.localVelocity.y * Mathf.Sign(wheelAcceleInput);
			}

			//ブレーキ・ハンドブレーキ
			float wheelBrakeInput = 0.0f;
			float wheelBrakeRatio = 0.0f;
			float wheelBrakeSlip = 0.0f;

			if (wd.wheel.brake && wd.wheel.handbrake)
			{
				wheelBrakeInput = Mathf.Max(brakeInput, handbrakeInput);
				
				if (handbrakeInput >= brakeInput)
				{
					CalculBrake(wd, handBrakeMode, maxHandBrakeSlip, maxHandBrakeRatio, out wheelBrakeSlip, out wheelBrakeRatio);
				}
				else
				{
					CalculBrake(wd, brakeMode, maxBrakeSlip, maxBrakeRatio, out wheelBrakeSlip, out wheelBrakeRatio);
				}
					
			}
			else if (wd.wheel.brake)
			{
				wheelBrakeInput = brakeInput;
				CalculBrake(wd, brakeMode, maxBrakeSlip, maxBrakeRatio, out wheelBrakeSlip, out wheelBrakeRatio);
			}
			else if (wd.wheel.handbrake)
			{
				wheelBrakeInput = handbrakeInput;
				CalculBrake(wd, handBrakeMode, maxHandBrakeSlip, maxHandBrakeRatio, out wheelBrakeSlip, out wheelBrakeRatio);
			}

			//　アクセル制御
			float absAcceleInput = Mathf.Abs(wheelAcceleInput);
			float combinedInput = -rollingResistance + absAcceleInput * (1.0f + rollingResistance) - wheelBrakeInput * (1.0f - rollingResistance);

			if (combinedInput >= 0)
			{
				wd.finalInput = combinedInput * Mathf.Sign(wheelAcceleInput);
				wd.isBraking = false;
			}
			else
			{
				wd.finalInput = -combinedInput;
				wd.isBraking = true;
			}

			float requireForce;

			if (wd.isBraking)
			{
				requireForce = wd.finalInput * GetBalance(maxBrakeForce, brakeBalance, wd.positionRatio);
			}
			else
			{
				float balanceDriveForce = GetBalance(maxAcceleForce, acceleBalance, wd.positionRatio);
				requireForce = CalculAcceleForce(wd.finalInput * balanceDriveForce, balanceDriveForce, wd.grounded);
			}

			// ABS、TC 制御
			if (wd.grounded)
			{
				if (tcEnabled)
					wheelMaxDriveSlip = Mathf.Lerp(wheelMaxDriveSlip, 0.1f, tcRatio);

				if(brakeAssist && brakeInput > handbrakeInput)
				{
					wheelBrakeSlip = Mathf.Lerp(wheelBrakeSlip, 0.1f, brakeAssistRatio);
					wheelBrakeRatio = Mathf.Lerp(wheelBrakeRatio, wheelBrakeRatio * 0.1f, brakeAssistRatio);
				}
			}

			//タイヤ計算
			if (wd.grounded)
			{
				wd.tireSlip.x = wd.localVelocity.x;
				wd.tireSlip.y = wd.localVelocity.y - wd.angularVelocity * wd.collider.radius;

				//地面の摩擦を取得
				float groundGrip;
				float groundDrag;

				if (wd.groundPhysicMaterial != null)
				{
					groundGrip = wd.groundPhysicMaterial.grip;
					groundDrag = wd.groundPhysicMaterial.drag;
				}
				else
				{
					//デフォルト摩擦
					groundGrip = defaultGroundGrip;
					groundDrag = defaultGroundDrag;
				}

				//結果計算
				float balancedFriction = GetBalance(tireFriction, tireFrictionBalance, wd.positionRatio);
				float forceMagnitude = balancedFriction * wd.downforce * groundGrip;

				float minSlipY;
				
				if (wd.isBraking)
				{
					float wheelMaxBrakeSlip = Mathf.Max(Mathf.Abs(wd.localVelocity.y * wheelBrakeRatio), wheelBrakeSlip);
					minSlipY = Mathf.Clamp(Mathf.Abs(requireForce * wd.tireSlip.x) / forceMagnitude, 0.0f, wheelMaxBrakeSlip);
				}
				else
				{
					minSlipY = Mathf.Min(Mathf.Abs(requireForce * wd.tireSlip.x) / forceMagnitude, wheelMaxDriveSlip);
					if (requireForce != 0.0f && minSlipY < 0.1f) minSlipY = 0.1f;
				}

				if (Mathf.Abs(wd.tireSlip.y) < minSlipY)
					wd.tireSlip.y = minSlipY * Mathf.Sign(wd.tireSlip.y);

				//タイヤの力の結合
				wd.rawTireForce = -forceMagnitude * wd.tireSlip.normalized;
				wd.rawTireForce.x = Mathf.Abs(wd.rawTireForce.x);
				wd.rawTireForce.y = Mathf.Abs(wd.rawTireForce.y);

				//横の力を代入
				wd.tireForce.x = Mathf.Clamp(wd.localRigForce.x, -wd.rawTireForce.x, +wd.rawTireForce.x);

				//縦の力
				if (wd.isBraking)
				{
					float maxForceY = Mathf.Min(wd.rawTireForce.y, requireForce);
					wd.tireForce.y = Mathf.Clamp(wd.localRigForce.y, -maxForceY, +maxForceY);
				}
				else
				{
					wd.tireForce.y = Mathf.Clamp(requireForce, -wd.rawTireForce.y, +wd.rawTireForce.y);
				}

				wd.dragForce = -(forceMagnitude * wd.localVelocity.magnitude * groundDrag * 0.001f) * wd.localVelocity;
			}
			else
			{
				wd.tireSlip = Vector2.zero;
				wd.tireForce = Vector2.zero;
				wd.dragForce = Vector2.zero;
			}

			float slipToForce = wd.isBraking? brakeForceToMaxSlip : driveForceToMaxSlip;
			float slipRatio = Mathf.Clamp01((Mathf.Abs(requireForce) - Mathf.Abs(wd.tireForce.y)) / slipToForce);

			float slip;
			if (wd.isBraking)
				slip = Mathf.Clamp(-slipRatio * wd.localVelocity.y * wheelBrakeRatio, -wheelBrakeSlip, wheelBrakeSlip);
			else
				slip = slipRatio * wheelMaxDriveSlip * Mathf.Sign(requireForce);

			wd.angularVelocity = (wd.localVelocity.y + slip) / wd.collider.radius;

			//タイヤ更新
			if (wd.grounded)
			{
				Vector3 forwardForce = wd.hit.forwardDir * (wd.tireForce.y + wd.dragForce.y);
				Vector3 sidewaysForce = wd.hit.sidewaysDir * (wd.tireForce.x + wd.dragForce.x);
				Vector3 sidewaysForcePoint = GetSidewaysForcePoint(wd,wd.hit.point);

				m_rigidbody.AddForceAtPosition(forwardForce, wd.hit.point);
				m_rigidbody.AddForceAtPosition(sidewaysForce,sidewaysForcePoint);

				Rigidbody rb = wd.hit.collider.attachedRigidbody;
				if (rb != null && !rb.isKinematic)
				{
					rb.AddForceAtPosition(-forwardForce, wd.hit.point);
					rb.AddForceAtPosition(-sidewaysForce, sidewaysForcePoint);
				}
			}

		}

		//================================================================================================
        // ホイール更新関数
        // 引数：ホイールのデータ
        //================================================================================================
		void UpdateWheelS (WheelData wd)
		{
			if (wd.localVelocity.magnitude < sleepVelocity
				&& Time.time - m_lastStrongImpactTime > 0.2f
				&& (wd.isBraking && wd.rawTireForce.y >= Mathf.Abs(wd.localRigForce.y)
				&& wd.rawTireForce.x >= Mathf.Abs(wd.localRigForce.x)
				|| m_usesHandbrake && handbrakeInput > 0.1f))
			{
				wd.collider.motorTorque = 0.0f;
			}
			else
			{
				wd.collider.motorTorque = 0.00001f;
			}
		}

		//================================================================================================
        // ブレーキ計算関数
        // 引数：ホイールデータ、ブレーキの種類、滑り、比率、ブレーキの滑り、ブレーキの比率
        //================================================================================================
		void CalculBrake (WheelData wd, BrakeMode mode, float maxSlip, float maxRatio, out float brakeSlip, out float brakeRatio)
		{
			if (mode == BrakeMode.Slip)
			{
				brakeSlip = maxSlip;
				brakeRatio = 1.0f;
			}
			else
			{
				brakeSlip = Mathf.Abs(wd.localVelocity.y);
				brakeRatio = maxRatio;
			}
		}

		//================================================================================================
        // 車のフレーム制御関数
        // 引数：なし
        //================================================================================================
		CarFrame CalculCarFrame()
		{
			//中心位置の計算
			float middlePoint = 0.0f;
			int middleCount = 0;
			foreach(Wheel w in wheels)
			{
				if (w.wheelCollider != null)
				{
					middlePoint += transform.InverseTransformPoint(w.wheelCollider.transform.TransformPoint(w.wheelCollider.center)).z;
					middleCount++;
				}
			}

			if (middleCount > 0) middlePoint /= middleCount;

			//横幅と高度の計算
			float frontPos = 0.0f;
			float frontWidth = 0.0f;
			int frontCount = 0;

			float rearPos = 0.0f;
			float rearWidth = 0.0f;
			int rearCount = 0;

			float baseHeight = 0.0f;
			int baseHeightCount = 0;

			foreach (Wheel w in wheels)
			{
				if (w.wheelCollider!=null)
				{
					Vector3 localPos = transform.InverseTransformPoint(w.wheelCollider.transform.TransformPoint(w.wheelCollider.center));

					float wheelPos = localPos.z;
					float axleWidth = Mathf.Abs(localPos.x);

					if(wheelPos >= middlePoint)
					{
						frontPos += wheelPos;
						frontWidth +=axleWidth;
						frontCount++;
					}
					else
					{
						rearPos += wheelPos;
						rearWidth += axleWidth;
						rearCount++;
					}

					baseHeight += localPos.y;
					baseHeightCount++;
				}
			}

			if (frontCount > 0)
			{
				frontPos = frontPos / frontCount;
				frontWidth = frontWidth / frontCount;
			}

			if (rearCount > 0)
			{
				rearPos = rearPos / rearCount;
				rearWidth = rearWidth / rearCount;
			}
			else
			{
				rearPos = frontPos;
				rearWidth = frontWidth;
			}

			if (baseHeightCount > 0)
			{
				baseHeight = baseHeight / baseHeightCount;
			}

			CarFrame frame = new CarFrame();
			frame.frontPosition = frontPos;
			frame.rearPosition = rearPos;
			frame.baseHeight = baseHeight;
			frame.frontWidth = frontWidth;
			frame.rearWidth = rearPos;
			frame.middlePoint = middlePoint;
			return frame;
		}
		
		//================================================================================================
        // 質量の中心関数
        // 引数：なし
        //================================================================================================
		void CenterOfMass()
		{
			Vector3 COM = new Vector3(0.0f, carFrame.baseHeight + centerOfMassHeightOffset, Mathf.Lerp(carFrame.rearPosition, carFrame.frontPosition,centerOfMassPosition));

			if (m_rigidbody.centerOfMass != COM)
				m_rigidbody.centerOfMass = COM;
		}

		//================================================================================================
        // フレーム関数
        // 引数：ホイールのデータ
        //================================================================================================
		void LocalFrame (WheelData wd)
		{
			if (!wd.grounded)
			{
				wd.hit.point = wd.wheelCenter - wd.transform.up * (wd.collider.suspensionDistance + wd.collider.radius);
				wd.hit.normal = wd.transform.up;
				wd.hit.collider = null;
			}

			Vector3 wheelVector = m_rigidbody.GetPointVelocity(wd.hit.point);

			if (wd.hit.collider != null)
			{
				Rigidbody rb = wd.hit.collider.attachedRigidbody;
				if (rb != null)
				{
					wheelVector -=rb.GetPointVelocity(wd.hit.point);
				}

				if (rb != m_referenceBody)
				{
					m_referenceCand = rb;
					m_referenceCandCount++;
				}
			}

			wd.velocity = wheelVector - Vector3.Project(wheelVector, wd.hit.normal);
			wd.localVelocity.y = Vector3.Dot(wd.hit.forwardDir, wd.velocity);
			wd.localVelocity.x = Vector3.Dot(wd.hit.sidewaysDir, wd.velocity);

			if (!wd.grounded)
			{
				wd.localRigForce = Vector2.zero;
				return;
			}

			Vector2 localSurfaceForce;

			float surfaceForceRatio = Mathf.InverseLerp(1.0f, 0.25f, wd.velocity.sqrMagnitude);
			if (surfaceForceRatio > 0.0f)
			{
				Vector3 surfaceForce;

				float upNormal = Vector3.Dot(Vector3.up, wd.hit.normal);
				if (upNormal > 0.000001f)
				{
					Vector3 downForceUp = Vector3.up * wd.hit.force / upNormal;
					surfaceForce = downForceUp - Vector3.Project(downForceUp, wd.hit.normal);
				}
				else
				{
					surfaceForce = Vector3.up * 100000.0f;
				}

				localSurfaceForce.y = Vector3.Dot(wd.hit.forwardDir, surfaceForce);
				localSurfaceForce.x = Vector3.Dot(wd.hit.sidewaysDir, surfaceForce);
				localSurfaceForce *= surfaceForceRatio;
			}
			else
			{
				localSurfaceForce = Vector2.zero;
			}

			float estimatedSprungMass = Mathf.Clamp(wd.hit.force / -Physics.gravity.y, 0.0f, wd.collider.sprungMass) * 0.5f;
			Vector2 localVelocityForce = -estimatedSprungMass * wd.localVelocity / Time.deltaTime;

			wd.localRigForce = localVelocityForce + localSurfaceForce;
		}

		//================================================================================================
        // 摩擦更新関数
        // 引数：ホイールのデータ
        //================================================================================================
		void UpdateGroundMaterial (WheelData wd)
		{
			if (wd.grounded)
				GroundMaterialCached(wd.hit.collider.sharedMaterial, ref wd.lastPhysicMaterial, ref wd.groundPhysicMaterial);
		}

		//================================================================================================
        // 摩擦マテリアル取得関数
        // 引数：ホイールのデータ
        //================================================================================================
		void GroundMaterialCached (PhysicMaterial colliderMaterial, ref PhysicMaterial cachMaterail, ref GroundPhysicMaterial groundPhysicMaterial)
		{
			if (m_physicMaterialManager != null)
			{
				if (colliderMaterial != cachMaterail)
				{
					cachMaterail = colliderMaterial;
					groundPhysicMaterial = m_physicMaterialManager.GetGroundPhysicMaterial(colliderMaterial);
				}
			}
		}

		
		
		//================================================================================================
        // バランス調整関数
        // 引数：
        //================================================================================================
		public static float GetBalance (float value, float bias, float positionRatio)
		{
			float frontRatio = bias;
			float rearRatio = 1.0f - bias;

			return value * (positionRatio * frontRatio + (1.0f - positionRatio) * rearRatio) * 2.0f;
		}
		
		//================================================================================================
        // アクセス計算関数
        // 引数：
        //================================================================================================
		float CalculAcceleForce(float requireForce, float maxForce, bool grounded)
		{
			float absSpeed = Mathf.Abs(m_speed);
			float speedLimit = m_speed >= 0.0f? maxSpeedForward : maxSpeedReverse;

			if (absSpeed < speedLimit)
			{
				if (!(m_speed < 0.0f && requireForce > 0.0f || m_speed > 0.0f && requireForce < 0.0f))
				{
					maxForce *= Tools.BiasedLerp(1.0f - absSpeed/speedLimit, forceCurveShape, m_forceBiasCtx);
				}

				return Mathf.Clamp(requireForce, -maxForce, +maxForce);
			}
			else
			{
				float opposingForce = maxForce * Mathf.Max(1.0f - absSpeed/speedLimit, -1.0f) * Mathf.Sign(m_speed);

				if(m_speed < 0.0f && requireForce > 0.0f || m_speed > 0.0f && requireForce < 0.0f)
				{
					opposingForce = Mathf.Clamp(opposingForce + requireForce, -maxForce, +maxForce);
				}

				return opposingForce;
			}
		}

		//================================================================================================
        // 横向きの力関数
        // 引数：ホイールのデータ、ベクトル
        //================================================================================================
		public Vector3 GetSidewaysForcePoint (WheelData wd, Vector3 point)
		{
			Vector3 sidewaysForcePoint = point+wd.transform.up * antiRoll * wd.forceDistance;

			if (wd.wheel.steer && wd.steerAngle != 0.0f && Mathf.Sign(wd.steerAngle) != Mathf.Sign(wd.tireSlip.x))
				sidewaysForcePoint += wd.transform.forward * (carFrame.frontPosition - carFrame.rearPosition) * (handlingBias - 0.5f);

			return sidewaysForcePoint;
		}
		
		//================================================================================================
        // ホイール力距離取得関数
        // 引数：ホイールのデータ
        //================================================================================================
		float GetWheelForceDistance (WheelCollider col)
		{
			return m_rigidbody.centerOfMass.y - m_transform.InverseTransformPoint(col.transform.position).y + col.radius + (1.0f - col.suspensionSpring.targetPosition) * col.suspensionDistance;
		}

		//================================================================================================
        // コライダー取得関数
        // 引数：なし
        //================================================================================================
		void FindColliders ()
		{
			Collider[] orignColliders = GetComponentsInChildren<Collider>(true);
			List<Collider> filteColliders = new List<Collider>();

			foreach (Collider col in orignColliders)
			{
				if (!col.isTrigger && !(col is WheelCollider))
					filteColliders.Add(col);
			}

			colliders = filteColliders.ToArray();
			colLayers = new int[colliders.Length];
		}

		float m_lastStrongImpactTime = 0.0f;
		PhysicMaterial m_lastImpactedMaterial;
		GroundPhysicMaterial m_impactedGroundMaterial = null;


        int m_sumImpactCount = 0;
        Vector3 m_sumImpactPosition = Vector3.zero;
        Vector3 m_sumImpactVelocity = Vector3.zero;
        int m_sumImpactHardness = 0;
        //float m_lastImpactTime = 0.0f;

        Vector3 m_localDragPosition = Vector3.zero;
        Vector3 m_localDragVelocity = Vector3.zero;
        int m_localDragHardness = 0;


        void Process (Collision col, bool forceImpact)
		{
			int impactCount = 0;
			Vector3 impactPosition = Vector3.zero;
			Vector3 impactVelocity = Vector3.zero;
            int impactHardness = 0;

            int dragCount = 0;
            Vector3 dragPosition = Vector3.zero;
            Vector3 dragVelocity = Vector3.zero;
            int dragHardness = 0;

            float sqrImpactSpeed = impactMinSpeed * impactMinSpeed;

            foreach (ContactPoint contact in col.contacts)
			{
				Collider collider = contact.otherCollider;

				//接触したマテリアルの取得　　硬：+1、柔：-1
				int hardness = 0;
				GroundMaterialCached(collider.sharedMaterial, ref m_lastImpactedMaterial, ref m_impactedGroundMaterial);

				if (m_impactedGroundMaterial != null)
					hardness = m_impactedGroundMaterial.surfaceType == GroundPhysicMaterial.SurfaceType.Hard? +1 : -1;

				//本体の速度計算
				Vector3 v = m_rigidbody.GetPointVelocity(contact.point);
				if (collider.attachedRigidbody != null)
					v -= collider.attachedRigidbody.GetPointVelocity(contact.point);

				float dragRatio = Vector3.Dot(v, contact.normal);

                if (dragRatio < -impactThreeshold || forceImpact && col.relativeVelocity.sqrMagnitude > sqrImpactSpeed)
                {
                    impactCount++;
                    impactPosition += contact.point;
                    impactVelocity += col.relativeVelocity;
                    impactHardness += hardness;
                }
                else if (dragRatio < impactThreeshold)
                {
                    dragCount++;
                    dragPosition += contact.point;
                    dragVelocity += v;
                    dragHardness += hardness;
                }
            }

            if (impactCount > 0)
            {
                float invCount = 1.0f / impactCount;
                impactPosition *= invCount;
                impactVelocity *= invCount;

                m_sumImpactCount++;
                m_sumImpactPosition += m_transform.InverseTransformPoint(impactPosition);
                m_sumImpactVelocity += m_transform.InverseTransformDirection(impactVelocity);
                m_sumImpactHardness += impactHardness;
            }


            if (dragCount > 0)
            {
                float invCount = 1.0f / dragCount;
                dragPosition *= invCount;
                dragVelocity *= invCount;

                UpdateDragState(m_transform.InverseTransformPoint(dragPosition), m_transform.InverseTransformDirection(dragVelocity), dragHardness);
            }
        }

        void UpdateDragState(Vector3 dragPosition, Vector3 dragVelocity, int dragHardness)
        {
            if (dragVelocity.sqrMagnitude > 0.001f)
            {
                m_localDragPosition = Vector3.Lerp(m_localDragPosition, dragPosition, 10.0f * Time.deltaTime);
                m_localDragVelocity = Vector3.Lerp(m_localDragVelocity, dragVelocity, 20.0f * Time.deltaTime);
                m_localDragHardness = dragHardness;
            }
            else
            {
                m_localDragVelocity = Vector3.Lerp(m_localDragVelocity, Vector3.zero, 10.0f * Time.deltaTime);
            }
        }

		//==============================================================================================================================
		// GUI
		//==============================================================================================================================
		private void OnGUI()
		{
			string gs;
			gs = "Speed：" + (int)(m_speed * 3.6f) + "km/h";
	
			GUI.Box(new Rect(10, 10, 110, 40), gs);
		}

        //---------------------------------------------スクリプトの右クリック機能----------------------------------------------------------

		//==============================================================================================================================
		// ホイールコライダーセッティング関数
		//==============================================================================================================================
        [ContextMenu("ホイールコライダー位置をセットアップ")]
        void AdjustWheelColliders()
        {
            foreach (Wheel wheel in wheels)
            {
                if (wheel.wheelCollider != null)
                    AdjustColliderToWheelMesh(wheel.wheelCollider, wheel.wheelTransform);
            }
        }

        static void AdjustColliderToWheelMesh(WheelCollider wheelCollider, Transform wheelTransform)
        {
            // 位置と回転の調整

            if (wheelTransform == null)
            {
                Debug.LogError(wheelCollider.gameObject.name + ": ホイールの位置が必要");
                return;
            }

            wheelCollider.transform.position = wheelTransform.position + wheelTransform.up * wheelCollider.suspensionDistance * 0.5f;
            wheelCollider.transform.rotation = wheelTransform.rotation;

            // 半径
            MeshFilter[] meshFilters = wheelTransform.GetComponentsInChildren<MeshFilter>();
            if (meshFilters == null || meshFilters.Length == 0)
            {
                Debug.LogWarning(wheelTransform.gameObject.name + ": ホイールの半径の計算はできない メッシュのデータがない");
                return;
            }

            // 範囲の計算
            Bounds bounds = GetScaledBounds(meshFilters[0]);

            for (int i = 1, c = meshFilters.Length; i < c; i++)
            {
                Bounds meshBounds = GetScaledBounds(meshFilters[i]);
                bounds.Encapsulate(meshBounds.min);
                bounds.Encapsulate(meshBounds.max);
            }

            // ホイールのY軸とZ軸が同じかどうか
            if (Mathf.Abs(bounds.extents.y - bounds.extents.z) > 0.01f)
                Debug.LogWarning(wheelTransform.gameObject.name + ": ホイールは正円ではない");

            wheelCollider.radius = bounds.extents.y;
        }


        static Bounds GetScaledBounds(MeshFilter meshFilter)
        {
            Bounds bounds = meshFilter.sharedMesh.bounds;
            Vector3 scale = meshFilter.transform.lossyScale;
            bounds.max = Vector3.Scale(bounds.max, scale);
            bounds.min = Vector3.Scale(bounds.min, scale);
            return bounds;
        }

		//-------------------------------------------------------デバッグギズモ-------------------------------------------------------------------

		public void OnDrawGizmos()
		{
			if (!enabled) return;

			CarFrame frame = CalculCarFrame();

			UnityEditor.Handles.color = ColorA(Color.green, 0.5f);

			foreach (Wheel w in wheels)
			{
				if (w.wheelCollider != null)
				{
					Vector3 basePos = w.wheelCollider.transform.TransformPoint(w.wheelCollider.center);
					Vector3 basePosR = basePos + w.wheelTransform.right * 10.0f;
					Vector3 basePosL = basePos - w.wheelTransform.right * 10.0f;
					Vector3 front = transform.TransformPoint(new Vector3(0.0f, frame.baseHeight, frame.frontPosition));

					UnityEditor.Handles.DrawSolidDisc(basePos, transform.right, w.wheelCollider.radius * 0.2f);
					UnityEditor.Handles.DrawLine(basePos, front);
					UnityEditor.Handles.DrawLine(basePosR, basePosL);
				}
			}
		}

		static Color ColorA (Color col, float alpha = 1.0f)
		{
			col.a = alpha;
			return col;
		}
	}
}

