using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashButtonPrompt : ButtonPromptBase
{
    protected override void Update()
    {
        base.Update();

        visible = player.DashIsUnlocked && player.CanDash() && player.FacingDirection != Vector2.zero;
    }
}
