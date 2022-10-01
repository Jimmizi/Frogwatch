using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZSortSystem : SystemObject
{
    public List<ZSortObject> ActiveZSortObjects = new();

    public override void AwakeService()
    {
        
    }

    public override void StartService()
    {
        
    }

    public override void UpdateService()
    {
        Dictionary<int, SortedDictionary<float, ZSortObject>> sortedYPositions = new();

        foreach (var obj in ActiveZSortObjects)
        {
            float flippedY = -obj.GetSortPosition().y;
            int roundedLayer = Mathf.FloorToInt(flippedY);

            if (!sortedYPositions.ContainsKey(roundedLayer))
            {
                sortedYPositions.Add(roundedLayer, new SortedDictionary<float, ZSortObject>());
            }

            sortedYPositions[roundedLayer].Add(flippedY, obj);
        }

        int precisionPerLayer = 10;

        foreach (var sortedY in sortedYPositions)
        {
            int baseLayer = sortedY.Key * precisionPerLayer;

            foreach (var objInLayer in sortedY.Value)
            {
                if (objInLayer.Value.IsFrog)
                {
                    var frogController = objInLayer.Value.GetComponent<FrogController>();
                    if (frogController != null)
                    {
                        if (frogController.GetState() == FrogController.State.Carried)
                        {
                            continue;
                        }

                        if (frogController.GetState() == FrogController.State.Thrown && frogController.ShouldDrawInFrontDuringThrow())
                        {
                            objInLayer.Value.SpriteRend.sortingOrder = 999;
                            continue;
                        }
                    }
                }

                int precisionLayer = Mathf.FloorToInt((objInLayer.Key - Mathf.FloorToInt(objInLayer.Key)) * 10.0f);
                int actualLayer = baseLayer + precisionLayer;

                if (objInLayer.Value.IsPlayer || objInLayer.Value.IsWitch)
                {
                    var humanCon = objInLayer.Value.GetComponent<HumanoidController>();
                    if (humanCon != null)
                    {
                        if (humanCon.IsCarryingFrog)
                        {
                            humanCon.FrogCarrying.ZSort.SpriteRend.sortingOrder = actualLayer + 1;
                        }
                    }
                }

                objInLayer.Value.SpriteRend.sortingOrder = actualLayer;
            }
        }

    }

    public override void FixedUpdateService()
    {
        
    }
}
