using System.Collections;
using System;
using System.Reflection;

public sealed class GameMachine
{
    private IEnumerator codePointer;
    private Func<object, IEnumerator> dynamicDispatch;
    
    private GameMachine ()
    {
    }
    
    public static IEnumerator Start (IEnumerator codePointer, Func<object, IEnumerator> dynamicDispatch)
    {
        return new GameMachine ()
        {
            codePointer = codePointer,
            dynamicDispatch = dynamicDispatch,
        }.run();
    }
    
    public class GameMachineGoSub
    {
        internal IEnumerator CodePointer;
    }
    
    public static GameMachineGoSub GoSub (IEnumerator codePointer)
    {
        return new GameMachineGoSub
        {
            CodePointer = codePointer,
        };
    }
    
    public class GameMachineGoTo
    {
        public Func<IEnumerator> Code;
    }
    
    public static GameMachineGoTo GoTo (Func<IEnumerator> code)
    {
        return new GameMachineGoTo
        {
            Code = code
        };
    }
    
    IEnumerator run ()
    {
        while (codePointer.MoveNext())
        {
            object retval = codePointer.Current;
            var requestGoSub = retval as GameMachineGoSub;
            if (requestGoSub != null)
            {
                var sub = new GameMachine ()
                {
                    codePointer = requestGoSub.CodePointer,
                    dynamicDispatch = this.dynamicDispatch,
                };
                var subCodePointer = sub.run();
                while (subCodePointer.MoveNext())
                {
                    yield return subCodePointer.Current;
                }
                continue;
            }
            var requestGoTo = retval as GameMachineGoTo;
            if (requestGoTo != null)
            {
                codePointer = requestGoTo.Code.Invoke();
                continue;
            }
            if (retval != null)
            {
                var queryCodePointer = (IEnumerator)dynamicDispatch (retval);
                if (queryCodePointer == null)
                {
                    continue;
                }
                while (queryCodePointer.MoveNext())
                {
                    yield return queryCodePointer.Current;
                }
                continue;
            }
            else
            {
                yield return null;
            }
        }
    }

    public static Func<object, IEnumerator> CreateDynamicDispatchDelegate (object target, string methodName)
    {
        return delegate (object parameter)
        {
            var method = target.GetType().GetMethod(
                methodName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { parameter.GetType() },
                null);
            return (IEnumerator)method.Invoke (target, new object[] { parameter });
        };
    }
}

public class GameMain
{
    public static IEnumerator Core (IEnumerator codePointer, Func<object, IEnumerator> dynamicDispatch)
    {
        while (codePointer.MoveNext())
        {
            object retval = codePointer.Current;
            var delegateMethod = retval as Func<IEnumerator>;
            if (delegateMethod != null)
            {
                codePointer = delegateMethod.Invoke();
            }
            else if (retval != null)
            {
                var queryCodePointer = (IEnumerator)dynamicDispatch (retval);
                while (queryCodePointer.MoveNext())
                {
                    yield return queryCodePointer.Current;
                }
            }
            else
            {
                yield return null;
            }
        }
    }
    
}
