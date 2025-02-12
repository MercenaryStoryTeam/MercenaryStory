using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;

public class StateMachine<T, TStateType> where T : class where TStateType : System.Enum
{
	private T ownerEntity;
	private State<T> currentState;
	private State<T> previousState;
	private Dictionary<TStateType, State<T>> states;

	public State<T> CurrentState => currentState;
	public TStateType CurrentStateType { get; private set; }

	public void Setup(T owner, State<T> entryState,
		Dictionary<TStateType, State<T>> stateMap)
	{
		ownerEntity = owner;
		states = stateMap;
		currentState = null;
		if (entryState != null)
		{
			ChangeState(entryState);
		}
	}

	public void ChangeState(State<T> newState)
	{
		if (currentState != null)
		{
			previousState = currentState;
			currentState.ExitState(ownerEntity);
		}

		currentState = newState;
		CurrentStateType = states.FirstOrDefault(x => x.Value == newState).Key;
		currentState.EnterState(ownerEntity);
	}

	public void RevertToPreviousState()
	{
		if (previousState != null)
		{
			ChangeState(previousState);
		}
	}

	public void ExecuteCurrentState()
	{
		currentState?.ExecuteState(ownerEntity);
	}
}