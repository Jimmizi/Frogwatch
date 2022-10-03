using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialVars : ServiceVars
{
    public Transform FrogSpawnPosition;

    public List<GameObject> TutorialBoundaries = new ();
    public BoxCollider2D TutorialFrogBounds;

    public BoxCollider2D TutorialPlayerBounds;
}
