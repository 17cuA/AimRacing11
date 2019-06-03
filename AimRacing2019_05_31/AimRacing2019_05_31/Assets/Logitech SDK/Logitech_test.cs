using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Text;

public class Logitech_test : MonoBehaviour
{
	LogitechGSDK.LogiControllerPropertiesData properties;

	[SerializeField]
	private float xAxes, GasInput, BreakInput, ClutchInput;

	public bool HShift = true;
	bool isInGear;

	public int CurrentGear;

	private void Start()
	{
		properties = new LogitechGSDK.LogiControllerPropertiesData();
		properties.forceEnable = false;
		properties.overallGain = 100;
		properties.springGain = 100;
		properties.damperGain = 100;
		properties.defaultSpringEnabled = true;
		properties.defaultSpringGain = 100;
		properties.combinePedals = false;
		properties.wheelRange = 900;//ハンドル取得範囲

		properties.gameSettingsEnabled = false;
		properties.allowGameSettings = false;

		LogitechGSDK.LogiSetPreferredControllerProperties(properties);
		LogitechGSDK.LogiSteeringInitialize(true);
	}

	public void carInputUpdate()
	{
		if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
		{
			LogitechGSDK.DIJOYSTATE2ENGINES rec;
			rec = LogitechGSDK.LogiGetStateUnity(0);

			//ハンドコントローラ
			xAxes = rec.lX / 32768f;
			//アクセル
			if (rec.lY > 0)
			{
				GasInput = 0;
			}
			else if (rec.lY < 0)
			{
				GasInput = rec.lY / -32768f;
			}
			//ブレーキ
			if (rec.lRz > 0)
			{
				BreakInput = 0;
			}
			else if (rec.lRz < 0)
			{
				BreakInput = rec.lRz / -32768f;
			}
			//クラッチ
			if (rec.rglSlider[0] > 0)
			{
				ClutchInput = 0;
			}
			else if (rec.rglSlider[0] < 0)
			{
				ClutchInput = rec.rglSlider[0] / -32768f;
			}
		}
		else
		{
			print("No Steering Wheel connected!");
		}
	}

	public float getSteer()
	{
		return xAxes;
	}
	public float getAccele()
	{
		return GasInput;
	}

	public float getBreak()
	{
		return BreakInput;
	}

	void SteeringRange()
	{
	}
	void HShifter(LogitechGSDK.DIJOYSTATE2ENGINES shifter)
	{
		for (int i = 0; i < 128; i++)
		{
			if (shifter.rgbButtons[i] == 128)
			{
				if (ClutchInput > 0.5f)
				{
					if (i == 12)
					{
						CurrentGear = 1;
						isInGear = true;
					}
					else if (i == 13)
					{
						CurrentGear = 2;
						isInGear = true;
					}
					else if (i == 14)
					{
						CurrentGear = 3;
						isInGear = true;
					}
					else if (i == 15)
					{
						CurrentGear = 4;
						isInGear = true;
					}
					else if (i == 16)
					{
						CurrentGear = 5;
						isInGear = true;
					}
					else if (i == 17)
					{
						CurrentGear = 6;
						isInGear = true;
					}
					else if (i == 18)
					{
						CurrentGear = -1;
						isInGear = true;
					}
					else
					{
						isInGear = false;
						CurrentGear = 0;
					}
				}
			}
		}
	}
}
