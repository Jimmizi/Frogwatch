using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashButtonPrompt : ButtonPromptBase
{
    bool IsPlayerMoving()
    {
        return player.InputDirection.x != 0.0f || player.InputDirection.y != 0.0f;
    }

    protected override void Update()
    {
        base.Update();

        visible = player.DashIsUnlocked && player.CanDash() && IsPlayerMoving();
    }
}
