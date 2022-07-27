namespace PrimitiveSurvival.ModSystem
{
    using System;
    using System.Collections.Generic;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Server;
    using PrimitiveSurvival.ModConfig;
    //using System.Diagnostics;
    //using System.Linq;

    public class TreeHollows : ModSystem
    {
        private const int MinItems = 1;
        private const int MaxItems = 8;
        private ICoreServerAPI sapi; //The main interface we will use for interacting with Vintage Story
        private int chunkSize; //Size of chunks. Chunks are cubes so this is the size of the cube.
        private ISet<string> treeTypes; //Stores tree types that will be used for detecting trees for placing our tree hollows
        private IBlockAccessor chunkGenBlockAccessor; //Used for accessing blocks during chunk generation
        private IBlockAccessor worldBlockAccessor; //Used for accessing blocks after chunk generation

        private readonly string[] dirs = { "north", "south", "east", "west" };
        private readonly string[] woods = { "acacia", "birch", "kapok", "larch", "maple", "oak", "pine", "walnut" };





        public override void StartServerSide(ICoreServerAPI api)
        {
            this.sapi = api;
            this.worldBlockAccessor = api.World.BlockAccessor;
            this.chunkSize = this.worldBlockAccessor.ChunkSize;
            this.treeTypes = new HashSet<string>();
            this.LoadTreeTypes(this.treeTypes);

            //Registers our command with the system's command registry.
            //1.17 disable /hollow
            //this.sapi.RegisterCommand("hollow", "Place a tree hollow with random items", "", this.PlaceTreeHollowInFrontOfPlayer, Privilege.controlserver);
            //Registers a delegate to be called so we can get a reference to the chunk gen block accessor
            this.sapi.Event.GetWorldgenBlockAccessor(this.OnWorldGenBlockAccessor);
            //Registers a delegate to be called when a chunk column is generating in the Vegetation phase of generation
            this.sapi.Event.ChunkColumnGeneration(this.OnChunkColumnGeneration, EnumWorldGenPass.PreDone, "standard");
        }

        // Our mod only needs to be loaded by the server
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        private void LoadTreeTypes(ISet<string> treeTypes)
        {
            //var treeTypesFromFile = this.sapi.Assets.TryGet("worldproperties/block/wood.json").ToObject<StandardWorldProperty>();
            foreach (var variant in this.woods)
            {
                treeTypes.Add($"log-grown-" + variant + "-ud");
            }
        }

        /// <summary>
        /// Stores the chunk gen thread's IBlockAccessor for use when generating tree hollows during chunk gen. This callback
        /// is necessary because chunk loading happens in a separate thread and it's important to use this block accessor
        /// when placing tree hollows during chunk gen.
        /// </summary>
        private void OnWorldGenBlockAccessor(IChunkProviderThread chunkProvider)
        {
            this.chunkGenBlockAccessor = chunkProvider.GetBlockAccessor(true);
        }

        /// <summary>
        /// Called when a number of chunks have been generated. For each chunk we first determine if we should place a tree hollow
        /// and if we should we then loop through each block to find a tree. When one is found we place the block.
        /// </summary>
        private void OnChunkColumnGeneration(IServerChunk[] chunks, int chunkX, int chunkZ, ITreeAttribute chunkgenparams)
        {
            //Moved from PlaceTreeHollow to hopefully speed things up...a lot
            if (!this.ShouldPlaceHollow())
            { return; }

            //Debug.WriteLine("Entering the death loop for chunk " + chunkX + " " + chunkZ);
            var hollowsPlacedCount = 0;
            for (var i = 0; i < chunks.Length; i++)
            {
                var blockPos = new BlockPos();
                //arbitrarily limit x axis scan for performance reasons (/4)
                for (var x = 0; x < this.chunkSize / 4; x++)
                {
                    //arbitrarily limit z axis scan for performance reasons (/4)
                    for (var z = 0; z < this.chunkSize / 4; z++)
                    {
                        for (var y = 0; y < this.worldBlockAccessor.MapSizeY; y++)
                        {
                            if (hollowsPlacedCount < ModConfig.Loaded.TreeHollowsMaxPerChunk)
                            {
                                blockPos.X = (chunkX * this.chunkSize) + x;
                                blockPos.Y = y;
                                blockPos.Z = (chunkZ * this.chunkSize) + z;

                                var hollowLocation = this.TryGetHollowLocation(blockPos);
                                if (hollowLocation != null)
                                {
                                    var hollowWasPlaced = this.PlaceTreeHollow(this.chunkGenBlockAccessor, hollowLocation);
                                    if (hollowWasPlaced)
                                    {
                                        hollowsPlacedCount++;
                                    }
                                }
                            }
                            else //Max hollows have been placed for this chunk
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        // Returns the location to place the hollow if the given world coordinates is a tree, null if it's not a tree.
        private BlockPos TryGetHollowLocation(BlockPos pos)
        {
            var block = this.chunkGenBlockAccessor.GetBlock(pos, BlockLayersAccess.Default);
            if (this.IsTreeLog(block))
            {
                for (var posY = pos.Y; posY >= 0; posY--)
                {
                    while (pos.Y-- > 0)
                    {
                        var underBlock = this.chunkGenBlockAccessor.GetBlock(pos, BlockLayersAccess.Default);
                        if (this.IsTreeLog(underBlock))
                        {
                            continue;
                        }
                        return pos.UpCopy();
                    }
                }
            }
            return null;
        }

        private bool IsTreeLog(Block block)
        {
            return this.treeTypes.Contains(block.Code.Path);
        }

        // Delegate for /hollow command. Places a treehollow 2 blocks in front of the player
        private void PlaceTreeHollowInFrontOfPlayer(IServerPlayer player, int groupId, CmdArgs args)
        {
            this.PlaceTreeHollow(this.sapi.World.BlockAccessor, player.Entity.Pos.HorizontalAheadCopy(2).AsBlockPos);
        }

        // Places a tree hollow filled with random items at the given world coordinates using the given IBlockAccessor
        private bool PlaceTreeHollow(IBlockAccessor blockAccessor, BlockPos pos)
        {
            //Moved this to chunk gen to hopefully speed things up...a lot
            /*
            if (!this.ShouldPlaceHollow())
            {
                //Debug.WriteLine("cancelled!");
                return true;
            }
            */

            //consider moving it upwards
            var upCount = this.sapi.World.Rand.Next(4);
            var upCandidateBlock = blockAccessor.GetBlock(pos.UpCopy(upCount), BlockLayersAccess.Default);

            if (upCandidateBlock.FirstCodePart() == "log")
            { pos = pos.Up(upCount); }

            var treeBlock = blockAccessor.GetBlock(pos, BlockLayersAccess.Default);
            //Debug.WriteLine("Will replace:" + treeBlock.Code.Path);
            var woodType = "pine";

            if (treeBlock.FirstCodePart() == "log")
            {
                woodType = treeBlock.FirstCodePart(2);
            }

            var hollowType = "up";
            if (this.sapi.World.Rand.Next(2) == 1)
            { hollowType = "up2"; }
            var belowBlock = blockAccessor.GetBlock(pos.DownCopy(), BlockLayersAccess.Default);
            if (belowBlock.Fertility > 0) //fertile ground below?
            {
                if (this.sapi.World.Rand.Next(2) == 1)
                { hollowType = "base"; }
                else
                { hollowType = "base2"; }
            }

            var withPath = "primitivesurvival:treehollowgrown-" + hollowType + "-" + woodType + "-" + this.dirs[this.sapi.World.Rand.Next(4)];
            //Debug.WriteLine("With: " + withPath);
            var withBlockID = this.sapi.WorldManager.GetBlockId(new AssetLocation(withPath));
            var withBlock = blockAccessor.GetBlock(withBlockID);
            blockAccessor.SetBlock(0, pos);
            if (withBlock.TryPlaceBlockForWorldGen(blockAccessor, pos, BlockFacing.UP, null))
            {
                var block = blockAccessor.GetBlock(pos, BlockLayersAccess.Default) as BlockTreeHollowGrown;
                if (block.EntityClass != null)
                {
                    if (block.EntityClass == withBlock.EntityClass)
                    {
                        blockAccessor.SpawnBlockEntity(block.EntityClass, pos);
                        var be = blockAccessor.GetBlockEntity(pos);
                        if (be is BETreeHollowGrown)
                        {
                            var hollow = blockAccessor.GetBlockEntity(pos) as BETreeHollowGrown;
                            //hollow.Initialize(this.sapi); //SpawnBlockEntity does this already
                            var makeStacks = this.MakeItemStacks(hollowType, this.sapi);
                            if (makeStacks != null)
                            { this.AddItemStacks(hollow, makeStacks); }
                        }
                    }
                }
                return true;
            }
            else
            { return false; }
        }

        private bool ShouldPlaceHollow()
        {
            var randomNumber = this.sapi.World.Rand.Next(0, 100);
            return randomNumber > 0 && randomNumber <= ModConfig.Loaded.TreeHollowsSpawnProbability * 100;
        }

        // Makes a list of random ItemStacks to be placed inside our tree hollow
        public IEnumerable<ItemStack> MakeItemStacks(string hollowType, ICoreServerAPI sapi)
        {
            var shuffleBag = this.MakeShuffleBag(hollowType, sapi);
            var itemStacks = new Dictionary<string, ItemStack>();
            var minDrops = MinItems;
            if (ModConfig.Loaded.TreeHollowsMaxItems < MinItems)
            { minDrops = 0; }
            var maxDrops = MaxItems;
            if (ModConfig.Loaded.TreeHollowsMaxItems < MaxItems)
            { maxDrops = ModConfig.Loaded.TreeHollowsMaxItems; }
            var grabCount = sapi.World.Rand.Next(minDrops, maxDrops);
            for (var i = 0; i < grabCount; i++)
            {
                var nextItem = shuffleBag.Next();
                if (nextItem.Contains("item-"))
                {
                    nextItem = nextItem.Replace("item-", "");
                    var item = sapi.World.GetItem(new AssetLocation(nextItem));
                    if (itemStacks.ContainsKey(nextItem))
                    { itemStacks[nextItem].StackSize++; }
                    else
                    { itemStacks.Add(nextItem, new ItemStack(item)); }
                }
                else //block
                {
                    nextItem = nextItem.Replace("block-", "");
                    var item = this.sapi.World.GetBlock(new AssetLocation(nextItem));
                    if (itemStacks.ContainsKey(nextItem))
                    { itemStacks[nextItem].StackSize++; }
                    else
                    { itemStacks.Add(nextItem, new ItemStack(item)); }
                }
            }
            return itemStacks.Values;
        }

        //Adds the given list of ItemStacks to the first slots in the given hollow.
        public void AddItemStacks(IBlockEntityContainer hollow, IEnumerable<ItemStack> itemStacks)
        {
            var slotNumber = 0;
            if (itemStacks != null)
            {
                foreach (var itemStack in itemStacks)
                {
                    slotNumber = Math.Min(slotNumber, hollow.Inventory.Count - 1);
                    var slot = hollow.Inventory[slotNumber];
                    slot.Itemstack = itemStack;
                    slotNumber++;
                }
            }
        }

        // Creates our ShuffleBag to pick from when generating items for the tree hollow
        private ShuffleBag<string> MakeShuffleBag(string hollowType, ICoreServerAPI sapi)
        {
            var shuffleBag = new ShuffleBag<string>(100, sapi.World.Rand);
            if (hollowType.Contains("base"))
            {

                shuffleBag.Add("primitivesurvival:item-earthworm", 6);
                shuffleBag.Add("primitivesurvival:item-earthworm", 6);
                shuffleBag.Add("primitivesurvival:item-earthworm", 6);
                shuffleBag.Add("primitivesurvival:item-earthwormcastings", 6);
                shuffleBag.Add("primitivesurvival:item-earthwormcastings", 6);
                shuffleBag.Add("primitivesurvival:item-snake-blackrat", 1);
                shuffleBag.Add("primitivesurvival:item-snake-pitviper", 1);
                shuffleBag.Add("primitivesurvival:item-snake-coachwhip", 1);

                // 1.16
                // shuffleBag.Add("block-mushroom-bolete-normal-free", 3);
                // shuffleBag.Add("block-mushroom-fieldmushroom-normal-free", 5);

                shuffleBag.Add("block-mushroom-kingbolete-normal", 3);
                shuffleBag.Add("block-mushroom-fieldmushroom-normal", 5);
            }
            else
            {
                shuffleBag.Add("item-honeycomb", 3);
                shuffleBag.Add("item-resin", 4);
                shuffleBag.Add("item-stick", 4);
                shuffleBag.Add("item-insect-grub", 3);
                shuffleBag.Add("item-insect-termite", 3);
            }
            shuffleBag.Add("item-seeds-turnip", 3);
            shuffleBag.Add("item-seeds-onion", 3);
            shuffleBag.Add("item-seeds-peanut", 3);
            shuffleBag.Add("item-seeds-flax", 3);
            shuffleBag.Add("item-seeds-spelt", 3);
            shuffleBag.Add("item-seeds-pumpkin", 3);
            shuffleBag.Add("item-seeds-pineapple", 3);
            shuffleBag.Add("item-seeds-carrot", 3);
            shuffleBag.Add("item-seeds-rice", 3);
            shuffleBag.Add("item-seeds-rye", 3);
            shuffleBag.Add("item-seeds-cabbage", 3);
            shuffleBag.Add("item-seeds-cassava", 3);
            shuffleBag.Add("item-seeds-amaranth", 3);
            shuffleBag.Add("item-seeds-sunflower", 3);

            shuffleBag.Add("item-treeseed-oak", 3);
            shuffleBag.Add("item-treeseed-pine", 3);
            shuffleBag.Add("item-treeseed-larch", 3);
            shuffleBag.Add("item-treeseed-crimsonkingmaple", 3);
            shuffleBag.Add("item-treeseed-redwood", 3);
            shuffleBag.Add("item-treeseed-ebony", 3);
            shuffleBag.Add("item-treeseed-acacia", 3);
            shuffleBag.Add("item-treeseed-walnut", 3);
            shuffleBag.Add("item-treeseed-baldcypress", 3);
            shuffleBag.Add("item-treeseed-birch", 3);
            shuffleBag.Add("item-treeseed-maple", 3);
            shuffleBag.Add("item-treeseed-kapok", 3);
            return shuffleBag;
        }
    }
}
