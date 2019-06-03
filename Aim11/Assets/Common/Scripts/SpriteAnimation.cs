//===================================================
// ファイル名	：SpriteAnimation.cs
// 概要			：画像データが分かれているSpriteをパスで読み込みアニメーションをさせる。
// 作成者		：藤森 悠輝
// 作成日		：2019.05.09
//===================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SpriteAnimation : MonoBehaviour
{
	[SerializeField] bool playOnAwake;
	[SerializeField] string extension = ".png";
	[SerializeField] string filePath = "";
	[SerializeField] SpriteRenderer spr;
	[SerializeField] Image img;
	[SerializeField] bool useSprite = false;
	[SerializeField] List<Sprite> sprites = new List<Sprite>();
	[SerializeField] float animationTime = 5.0f;
	[SerializeField] bool isRoop = false;

	public int spriteNumber = 0;   //!< 今の画像番号

	private IEnumerator awakeAnim;

	void Start()
	{
		if (useSprite)
		{
			if (spr == null)
			{
				spr = (GetComponent<SpriteRenderer>() != null)
				? GetComponent<SpriteRenderer>() : gameObject.AddComponent<SpriteRenderer>();
			}
		}
		else
		{
			if (img == null)
			{
				img = (GetComponent<Image>() != null)
				? GetComponent<Image>() : gameObject.AddComponent<Image>();
				//取得したパスからリストに画像を組み込み（先輩のデータが5桁で統一されていたため、ifが入ってしまっている。）
				for (int i = 0; i < sprites.Count; i++)
				{
					if (i < 10)
					{
						sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(filePath + "00" + i.ToString() + extension);
					}
					else if (i < 100)
					{
						sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(filePath + "0" + i.ToString() + extension);
					}
					else
					{
						sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(filePath + i.ToString() + extension);
					}
				}
			}
		}

		if (playOnAwake)
		{
			awakeAnim = Animation();
			StartCoroutine(awakeAnim);
		}
	}

	public void StopAwakeAnim()
	{
		if (playOnAwake)
		{
			StopCoroutine(awakeAnim);
			playOnAwake = false;
		}
	}

	public IEnumerator Animation()
	{
		int countUp = 1;        //!< めくる枚数
		int countMax = sprites.Count;   //!< 最大数
		float waitTime = animationTime / countMax;  //!< 待ち時間

		//!< めくる枚数を決定
		while (Time.deltaTime > waitTime * countUp)
		{
			++countUp;
		}

		//!< 画像をアニメーションさせる
		while (true)
		{
			if (useSprite)
			{
				spr.sprite = sprites[spriteNumber];
			}
			else
			{
				img.sprite = sprites[spriteNumber];
			}
			spriteNumber += countUp;

			//!< waitTime秒待つ
			yield return new WaitForSeconds(waitTime);

			if (countMax <= spriteNumber)
			{
				if (isRoop)
				{
					spriteNumber -= countMax;
				}
				else
				{
					break;
				}
			}
		}

	}
}