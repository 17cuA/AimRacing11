//===================================================
// ファイル名	：Car.cs
// 概要			：車の制御スクリプト
// 作成者		：謝 敬鴻
// 作成日		：2018.11.30
//===================================================
using UnityEngine;

namespace AIM
{
	public class Car : MonoBehaviour
	{
		//プロパティ
		[Header("格納用")]
		public Engine engine;
		public Gearbox gearbox;
		public GameObject wheelPrefab;
	
		public WheelCollider[] wheels;
	    public WheelCollider wheelFL;
	    public WheelCollider wheelFR;
	    public WheelCollider wheelRL;
	    public WheelCollider wheelRR;
	    // 車パラメータ--------------------------------------------------------------------------------------
	    [Header("パラメータ")]
	
		[Tooltip("ハンドルの最大角度")]
	    public float maxSteerAngle = 45.0f;
	
	    [Tooltip("X軸 = 速度, y軸 = スピードで曲げる角度を制限")]
	    public AnimationCurve steerCurve = AnimationCurve.Linear(0, 45, 400, 0.1f);
	
	    [Tooltip("四輪駆動")]
	    [SerializeField] public bool is4WD = false;
	
	    [Header("エンジン")]
	
	    [Tooltip("最大トルク")]
	    public float maxTorque = 800.0f;
	
	    
	
	    [Tooltip("トルクの最大ブレーキ量")]
	    public float brakeTorque = 30000.0f;
	
		public float torque;
		public float currentSpeed;
		public int allDistance = 0;
	    public int allDistanceCnt = 0;
	
		private int gearGUI;
		private int speedGUI;
	
		//---------------------------------------------------------------------------------------------------
	
	
		//// Substeps(サブステップ)----------------------------------------------------------------------------
		//// 参考："https://docs.unity3d.com/ja/current/ScriptReference/WheelCollider.ConfigureVehicleSubsteps.html"
		//[Tooltip("サブステップするアルゴリズムの速度しきい値")]
		//   [SerializeField] private float criticalSpeed = 8f;
		//   [Tooltip("車両の速度が speedThreshold 以下のときのシミュレーションのサブステップの量")]
		//   [SerializeField] private int stepsBelow = 8;
		//   [Tooltip("車両の速度が speedThreshold を超えるときのシミュレーションのサブステップの量")]
		//   [SerializeField] private int stepsAbove = 1;
		//   //---------------------------------------------------------------------------------------------------
	
	
		//// Suspension用--------------------------------------------------------------------------------------
		//[Tooltip("固有振動数(Hz)")]
		//[SerializeField][Range(0, 10)] private float naturalFrequency = 8.3f;
		//[Tooltip("振動を減衰量")]
		//[SerializeField][Range(0, 3)]  private float dampingRatio = 1.66f;
		//[Tooltip("移る力")]
		//[SerializeField][Range(-5, 5)] private float forceShift = -0.15f;
		//public bool setSuspensionDistance = true;
		////---------------------------------------------------------------------------------------------------
	
	
	
	
		//==============================================================================================================================
		//　初期化
		//==============================================================================================================================
		private void Start()
	    {
			//kiss me
			engine = GetComponent<Engine>();
			gearbox = GetComponent<Gearbox>();
			wheels = GetComponentsInChildren<WheelCollider>();
	
	        foreach (WheelCollider wheel in wheels)
	        {
	            //必要な時にホーイル生成
	            if (wheelPrefab != null)
	            {
	                GameObject wp = Instantiate(wheelPrefab);
	                wp.transform.parent = wheel.transform;
	
	                wp.transform.position = wheel.transform.position;
	                wp.transform.localScale = wp.transform.parent.localScale;       //大きさ修正
	                if (wheel.transform.localPosition.x > 0.0f)                     //左右修正
	                {
	                    wp.transform.localScale = new Vector3(wp.transform.localScale.x * -1.0f,
	                                                          wp.transform.localScale.y,
	                                                          wp.transform.localScale.z);
	                }
	
	                //前輪
	                if (wheel.transform.localPosition.z > 0)
	                {
	                    if (wheel.transform.localPosition.x < 0) wheelFL = wheel;
	                    else wheelFR = wheel;
	                }
	                else
	                {
	                    if (wheel.transform.localPosition.x > 0) wheelRL = wheel;
	                    else wheelRR = wheel;
	                }
	            }
	        }
	    }
	
		//==============================================================================================================================
		//移動関数(対外)
		//==============================================================================================================================
		public void Move(ref float steering, ref float accel, ref float handbreak)
	    {
			//torque = gearbox.gearRatio[gearbox.currentGear] * engine.torqueCurve.Evaluate(engine.RPM);
	
			torque = engine.bhp * gearbox.gearRatio[gearbox.currentGear];
	
			currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
	
	        //engine.RPM = ((wheelRR.rpm + wheelFL.rpm) / 2) * gearbox.gearRatio[gearbox.currentGear] * gearbox.finalGear;
	
			Accele(ref accel, ref handbreak);
	        Steer(ref steering, ref accel);
	
	        WheelsUpdata();
			notTurnOver();
	    }
	
