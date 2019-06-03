using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Entry : StateBaseScriptMonoBehaviour
{
	public float time = 0;
	Fade fade;
	private void Start()
	{
		fade = GameObject.Find("Fade").GetComponent<Fade>();
	}
	private void Update()
	{
		time += Time.deltaTime;
	}
	public void StartFadeOut(float sec)
	{
		Debug.Log(fade);
		StartCoroutine(fade.FadeOut(sec));
	}
	public void StartFadeIn(float sec)
	{
		Debug.Log(fade);
		StartCoroutine(fade.FadeIn(sec));
	}

	public void StartImageFadeIn(Image image, float sec)
	{
		StartCoroutine(fade.ImageFadeIn(image, sec));
	}
	public void StartImageFadeOut(Image image, float sec)
	{
		StartCoroutine(fade.ImageFadeOut(image, sec));
	}

	public bool flowFade0(float sec)
	{
		if (time >= sec)
		{
			time = 0;
			return true;
		}
		return false;
	}

}
