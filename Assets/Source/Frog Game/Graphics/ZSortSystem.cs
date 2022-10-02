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
        Dictionary<int, SortedDictionary<float, List<ZSortObject>>> sortedYPositions = new();

        foreach (var obj in ActiveZSortObjects)
        {
            float flippedY = -obj.GetSortPosition().y;
            int roundedLayer = Mathf.FloorToInt(flippedY);

            if (!sortedYPositions.ContainsKey(roundedLayer))
            {
                sortedYPositions.Add(roundedLayer, new SortedDictionary<float, List<ZSortObject>>());
            }

            if (!sortedYPositions[roundedLayer].ContainsKey(flippedY))
            {
                sortedYPositions[roundedLayer].Add(flippedY, new List<ZSortObject>());
            }

            sortedYPositions[roundedLayer][flippedY].Add(obj);
        }

        int precisionPerLayer = 10;

        foreach (var sortedY in sortedYPositions)
        {
            int baseLayer = sortedY.Key * precisionPerLayer;

            foreach (var objInLayer in sortedY.Value)
            {
                foreach (var obj in objInLayer.Value)
                {
                    if (obj.IsFrog)
                    {
                        var frogController = obj.GetComponent<FrogController>();
                        if (frogController != null)
                        {
                            if (frogController.GetState() == FrogController.State.Carried)
                            {
                                continue;
                            }

                            if (frogController.GetState() == FrogController.State.Thrown && frogController.ShouldDrawInFrontDuringThrow())
                            {
                                obj.SpriteRend.sortingOrder = 999;
                                continue;
                            }
                        }
                    }

                    int precisionLayer = Mathf.FloorToInt((objInLayer.Key - Mathf.FloorToInt(objInLayer.Key)) * 10.0f);
                    int actualLayer = baseLayer + precisionLayer;

                    if (obj.IsPlayer || obj.IsWitch)
                    {
                        var humanCon = obj.GetComponent<HumanoidController>();
                        if (humanCon != null)
                        {
                            if (humanCon.IsCarryingFrog)
                            {
                                humanCon.FrogCarrying.ZSort.SpriteRend.sortingOrder = actualLayer + 1;
                            }
                        }
                    }

                    obj.SpriteRend.sortingOrder = actualLayer;
                }
            }
        }

    }

    public override void FixedUpdateService()
    {
        
    }
}
