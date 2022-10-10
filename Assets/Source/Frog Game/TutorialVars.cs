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

    public CanvasGroup ChargeText;
    public CanvasGroup ThrowText;

    public void StageOneText()
    {
        StartCoroutine(HideCanvasGroup(MoveText));
        StartCoroutine(ShowCanvasGroup(ChargeText));
    }
    public void StageTwoText()
    {
        StartCoroutine(HideCanvasGroup(ChargeText));
        StartCoroutine(ShowCanvasGroup(ThrowText));
    }

    IEnumerator HideCanvasGroup(CanvasGroup group)
    {
        while (group.alpha > 0.0f)
        {
            group.alpha -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        group.alpha = 0;
    }

    IEnumerator ShowCanvasGroup(CanvasGroup group)
    {
        while (group.alpha < 1.0f)
        {
            group.alpha += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        group.alpha = 1;
    }

    public void StartWitchFrogFadeIn(SpriteRenderer witch, SpriteRenderer frog)
    {
        StartCoroutine(DoWitchFrogFadeIn(witch, frog));
    }
    IEnumerator DoWitchFrogFadeIn(SpriteRenderer witch, SpriteRenderer frog)
    {
        Color c = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        while (c.a < 1.0f)
        {
            c.a += Time.deltaTime;
            witch.color = c;
            frog.color = c;

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}
