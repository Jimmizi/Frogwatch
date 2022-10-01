using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZSortSystem : SystemObject
{
    public List<ZSortObject> ActiveFrogs = new();

    public override void AwakeService()
    {
        
    }

    public override void StartService()
    {
        
    }

    public override void UpdateService()
    {
        Dictionary<int, SortedDictionary<float, ZSortObject>> sortedYPositions = new();

        foreach (var frog in ActiveFrogs)
        {
            float flippedY = -frog.GetSortPosition().y;
            int roundedLayer = Mathf.FloorToInt(flippedY);

            if (!sortedYPositions.ContainsKey(roundedLayer))
            {
                sortedYPositions.Add(roundedLayer, new SortedDictionary<float, ZSortObject>());
            }

            sortedYPositions[roundedLayer].Add(flippedY, frog);
        }

        int precisionPerLayer = 10;

        foreach (var sortedY in sortedYPositions)
        {
            int baseLayer = sortedY.Key * precisionPerLayer;

            foreach (var frogInLayer in sortedY.Value)
            {
                int precisionLayer = Mathf.FloorToInt((frogInLayer.Key - Mathf.FloorToInt(frogInLayer.Key)) * 10.0f);
                frogInLayer.Value.SpriteRend.sortingOrder = baseLayer + precisionLayer;
            }
        }

    }

    public override void FixedUpdateService()
    {
        
    }
}
