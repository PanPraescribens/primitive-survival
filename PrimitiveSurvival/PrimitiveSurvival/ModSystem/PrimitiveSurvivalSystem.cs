namespace PrimitiveSurvival.ModSystem
{
    using System.Linq;
    using System.Collections.Generic;
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    using Vintagestory.API.Client;
    using Vintagestory.API.Util;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent;
    using PrimitiveSurvival.ModConfig;
    using Vintagestory.Client.NoObf;
    //using System.Diagnostics;


    public class PrimitiveSurvivalSystem : ModSystem
    {
        public IShaderProgram EntityGenericShaderProgram { get; private set; }

        private readonly string thisModID = "primitivesurvival";
        private static Dictionary<IServerChunk, int> fishingChunks;

        public static List<string> chunkList;

        private bool prevChunksLoaded;

        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;

        //readonly IShaderProgram overlayShaderProg;
        private VenomOverlayRenderer vrenderer;

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;

            this.capi.Event.ReloadShader += this.LoadCustomShaders;
            this.LoadCustomShaders();

            this.capi.RegisterEntityRendererClass("entitygenericshaperenderer", typeof(EntityGenericShapeRenderer));

            this.vrenderer = new VenomOverlayRenderer(api);
            api.Event.RegisterRenderer(this.vrenderer, EnumRenderStage.Ortho);
        }

        public bool LoadCustomShaders()
        {

            this.EntityGenericShaderProgram = this.capi.Shader.NewShaderProgram();
            // 1.17 My custom shader broke, but the built in shader works really well
            (this.EntityGenericShaderProgram as ShaderProgram).AssetDomain = "game"; // this.Mod.Info.ModID;
            //this.capi.Shader.RegisterFileShaderProgram("entitygenericshader", this.EntityGenericShaderProgram);
            this.capi.Shader.RegisterFileShaderProgram("entityanimated", this.EntityGenericShaderProgram);
            this.EntityGenericShaderProgram.Compile();
            return true;
        }

        public void RegisterClasses(ICoreAPI api)
        {
            api.RegisterEntity("entityearthworm", typeof(EntityEarthworm));
            api.RegisterEntity("entityfireflies", typeof(EntityFireflies));
            api.RegisterEntity("entitypsglowingagent", typeof(EntityPSGlowingAgent));
            api.RegisterEntity("entitylivingdead", typeof(EntityLivingDead));
            api.RegisterEntity("entitygenericglowingagent", typeof(EntityGenericGlowingAgent));
            api.RegisterEntity("entityskullofthedead", typeof(EntitySkullOfTheDead));
            api.RegisterEntity("entitywillowisp", typeof(EntityWillowisp));
            api.RegisterEntity("entitybioluminescent", typeof(EntityBioluminescent));

            api.RegisterEntityBehaviorClass("carryable", typeof(EntityBehaviorCarryable));

            AiTaskRegistry.Register("meleeattackvenomous", typeof(AiTaskMeleeAttackVenomous));
            AiTaskRegistry.Register("meleeattackcrab", typeof(AiTaskMeleeAttackCrab));

            api.RegisterBlockBehaviorClass("RightClickPickupSpawnWorm", typeof(RightClickPickupSpawnWorm));
            api.RegisterBlockBehaviorClass("RightClickPickupRaft", typeof(RightClickPickupRaft));
            api.RegisterBlockBehaviorClass("RightClickPickupFireflies", typeof(RightClickPickupFireflies));

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
            api.RegisterBlockEntityClass("befireflies", typeof(BEFireflies));
            api.RegisterBlockEntityClass("befirework", typeof(BEFirework));
            api.RegisterBlockEntityClass("befuse", typeof(BEFuse));
            api.RegisterBlockEntityClass("beparticulator", typeof(BEParticulator));
            api.RegisterBlockEntityClass("betreehollowgrown", typeof(BETreeHollowGrown));
            api.RegisterBlockEntityClass("betreehollowplaced", typeof(BETreeHollowPlaced));
            api.RegisterBlockEntityClass("bebombfuse", typeof(BEBombFuse));
            api.RegisterBlockEntityClass("besupport", typeof(BESupport));
            api.RegisterBlockEntityClass("beirrigationvessel", typeof(BEIrrigationVessel));

            api.RegisterBlockClass("blockstake", typeof(BlockStake));
            api.RegisterBlockClass("blockfuse", typeof(BlockFuse));
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
            api.RegisterBlockClass("blockraft", typeof(BlockRaft));
            api.RegisterBlockClass("blockblood", typeof(BlockBlood));
            api.RegisterBlockClass("blockfireflies", typeof(BlockFireflies));
            api.RegisterBlockClass("blockfirework", typeof(BlockFirework));
            api.RegisterBlockClass("blockparticulator", typeof(BlockParticulator));
            api.RegisterBlockClass("blockbstairs", typeof(BlockBStairs));
            api.RegisterBlockClass("blocktreehollowgrown", typeof(BlockTreeHollowGrown));
            api.RegisterBlockClass("blocktreehollowplaced", typeof(BlockTreeHollowPlaced));
            api.RegisterBlockClass("blockhandofthedead", typeof(BlockHandOfTheDead));
            api.RegisterBlockClass("blockskullofthedead", typeof(BlockSkullOfTheDead));
            api.RegisterBlockClass("blockbombfuse", typeof(BlockBombFuse));
            api.RegisterBlockClass("blocksupport", typeof(BlockSupport));
            api.RegisterBlockClass("blockpipe", typeof(BlockPipe));
            api.RegisterBlockClass("blockirrigationvessel", typeof(BlockIrrigationVessel));

            api.RegisterItemClass("itemcordage", typeof(ItemCordage));
            api.RegisterItemClass("itemfuse", typeof(ItemFuse));
            api.RegisterItemClass("itemwoodspikebundle", typeof(ItemWoodSpikeBundle));
            api.RegisterItemClass("itempsgear", typeof(ItemPSGear));
            api.RegisterItemClass("itemmonkeybridge", typeof(ItemMonkeyBridge));
            api.RegisterItemClass("itemhide", typeof(ItemHide));
            api.RegisterItemClass("itemearthworm", typeof(ItemEarthworm));
            api.RegisterItemClass("itemsnake", typeof(ItemSnake));
            api.RegisterItemClass("itemcrab", typeof(ItemCrab));
            api.RegisterItemClass("itempsfish", typeof(ItemPSFish));
            api.RegisterItemClass("itemfisheggs", typeof(ItemFishEggs));
            api.RegisterItemClass("itemlinktool", typeof(ItemLinkTool));
            api.RegisterItemClass("itemlivingdead", typeof(ItemLivingDead));
            api.RegisterItemClass("itemwillowisp", typeof(ItemWillowisp));
            api.RegisterItemClass("itembioluminescent", typeof(ItemBioluminescent));
            api.RegisterItemClass("itemstick", typeof(ItemStick));
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
            var repleteTick = api.Event.RegisterGameTickListener(this.RepleteFishStocks, 60000 * ModConfig.Loaded.FishChunkRepletionMinutes);
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
            var data = this.sapi.WorldManager.SaveGame.GetData("chunklist");
            chunkList = data == null ? new List<string>() : SerializerUtil.Deserialize<List<string>>(data);
            /*
            foreach (var entry in chunkList)
            { Debug.WriteLine(entry); }
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
            this.sapi.WorldManager.SaveGame.StoreData("chunklist", SerializerUtil.Serialize(chunkList));
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






