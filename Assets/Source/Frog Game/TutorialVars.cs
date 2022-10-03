using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialVars : ServiceVars
{
    public Transform FrogSpawnPosition;

    public List<GameObject> TutorialBoundaries = new ();
    public BoxCollider2D TutorialFrogBounds;

    public BoxCollider2D TutorialPlayerBounds;

    public GameObject ToDisableWhenDone;

    public CanvasGroup MoveText;
    public BoxCollider2D MoveTextBounds;


    public void HideMoveText()
    {
        StartCoroutine(HideCanvasGroup(MoveText));
    }

    IEnumerator HideCanvasGroup(CanvasGroup group)
    {
        while (group.alpha > 0.0f)
        {
            group.alpha -= Time.deltaTime / 2;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        group.alpha = 0;
    }
}
