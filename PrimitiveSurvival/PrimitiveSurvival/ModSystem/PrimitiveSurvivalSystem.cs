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
        public static List<string> chunkList;
        bool prevChunksLoaded;

        private ICoreServerAPI sapi;

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
            api.RegisterItemClass("itempsfish", typeof(ItemPSFish));
            api.RegisterItemClass("itemfisheggs", typeof(ItemFishEggs));
        }


        public override void StartServerSide(ICoreServerAPI api)
        {
            this.prevChunksLoaded = false;
            base.StartServerSide(api);
            this.sapi = api;

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

            api.Event.SaveGameLoaded += this.OnSaveGameLoading;
            api.Event.GameWorldSave += this.OnSaveGameSaving;
            var repleteTick = api.Event.RegisterGameTickListener(this.RepleteFishStocks, 60000 * ModConfig.Loaded.FishRepletionMinutes);
        }


        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.RegisterClasses(api);
        }


        private void OnSaveGameLoading()
        {
            fishingChunks = new Dictionary<IServerChunk, int>();
            // attempt to load the (short) list of all active fishing chunks
            var data = this.sapi.WorldManager.SaveGame.GetData("chunklist6");
            chunkList = data == null ? new List<string>() : SerializerUtil.Deserialize<List<string>>(data);

            /*
            foreach (var entry in chunkList)
            {
                Debug.WriteLine(entry);
            }
            */
        }


        private void OnSaveGameSaving()
        {
            var chunkcount = 0;
            foreach (var chunk in fishingChunks)
            {
                if (chunk.Value == 0)
                { continue; }

                chunk.Key.SetServerModdata(this.thisModID, SerializerUtil.Serialize(chunk.Value));
                chunkcount++;
            }
            //Debug.WriteLine("----------- Chunk depletion data saved to " + chunkcount + " chunks");

            // now attempt to save the (short) list of all active fishing chunks
            this.sapi.WorldManager.SaveGame.StoreData("chunklist6", SerializerUtil.Serialize(chunkList));
            /*
            Debug.WriteLine("----------- Chunk depletion data saved for " + chunkList.Count() + " chunks");
            foreach (var entry in chunkList)
            {
                Debug.WriteLine(entry);
            }
            */
        }

        private static void AddChunkToDictionary(IServerChunk chunk)
        {
            var data = chunk.GetServerModdata("primitivesurvival");
            var fishing = data == null ? 0 : SerializerUtil.Deserialize<int>(data);
            fishingChunks.Add(chunk, fishing);
        }


        public static void UpdateChunkInDictionary(ICoreServerAPI api, BlockPos pos, int rate)
        {
            //deplete
            var chunk = api.WorldManager.GetChunk(pos);
            if (!fishingChunks.ContainsKey(chunk))
            {
                AddChunkToDictionary(chunk);
            }
            var chunkindex = pos.X.ToString() + "," + pos.Y.ToString() + "," + pos.Z.ToString();
            if (!chunkList.Contains(chunkindex))
            {
                chunkList.Add(chunkindex);
            }
            if (0 <= fishingChunks[chunk] && fishingChunks[chunk] < ModConfig.Loaded.FishChunkMaxDepletionPercent)
            { fishingChunks[chunk] += rate; }

            if (fishingChunks[chunk] < 0)
            { fishingChunks[chunk] = 0; }
            if (fishingChunks[chunk] > ModConfig.Loaded.FishChunkMaxDepletionPercent)
            { fishingChunks[chunk] = ModConfig.Loaded.FishChunkMaxDepletionPercent; }

            chunk.MarkModified();

            //Debug
            /*
            var fishing = fishingChunks[chunk];
            var msg = "depleted (caught)";
            if (rate < 0)
            { msg = "repleted (escaped)"; }
            Debug.WriteLine("----------- Chunk " + pos.ToVec3d() + " - " + msg + ":" + fishing);
            */
        }

        private void RepleteFishStocks(float par)
        {
            if (!this.prevChunksLoaded)
            {
                var chunklistcount = 0;
                foreach (var chunk in chunkList)
                {
                    var coords = chunk.Split(',');
                    var pos = new BlockPos
                    {
                        X = coords[0].ToInt(),
                        Y = coords[1].ToInt(),
                        Z = coords[2].ToInt()
                    };

                    var getchunk = this.sapi.WorldManager.GetChunk(pos);
                    if (getchunk != null)
                    {
                        var getdata = getchunk.GetServerModdata("primitivesurvival");
                        var fishing = getdata == null ? 0 : SerializerUtil.Deserialize<int>(getdata);
                        if (!fishingChunks.ContainsKey(getchunk))
                        {
                            fishingChunks.Add(getchunk, fishing);
                            chunklistcount++;
                            this.prevChunksLoaded = true;
                        }
                        getchunk.MarkModified();
                    }
                }
                //Debug.WriteLine("Chunk data restored for " + chunklistcount + " chunks");
            }

            foreach (var key in fishingChunks.Keys.ToList())
            {
                fishingChunks[key] = fishingChunks[key] - ModConfig.Loaded.FishChunkRepletionRate;
                if (fishingChunks[key] < 0)
                { fishingChunks[key] = 0; }
                //Debug
               // Debug.WriteLine("----------- Chunk repletion:" + fishingChunks[key]);
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



