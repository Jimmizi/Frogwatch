using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Reflection
{
    public static T MakeFromType<T>(Type type)
    {
        return (T)Activator.CreateInstance(type);
    }

    public static T MakeFromType<T>(Type type, object arg)
    {
        return (T)Activator.CreateInstance(type, arg);
    }

    public static T MakeFromType<T>(Type type, object[] args)
    {
        return (T)Activator.CreateInstance(type, args);
    }
}

public class ReflectionOnType<T>
{
    public delegate void Callback(Type cSharpType);

    public delegate void CallbackWithAttribute<in AttrType>(Type type, AttrType attribute);

    public void ForEach(Callback cb)
    {
        OperateOnType(type => cb(type));
    }

    public void ForEach<AttributeType>(CallbackWithAttribute<AttributeType> cb) where AttributeType : Attribute
    {
        OperateOnType(type => cb(type, type.GetCustomAttribute<AttributeType>(false)));
    }

    public void ForSingle<AttributeType>(CallbackWithAttribute<AttributeType> cb) where AttributeType : Attribute
    {
        OperateOnType(type => cb(type, type.GetCustomAttribute<AttributeType>(false)), true);
    }

    public T Make(Type type)
    {
        return (T)Activator.CreateInstance(type);
    }

    public T Make(Type type, object arg)
    {
        return (T)Activator.CreateInstance(type, arg);
    }

    public T Make(Type type, object[] args)
    {
        return (T)Activator.CreateInstance(type, args);
    }

    private void OperateOnType(Action<Type> cb, bool onlySingle = false)
    {
        var serviceTypes = Assembly.GetAssembly(typeof(T)).GetTypes();
        foreach (var type in serviceTypes.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            cb(type);

            if (onlySingle)
            {
                return;
            }
        }
    }
}
