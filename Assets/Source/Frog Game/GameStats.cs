using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameStats
{
    public struct InternalStats
    {
        public float DistanceMoved;
        public int WitchesBonked;
        public int FrogsThrownIntoPonds;
    }

    public static InternalStats Stats => stats;
    private static InternalStats stats;

    public static void Reset()
    {
        stats = new InternalStats();
    }

    public static void AddDistanceMoved(float fDist)
    {
        stats.DistanceMoved += fDist;
    }

    public static void RecordWitchBonk()
    {
        stats.WitchesBonked++;
    }
    public static void RecordFrogThrownIntoPond()
    {
        stats.FrogsThrownIntoPonds++;
    }
}
