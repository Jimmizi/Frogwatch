using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoggerSystem : SystemObject
{
    public static string Filename = "log.txt";

    private StreamWriter stringWriter;

    public void Write(string str)
    {
        //stringWriter.WriteLine(str);
    }

    //public static void ReadString()
    //{
    //    string path = Application.persistentDataPath + "/test.txt";
    //    StreamReader reader = new StreamReader(path);
    //    Debug.Log(reader.ReadToEnd());
    //    reader.Close();
    //}

    public override void AwakeService()
    {
        //string path = string.Format($"{Application.dataPath}/{Filename}");
        //stringWriter = new StreamWriter(path, false);
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

    public override void ShutdownService()
    {
        //stringWriter.Close();
    }
}
