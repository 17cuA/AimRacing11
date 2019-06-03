//===================================================
// ファイル名	：PhysicMaterialManager.cs
// 概要		：摩擦処理マネージャー
// 作成者	：謝 敬鴻
// 作成日	：2019.05.07
//===================================================

using UnityEngine;
using System;

namespace AIM
{
	[Serializable]
	public class GroundPhysicMaterial
	{
		public PhysicMaterial physicMaterial;		//摩擦格納用

		public float grip = 1.0f;       //摩擦力
		public float drag = 0.1f;		//摩擦抵抗


		public enum SurfaceType {Hard, Soft};
		public SurfaceType surfaceType = SurfaceType.Hard;
	}


	public class PhysicMaterialManager : MonoBehaviour
	{
		public GroundPhysicMaterial[] groundPhysicMaterials = new GroundPhysicMaterial[0];

		bool deserialization = true;
		int materialsLength = 0;

		//==================================================
		// GroundPhysicMaterial配列制御
		//==================================================
		private void OnValidate()
		{
			if(deserialization)
			{
				materialsLength = groundPhysicMaterials.Length;
				deserialization = false;
			}
			else
			{
				if(groundPhysicMaterials.Length != materialsLength)
				{
					if(materialsLength == 0)
					{
						for(int i = 0; i < groundPhysicMaterials.Length; i++)
							groundPhysicMaterials[i] = new GroundPhysicMaterial();
					}

					materialsLength = groundPhysicMaterials.Length;
				}
			}
		}

		//==================================================
		// 摩擦マテリアル取得関数
		//==================================================
		public GroundPhysicMaterial GetGroundPhysicMaterial (PhysicMaterial physicMaterial)
		{
			for (int i = 0, j = groundPhysicMaterials.Length; i < j; i++)
			{
				if (groundPhysicMaterials[i].physicMaterial == physicMaterial)
					return groundPhysicMaterials[i];
			}

			return null;
		}
	}
}


