using System.Collections;
using System;
using UnityEngine;

// Usage:
// private EventPipe mainPipe = new EventPipe ();
// ...
// IEnumerator SomeCoroutine ()
// {
// 	   yield return this.mainPipe.WaitForEventOfType (typeof(...));
// }

// Sample dynamic-dispatch code for these requests using the method "Handle"
/*
	IEnumerator Handle (EventPipe.WaitForEventTypeEnumerator request)
	{
		while (request.MoveNext())
		{
			yield return request.Current;
		}
	}

	
	IEnumerator Handle (EventPipe.WaitForEventInTypeSetEnumerator request)
	{
		while (request.MoveNext())
		{
			yield return request.Current;
		}
	}
	
	IEnumerator Handle (EventPipe.WaitForTriggerOrEventTypeEnumerator request)
	{
		while (request.MoveNext())
		{
			yield return request.Current;
		}
	}
*/

public class EventPipe
{
	private ArrayList queue = new ArrayList();

	public void Add (object obj)
	{
		this.queue.Add (obj);
	}

	public WaitForEventTypeEnumerator WaitForEventType (Type type)
	{
		return new WaitForEventTypeEnumerator (this, type);
	}

	public class WaitForEventTypeEnumerator : IEnumerator
	{
		private EventPipe owner;
		
		public Type EventType { get; private set; }

        public object ReturnedEvent;

		internal WaitForEventTypeEnumerator (EventPipe owner, Type type)
		{
			this.EventType = type;
			this.owner = owner;
		}

		public bool MoveNext ()
		{
			for (int i = 0; i < this.owner.queue.Count; ++i)
			{
				var e = this.owner.queue[i];
				if (this.EventType.IsAssignableFrom (e.GetType()))
				{
					this.ReturnedEvent = e;
					this.owner.queue.RemoveAt (i);
					return false;
				}
			}
			this.owner.queue.Clear();
			return true;
		}

        public object Current
        {
            get
            {
                return null;
            }
        }

		public void Reset ()
		{
			throw new System.NotImplementedException();
		}
    }

	public WaitForEventInTypeSetEnumerator WaitForEventInTypeSet (params Type[] types)
	{
		return new WaitForEventInTypeSetEnumerator (this, types);
	}

	public class WaitForEventInTypeSetEnumerator : IEnumerator
	{
		private EventPipe owner;

		internal WaitForEventInTypeSetEnumerator (EventPipe owner, Type[] types)
		{
			this.owner = owner;
			this.EventTypes = types;
		}

		public Type[] EventTypes { get; private set; }
		public object ReturnedEvent;
		public int ReturnedIndex;

		
		public bool MoveNext ()
		{
			for (int i = 0; i < this.owner.queue.Count; ++i)
			{
				var e = this.owner.queue[i];
				for (int j = 0; j < this.EventTypes.Length; ++j)
				{
					var eventType = this.EventTypes[j];
					if (eventType.IsAssignableFrom (e.GetType()))
					{
						this.ReturnedEvent = e;
						this.ReturnedIndex = j;
						this.owner.queue.RemoveAt (i);
						return false;
					}
				}
			}
			this.owner.queue.Clear();
			return true;
		}

        public object Current
        {
            get
            {
                return null;
            }
        }

		public void Reset ()
		{
			throw new System.NotImplementedException();
		}
	}

	public WaitForTriggerOrEventTypeEnumerator WaitForTriggerOrEventType (Func<bool> trigger, Type type)
	{
		return new WaitForTriggerOrEventTypeEnumerator (this, trigger, type);
	}

	#if UNITY
	public WaitForTriggerOrEventTypeEnumerator WaitForSecondsOrEventType (float seconds, Type type)
	{
		float endTime = UnityEngine.Time.time + seconds;
		return new WaitForTriggerOrEventTypeEnumerator (this, () => return UnityEngine.Time.realtimeSinceStartup > endTime, type);
	}
	public WaitForTriggerOrEventTypeEnumerator WaitForSecondsRealtimeOrEventType (float seconds, Type type)
	{
		float endTime = UnityEngine.Time.realtimeSinceStartup + seconds;
		return new WaitForTriggerOrEventTypeEnumerator (this, () => return UnityEngine.Time.realtimeSinceStartup > endTime, type);
	}
	#endif

	public class WaitForTriggerOrEventTypeEnumerator : IEnumerator
	{
		private EventPipe owner;
		internal WaitForTriggerOrEventTypeEnumerator (EventPipe owner, Func<bool> trigger, Type type)
		{
			this.owner = owner;
			this.Trigger = trigger;
			this.EventType = type;
		}

		public Func<bool> Trigger;
		public Type EventType;
		public object ReturnedEvent;

		
		public bool MoveNext ()
		{
			if (this.Trigger())
			{
				this.ReturnedEvent = null;
				return false;
			}
			for (int i = 0; i < this.owner.queue.Count; ++i)
			{
				var e = this.owner.queue[i];
				if (this.EventType.IsAssignableFrom (e.GetType()))
				{
					this.ReturnedEvent = e;
					this.owner.queue.RemoveAt (i);
					return false;
				}
			}
			this.owner.queue.Clear();
			return true;
		}

        public object Current
        {
            get
            {
                return null;
            }
        }

		public void Reset ()
		{
			throw new System.NotImplementedException();
		}
	}

}