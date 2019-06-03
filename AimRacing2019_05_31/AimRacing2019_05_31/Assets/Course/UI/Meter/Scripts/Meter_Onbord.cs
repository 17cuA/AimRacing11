//===================================================
// ファイル名	：Metar_Onbord.cs
// 概要		：メーターの表示
// 作成者	：藤森 悠輝
// 作成日	：2019.05.16
// 
//---------------------------------------------------
// 更新履歴：
//===================================================
using UnityEngine;
using UnityEngine.UI;

public class Meter_Onbord : MonoBehaviour
{
	[SerializeField] Image gauge;
	[SerializeField] ConvertNumberToSprite[] rpmNumbers = new ConvertNumberToSprite[4];
	[SerializeField] ConvertNumberToSprite[] speedNumbers = new ConvertNumberToSprite[3];
	[SerializeField] ConvertNumberToSprite gearNumbers;

	[SerializeField] AIM.CarController carController;

	private void Start()
	{
		carController = new AIM.CarController();
		
		if(GameObject.Find("GTR-R35") != null)
        {
    		carController = GameObject.Find("GTR-R35").GetComponent<AIM.CarController>();
        }
        else
        {
            Debug.Log("Meter_Onbord：playerが取得できません。");
			carController = null;
        }

		FindMetarNumber("RPM_Text", ref rpmNumbers);
		FindMetarNumber("Speed_Text", ref speedNumbers);
		gearNumbers = GameObject.Find("Gear_Text").GetComponent<ConvertNumberToSprite>();
	}

	public void MetarUpdate()
	{
		if (carController != null)
		{
			//ギアの表示		
			//gearNumbers.SpriteUpdate(userControl.m_car.gearbox.currentGear + 1);

			//速さの表示
			for(int i = 0; i < speedNumbers.Length; i++)
			{
				if(carController.speed > 0)
					speedNumbers[i].SpriteUpdate(((int)(carController.speed  * 3.6f) / ((int)Mathf.Pow(10, i))) % 10);
			}

			//RPMの表示

		}
	}

	private void FindMetarNumber(string TextName,ref ConvertNumberToSprite[] sprite)
	{
		GameObject parentText = GameObject.Find(TextName);
		int metarNumber = 1;

		for (int i = 0; i < sprite.Length; i++)
		{
			sprite[i] = parentText.transform.Find(metarNumber.ToString()).GetComponent<ConvertNumberToSprite>();
			metarNumber *= 10;
		}
	}
}
