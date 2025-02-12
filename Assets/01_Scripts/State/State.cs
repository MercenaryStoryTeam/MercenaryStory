using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State<T> where T : class
{
	public abstract void EnterState(T entity);
	public abstract void ExecuteState(T entity);
	public abstract void ExitState(T entity);
}