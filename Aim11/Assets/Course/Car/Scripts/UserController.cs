//===================================================
// ファイル名	：UserController.cs
// 概要			：コントローラー
// 作成者		：謝 敬鴻
// 作成日		：2018.12.07
//===================================================

using UnityEngine;

namespace AIM
{
	public class UserController : MonoBehaviour
	{
		public CarController carController;
		public Logitech_test carInputLogi;
		public bool keybordControl;

		public bool forwRev = true;

		public string steerAxis = "Horizontal";
		public string acceleAxis = "Vertical";
		public string handbrakeAxis = "Jump";


		public KeyCode resetCarKey = KeyCode.F8;
		public KeyCode changeInput = KeyCode.F7;

		bool doReset = false;

		// Start is called before the first frame update
		void Start()
		{
			if (carController == null)
			{
				carController = GetComponent<CarController>();
				carInputLogi = GetComponent<Logitech_test>();
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (carController == null) return;

			if (Input.GetKeyDown(resetCarKey)) doReset = true;
			if (Input.GetKeyDown(changeInput))
			{
				if (keybordControl) keybordControl = false;
				else keybordControl = true;
			}
			
		}

		private void FixedUpdate()
		{
			if (carController == null) return;

			float steerInput = 0.0f;
			float handbrakeInput = 0.0f;
			float forwardInput = 0.0f;
			float reverseInput = 0.0f;

			//入力
			if (keybordControl)
			{
				steerInput = Mathf.Clamp(Input.GetAxis(steerAxis), -1.0f, 1.0f);
				handbrakeInput = Mathf.Clamp01(Input.GetAxis(handbrakeAxis));

				forwardInput = Mathf.Clamp01(Input.GetAxis(acceleAxis));
				reverseInput = -Mathf.Clamp01(Input.GetAxis(handbrakeAxis));
			}
			else
			{
				carInputLogi.carInputUpdate();

				steerInput = Mathf.Clamp(carInputLogi.getSteer(), -1.0f, 1.0f);
				handbrakeInput = Mathf.Clamp01(carInputLogi.getBreak());

				forwardInput = Mathf.Clamp01(carInputLogi.getAccele());
				reverseInput = -Mathf.Clamp01(carInputLogi.getBreak());
			}

			float acceleInput = 0.0f;
			float brakeInput = 0.0f;

			if (forwRev)
			{
				//アクセスとブレーキとバックは同じボタン
				float minSpeed = 0.1f;
				float minInput = 0.1f;

				if (carController.speed > minSpeed)
				{
					acceleInput = forwardInput;
					brakeInput = reverseInput;
				}
				else
				{
					if (reverseInput > minInput)
					{
						acceleInput = -reverseInput;
						brakeInput = 0.0f;
					}
					else if (forwardInput > minInput)
					{
						if (carController.speed < -minSpeed)
						{
							acceleInput = 0.0f;
							brakeInput = forwardInput;
						}
						else
						{
							acceleInput = forwardInput;
							brakeInput = 0;
						}
					}
				}
			}
			else
			{
				//アクセスとバックを分ける
				acceleInput = forwardInput;
				brakeInput = reverseInput;
			}

			carController.steerInput = steerInput;
			carController.acceleInput = acceleInput;
			carController.brakeInput = brakeInput;
			carController.handbrakeInput = handbrakeInput;

			if (doReset)
			{
				carController.ResetCar();
				doReset = false;
			}

		}
	}
}