		//==============================================================================================================================
		// アクセルメッソド
		// WheelCollider.motorTorqueを使い、加速する。
		// Torque：トルクはホーイルを回す力。
		// 参考：
		// トルク説明                "https://ja.wikipedia.org/wiki/%E3%83%88%E3%83%AB%E3%82%AF"
		// Wheel Colliderマニュアル  "https://docs.unity3d.com/ja/current/Manual/class-WheelCollider.html"
		// Wheel Collider API        "https://docs.unity3d.com/ja/current/ScriptReference/WheelCollider.html"
		//==============================================================================================================================
		private void Accele(ref float accel, ref float handbreak)
	    {
			//加速制御
			if (engine.RPM > 7500)
			{
				engine.RPM = 7500;
				torque = 0;
	
			}
			else
			{
				wheelRL.motorTorque = torque * accel;
				wheelRR.motorTorque = torque * accel;
				if (is4WD)
				{
					wheelFL.motorTorque = torque * accel;
					wheelFR.motorTorque = torque * accel;
				}
			}
			//wheelRL.motorTorque = torque;
			//wheelRR.motorTorque = torque;
	
	
			//ハンドブレーキ制御
			wheelRL.brakeTorque = handbreak;
	        wheelRR.brakeTorque = handbreak;
	
	
	    }
	
		//==============================================================================================================================
		//左右のコントロールメッソド
		//==============================================================================================================================
		private void Steer(ref float steering, ref float accel)
	    {
	        //ハンドル制御
	        wheelFL.steerAngle = maxSteerAngle * steering;
	        wheelFR.steerAngle = maxSteerAngle * steering;
		}
	
	
		//==============================================================================================================================
		// ホーイル処理
		//==============================================================================================================================
		private void WheelsUpdata()
		{
			if(wheelPrefab)
			{
	            foreach (WheelCollider wheel in wheels)
	            {
	                // ホーイル回転---------------------------------------------------------------------------------------------------------------
	                Quaternion q;
	                Vector3 p;
	                wheel.GetWorldPose(out p, out q);
	
	                Transform shapeTransform = wheel.transform.GetChild(0);
	                shapeTransform.position = p;
	                shapeTransform.rotation = q;
	                //----------------------------------------------------------------------------------------------------------------------------
	
	
	                //// suspension処理-------------------------------------------------------------------------------------------------------------
	                //// WheelColliderのsuspensionSpringを使う
	                //// 引用 "https://github.com/Unity-Technologies/VehicleTools/blob/master/Assets/Scripts/EasySuspension.cs"
	                //// 参考："https://docs.unity3d.com/ja/current/ScriptReference/WheelCollider.html"
	                ////       "https://www.drtuned.com/tech-ramblings/2017/10/2/spring-rates-suspension-frequencies"
	                ////       "https://dskjal.com/car/natural-frequency-for-car-spring.html"
	                //JointSpring spring = wheel.suspensionSpring;
	
	                //spring.spring = Mathf.Pow(Mathf.Sqrt(wheel.sprungMass) * naturalFrequency, 2);
	                //spring.damper = 2 * dampingRatio * Mathf.Sqrt(spring.spring * wheel.sprungMass);
	
	                //wheel.suspensionSpring = spring;
	
	                //Vector3 wheelRelativeBody = transform.InverseTransformPoint(wheel.transform.position);
	                //float distance = GetComponent<Rigidbody>().centerOfMass.y - wheelRelativeBody.y + wheel.radius;
	
	                //wheel.forceAppPointDistance = distance - forceShift;
	
	                //// 最大ドループ時にspring forceが0かどうかを確認する
	                //if (spring.targetPosition > 0 && setSuspensionDistance)
	                //    wheel.suspensionDistance = wheel.sprungMass * Physics.gravity.magnitude / (spring.targetPosition * spring.spring);
	                ////----------------------------------------------------------------------------------------------------------------------------
	            }
			}
		}
	
		//==============================================================================================================================
		// GUI
		//==============================================================================================================================
		private void OnGUI()
		{
			speedGUI = (int)currentSpeed;
			gearGUI = gearbox.currentGear + 1;
			string gs;
			gs = "Speed：" + speedGUI.ToString() + "km/h" + "\n" + 
				 "Gear：" + gearGUI.ToString() + "          " + "\n";
			allDistance += speedGUI;
			if (allDistance >= 300000)
			{
				allDistanceCnt++;
				allDistance = 0;
			}
	
			GUI.Box(new Rect(10, 10, 110, 40), gs);
			
		}
		public void allDistanceReset()
	    {
	        allDistance = 0;
	        allDistanceCnt = 0;
	    }
	
		//==============================================================================================================================
	    // Carがひっくり帰らない処理
	    //==============================================================================================================================
	    private void notTurnOver()
	    {
	        Quaternion nowRotation = transform.rotation;
	        if(transform.rotation.z > 0.1f)
	        {
	            Debug.Log("Z軸に36度回転");
	            //transform.rotation = Quaternion.Euler(nowRotation.x, nowRotation.y, 0.1f);
	            gameObject.transform.Rotate(new Vector3(0f, 0f, -2f));
	        }
	        else if (transform.rotation.z < -0.1f)
	        {
	            Debug.Log("Z軸に-36度回転");
	            //transform.rotation = Quaternion.Euler(nowRotation.x, nowRotation.y, -0.1f);
	            gameObject.transform.Rotate(new Vector3(0f, 0f, 2f));
	        }
	        //if (transform.rotation.x > 0.1f)
	        //{
	        //    Debug.Log("X軸に36度回転");
	        //    transform.rotation = Quaternion.Euler(0.1f, nowRotation.y, nowRotation.z);
	        //}
	        //else if (transform.rotation.x < -0.1f)
	        //{
	        //    Debug.Log("X軸に-36度回転");
	        //    transform.rotation = Quaternion.Euler(-0.1f, nowRotation.y, nowRotation.z);
	        //}
	    }
	
	}
}

