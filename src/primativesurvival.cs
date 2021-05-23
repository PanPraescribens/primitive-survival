using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;


namespace primitiveSurvival
{
    public partial class PrimitiveSurvivalMod : ModSystem
    {
        string thisModID = "primitivesurvival";
        ICoreClientAPI capi;
        ICoreServerAPI sapi;

        public void RegisterClasses(ICoreAPI api)
        {
			api.RegisterEntity("entityearthworm", typeof(EntityEarthworm)); 
			
			api.RegisterBlockBehaviorClass ("RightClickPickupSpawnWorm", typeof(RightClickPickupSpawnWorm));
            api.RegisterBlockBehaviorClass("RightClickPickupRaft", typeof(RightClickPickupRaft));
            //api.RegisterBlockBehaviorClass("FiniteLiquid", typeof(FiniteLiquid));

            api.RegisterBlockEntityClass("bedeadfall", typeof(BEDeadfall));
            api.RegisterBlockEntityClass("besnare", typeof(BESnare));
            api.RegisterBlockEntityClass("belimbtrotlinelure", typeof(BELimbTrotLineLure));
            api.RegisterBlockEntityClass("befishbasket", typeof(BEFishBasket));
            api.RegisterBlockEntityClass("beweirtrap", typeof(BEWeirTrap));
            api.RegisterBlockEntityClass("betemporalbase", typeof(BETemporalBase));
            api.RegisterBlockEntityClass("betemporallectern", typeof(BETemporallectern));
            api.RegisterBlockEntityClass("betemporalcube", typeof(BETemporalCube));
            api.RegisterBlockEntityClass("bewoodsupportspikes", typeof(BEWoodSupportSpikes));
            api.RegisterBlockEntityClass("bealcove", typeof(BEAlcove));
            api.RegisterBlockEntityClass("bemetalbucket", typeof(BEMetalBucket));
            api.RegisterBlockEntityClass("bemetalbucketfilled", typeof(BEMetalBucketFilled));

            api.RegisterBlockClass("blockstake", typeof(BlockStake));
            api.RegisterBlockClass("blockstakeinwater", typeof(BlockStakeInWater));
            api.RegisterBlockClass("blockdeadfall", typeof(BlockDeadfall));
            api.RegisterBlockClass("blocksnare", typeof(BlockSnare));
            api.RegisterBlockClass("blocklimbtrotlinelure", typeof(BlockLimbTrotLineLure));
            api.RegisterBlockClass("blockfishbasket", typeof(BlockFishBasket));
            api.RegisterBlockClass("blockweirtrap", typeof(BlockWeirTrap));
            api.RegisterBlockClass("blocktemporal", typeof(BlockTemporal));
            api.RegisterBlockClass("blocktemporalbase", typeof(BlockTemporalBase));
            api.RegisterBlockClass("blocktemporallectern", typeof(BlockTemporallectern));
            api.RegisterBlockClass("blocktemporalcube", typeof(BlockTemporalCube));
            api.RegisterBlockClass("blockspiketrap", typeof(BlockSpikeTrap));
            api.RegisterBlockClass("blockwoodsupportspikes", typeof(BlockWoodSupportSpikes));
            api.RegisterBlockClass("blockpsstairs", typeof(BlockPSStairs));
            api.RegisterBlockClass("blockpspillar", typeof(BlockPSPillar));
            api.RegisterBlockClass("blocknsew", typeof(BlockNSEW));
            api.RegisterBlockClass("blockalcove", typeof(BlockAlcove));
			api.RegisterBlockClass("blockmetalbucket", typeof(BlockMetalBucket));
            api.RegisterBlockClass("blockmetalbucketfilled", typeof(BlockMetalBucketFilled));
            api.RegisterBlockClass("blockmonkeybridge", typeof(BlockMonkeyBridge));
            api.RegisterBlockClass("blockhide", typeof(BlockHide));
            api.RegisterBlockClass("blocknailspike", typeof(BlockNailSpike));
            api.RegisterBlockClass("blockraft", typeof(BlockRaft));
            api.RegisterBlockClass("blockblood", typeof(BlockBlood));

            api.RegisterItemClass("itemfishinghook", typeof(ItemFishingHook));
            api.RegisterItemClass("itemcordage", typeof(ItemCordage));
            api.RegisterItemClass("itemwoodspikebundle", typeof(ItemWoodSpikeBundle));
            api.RegisterItemClass("itempsgear", typeof(ItemPSGear));
            api.RegisterItemClass("itemmonkeybridge", typeof(ItemMonkeyBridge));
            api.RegisterItemClass("itemhide", typeof(ItemHide)); 
			api.RegisterItemClass ("itemearthworm", typeof(ItemEarthworm));
        }

        public override void StartServerSide(ICoreServerAPI Api)
        {
            sapi = Api;
        }

        public override void StartClientSide(ICoreClientAPI Api)
        {
            capi = Api;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            RegisterClasses(api);
            // Load/create common config file in ..\VintageStoryData\ModConfig\primitivesurvival.json
            string cfgFileName = thisModID + ".json";
            try
            {
                PrimitiveSurvivalConfig FromDisk;
                if ((FromDisk = api.LoadModConfig<PrimitiveSurvivalConfig>(cfgFileName)) == null)
                {    api.StoreModConfig<PrimitiveSurvivalConfig>(PrimitiveSurvivalConfig.Loaded, cfgFileName); }
                else
                {    PrimitiveSurvivalConfig.Loaded = FromDisk; }
            }
            catch
            {
                api.StoreModConfig<PrimitiveSurvivalConfig>(PrimitiveSurvivalConfig.Loaded, cfgFileName);
            }
        }
    }


    public class PrimitiveSurvivalConfig
    {
        public static PrimitiveSurvivalConfig Loaded { get; set; } = new PrimitiveSurvivalConfig();
        public bool altarDropsGold { get; set; } = true;

        public float deadfallMaxAnimalHeight { get; set; } = 0.7f;
        public int deadfallMaxDamageSet { get; set; } = 10;
        public int deadfallMaxDamageBaited { get; set; } = 20;

        public int fishBasketCatchPercent { get; set; } = 4;
        public int fishBasketBaitedCatchPercent { get; set; } = 10;
        public int fishBasketBaitStolenPercent { get; set; } = 5;
        public int fishBasketEscapePercent { get; set; } = 15;
        public double fishBasketUpdateMinutes { get; set; } = 2.2;
        public int fishBasketRotRemovedPercent { get; set; } = 10;

        public int limbTrotlineCatchPercent { get; set; } = 4;
        public int limbTrotlineBaitedCatchPercent { get; set; } = 10;
        public int limbTrotlineLuredCatchPercent { get; set; } = 7;
        public int limbTrotlineBaitedLuredCatchPercent { get; set; } = 13;
        public int limbTrotlineBaitStolenPercent { get; set; } = 5;
        public double limbTrotlineUpdateMinutes { get; set; } = 2.4;
        public int limbTrotlineRotRemovedPercent { get; set; } = 10;

        public float raftWaterSpeedModifier { get; set; } = 0.5f;
        public float raftFlotationModifier { get; set; } = 0.03f;

        public float snareMaxAnimalHeight { get; set; } = 0.8f;
        public int snareMaxDamageSet { get; set; } = 12;
        public int snareMaxDamageBaited { get; set; } = 24;

        public int weirTrapCatchPercent { get; set; } = 4;
         public int weirTrapEscapePercent { get; set; } = 10;
        public double weirTrapUpdateMinutes { get; set; } = 2.6;
        public int weirTrapRotRemovedPercent { get; set; } = 10;
    }
}



