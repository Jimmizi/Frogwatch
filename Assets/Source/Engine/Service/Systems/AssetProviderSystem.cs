using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AssetProviderSystem : SystemObject
{
    public const string DefaultResourceFolder = "Assets/Resources";
    public const string DefaultResourcePath = "Resources";
    public const string DefaultScriptResourcePath = "Script";
    public const string DefaultConfigPath = "Config";

    private List<Config> loadedConfigs = new List<Config>();
    private List<TextAsset> rawScripts = new List<TextAsset>();

    public override void AwakeService()
    {
        LoadAllConfigs();
        LoadAllScripts();
    }

    public override void StartService()
    {
        
    }

    public override void UpdateService()
    {
        
    }

    public override void FixedUpdateService()
    {
        
    }

    public T GetConfig<T>() where T : Config
    {
        return loadedConfigs.First(x => x.GetType() == typeof(T)) as T;
    }

    public List<TextAsset> GetRawScripts()
    {
        return rawScripts;
    }

    private void LoadAllConfigs()
    {
        Type[] serviceTypes = Assembly.GetAssembly(typeof(Config)).GetTypes();
        Debug.Log("Loading all config files...");
        foreach (Type type in serviceTypes.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Config))))
        {
            loadedConfigs.Add(AddOrLoadConfig(type));
        }
    }

    private void LoadAllScripts()
    {
        TextAsset[] textFiles = Resources.LoadAll<TextAsset>(DefaultScriptResourcePath);
        rawScripts.AddRange(textFiles);
    }

    public static T AddOrLoadConfig<T>(string filepath = DefaultConfigPath) where T : Config
    {
        return AddOrLoadConfig(typeof(T), filepath) as T;
    }
    
    public static Config AddOrLoadConfig(Type type, string filepath = DefaultConfigPath)
    {
        var resourcePath = $"{filepath}/{type.Name}";
        var configAsset = Resources.Load(resourcePath, type) as Config;

#if UNITY_EDITOR
        Debug.Log($"AddOrLoadConfig: Loading config '{type}'...");

        if (!configAsset)
        {
            Debug.Log($"AddOrLoadConfig: Config of type '{type}' didn't exist in {filepath}. Creating a default version.");
            configAsset = ScriptableObject.CreateInstance(type) as Config;

            if (!AssetDatabase.Contains(configAsset))
            {
                Directory.CreateDirectory($"{DefaultResourceFolder}/{DefaultConfigPath}");
                AssetDatabase.CreateAsset(configAsset, $"{DefaultResourceFolder}/{DefaultConfigPath}/{type.Name}.asset");
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
                
        }
#else
        Debug.Log($"Failed loading config '{type}'. Cannot create asset at runtime.");
#endif

        return configAsset;
    }
}
