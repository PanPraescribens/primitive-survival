namespace PrimitiveSurvival.ModSystem
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    using Vintagestory.API.Util;
    using Vintagestory.API.MathTools;
    using PrimitiveSurvival.ModConfig;

    public class PrimitiveSurvivalSystem : ModSystem
    {
        private readonly string thisModID = "primitivesurvival";
        private static Dictionary<IServerChunk, int> fishingChunks;

        public void RegisterClasses(ICoreAPI api)
        {
            api.RegisterEntity("entityearthworm", typeof(EntityEarthworm));

            api.RegisterBlockBehaviorClass("RightClickPickupSpawnWorm", typeof(RightClickPickupSpawnWorm));
            api.RegisterBlockBehaviorClass("RightClickPickupRaft", typeof(RightClickPickupRaft));

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

            api.RegisterItemClass("itemcordage", typeof(ItemCordage));
            api.RegisterItemClass("itemwoodspikebundle", typeof(ItemWoodSpikeBundle));
            api.RegisterItemClass("itempsgear", typeof(ItemPSGear));
            api.RegisterItemClass("itemmonkeybridge", typeof(ItemMonkeyBridge));
            api.RegisterItemClass("itemhide", typeof(ItemHide));
            api.RegisterItemClass("itemearthworm", typeof(ItemEarthworm));
        }

        public override void StartServerSide(ICoreServerAPI Api)
        {
            Api.Event.SaveGameLoaded += this.OnSaveGameLoading;
            Api.Event.GameWorldSave += this.OnSaveGameSaving;
            Api.Event.RegisterGameTickListener(this.RepleteFishStocks, 6000);
        }


        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.RegisterClasses(api);
            // Load/create common config file in ..\VintageStoryData\ModConfig\primitivesurvival.json
            var cfgFileName = this.thisModID + ".json";
            try
            {
                ModConfig fromDisk;
                if ((fromDisk = api.LoadModConfig<ModConfig>(cfgFileName)) == null)
                { api.StoreModConfig(ModConfig.Loaded, cfgFileName); }
                else
                { ModConfig.Loaded = fromDisk; }
            }
            catch
            {
                api.StoreModConfig(ModConfig.Loaded, cfgFileName);
            }
        }


        private void OnSaveGameLoading()
        {
            fishingChunks = new Dictionary<IServerChunk, int>();
        }

        private void OnSaveGameSaving()
        {
            foreach (var chunk in fishingChunks)
            {
                if (chunk.Value == 0)
                { continue; }
                chunk.Key.SetServerModdata(this.thisModID, SerializerUtil.Serialize(chunk.Value));
            }
        }

        private static void AddChunkToDictionary(IServerChunk chunk)
        {
            var data = chunk.GetServerModdata("primitivesurvival");
            var fishing = data == null ? 0 : SerializerUtil.Deserialize<int>(data);
            fishingChunks.Add(chunk, fishing);
        }

        public static void UpdateChunkInDictionary(ICoreServerAPI api, BlockPos Pos, int rate)
        {
            //deplete
            var chunk = api.WorldManager.GetChunk(Pos);
            if (!fishingChunks.ContainsKey(chunk))
            { AddChunkToDictionary(chunk); }
            if (0 <= fishingChunks[chunk] && fishingChunks[chunk] <= 100)
            { fishingChunks[chunk] += rate; }

            if (fishingChunks[chunk] < 0)
            { fishingChunks[chunk] = 0; }
            if (fishingChunks[chunk] > 100)
            { fishingChunks[chunk] = 100; }

            //Debug
            var fishing = fishingChunks[chunk];
            var msg = "depleted (caught)";
            if (rate < 0)
            { msg = "repleted (escaped)"; }
            Debug.WriteLine("Chunk " + msg + ":" + fishing);
        }

        private void RepleteFishStocks(float par)
        {
            foreach (var key in fishingChunks.Keys.ToList())
            {
                fishingChunks[key] = fishingChunks[key] - ModConfig.Loaded.FishChunkRepletionRate;
                if (fishingChunks[key] < 0)
                { fishingChunks[key] = 0; }
                //Debug
                Debug.WriteLine("Chunk repletion:" + fishingChunks[key]);
            }
        }

        public static int FishDepletedPercent(ICoreServerAPI api, BlockPos pos)
        {
            var rate = 0;
            var chunk = api.WorldManager.GetChunk(pos);
            if (fishingChunks.ContainsKey(chunk))
            {
                rate = fishingChunks[chunk];
            }
            return rate;
        }
    }
}



