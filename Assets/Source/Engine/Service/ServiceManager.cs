using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Service
{
    /// <summary>
    /// Get the service of T passed in. Class passed must inherit from ServiceSystem.
    /// </summary>
    public static T Get<T>() where T : SystemObject
    {
        return ServiceManager.instance.GetService<T>();
    }

    /// <summary>
    /// Check if the passed T type exists within the service provider.
    /// </summary>
    public static bool Has<T>() where T : SystemObject
    {
        return ServiceManager.instance.HasService<T>();
    }

    public static T Vars<T>() where T : ServiceVars
    {
        return ServiceManager.instance.GetVars<T>();
    }

    public static GameObject GetGO()
    {
        return ServiceManager.instance.gameObject;
    }
}

[DefaultExecutionOrder(-999)]
public class ServiceManager : MonoBehaviour
{
    public static ServiceManager instance;

    private SortedList<int, List<SystemObject>> serviceList = new SortedList<int, List<SystemObject>>();
    private Dictionary<string, SystemObject> namedServiceList = new Dictionary<string, SystemObject>();
    private Dictionary<Type, ServiceVars> serviceVarsList = new Dictionary<Type, ServiceVars>();

#if DEBUG
    public bool EnableLogging = true;
#endif

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            gameObject.name = "SERVICE";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        CacheAllServiceVars();
        CreateAllServices();

        CallServiceAwake();
    }


    void Start()
    {
        CallServiceStart();
    }
    
    void Update()
    {
        CallServiceUpdate();
    }

    void FixedUpdate()
    {
        CallServiceFixedUpdate();
    }

    void OnDestroy()
    {
        CallServiceShutdown();
    }

    public T GetService<T>() where T : SystemObject
    {
        Debug.Assert(namedServiceList.ContainsKey(typeof(T).Name));
        return namedServiceList[typeof(T).Name] as T;
    }

    public bool HasService<T>() where T : SystemObject
    {
        return namedServiceList.ContainsKey(typeof(T).Name);
    }
    
    public T GetVars<T>() where T : ServiceVars
    {
        Debug.Assert(serviceVarsList.ContainsKey(typeof(T)));
        return serviceVarsList[typeof(T)] as T;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CacheAllServiceVars();
    }

    private void CacheAllServiceVars()
    {
        List<GameObject> rootObjects = new List<GameObject>();
        Scene scene = SceneManager.GetActiveScene();
        scene.GetRootGameObjects(rootObjects);

        foreach (var go in rootObjects)
        {
            // Due to this being set as DontDestroyOnLoad we shouldn't receive this gameobject in the list
            //  but just do this to be safe in-case that changes
            if (go == gameObject)
            {
                continue;
            }

            ServiceVars[] goServiceVars = go.GetComponentsInChildren<ServiceVars>();

            if (goServiceVars.Length == 0)
            {
                continue;
            }

            foreach (var servVar in goServiceVars)
            {
                if (serviceVarsList.ContainsKey(servVar.GetType()))
                {
                    Log($"SERVICE: CacheAllServiceVars: Updating service vars '{servVar.GetType()}' received from new scene.");

                    GameObject previousVarsGo = serviceVarsList[servVar.GetType()].gameObject;

                    serviceVarsList.Remove(servVar.GetType());
                    Destroy(previousVarsGo);
                }

                Log($"SERVICE: CacheAllServiceVars: Adding service vars '{servVar.GetType()}'.");
                serviceVarsList.Add(servVar.GetType(), servVar);

                servVar.gameObject.transform.parent = gameObject.transform;
            }
        }

#if DEBUG
        ReflectionOnType<ServiceVars> reflectorServiceVars = new();
        reflectorServiceVars.ForEach<ServiceOrder>((type, attribute) =>
        {
            if (!serviceVarsList.ContainsKey(type))
            {
                Debug.LogWarning($"SERVICE: Found service vars type {type} that isn't being used. Is this on purpose?");
            }
        });

        bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        ReflectionOnType<SystemObject> reflector = new();
        reflector.ForEach((type) =>
        {
            if (IsSubclassOfRawGeneric(typeof(SystemObjectWithVars<>), type))
            {
                Debug.Assert(type.BaseType != null);

                var argsType = type.BaseType?.GetGenericArguments()[0];
                if (argsType == null || !serviceVarsList.ContainsKey(argsType))
                {
                    throw new Exception($"SERVICE: Service object '{type}' expects vars '{argsType}' which have not been found.");
                }
            }
        });
#endif
    }

    private void CreateAllServices()
    {
        Debug.Assert(serviceList.Count == 0);

        ReflectionOnType<SystemObject> reflector = new();
        reflector.ForEach<ServiceOrder>((type, attribute) =>
        {
            var serviceUpdateOrder = attribute?.Order ?? 0;

            if (!serviceList.ContainsKey(serviceUpdateOrder))
            {
                serviceList.Add(serviceUpdateOrder, new List<SystemObject>());
            }

            Log($"SERVICE: CreateAllServices: Creating service '{type}' with a update order of {serviceUpdateOrder}");
            var serv = Reflection.MakeFromType<SystemObject>(type);
            
            serviceList[serviceUpdateOrder].Add(serv);
            namedServiceList.Add(type.Name, serv);
        });
    }

    #region Service Function Calls

    private void CallServiceAwake()
    {
        foreach (var servicesAtPriority in serviceList)
        {
            foreach (var serv in servicesAtPriority.Value)
            {
                Log($"SERVICE: Calling awake on {serv.GetType()}");
                serv.AwakeService();
            }
        }
    }
    private void CallServiceStart()
    {
        foreach (var servicesAtPriority in serviceList)
        {
            foreach (var serv in servicesAtPriority.Value)
            {
                Log($"SERVICE: Calling start on {serv.GetType()}");
                serv.StartService();
            }
        }
    }
    private void CallServiceUpdate()
    {
        foreach (var servicesAtPriority in serviceList)
        {
            foreach (var serv in servicesAtPriority.Value)
            {
                serv.UpdateService();
            }
        }
    }
    private void CallServiceFixedUpdate()
    {
        foreach (var servicesAtPriority in serviceList)
        {
            foreach (var serv in servicesAtPriority.Value)
            {
                serv.FixedUpdateService();
            }
        }
    }

    private void CallServiceShutdown()
    {
        foreach (var servicesAtPriority in serviceList)
        {
            foreach (var serv in servicesAtPriority.Value)
            {
                serv.ShutdownService();
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var servicesAtPriority in serviceList)
        {
            foreach (var serv in servicesAtPriority.Value)
            {
                serv.OnDrawGizmos();
            }
        }
    }

    #endregion

    private void Log(object message)
    {
#if DEBUG
        if (EnableLogging)
        {
            Debug.Log(message);
        }
#endif
    }
}
