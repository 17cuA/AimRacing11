//===================================================
// ファイル名	：UserController.cs
// 概要			：コントローラー
// 作成者		：謝 敬鴻
// 作成日		：2018.12.07
//===================================================

using UnityEngine;

namespace AIM
{
	[RequireComponent(typeof(Car))]

	public class UserControl : MonoBehaviour
	{
	
	    // コントロール用
	    private float h;    //左右
	    private float v;    //前後
	    private float handbreak;
	
	    [SerializeField] public Car m_car;
	
		//---------------------------------------------------------------------------------------------------
		//　初期化
		//---------------------------------------------------------------------------------------------------
		void Awake ()
	    {
	        m_car = GetComponent<Car>();
	    }
	
		//---------------------------------------------------------------------------------------------------
		//　キーボード操作
		//---------------------------------------------------------------------------------------------------
		public void KeyboardCtrl()
	    {
			h = Input.GetAxis("Horizontal");
	        v = Input.GetAxis("Vertical");
	
			//ハンドブレーキ
	        handbreak = Input.GetKey(KeyCode.Space) ? m_car.brakeTorque : 0;
	
	        m_car.Move(ref h, ref v, ref handbreak);
	
			//ギアの操作処理
			if(m_car.gearbox.currentGear >= 0 && m_car.gearbox.currentGear < m_car.gearbox.gearRatio.Length)
			{
				//if (Input.GetButtonDown("gearUp"))
				if(Input.GetButtonDown("gearUp"))
				{
					m_car.gearbox.currentGear++;
				}
				else if (Input.GetButtonDown("gearDown"))
				{
					m_car.gearbox.currentGear--;
				}
			}
	
			//ギアの制限
			if (m_car.gearbox.currentGear <= -1)
			{
				m_car.gearbox.currentGear = 0;
			}
	
			if (m_car.gearbox.currentGear >= 6)
			{
				m_car.gearbox.currentGear = 5;
			}
		}
	
		//---------------------------------------------------------------------------------------------------
		//　ゲームパッド操作
		//---------------------------------------------------------------------------------------------------
		public void GamePad()
		{
			//h = Input.GetAxis("GamePadX");
			//v = Input.GetAxis("accel");
			//handbreak = Input.GetKey(KeyCode.Space) ? m_car.brakeTorque : 0;
			//m_car.Move(ref h, ref v, ref handbreak);
		}
	}
}
