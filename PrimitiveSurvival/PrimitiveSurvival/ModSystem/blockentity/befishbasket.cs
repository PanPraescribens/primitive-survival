namespace PrimitiveSurvival.ModSystem
{
    using System;
    using System.Linq;
    using System.Text;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent;
    using Vintagestory.API.Config;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.Server;
    using PrimitiveSurvival.ModConfig;


    public class BEFishBasket : BlockEntityDisplay
    {
        private readonly int catchPercent = ModConfig.Loaded.FishBasketCatchPercent; //4
        private readonly int baitedCatchPercent = ModConfig.Loaded.FishBasketBaitedCatchPercent; //10
        private readonly int baitStolenPercent = ModConfig.Loaded.FishBasketBaitStolenPercent; //5
        private readonly int escapePercent = ModConfig.Loaded.FishBasketEscapePercent; //15
        private readonly double updateMinutes = ModConfig.Loaded.FishBasketUpdateMinutes; //2.2
        private readonly int rotRemovedPercent = ModConfig.Loaded.FishBasketRotRemovedPercent; //10

        private readonly int tickSeconds = 4;
        private readonly int maxSlots = 3;
        private readonly string[] baitTypes = { "fruit", "grain", "legume", "meat", "vegetable", "jerky", "mushroom", "bread", "poultry", "pickledvegetable", "redmeat", "bushmeat", "earthworm", "cheese", "fishfillet" };
        private readonly string[] fishTypes = { "trout", "perch", "carp", "bass", "pike", "arcticchar", "catfish", "bluegill" };
        private readonly string[] shellStates = { "scallop", "sundial", "turritella", "clam", "conch", "seastar", "volute" };
        private readonly string[] shellColors = { "latte", "plain", "seafoam", "darkpurple", "cinnamon", "turquoise" };
        private readonly string[] relics = { "psgear-astral", "psgear-ethereal" };

        protected static readonly Random Rnd = new Random();

        public override string InventoryClassName => "fishbasket";

        protected InventoryGeneric inventory;

        public override InventoryBase Inventory => this.inventory;


        public BEFishBasket()
        {
            this.inventory = new InventoryGeneric(this.maxSlots, null, null);
            this.meshes = new MeshData[this.maxSlots];
        }


        public ItemSlot BaitSlot => this.inventory[0];

        public ItemSlot Catch1Slot => this.inventory[1];

        public ItemSlot Catch2Slot => this.inventory[2];

        public ItemStack BaitStack
        {
            get => this.inventory[0].Itemstack;
            set => this.inventory[0].Itemstack = value;
        }

        public ItemStack Catch1Stack
        {
            get => this.inventory[1].Itemstack;
            set => this.inventory[1].Itemstack = value;
        }

        public ItemStack Catch2Stack
        {
            get => this.inventory[2].Itemstack;
            set => this.inventory[2].Itemstack = value;
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side.IsServer())
            {
                this.RegisterGameTickListener(this.ParticleUpdate, this.tickSeconds * 1000);
                this.RegisterGameTickListener(this.FishBasketUpdate, (int)(this.updateMinutes * 60000));
            }
        }


        private void GenerateWaterParticles(int slot, string type, BlockPos pos, IWorldAccessor world)
        {
            float minQuantity = 1;
            float maxQuantity = 8;
            if (type == "seashell")
            { maxQuantity = 2; }
            var color = ColorUtil.ToRgba(40, 125, 185, 255);
            var minPos = new Vec3d();
            var addPos = new Vec3d();
            var minVelocity = new Vec3f(0.2f, 0.0f, 0.2f);
            var maxVelocity = new Vec3f(0.6f, 0.4f, 0.6f);
            var lifeLength = 5f;
            var gravityEffect = -0.1f;
            var minSize = 0.1f;
            var maxSize = 0.5f;

            var waterParticles = new SimpleParticleProperties(
                minQuantity, maxQuantity,
                color,
                minPos, addPos,
                minVelocity, maxVelocity,
                lifeLength,
                gravityEffect,
                minSize, maxSize,
                EnumParticleModel.Quad
            );
            if (type == "fish")
            {
                waterParticles.MinPos.Set(pos.ToVec3d().AddCopy(0.5, 0.2, 0.5));
                waterParticles.AddPos.Set(new Vec3d(0.1, 0, 0));
            }
            else
            { waterParticles.MinPos.Set(pos.ToVec3d().AddCopy(0.5, 0.2, 0.3)); }

            waterParticles.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARINCREASE, 0.5f);
            waterParticles.ShouldDieInAir = true;
            waterParticles.SelfPropelled = true;
            world.SpawnParticles(waterParticles);
        }


        public void ParticleUpdate(float par)
        {
            if (this.Block.Code.Path.Contains("inwater"))
            {
                var rando = Rnd.Next(2);
                if (rando == 0)
                {
                    for (var slot = 1; slot <= 2; slot++)
                    {
                        if (!this.inventory[slot].Empty)
                        {
                            if (this.inventory[slot].Itemstack.Item != null) //fish
                            { this.GenerateWaterParticles(slot, "fish", this.Pos, this.Api.World); }
                            else if (this.inventory[slot].Itemstack.Block != null) //seashell
                            { this.GenerateWaterParticles(slot, "seashell", this.Pos, this.Api.World); }
                        }
                    }
                }
            }
        }


        public void FishBasketUpdate(float par)
        {
            var waterAllAround = true;
            int rando;
            Block testBlock;
            var neibPos = new BlockPos[] { this.Pos.EastCopy(), this.Pos.NorthCopy(), this.Pos.SouthCopy(), this.Pos.WestCopy() };

            // Examine sides 
            foreach (var neib in neibPos)
            {
                testBlock = this.Api.World.BlockAccessor.GetBlock(neib);
                if (testBlock.LiquidCode != "water")
                { waterAllAround = false; }
            }
            if (waterAllAround)
            {
                int escaped;
                var caught = Rnd.Next(100);
                if (!this.BaitSlot.Empty)
                {
                    rando = Rnd.Next(100);
                    if (rando < this.baitStolenPercent)
                    {
                        if (this.WorldTake(0, this.Pos))
                        { this.GenerateWaterParticles(1, "fish", this.Pos, this.Api.World); }
                    }
                }
                var caughtOk = false;
                if ((!this.BaitSlot.Empty && caught < this.baitedCatchPercent) || (this.BaitSlot.Empty && caught < this.catchPercent))
                {
                    rando = Rnd.Next(2);
                    caughtOk = this.WorldPut(rando + 1, this.Pos);
                    if (!caughtOk)
                    {
                        rando = 1 - rando;
                        caughtOk = this.WorldPut(rando + 1, this.Pos);
                    }
                }
                if (!caughtOk && (!this.Catch1Slot.Empty || !this.Catch2Slot.Empty))
                {
                    escaped = Rnd.Next(100);
                    if (escaped < this.escapePercent)
                    {
                        var escapedOk = false;
                        rando = Rnd.Next(2);
                        escapedOk = this.WorldTake(rando + 1, this.Pos);
                        if (!escapedOk)
                        {
                            rando = 1 - rando;
                            escapedOk = this.WorldTake(rando + 1, this.Pos);
                        }
                        if (escapedOk)
                        { this.GenerateWaterParticles(2, "fish", this.Pos, this.Api.World); }
                    }
                }

                if (!this.Catch1Slot.Empty)
                {
                    if (this.Catch1Stack.Item != null)
                    {
                        if (this.Catch1Stack.Item.Code.Path == "rot")
                        {
                            escaped = Rnd.Next(100);
                            if (escaped < this.rotRemovedPercent)
                            { this.WorldTake(1, this.Pos); } //remove rot from slot 1
                        }
                    }
                }

                if (!this.Catch2Slot.Empty)
                {
                    if (this.Catch2Stack.Item != null)
                    {
                        if (this.Catch2Stack.Item.Code.Path == "rot")
                        {
                            escaped = Rnd.Next(100);
                            if (escaped < this.rotRemovedPercent)
                            { this.WorldTake(2, this.Pos); } //remove rot from slot 2
                        }
                    }
                }
            }
        }


        public bool WorldPut(int slot, BlockPos pos)
        {
            ItemStack newStack = null;
            var rando = Rnd.Next(5);
            if (rando < 1) // 20% chance of a seashell or relic
            {
                rando = Rnd.Next(10); //10

                if (rando < 1) //10% chance of a relic
                {
                    var thisRelic = this.relics[Rnd.Next(this.relics.Count())];
                    newStack = new ItemStack(this.Api.World.GetItem(new AssetLocation("primitivesurvival:" + thisRelic)), 1);
                }
                else
                { newStack = new ItemStack(this.Api.World.GetBlock(new AssetLocation("game:seashell-" + this.shellStates[Rnd.Next(this.shellStates.Count())] + "-" + this.shellColors[Rnd.Next(this.shellColors.Count())])), 1); }
            }
            else
            { newStack = new ItemStack(this.Api.World.GetItem(new AssetLocation("primitivesurvival:psfish-" + this.fishTypes[Rnd.Next(this.fishTypes.Count())] + "-raw")), 1); }
            if (this.inventory[slot].Empty)
            {
                if (newStack.Collectible.Code.Path.Contains("psfish"))
                {
                    /*********************************************/
                    //depletion check last
                    var rate = PrimitiveSurvivalSystem.FishDepletedPercent(this.Api as ICoreServerAPI, this.Pos);
                    rando = Rnd.Next(100);
                    if (rando < rate) //depleted!
                    { return false; }
                    else
                    {
                        // deplete
                        PrimitiveSurvivalSystem.UpdateChunkInDictionary(this.Api as ICoreServerAPI, this.Pos, ModConfig.Loaded.FishChunkDepletionRate);
                    }
                    /*********************************************/
                }

                this.inventory[slot].Itemstack = newStack;
                //Api.World.BlockAccessor.MarkBlockDirty(pos);
                this.MarkDirty(true);
                return true;
            }
            return false;
        }


        public bool WorldTake(int slot, BlockPos pos)
        {
            if (!this.inventory[slot].Empty)
            {
                /*********************************************/
                //Debug.WriteLine("Escaped: " + inventory[slot].Itemstack.Collectible.Code.Path);
                if (this.inventory[slot].Itemstack.Collectible.Code.Path.Contains("psfish"))
                {
                    //replete (at deplete rate)
                    PrimitiveSurvivalSystem.UpdateChunkInDictionary(this.Api as ICoreServerAPI, this.Pos, -ModConfig.Loaded.FishChunkDepletionRate);
                }
                /*********************************************/
                this.inventory[slot].TakeOutWhole();
                this.Api.World.BlockAccessor.MarkBlockDirty(pos);
                this.MarkDirty();
                return true;
            }
            return false;
        }


        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty)
            {
                if (this.TryTake(byPlayer))
                { return true; }
                return false;
            }
            else
            {
                var colObj = slot.Itemstack.Collectible;
                if (colObj.Attributes != null)
                {
                    if (this.TryPut(slot, blockSel))
                    { return true; }
                    return false;
                }
            }
            return false;
        }


        private bool TryPut(ItemSlot playerSlot, BlockSelection blockSel)
        {
            var index = -1;
            var moved = 0;
            var playerStack = playerSlot.Itemstack;
            if (this.inventory != null)
            {
                var stacks = this.inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
                if (stacks.Count() >= this.maxSlots)
                { return false; }
            }

            if (playerStack.Item != null)
            {
                //Debug.WriteLine("Putting a " + playerStack.Item.FirstCodePart());
                if (Array.IndexOf(this.baitTypes, playerStack.Item.FirstCodePart()) >= 0 && this.BaitSlot.Empty)
                { index = 0; }
                else if (playerStack.Item.Code.Path.Contains("psfish"))
                {
                    if (this.Catch1Slot.Empty)
                    { index = 1; }
                    else if (this.Catch2Slot.Empty)
                    { index = 2; }
                }
            }
            else if (playerStack.Block != null)
            {
                if (Array.IndexOf(this.baitTypes, playerStack.Block.FirstCodePart()) >= 0 && this.BaitSlot.Empty)
                { index = 0; }
                else if (playerStack.Block.Code.Path.Contains("seashell"))
                {
                    if (this.Catch1Slot.Empty)
                    { index = 1; }
                    else if (this.Catch2Slot.Empty)
                    { index = 2; }
                }
            }
            if (index >= 0)
            {
                moved = playerSlot.TryPutInto(this.Api.World, this.inventory[index]);
                if (moved > 0)
                {
                    this.MarkDirty(true);
                    return moved > 0;
                }
            }
            return false;
        }


        private bool TryTake(IPlayer byPlayer)
        {
            if (!this.Catch2Slot.Empty)
            {
                var rando = Rnd.Next(3);
                if (rando < 2 && this.Catch2Stack.Item != null) //fish
                {
                    //ItemStack drop = catch2Stack.Clone();
                    //drop.StackSize = 1;
                    //Api.World.SpawnItemEntity(drop, new Vec3d(Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5), null);
                    byPlayer.InventoryManager.TryGiveItemstack(this.Catch2Stack);
                }
                else
                { byPlayer.InventoryManager.TryGiveItemstack(this.Catch2Stack); }
                this.Catch2Slot.TakeOutWhole();
                this.MarkDirty(true);
                return true;
            }
            else if (!this.Catch1Slot.Empty)
            {
                var rando = Rnd.Next(3);
                if (rando < 2 && this.Catch1Stack.Item != null) //fish
                {
                    //ItemStack drop = catch1Stack.Clone();
                    //drop.StackSize = 1;
                    //Api.World.SpawnItemEntity(drop, new Vec3d(Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5), null);
                    byPlayer.InventoryManager.TryGiveItemstack(this.Catch1Stack);
                }
                else
                {
                    byPlayer.InventoryManager.TryGiveItemstack(this.Catch1Stack);
                }
                this.Catch1Slot.TakeOutWhole();
                this.MarkDirty(true);
                return true;
            }
            else if (!this.BaitSlot.Empty)
            {
                byPlayer.InventoryManager.TryGiveItemstack(this.BaitStack);
                this.BaitSlot.TakeOutWhole();
                this.MarkDirty(true);
                return true;
            }
            return false;
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            var block = this.Api.World.BlockAccessor.GetBlock(this.Pos);
            if (block.Code.Path.Contains("inwater"))
            {
                var rot = false;
                if (!this.Catch1Slot.Empty || !this.Catch2Slot.Empty)
                {
                    sb.Append(Lang.Get("There's something in your basket."));
                    if (!this.Catch1Slot.Empty)
                    {
                        if (this.Catch1Stack.Block != null)
                        { }
                        else if (!this.Catch1Stack.Item.Code.Path.Contains("psfish"))
                        { sb.Append(" " + Lang.Get("It smells a little funky in there.")); }
                        rot = true;
                    }
                    if (!this.Catch2Slot.Empty && !rot)
                    {
                        if (this.Catch2Stack.Block != null)
                        { }
                        else if (!this.Catch2Stack.Item.Code.Path.Contains("psfish"))
                        { sb.Append(" " + Lang.Get("It smells a little funky in there.")); }
                    }
                    sb.AppendLine().AppendLine();
                }
                else
                {
                    if (!this.BaitSlot.Empty)
                    {
                        if (this.BaitStack.Item != null)
                        {
                            if (Array.IndexOf(this.baitTypes, this.BaitStack.Item.FirstCodePart()) < 0)
                            { sb.Append(Lang.Get("Your bait has gone rotten. Replace it with fresh bait.")); }
                            else
                            { sb.Append(Lang.Get("It's baited so your odds of catching something are pretty good.")); }
                        }
                        else if (this.BaitStack.Block != null)
                        {
                            if (Array.IndexOf(this.baitTypes, this.BaitStack.Block.FirstCodePart()) < 0)
                            { sb.Append(Lang.Get("Your bait has gone rotten. Replace it with fresh bait.")); }
                            else
                            { sb.Append(Lang.Get("It's baited so your odds of catching something are pretty good.")); }
                        }
                    }
                    else if (this.BaitSlot.Empty)
                    { sb.Append(Lang.Get("Bait it with some food to increase your odds of catching something.")); }
                    sb.AppendLine().AppendLine();
                }
            }
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
        {
            base.FromTreeAttributes(tree, worldForResolve);
            if (this.Api != null)
            {
                if (this.Api.Side == EnumAppSide.Client)
                { this.Api.World.BlockAccessor.MarkBlockDirty(this.Pos); }
            }
        }


        // Note: There's a bug "of sorts" if the water isn't full block fishbasket is land type not water type...
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh;
            string shapePath;
            var alive = false;
            Block tmpBlock;
            var block = this.Api.World.BlockAccessor.GetBlock(this.Pos) as BlockFishBasket;
            var texture = tesselator.GetTexSource(block);
            var tmpTextureSource = texture;
            shapePath = "primitivesurvival:shapes/block/fishbasket/fishbasket";
            mesh = block.GenMesh(this.Api as ICoreClientAPI, shapePath, texture, -1, alive, tesselator);
            mesher.AddMeshData(mesh);

            if (this.inventory != null)
            {
                if (!this.BaitSlot.Empty) //bait or rot
                {
                    if (this.BaitStack.Item != null)
                    {
                        if (Array.IndexOf(this.baitTypes, this.BaitStack.Item.FirstCodePart()) < 0)
                        {
                            var tempblock = this.Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tempblock);
                        }
                        else
                        { tmpTextureSource = texture; }
                    }
                    else if (this.BaitStack.Block != null)
                    {
                        if (Array.IndexOf(this.baitTypes, this.BaitStack.Block.FirstCodePart()) < 0)
                        {
                            var tempblock = this.Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tempblock);
                        }
                        else
                        { tmpTextureSource = texture; }
                    }
                    shapePath = "primitivesurvival:shapes/item/fishing/hookbait"; //baited (for now)
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapePath, tmpTextureSource, 0, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }

                for (var i = 1; i <= 2; i++)
                {
                    shapePath = "";
                    alive = false;
                    if (!this.inventory[i].Empty) //fish, shell, or rot
                    {
                        if (this.inventory[i].Itemstack.Block != null) //shell
                        {
                            shapePath = "game:shapes/block/seashell/" + this.inventory[i].Itemstack.Block.FirstCodePart(1);
                            tmpTextureSource = tesselator.GetTexSource(this.inventory[i].Itemstack.Block);
                        }
                        else if (this.inventory[i].Itemstack.Item.Code.Path.Contains("gear"))
                        {
                            var gearType = this.inventory[i].Itemstack.Item.FirstCodePart(1);
                            tmpBlock = this.Api.World.GetBlock(block.CodeWithPath("texture" + gearType));
                            if (gearType != "rusty")
                            { gearType = "temporal"; }
                            shapePath = "game:shapes/item/gear-" + gearType;
                            tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tmpBlock);
                        }
                        else if (!this.inventory[i].Itemstack.Item.Code.Path.Contains("psfish"))
                        {
                            tmpBlock = this.Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tmpBlock);
                            shapePath = "primitivesurvival:shapes/item/fishing/fish-pike";
                        }
                        else if (this.inventory[i].Itemstack.Item.Code.Path.Contains("psfish"))
                        {
                            shapePath = "primitivesurvival:shapes/item/fishing/fish-" + this.inventory[i].Itemstack.Item.LastCodePart(1).ToString();
                            if (this.inventory[i].Itemstack.Item.Code.Path.Contains("cooked"))
                            {
                                tmpBlock = this.Api.World.GetBlock(block.CodeWithPath("texturecooked"));
                                tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tmpBlock);
                            }
                            else
                            {
                                alive = true;
                                tmpTextureSource = texture;
                            }
                        }
                        if (shapePath != "")
                        {
                            mesh = block.GenMesh(this.Api as ICoreClientAPI, shapePath, tmpTextureSource, i, alive, tesselator);
                            mesher.AddMeshData(mesh);
                        }
                    }
                }
            }
            return true;
        }
    }
}
