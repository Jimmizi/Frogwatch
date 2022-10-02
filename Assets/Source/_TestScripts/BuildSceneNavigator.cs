using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildSceneNavigator : MonoBehaviour
{
#if UNITY_EDITOR
    [Tooltip("If populated only navigates through these scenes, otherwise will navigate through all in the build list.")]
    public List<SceneAsset> ScenesToNavigate = new List<SceneAsset>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            LoadScene(GetNextScene(false));
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            LoadScene(GetNextScene(true));
        }
    }

    private int GetNextScene(bool pressedRight)
    {
        int sceneCount = 0;
        int currentSceneIndex = -1;
        string activeSceneName = SceneManager.GetActiveScene().name;

        if (ScenesToNavigate.IsEmpty())
        {
            sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            string[] scenes = new string[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                scenes[i] = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));

                if (scenes[i] == activeSceneName)
                {
                    currentSceneIndex = i;
                }
            }
        }
        else
        {
            sceneCount = ScenesToNavigate.Count;

            for (var i = 0; i < ScenesToNavigate.Count; ++i)
            {
                var scene = ScenesToNavigate[i];
                if (scene.name == activeSceneName)
                {
                    currentSceneIndex = i;
                }
            }
        }

        currentSceneIndex = pressedRight ? currentSceneIndex + 1 : currentSceneIndex - 1;

        if (currentSceneIndex >= sceneCount)
        {
            currentSceneIndex = 0;
        }
        else if (currentSceneIndex < 0)
        {
            currentSceneIndex = sceneCount - 1;
        }

        return currentSceneIndex;
    }

    private void LoadScene(int index)
    {
        if (ScenesToNavigate.IsEmpty())
        {
            SceneManager.LoadScene(index);
        }
        else
        {
            SceneManager.LoadScene(ScenesToNavigate[index].name);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(6, 6, 500, 24), $"Scene: {SceneManager.GetActiveScene().name}");
    }
#endif
}
