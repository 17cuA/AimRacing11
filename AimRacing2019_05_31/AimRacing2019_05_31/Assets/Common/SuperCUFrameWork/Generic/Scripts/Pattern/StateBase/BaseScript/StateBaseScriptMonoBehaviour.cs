using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBaseScriptMonoBehaviour : MonoBehaviour
{
	public string name;
    public virtual void FixedUpdateGame()
    {
    }
    public virtual void LateUpdateGame()
    {
    }
    public virtual void UpdateGame()
    {
    }
}
