using UnityEngine;
using UnityEngine.UI;

public class ConvertNumberToSprite : MonoBehaviour
{
	[SerializeField] Sprite[] sprites;
	Image image;

	// Use this for initialization
	void Start()
	{
		image = GetComponent<Image>();
		image.sprite = sprites[0];
	}

	// Update is called once per frame
	public void SpriteUpdate(int spriteNumber)
	{
		image.sprite = sprites[spriteNumber];
	}
}
