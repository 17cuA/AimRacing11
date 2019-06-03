//===================================================
// ファイル名	：CarAI.cs
// 概要			：対戦相手のAI
// 作成者		：藤森 悠輝
// 作成日		：2019.04.16
//===================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
	[RequireComponent(typeof(Car))]
	public class CarAI : MonoBehaviour
	{
	    enum DistancePlayerStatus
	    {
	        front,
	        normal,
	        back
	    }
	    // コントロール用
	    private float steer;    //左右
	    private float accel;    //前後
	    private float handbreak;
	    //当たり判定用レイ
	    static private RayTest rayFR;   //右前
	    static private RayTest rayFL;   //左前
	    static private RayTest rayFR_middle;   //右前
	    static private RayTest rayFL_middle;   //左前
	    static private RayTest rayFR_short;   //右前
	    static private RayTest rayFL_short;   //左前
	    static private RayTest rayR;    //右
	    static private RayTest rayL;    //左
	    public bool debug = false;
	
	    [SerializeField] public Car m_car;
	    
	    // Start is called before the first frame update
	    void Start()
	    {
	        steer = 0f;
	        accel = 1;
	
	        m_car = GetComponent<Car>();
	        rayFR = GameObject.Find("RayFRight").GetComponent<RayTest>();
	        rayFL = GameObject.Find("RayFLeft").GetComponent<RayTest>();
	        rayFR_middle = GameObject.Find("RayFRight_middle").GetComponent<RayTest>();
	        rayFL_middle = GameObject.Find("RayFLeft_middle").GetComponent<RayTest>();
	        rayFR_short = GameObject.Find("RayFRight_short").GetComponent<RayTest>();
	        rayFL_short = GameObject.Find("RayFLeft_short").GetComponent<RayTest>();
	        rayR = GameObject.Find("RayRight").GetComponent<RayTest>();
	        rayL = GameObject.Find("RayLeft").GetComponent<RayTest>();
	    }
	    //---------------------------------------------------------------------------------------------------
	    //　AIの操作
	    //---------------------------------------------------------------------------------------------------
	    public void AICtrl()
	    {
	        //壁の当たり判定処理
	        rayCastAI_Wall();
	
	        //ギアチェンジ
	        gearChange_AI();
	
	        //車の挙動へ反映
	        m_car.Move(ref steer, ref accel, ref handbreak);
	
	        //ステアリングの回転上限
	        if (steer > 1.0f)
	        {
	            steer = 1.0f;
	        }
	        else if(steer < -1.0f)
	        {
	            steer = -1.0f;
	        }
	    }
	
	    //---------------------------------------------------------------------------------------------------
	    //壁との当たり判定
	    //---------------------------------------------------------------------------------------------------
	    private void rayCastAI_Wall()
	    {
	        //レイの判定処理
	        bool isRayFR = rayFR.RayCast();
	        bool isRayFL = rayFL.RayCast();
	        bool isRayFR_middle = rayFR_middle.RayCast();
	        bool isRayFL_middle = rayFL_middle.RayCast();
	        bool isRayFR_short = rayFR_short.RayCast();
	        bool isRayFL_short = rayFL_short.RayCast();
	        bool isRayR = rayR.RayCast();
	        bool isRayL = rayL.RayCast();
	        bool isRayFRL = false;  //FR,FLが両方とも反応している
	
	        //壁との衝突判定
	        if (isRayFR && !isRayFL)
	        {
	            if (steer > 0) steer = 0;
	            steer += -0.003f * (m_car.currentSpeed / 10.0f);
	            accel = 0.9f;
	            Debug.Log("正面右を検知、左へ：" + steer);
	        }
	        else if (isRayFL && !isRayFR)
	        {
	            if (steer < 0) steer = 0;
	            steer += 0.003f * (m_car.currentSpeed / 10.0f);
	            accel = 0.9f;
	            Debug.Log("正面左を検知、右へ：" + steer);
	        }
	        else if (isRayFL && isRayFR)
	        {
	            isRayFRL = true;
	
	            if (isRayFR_middle && !isRayFL_middle)
	            {
	                if (steer > 0) steer = 0;
	                steer += -0.004f * (m_car.currentSpeed / 10.0f);
	                accel = 0.8f;
	                Debug.Log("正面右を検知、左へ：" + steer);
	            }
	            else if (!isRayFR_middle && isRayFL_middle)
	            {
	                if (steer < 0) steer = 0;
	                steer += 0.004f * (m_car.currentSpeed / 10.0f);
	                accel = 0.8f;
	                Debug.Log("正面左を検知、右へ：" + steer);
	            }
	            else if(!isRayFR_middle && !isRayFL_middle)
	            {
	                if(m_car.currentSpeed == 0) isRayFRL = false;
	                accel = 0.9f;
	                Debug.Log("壁は近くにない、変更なし：" + steer);
	            }
	            else
	            {
	                isRayFRL = false;
	            }
	        }
	        else if (!isRayL && !isRayR && !isRayFL && !isRayFR)
	        {
	            steer = 0;
	            accel = 1f;
	            Debug.Log("壁は検知されていない、正面：" + steer);
	        }
	
	        if (isRayFR_short && !isRayFL_short)
	        {
	            if (m_car.gearbox.currentGear == 6) gameObject.transform.Rotate(new Vector3(0f, -15f, 0f));
	            else
	            {
	                steer += -0.01f;
	                gameObject.transform.Rotate(new Vector3(0f, -1f, 0f));
	            }
	            //if (steer > 0) steer = 0;
	            //steer += -0.006f * (m_car.gearbox.currentGear + 1);
	            Debug.Log("正面右を検知、左へ：" + steer);
	        }
	        else if (!isRayFR_short && isRayFL_short)
	        {
	            if (m_car.gearbox.currentGear == 6) gameObject.transform.Rotate(new Vector3(0f, 15f, 0f));
	            else
	            {
	                steer += 0.01f;
	                gameObject.transform.Rotate(new Vector3(0f, 1f, 0f));
	            }
	            //if (steer < 0) steer = 0;
	            //steer += 0.006f * (m_car.gearbox.currentGear + 1);
	            Debug.Log("正面左を検知、右へ：" + steer);
	        }
	        else
	        {
	        }
	
	        if (!isRayFRL)
	        {
	            if (isRayR && !isRayL)
	            {
	                accel = 0.7f;
	                if (m_car.currentSpeed <= 1.5f)
	                {
	                    steer = 0;
	                    if (m_car.gearbox.currentGear == 6) gameObject.transform.Rotate(new Vector3(0f, -2f, 0f));
	                    else gameObject.transform.Rotate(new Vector3(0f, -0.1f, 0f));
	                }
	                steer += -0.01f;
	                Debug.Log("右に壁を検知、左へ：" + steer);
	            }
	            else if (isRayL && !isRayR)
	            {
	                accel = 0.7f;
	                if (m_car.currentSpeed <= 1.5f)
	                {
	                    steer = 0;
	                    if (m_car.gearbox.currentGear == 6) gameObject.transform.Rotate(new Vector3(0f, 2f, 0f));
	                    else gameObject.transform.Rotate(new Vector3(0f, 0.1f, 0f));
	                }
	                steer += 0.01f;
	                Debug.Log("左に壁を検知、右へ" + steer);
	            }
	            else if (!isRayL && !isRayR)
	            {
	                accel = 0.8f;
	                Debug.Log("左右に壁なし、変更なし：" + steer);
	            }
	            else if (isRayL && isRayR)
	            {
	                accel = -1f;
	                Debug.Log("左右に壁を検知、バックをする:" + steer);
	            }
	        }
	        if (debug) accel = 0;
	    }
	
	    private DistancePlayerStatus distanceToPlayer()
	    {
	        GameObject player = GameObject.Find("alfaromeo");
	        Vector3 playerPos = player.transform.position;
	        Car playerCar = player.GetComponent<Car>();
	        float distance_player = Vector3.Distance(playerPos, gameObject.transform.position);
	
	        //プレイヤーの近くを走っている
	        if (distance_player < 10)
	        {
	            Debug.Log("普通。"　+ distance_player);
	            m_car.allDistanceReset();
	            playerCar.allDistanceReset();
	            m_car.is4WD = false;
	            gameObject.GetComponent<Rigidbody>().mass = 1850;
	            return DistancePlayerStatus.normal;
	        }
	        //allDistanceCntで比べる
	        if (m_car.allDistanceCnt > playerCar.allDistanceCnt)
	        {
	            Debug.Log("プレイヤーの前にいる。" + distance_player);
	            return DistancePlayerStatus.front;
	        }
	        if (m_car.allDistanceCnt < playerCar.allDistanceCnt)
	        {
	            Debug.Log("プレイヤーの後ろにいる。" + distance_player);
	            return DistancePlayerStatus.back;
	        }
	        //allDistanceで比べる
	        if (m_car.allDistance > playerCar.allDistance)
	        {
	            Debug.Log("プレイヤーの前にいる。" + distance_player);
	            return DistancePlayerStatus.front;
	        }
	        if (m_car.allDistance < playerCar.allDistance)
	        {
	            Debug.Log("プレイヤーの後ろにいる。" + distance_player);
	            return DistancePlayerStatus.back;
	        }
	
	        Debug.Log("distanceToPlayerで例外が処理されています。" + distance_player);
	        return DistancePlayerStatus.normal;
	    }
	    //---------------------------------------------------------------------------------------------------
	    //ギアの変更
	    //---------------------------------------------------------------------------------------------------
	    private void gearChange_AI()
	    {
	        switch (distanceToPlayer())
	        {
	            case DistancePlayerStatus.normal:
	                if (m_car.currentSpeed >= 90.0f)
	                {
	                    m_car.gearbox.currentGear = 5;
	                }
	                else if (m_car.currentSpeed >= 80.0f)
	                {
	                    m_car.gearbox.currentGear = 4;
	                }
	                else if (m_car.currentSpeed >= 60.0f)
	                {
	                    m_car.gearbox.currentGear = 3;
	                }
	                else if (m_car.currentSpeed >= 35.0f)
	                {
	                    m_car.gearbox.currentGear = 2;
	                }
	                else if (m_car.currentSpeed >= 10.0f)
	                {
	                    m_car.gearbox.currentGear = 1;
	                }
	                else
	                {
	                    m_car.gearbox.currentGear = 0;
	                }
	                break;
	            case DistancePlayerStatus.front:
	                gameObject.GetComponent<Rigidbody>().mass = 3000;
	                m_car.gearbox.currentGear = 0;
	                break;
	            case DistancePlayerStatus.back:
	                m_car.gearbox.currentGear = 6;
	                m_car.is4WD = true;
	                accel = 1f;
	                break;
	        }
	    }
	}
}

