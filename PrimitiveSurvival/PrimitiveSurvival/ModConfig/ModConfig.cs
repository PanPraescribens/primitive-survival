namespace PrimitiveSurvival.ModConfig
{
    public class ModConfig
    {
        public static ModConfig Loaded { get; set; } = new ModConfig();
        public bool AltarDropsGold { get; set; } = true;
        public float DeadfallMaxAnimalHeight { get; set; } = 0.7f;
        public int DeadfallMaxDamageSet { get; set; } = 10;
        public int DeadfallMaxDamageBaited { get; set; } = 20;
        public int FallDamageMultiplierWoodSpikes { get; set; } = 25;
        public int FallDamageMultiplierMetalSpikes { get; set; } = 80;
        public int FishBasketCatchPercent { get; set; } = 4;
        public int FishBasketBaitedCatchPercent { get; set; } = 10;
        public int FishBasketBaitStolenPercent { get; set; } = 5;
        public int FishBasketEscapePercent { get; set; } = 15;
        public double FishBasketUpdateMinutes { get; set; } = 2.2;
        public int FishBasketRotRemovedPercent { get; set; } = 10;
        public int FishChunkDepletionRate { get; set; } = 5;
        public int FishChunkRepletionRate { get; set; } = 1;
        public int LimbTrotlineCatchPercent { get; set; } = 4;
        public int LimbTrotlineBaitedCatchPercent { get; set; } = 10;
        public int LimbTrotlineLuredCatchPercent { get; set; } = 7;
        public int LimbTrotlineBaitedLuredCatchPercent { get; set; } = 13;
        public int LimbTrotlineBaitStolenPercent { get; set; } = 5;
        public double LimbTrotlineUpdateMinutes { get; set; } = 2.4;
        public int LimbTrotlineRotRemovedPercent { get; set; } = 10;
        public float RaftWaterSpeedModifier { get; set; } = 0.5f;
        public float RaftFlotationModifier { get; set; } = 0.03f;
        public float SnareMaxAnimalHeight { get; set; } = 0.8f;
        public int SnareMaxDamageSet { get; set; } = 12;
        public int SnareMaxDamageBaited { get; set; } = 24;
        public int WeirTrapCatchPercent { get; set; } = 4;
        public int WeirTrapEscapePercent { get; set; } = 10;
        public double WeirTrapUpdateMinutes { get; set; } = 2.6;
        public int WeirTrapRotRemovedPercent { get; set; } = 10;
        public int WormFoundPercentRock { get; set; } = 6;
        public int WormFoundPercentStickFlint { get; set; } = 16;

    }
}
