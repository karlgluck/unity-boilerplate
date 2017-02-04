using System;
using System.Collections;
using System.Reflection;
using UnityEngine.Scripting;

// [Bind] is used to annotate a static field or property of type Func or Action. It causes
// the value of that member to be mapped to a function somewhere else in the program with the
// same name as that field. The target function can be a static function or an instance
// function whose class has a static property/field of its own type named "Singleton".
//
// Every time you use [Bind] it must be assigned to an appropriate BoundAction or BoundFunc value.
// This is because iOS requires AOT compilation of SingletonMethodCall.Call{Action,Func}{n}{F,P}
// for the appropriate template types.
//
// Examples:
//      
//	[Bind]
//	public static Action<object> AddToScene = BoundAction.Null;
//
//	[Bind]
//	public static Func<string, object> GetObjectByName = BoundFunc<object>.Null;
//
//	[Bind]
//	public static Action<int,string,string> SomethingWithLongParameterList = BoundAction.Null;
//
// Note that ".Null" can be used regardless of the parameter set. The language will infer
// the correct types. However, BoundFunc must be used for Func types and BoundAction for
// action types because function binding cannot infer the return type.

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class Bind : Attribute
{
}

public static class BoundAction
{
    public static void Null ()
    {
        try
        {
            new Binder.SingletonMethodCall().CallAction0F ();
            new Binder.SingletonMethodCall().CallAction0P ();
        }
        finally
        {
            throw new NullReferenceException ("Invoked null Action");
        }
    }
    
    public static void Null<T0> (T0 a0)
    {
        try
        {
            new Binder.SingletonMethodCall().CallAction1F (a0);
            new Binder.SingletonMethodCall().CallAction1P (a0);
        }
        finally
        {
            throw new NullReferenceException ("Invoked null Action");
        }
    }

    public static void Null<T0,T1> (T0 a0, T1 a1)
    {
        try
        {
            new Binder.SingletonMethodCall().CallAction2F (a0, a1);
            new Binder.SingletonMethodCall().CallAction2P (a0, a1);
        }
        finally
        {
            throw new NullReferenceException ("Invoked null Action");
        }
    }

    public static void Null<T0,T1,T2> (T0 a0, T1 a1, T2 a2)
    {
        try
        {
            new Binder.SingletonMethodCall().CallAction3F (a0, a1, a2);
            new Binder.SingletonMethodCall().CallAction3P (a0, a1, a2);
        }
        finally
        {
            throw new NullReferenceException ("Invoked null Action");
        }
    }
}

public static class BoundFunc<TResult>
{
    public static TResult Null<T0> (T0 a0)
    {
        new Binder.SingletonMethodCall().CallFunc1F<T0,TResult> (a0);
        new Binder.SingletonMethodCall().CallFunc1P<T0,TResult> (a0);
        return default(TResult);
    }

    public static TResult Null<T0,T1> (T0 a0, T1 a1)
    {
        new Binder.SingletonMethodCall().CallFunc2F<T0,T1,TResult> (a0, a1);
        new Binder.SingletonMethodCall().CallFunc2P<T0,T1,TResult> (a0, a1);
        return default(TResult);
    }
    
    public static TResult Null<T0,T1,T2> (T0 a0, T1 a1, T2 a2)
    {
        new Binder.SingletonMethodCall().CallFunc3F<T0,T1,T2,TResult> (a0, a1, a2);
        new Binder.SingletonMethodCall().CallFunc3P<T0,T1,T2,TResult> (a0, a1, a2);
        return default(TResult);
    }
    
    public static TResult Null<T0,T1,T2,T3> (T0 a0, T1 a1, T2 a2, T3 a3)
    {
        new Binder.SingletonMethodCall().CallFunc4F<T0,T1,T2,T3,TResult> (a0, a1, a2, a3);
        new Binder.SingletonMethodCall().CallFunc4P<T0,T1,T2,T3,TResult> (a0, a1, a2, a3);
        return default(TResult);
    }
}

// [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
// public class Singleton : Attribute
// {
//     public Singleton () : this (0) {}
    
