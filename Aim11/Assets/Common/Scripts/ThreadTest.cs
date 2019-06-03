using System.Collections;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ThreadTest : MonoBehaviour
{
	public static SynchronizationContext _context;
	void Start()
	{
		_MoveAsync();
	}
	void Update()
    {
		Debug.Log("Update");
    }

	private async void _MoveAsync()
	{
		_context = SynchronizationContext.Current;
		await Task.Run(_Action);
	}

	private Action _Action = () =>
	{
		const int loopMax = 10000;
		for (int i = 0; i < loopMax; i++)
		{
			Debug.Log("Action i = " + i);
			if (loopMax == i + 1) 
			{
				_context.Post((state) =>
				{
					Debug.Log("in main thread");
				}, null);
			}
		}
		Debug.Log("finish");
	};
}
