using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PondDropArea : MonoBehaviour
{
    readonly float frogSizeX = 0.32f;
    readonly float frogSizeY = 0.16f;

    public static List<PondDropArea> Ponds = new();
    public static List<BoxCollider2D> Areas = new();

    public bool IsLarge = false;

    public BoxCollider2D CollectionBox;
    public BoxCollider2D DisplayBox;

    private List<Vector2> FrogSitOffsetsOriginal = new();

    public List<Vector2> FreeOffsets = new();
    public Dictionary<FrogController, Vector2> FilledSlots = new();
    public List<FrogController> ExtraFrogs = new();

    public int GetNumFrogsInPond()
    {
        return FilledSlots.Count + ExtraFrogs.Count;
    }

    void Start()
    {
        Ponds.Add(this);
        Areas.Add(CollectionBox);
        
        float fRandomXOffset = frogSizeX / 4.0f;
        float fRandomYOffset = frogSizeY / 4.0f;

        float fRandomXOffsetHalf = fRandomXOffset / 2.0f;
        float fRandomYOffsetHalf = fRandomYOffset / 2.0f;

        Vector2 vWorldPos = new Vector2(DisplayBox.transform.position.x, DisplayBox.transform.position.y) + DisplayBox.offset;
        float halfHori = (DisplayBox.size.x / 2.0f);
        float halfVert = (DisplayBox.size.y / 2.0f);

        // Top left to bottom right solving
        float fStartX = -halfHori + (frogSizeX / 2.0f);
        float fStartY = halfVert - (frogSizeY / 2.0f);

        int iAmountX = Mathf.RoundToInt(DisplayBox.size.x / frogSizeX);
        int iAmountY = Mathf.RoundToInt(DisplayBox.size.y / frogSizeY);

        // Populate grid points for frogs
        for (int y = 0; y < iAmountY; ++y)
        {
            for (int x = 0; x < iAmountX; ++x)
            {
                Vector2 vOffset = Vector2.zero;

                vOffset.x = fStartX + (x * frogSizeX);
                vOffset.y = fStartY - (y * frogSizeY);

                //Vector2 vRandomOffsetFromOffset = Random.insideUnitCircle.normalized * 0.25f;

                // Too close to sides, don't randomise towards side
                if (x == 0)
                {
                    vOffset.x += Random.Range(fRandomXOffsetHalf, fRandomXOffset);
                }
                else if (x == iAmountX - 1)
                {
                    vOffset.x -= Random.Range(fRandomXOffsetHalf, fRandomXOffset);
                }
                else
                {
                    vOffset.x += Random.Range(-fRandomXOffset, fRandomXOffset);
                }

                if (y == 0)
                {
                    vOffset.y -= Random.Range(fRandomYOffsetHalf, fRandomYOffset);
                }
                else if (y == iAmountY - 1)
                {
                    vOffset.y += Random.Range(fRandomYOffsetHalf, fRandomYOffset);
                }
                else
                {
                    vOffset.y += Random.Range(-fRandomYOffset, fRandomYOffset);
                }

                FrogSitOffsetsOriginal.Add(vOffset);
                FreeOffsets.Add(vOffset);
            }
        }
    }

    void OnDestroy()
    {
        Ponds.Remove(this);
        Areas.Remove(CollectionBox);
    }

    public void AddFrog(FrogController frog)
    {
        Vector2 vWorldPos = new Vector2(DisplayBox.transform.position.x, DisplayBox.transform.position.y) + DisplayBox.offset;

        // FilledSlots will give the positions back as free points when removing the frog
        if (FreeOffsets.Count > 0)
        {
            Vector2 vClosestOffset = GetClosestFreeOffset(frog.GetOffsetPosition());
            FreeOffsets.Remove(vClosestOffset);
            
            FilledSlots.Add(frog, vClosestOffset);
            frog.ExternalSetPosition(vWorldPos + vClosestOffset);
        }
        // ExtraFrogs won't do this
        else
        {
            Vector2 vRandomOffset = GetRandomOffsetInBox();

            ExtraFrogs.Add(frog);
            frog.ExternalSetPosition(vWorldPos + vRandomOffset);
        }
    }

    public void RemoveFrog(FrogController frog)
    {
        if (FilledSlots.ContainsKey(frog))
        {
            Vector2 vPositionToFree = FilledSlots[frog];
            FilledSlots.Remove(frog);

            FreeOffsets.Add(vPositionToFree);
        }
        else if (ExtraFrogs.Contains(frog))
        {
            ExtraFrogs.Remove(frog);
        }
        else
        {
            Debug.LogError("Trying to remove frog from pond it isn't in.");
        }
    }

    public static PondDropArea GetOverlapped(Vector2 point)
    {
        foreach (var box in Areas)
        {
            if (box.OverlapPoint(point))
            {
                return box.GetComponent<PondDropArea>();
            }
        }

        return null;
    }

    private Vector2 GetClosestFreeOffset(Vector2 pos)
    {
        Vector2 vWorldPos = new Vector2(DisplayBox.transform.position.x, DisplayBox.transform.position.y) + DisplayBox.offset;

        Vector2 vBestOffset = Vector2.zero;
        float fBestDist = float.PositiveInfinity;

        foreach (var offset in FreeOffsets)
        {
            float dist = Vector2.Distance(vWorldPos + offset, pos);
            if (dist < fBestDist)
            {
                fBestDist = dist;
                vBestOffset = offset;
            }
        }

        return vBestOffset;
    }

    private Vector2 GetRandomOffsetInBox()
    {
        float halfHori = (DisplayBox.size.x / 2) - frogSizeX;
        float halfVert = (DisplayBox.size.y / 2) - frogSizeY;

        return new Vector2(halfHori * Random.Range(-1.0f, 1.0f), halfVert * Random.Range(-1.0f, 1.0f));
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (DisplayBox != null)
            {
                Vector2 vWorldPos = new Vector2(DisplayBox.transform.position.x, DisplayBox.transform.position.y) + DisplayBox.offset;

                Gizmos.color = Color.gray;
                foreach (var offset in FrogSitOffsetsOriginal)
                {
                    Gizmos.DrawSphere(vWorldPos + offset, 0.01f);
                }
            }
        }
    }
}
