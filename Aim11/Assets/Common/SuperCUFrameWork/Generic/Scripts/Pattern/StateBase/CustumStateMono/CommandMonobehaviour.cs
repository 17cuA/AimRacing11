using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandMonobehaviour : StateMonobehavior
{
	[SerializeField]
	private int graceFrame;
	private int frame;

	protected override void Start()
	{
		base.Start();
		frame = graceFrame;
	}

	public override void UpdateGame()
	{
		base.UpdateGame();
		if (stateMove)
		{
			//Debug.Log(stateProcessor.State.getStateName());
			frame = graceFrame;
		}
		if (stateProcessor.State != states[0])
		{
			frame--;
		}
		if (frame <= 0)
		{
			stateProcessor.State = states[0];
		}
	}
}
