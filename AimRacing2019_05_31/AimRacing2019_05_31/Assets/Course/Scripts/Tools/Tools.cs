using UnityEngine;

public class Tools
{
	public class BiasLerpContext
	{
		public float lastBias = -1.0f;
		public float lastExponent = 0.0f;
	}

	static float BiasWithContext (float x, float bias, BiasLerpContext context)
	{
		if (x <= 0.0f) return 0.0f;
		if (x >= 1.0f) return 1.0f;

		if (bias != context.lastBias)
		{
			if (bias <= 0.0f) return x >= 1.0f? 1.0f : 0.0f;
			else if (bias >= 1.0f) return x > 0.0f? 1.0f : 0.0f;
			else if (bias == 0.5f) return x;

			context.lastExponent = Mathf.Log(bias) * -1.4427f;
			context.lastBias = bias;
		}

		return Mathf.Pow(x, context.lastExponent);
	}

	public static float BiasedLerp (float x, float bias, BiasLerpContext context)
	{
		float result = bias <= 0.5f? BiasWithContext(Mathf.Abs(x), bias, context) :
			1.0f - BiasWithContext(1.0f - Mathf.Abs(x), 1.0f - bias, context);

		return x < 0.0f? -result : result;
	}
}


