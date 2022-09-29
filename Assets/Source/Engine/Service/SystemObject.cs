using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

/// <summary>
/// Use to specify a custom processing order for this service object
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ServiceOrder : Attribute
{
    public int Order = 0;
}

/// <summary>
/// Specify this class as a ServiceObject that will be created at the start of runtime
/// </summary>
public abstract class SystemObject
{
    public abstract void AwakeService();
    public abstract void StartService();

    public abstract void UpdateService();
    public abstract void FixedUpdateService();
    //public abstract void OnSceneLoaded();

    public virtual void ShutdownService() {}
}

/// <summary>
/// Specify this as a service to be created at runtime, with monobehaviour vars class. Expects that a gameobject
/// with this ServiceVars child script is attached as a child to the SERVICE gameobject
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SystemObjectWithVars<T> : SystemObject
    where T : ServiceVars
{
    protected T GetVars()
    {
        return ServiceManager.instance.GetVars<T>();
    }
}

public abstract class ServiceVars : MonoBehaviour
{

}