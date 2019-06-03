using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

[System.Serializable]
public abstract class DelegateBoolBase
{
	public abstract bool Invoke();
	public int tes;
}

[System.Serializable]
public class FuncBool : DelegateBoolBase
{
	public void Init(UnityEngine.Object obj, string methodName)
	{
		func = (Func<bool>)obj.GetType().GetMethod(methodName,new Type[] { }).CreateDelegate(typeof(Func<bool>), obj);
	}

	private Func<bool> func;

	public override bool Invoke()
	{
		return func();
	}
}

[System.Serializable]
public class FuncBool<T>:DelegateBoolBase
{
	private Func<T, bool> func;
	private T arg;

	public void Init(UnityEngine.Object obj,string methodName,T ag)
	{
        var type = typeof(T);
        func = (Func<T,bool>)obj.GetType().GetMethod(methodName,new Type[] { type }).CreateDelegate(typeof(Func<T,bool>), obj);
		arg = ag;
	}

	public override bool Invoke()
	{
		return func(arg);
	}
}
[System.Serializable]
public class FuncBool<T,T2> : DelegateBoolBase
{
    private Func<T,T2, bool> func;
    private T arg;
    private T2 arg2;

    public void Init(UnityEngine.Object obj, string methodName, T ag, T2 ag2)
    {
        var type = typeof(T);
        var type2 = typeof(T2);
        func = (Func<T,T2, bool>)obj.GetType().GetMethod(methodName, new Type[] { type, type2}).CreateDelegate(typeof(Func<T,T2, bool>), obj);
        arg = ag;
        arg2 = ag2;
    }

    public override bool Invoke()
    {
        return func(arg,arg2);
    }
}



[System.Serializable]
public abstract class DelegateBase
{
	public abstract void Invoke();
	public int tes;
}

[System.Serializable]
public class ActionVoid : DelegateBase
{
	public void Init(UnityEngine.Object obj, string methodName)
	{
		func = (Action)obj.GetType().GetMethod(methodName,new Type[] { }).CreateDelegate(typeof(Action), obj);
	}

	private Action func;

	public override void Invoke()
	{
		func();
	}
}
[System.Serializable]
public class ActionVoid<T> : DelegateBase
{
	private Action<T> func;
	private T arg;

	public void Init(UnityEngine.Object obj, string methodName, T ag)
	{
        var type = typeof(T);
		func = (Action<T>)obj.GetType().GetMethod(methodName,new Type[] { type }).CreateDelegate(typeof(Action<T>), obj);
		arg = ag;
	}

	public override void Invoke()
	{
		func(arg);
	}
}
[System.Serializable]
public class ActionVoid<T,T2>:DelegateBase
{
    private Action<T, T2> func;
    private T arg;
    private T2 arg2;

    public void Init(UnityEngine.Object obj, string methodName, T ag, T2 ag2)
    {
        var type = typeof(T);
        var type2 = typeof(T2);

        func = (Action<T,T2>)obj.GetType().GetMethod(methodName, new Type[] { type,type2}).CreateDelegate(typeof(Action<T,T2>), obj);
        arg = ag;
        arg2 = ag2;
    }

    public override void Invoke()
    {
        func(arg,arg2);
    }

}




