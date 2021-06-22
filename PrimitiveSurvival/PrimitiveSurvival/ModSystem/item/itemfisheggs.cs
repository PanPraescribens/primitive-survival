namespace PrimitiveSurvival.ModSystem
{
    using System;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using PrimitiveSurvival.ModConfig;
 
    public class ItemFishEggs : Item
    {
        private readonly int RepleteRate = ModConfig.Loaded.FishEggsRepletionRate; //5
        private static readonly Random Rnd = new Random();

        //If you use the item on water, replete that chunk by RepleteRate
    }
}
