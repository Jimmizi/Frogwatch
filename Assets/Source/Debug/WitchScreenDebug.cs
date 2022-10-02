using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WitchScreenDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
        Vector2 currentPos = new Vector2(315, 30);

        void DrawText(string text)
        {
            GUI.Label(new Rect(currentPos, new Vector2(500, 24)), text);
            currentPos.y += 16;
        }

        void DrawWitchDebug(EnemyController e)
        {
            DrawText($"Witch: {e.name}");

            currentPos.x = 325;
            {
                DrawText($"State: {e.GetState()}");

                if (e.targetFrog != null)
                {
                    DrawText($"Target: {e.targetFrog.name}");
                }
            }
            currentPos.x = 315;

            currentPos.y += 48.0f;
        }

        foreach (var hc in HumanoidController.Controllers)
        {
            var wc = hc as EnemyController;
            if (wc != null)
            {
                //DrawWitchDebug(wc);
            }
        }

        // Stats debug
        
        //currentPos = new Vector2(15, 30);
        //DrawText($"Traveled: {GameStats.Stats.DistanceMoved}");
    }
}
