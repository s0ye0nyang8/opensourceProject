using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class MainThreadCaller : MonoBehaviour
{
    #region SingletonPattern
    private static MainThreadCaller instance;
	public static MainThreadCaller Instance
	{
		get
		{
			if (instance == null)
			{
				throw new Exception("Add the object for \"MainThreadCaller\"");
			}

			return instance;
		}

		private set
		{
			instance = value;
		}

	}
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
    #endregion

    private static readonly Queue<Action> actionQueue = new Queue<Action>();

	public void Update()
	{
		lock (actionQueue)
		{
			while (actionQueue.Count > 0)
			{
				actionQueue.Dequeue().Invoke();
			}
		}
	}

	public void Enqueue(IEnumerator action)
	{
		lock (actionQueue)
		{
			actionQueue.Enqueue(() => {
				StartCoroutine(action);
			});
		}
	}

	public void Enqueue(Action action)
	{
		Enqueue(Executor(action));
	}

	public Task EnqueueAsync(Action action)
	{
		var source = new TaskCompletionSource<bool>();

		void WrappedAction()
		{
			try
			{
				action();
				source.TrySetResult(true);
			}
			catch (Exception e)
			{
				source.TrySetException(e);
			}
		}

		Enqueue(Executor(WrappedAction));
		return source.Task;
	}

	IEnumerator Executor(Action action)
	{
		action();
		yield return null;
	}

	void OnDestroy()
	{
		instance = null;
	}
}