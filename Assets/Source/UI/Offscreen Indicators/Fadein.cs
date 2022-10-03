using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]

public class Fadein : MonoBehaviour
{
    private CanvasGroup myUIGroup;
    

    private void Start()
    {
        myUIGroup = GetComponent<CanvasGroup>();
    }
    void Update()
    {
        if (Service.Get<TutorialSystem>().IsTutorialActive)
        {
            myUIGroup.alpha = 0;
        } else
        {
            myUIGroup.alpha += Time.deltaTime;
        }
    }
}

