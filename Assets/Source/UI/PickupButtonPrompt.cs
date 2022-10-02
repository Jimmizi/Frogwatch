using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupButtonPrompt : ButtonPromptBase
{
    protected override void Update()
    {
        base.Update();

        if (player != null)
        {
            Vector3 targetWorldPos = Vector3.zero;
            if (player.FrogCarrying)
            {
                transform.position = Camera.main.WorldToScreenPoint(player.transform.position) + Vector3.down * 50;
                actionText = "Throw";
                visible = true;
            }
            else
            {
                var nearestFrog = player.GetBestFrogWithinPickupDistance();
                if (nearestFrog != null)
                {
                    transform.position = Camera.main.WorldToScreenPoint(nearestFrog.transform.position) + Vector3.down * 10;
                    actionText = "Pick-up";
                    visible = true;
                }
                else
                {
                    visible = false;
                }
            }
        }
    }
}