//     public int Priority { get; private set; }

//     public Singleton (int priority)
//     {
//         this.Priority = priority;
//     }
// }

// #if UNITY_EDITOR
// [UnityEditor.InitializeOnLoad]
// #endif
public class Binder
{
    public static void DoBinding ()
    {
        foreach (Type type in AllBindableClassTypes())
        {
            bindInstanceMethods (type);
            bindStaticMethods (type);
        }

#if UNITY_EDITOR
        UnityEngine.Debug.Log ("Binder.cs: -- TODO -- a bunch of this stuff should be optimized");
#endif
    }

    static IEnumerable AllBindableClassTypes()
    {
        System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        for (int k = 0; k < assemblies.Length; ++k)
        {
            Assembly assembly = assemblies[k];
            //UnityEngine.Debug.LogFormat ("Assembly[{0}]={1}", k, assembly.FullName);
            if (!assembly.FullName.StartsWith("Assembly-CSharp"))
            {
                continue;
            }
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
                Type type = types[i];
                if (!type.IsClass)
                {
                    continue;
                }
                if (type.IsGenericType)
                {
                    // ignore this type
                    continue;
                }
                yield return type;
            }
        }
    }

    private static bool findSingletonMethod (
        FieldInfo delegateFieldToBind,
        Type delegateFieldReturnType,
        Type[] delegateFieldParameterTypes,
        out Type singletonType,
        out FieldInfo staticSingletonField,
        out PropertyInfo staticSingletonProperty,
        out MethodInfo singletonMethod)
    {
        foreach (Type type in AllBindableClassTypes())
        {
            FieldInfo thisTypeSingletonField = type.GetField ("Singleton", BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public | BindingFlags.NonPublic);
            bool singletonFieldFound = thisTypeSingletonField != null;
            if (singletonFieldFound)
            {
                bool singletonFieldIsWrongType = !thisTypeSingletonField.FieldType.Equals (type);
                if (singletonFieldIsWrongType)
                {
                    //Debug.LogFormat ("findSingletonMethod found {0}.Singleton field but it's the wrong type", type.Name);
                    singletonFieldFound = false;
                }
            }

            PropertyInfo thisTypeSingletonProperty = null;
            bool singletonPropertyFound = false;
            if (!singletonFieldFound)
            {
                thisTypeSingletonProperty = type.GetProperty ("Singleton", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic);
                singletonPropertyFound = thisTypeSingletonProperty != null;
                if (singletonPropertyFound)
                {
                    bool singletonPropertyIsWrongType = !thisTypeSingletonProperty.PropertyType.Equals (type);
                    if (singletonPropertyIsWrongType)
                    {
                        //Debug.LogFormat ("findSingletonMethod found {0}.Singleton property but it's the wrong type", type.Name);
                        singletonPropertyFound = false;
                    }
                }
            }

            if (!singletonFieldFound && !singletonPropertyFound)
            {
                //Debug.LogFormat ("findSingletonMethod didn't find a static {0}.Singleton field or property", type.Name);
                continue;
            }

            //Debug.LogFormat ("findSingletonMethod found {0}.Singleton; checking for {0}.Singleton.{1}...", type.Name, delegateFieldToBind.Name);

            MethodInfo thisTypeMethodToBind = type.GetMethod (
                delegateFieldToBind.Name,
                BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public,
                null,
                delegateFieldParameterTypes,
                null);
            bool methodNotFound = thisTypeMethodToBind == null;
            if (methodNotFound)
            {
                //Debug.LogFormat ("{0}.Singleton.{1} not found", type.Name, delegateFieldToBind.Name);
                continue;
            }
            bool methodHasWrongReturnType = !thisTypeMethodToBind.ReturnType.Equals(delegateFieldReturnType);
            if (methodHasWrongReturnType)
            {
                //Debug.LogFormat ("{0}.Singleton.{1} has the wrong return type", type.Name, delegateFieldToBind.Name);
                continue;
            }

            //Debug.LogFormat ("{0}.Singleton.{1} found", type.Name, delegateFieldToBind.Name);
            singletonType = type;
            staticSingletonField = thisTypeSingletonField;
            staticSingletonProperty = thisTypeSingletonProperty;
            singletonMethod = thisTypeMethodToBind;
            return true;
        }
        singletonType = null;
        staticSingletonField = null;
        staticSingletonProperty = null;
        singletonMethod = null;
        return false;
    }

    private static bool findStaticMethod (
        FieldInfo delegateFieldToBind,
        Type delegateFieldReturnType,
        Type[] delegateFieldParameterTypes,
        out MethodInfo staticMethod)
    {
        foreach (Type type in AllBindableClassTypes())
        {
            MethodInfo thisTypeMethodToBind = type.GetMethod (
                delegateFieldToBind.Name,
                BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public,
                null,
                delegateFieldParameterTypes,
                null);
            bool methodNotFound = thisTypeMethodToBind == null;
            if (methodNotFound)
            {
                continue;
            }
            bool methodHasWrongReturnType = !thisTypeMethodToBind.ReturnType.Equals(delegateFieldReturnType);
            if (methodHasWrongReturnType)
            {
                continue;
            }

            staticMethod = thisTypeMethodToBind;
            return true;
        }
        staticMethod = null;
        return false;
    }

    internal class SingletonMethodCall
    {
        public FieldInfo Field;
        public PropertyInfo Property;
        public MethodInfo Method;

        public static Delegate Create (FieldInfo singletonField, PropertyInfo singletonProperty, Type delegateType, MethodInfo method)
        {
            var parameters = method.GetParameters();
            bool hasReturnValue = !method.ReturnType.Equals (typeof(void));
            var genericMethodName = "Call"+(hasReturnValue?"Func":"Action")+method.GetParameters().Length+(singletonField == null ? "P" : "F");
            var genericMethodInfo = typeof(SingletonMethodCall).GetMethod (genericMethodName);
            if (genericMethodInfo == null)
            {
                throw new InvalidOperationException ("Couldn't find method needed to dynamically bind: " + genericMethodName);
            }
            MethodInfo methodInfo;
            if (genericMethodInfo.IsGenericMethod)
            {
                var genericParameterTypes = new Type[parameters.Length + (hasReturnValue?1:0)];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    genericParameterTypes[i] = parameters[i].ParameterType;
                }
                if (hasReturnValue)
                {
                    genericParameterTypes[parameters.Length] = method.ReturnType;
                }
                methodInfo = genericMethodInfo.MakeGenericMethod (genericParameterTypes);
            }
            else
            {
                methodInfo = genericMethodInfo;
            }
            var instance = new SingletonMethodCall()
            {
                Field = singletonField,
                Property = singletonProperty,
                Method = method,
            };
            return Delegate.CreateDelegate (delegateType, instance, methodInfo);
        }
        
        public void CallAction0F () { Method.Invoke (this.Field.GetValue(null), null); }
        public void CallAction1F<T0>(T0 a0) { Method.Invoke (this.Field.GetValue(null), new object[]{a0}); }
        public void CallAction2F<T0, T1>(T0 a0, T1 a1) { Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1}); }
        public void CallAction3F<T0, T1, T2>(T0 a0, T1 a1, T2 a2) { Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1,a2}); }
        public void CallAction4F<T0, T1, T2, T3>(T0 a0, T1 a1, T2 a2, T3 a3) { Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1,a2,a3}); }
        public void CallAction5F<T0, T1, T2, T3, T4>(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4) { Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1,a2,a3,a4}); }
        public void CallAction6F<T0, T1, T2, T3, T4, T5>(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5) { Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1,a2,a4,a5}); }

        public void CallAction0P () { Method.Invoke (this.Property.GetValue(null,null), null); }
        public void CallAction1P<T0>(T0 a0) { Method.Invoke (this.Property.GetValue(null,null), new object[]{a0}); }
        public void CallAction2P<T0, T1>(T0 a0, T1 a1) { Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1}); }
        public void CallAction3P<T0, T1, T2>(T0 a0, T1 a1, T2 a2) { Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1,a2}); }
        public void CallAction4P<T0, T1, T2, T3>(T0 a0, T1 a1, T2 a2, T3 a3) { Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1,a2,a3}); }
        public void CallAction5P<T0, T1, T2, T3, T4>(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4) { Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1,a2,a3,a4}); }
        public void CallAction6P<T0, T1, T2, T3, T4, T5>(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5) { Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1,a2,a4,a5}); }

        public TResult CallFunc0F<TResult>() { return (TResult)Method.Invoke (this.Field.GetValue(null), null); }
        public TResult CallFunc1F<T0, TResult>(T0 a0) { return (TResult)Method.Invoke (this.Field.GetValue(null), new object[]{a0}); }
        public TResult CallFunc2F<T0, T1, TResult>(T0 a0, T1 a1) { return (TResult)Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1}); }
        public TResult CallFunc3F<T0, T1, T2, TResult>(T0 a0, T1 a1, T2 a2) { return (TResult)Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1,a2}); }
        public TResult CallFunc4F<T0, T1, T2, T3, TResult>(T0 a0, T1 a1, T2 a2, T3 a3) { return (TResult)Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1,a2,a3}); }
        public TResult CallFunc5F<T0, T1, T2, T3, T4, TResult>(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4) { return (TResult)Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1,a2,a3,a4}); }
        public TResult CallFunc6F<T0, T1, T2, T3, T4, T5, TResult>(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5) { return (TResult)Method.Invoke (this.Field.GetValue(null), new object[]{a0,a1,a2,a3,a4,a5}); }

        public TResult CallFunc0P<TResult>() { return (TResult)Method.Invoke (this.Property.GetValue(null,null), null); }
        public TResult CallFunc1P<T0, TResult>(T0 a0) { return (TResult)Method.Invoke (this.Property.GetValue(null,null), new object[]{a0}); }
        public TResult CallFunc2P<T0, T1, TResult>(T0 a0, T1 a1) { return (TResult)Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1}); }
        public TResult CallFunc3P<T0, T1, T2, TResult>(T0 a0, T1 a1, T2 a2) { return (TResult)Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1,a2}); }
        public TResult CallFunc4P<T0, T1, T2, T3, TResult>(T0 a0, T1 a1, T2 a2, T3 a3) { return (TResult)Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1,a2,a3}); }
        public TResult CallFunc5P<T0, T1, T2, T3, T4, TResult>(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4) { return (TResult)Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1,a2,a3,a4}); }
        public TResult CallFunc6P<T0, T1, T2, T3, T4, T5, TResult>(T0 a0, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5) { return (TResult)Method.Invoke (this.Property.GetValue(null,null), new object[]{a0,a1,a2,a3,a4,a5}); }
    }

    private static void bindStaticMethods (Type type)
    {
        MemberInfo[] members = type.FindMembers(
            MemberTypes.Field,
            BindingFlags.Static | BindingFlags.SetField | BindingFlags.Public | BindingFlags.NonPublic,
            delegate (MemberInfo memberInfo, object lastParameterOfFindMembers)
            {
                return memberInfo.MemberType == MemberTypes.Field &&
                    Attribute.IsDefined (memberInfo, typeof(Bind));
            },
            null);

        for (int j = 0; j < members.Length; ++j)
        {
            FieldInfo delegateFieldToBind = (FieldInfo)members[j];
            Type delegateFieldToBindType = delegateFieldToBind.FieldType;
            Type[] parameterTypes = GetDelegateParameterTypes(delegateFieldToBindType);
            Type returnType = GetDelegateReturnType(delegateFieldToBindType);

            bool alreadyHasValue = delegateFieldToBind.GetValue (null) != null;
            if (!alreadyHasValue)
            {
#if UNITY_EDITOR
                if (returnType != typeof(void))
                {
                    UnityEngine.Debug.LogErrorFormat ("{0}.{1} should be assigned to BoundAction.Null by default (to make sure the AOT compiler generates code for the binding)", type.Name, delegateFieldToBind.Name);
                }
                else
                {
                    UnityEngine.Debug.LogErrorFormat ("{0}.{1} should be assigned to BoundFunc<{2}>.Null by default (to make sure the AOT compiler generates code for the binding)", type.Name, delegateFieldToBind.Name, returnType.Name);
                }
#endif
            }

            //UnityEngine.Debug.LogFormat ("Attempting to bind {0}.{1}", type.Name, delegateFieldToBind.Name);

            Type singletonType;
            FieldInfo staticSingletonField;
            PropertyInfo staticSingletonProperty;
            MethodInfo methodToCall;
            bool wasFound = findSingletonMethod (
                delegateFieldToBind,
                returnType,
                parameterTypes,
                out singletonType,
                out staticSingletonField,
                out staticSingletonProperty,
                out methodToCall);
            if (!wasFound)
            {
                wasFound = findStaticMethod (
                    delegateFieldToBind,
                    returnType,
                    parameterTypes,
                    out methodToCall
                    );
                if (!wasFound)
                {
                    throw new InvalidOperationException (string.Format ("Unable to bind {0}.{1}", type.Name, delegateFieldToBind.Name));
                }
                var staticDelegate = Delegate.CreateDelegate (delegateFieldToBindType, null, methodToCall);
                delegateFieldToBind.SetValue (null, staticDelegate);
                continue;
            }

            var dynamicDelegate = SingletonMethodCall.Create (
                staticSingletonField,
                staticSingletonProperty,
                delegateFieldToBindType,
                methodToCall);
            // UnityEngine.Debug.LogFormat ("Binding {0}", delegateFieldToBind.Name);
            delegateFieldToBind.SetValue (null, dynamicDelegate);
        }

    }

    private static void bindInstanceMethods (Type type)
    {
        MemberInfo[] members = type.FindMembers(
            MemberTypes.Field,
            BindingFlags.Static | BindingFlags.SetField | BindingFlags.Public | BindingFlags.NonPublic,
            delegate (MemberInfo memberInfo, object lastParameterOfFindMembers)
            {
                return memberInfo.MemberType == MemberTypes.Field &&
                    Attribute.IsDefined (memberInfo, typeof(Bind));
            },
            null);
        for (int j = 0; j < members.Length; ++j)
        {
            FieldInfo fieldInfo = (FieldInfo)members[j];
            Type fieldType = fieldInfo.FieldType;
            Type[] parameterTypes = GetDelegateParameterTypes(fieldType);
            bool hasNoParameters = parameterTypes.Length == 0;
            if (hasNoParameters)
            {
                continue;
            }
            Type[] restOfTheParameterTypes = new Type[parameterTypes.Length-1];
            if (restOfTheParameterTypes.Length > 0)
            {
                Array.Copy (parameterTypes, 1, restOfTheParameterTypes, 0, restOfTheParameterTypes.Length);
            }
            MethodInfo method = parameterTypes[0].GetMethod (
                    fieldInfo.Name,
                    BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public,
                    null,
                    restOfTheParameterTypes,
                    null);
            bool thisIsNotAnInstanceMethod = method == null;
            if (thisIsNotAnInstanceMethod)
            {
                continue;
            }
            Delegate delegateFunction = Delegate.CreateDelegate (fieldType, null, method);
            fieldInfo.SetValue (null, delegateFunction);
        }
    }
    

    private static Type[] GetDelegateParameterTypes(Type d)
    {
        if (d.BaseType != typeof(MulticastDelegate))
            throw new ApplicationException("Not a delegate.");

        MethodInfo invoke = d.GetMethod("Invoke");
        if (invoke == null)
            throw new ApplicationException("Not a delegate.");

        ParameterInfo[] parameters = invoke.GetParameters();
        Type[] typeParameters = new Type[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            typeParameters[i] = parameters[i].ParameterType;
        }
        return typeParameters;
    }

    private static Type GetDelegateReturnType(Type d)
    {
        if (d.BaseType != typeof(MulticastDelegate))
            throw new ApplicationException("Not a delegate.");

        MethodInfo invoke = d.GetMethod("Invoke");
        if (invoke == null)
            throw new ApplicationException("Not a delegate.");

        return invoke.ReturnType;
    }
}