namespace PrimitiveSurvival.ModSystem
{
    using System;
    using System.Linq;
    using System.Text;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.Config;
    using Vintagestory.API.Server;
    using PrimitiveSurvival.ModConfig;
    using System.Diagnostics;

    public class BELimbTrotLineLure : BlockEntityDisplay
    {
        private readonly int catchPercent = ModConfig.Loaded.LimbTrotlineCatchPercent; //4
        private readonly int baitedCatchPercent = ModConfig.Loaded.LimbTrotlineBaitedCatchPercent; //10
        private readonly int luredCatchPercent = ModConfig.Loaded.LimbTrotlineLuredCatchPercent; //7
        private readonly int baitedLuredCatchPercent = ModConfig.Loaded.LimbTrotlineBaitedLuredCatchPercent; //13
        private readonly int baitStolenPercent = ModConfig.Loaded.LimbTrotlineBaitStolenPercent; //5
        private readonly double updateMinutes = ModConfig.Loaded.LimbTrotlineUpdateMinutes; //2.4
        private readonly int rotRemovedPercent = ModConfig.Loaded.LimbTrotlineRotRemovedPercent; //10

        private readonly int tickSeconds = 5;
        private readonly int maxSlots = 4;
        private readonly string[] baitTypes = { "fruit", "grain", "legume", "meat", "vegetable", "jerky", "mushroom", "bread", "poultry", "pickledvegetable", "redmeat", "bushmeat", "earthworm", "cheese", "fishfillet" };
        private readonly string[] fishTypes = { "trout", "perch", "carp", "bass", "pike", "arcticchar", "catfish", "bluegill" };
        private static readonly Random Rnd = new Random();

        private long particleTick;

        public override string InventoryClassName => "limbtrotlinelure";
        protected InventoryGeneric inventory;

        public override InventoryBase Inventory => this.inventory;

        private AssetLocation wetPickupSound;
        private AssetLocation dryPickupSound;

        public BELimbTrotLineLure()
        {
            this.inventory = new InventoryGeneric(this.maxSlots, null, null);
            this.meshes = new MeshData[this.maxSlots];
        }

        public ItemSlot HookSlot => this.inventory[0];

        public ItemSlot BaitSlot => this.inventory[1];

        public ItemSlot LureSlot => this.inventory[2];

        public ItemSlot CatchSlot => this.inventory[3];

        public ItemStack HookStack
        {
            get => this.inventory[0].Itemstack;
            set => this.inventory[0].Itemstack = value;
        }

        public ItemStack BaitStack
        {
            get => this.inventory[1].Itemstack;
            set => this.inventory[1].Itemstack = value;
        }

        public ItemStack LureStack
        {
            get => this.inventory[2].Itemstack;
            set => this.inventory[2].Itemstack = value;
        }

        public ItemStack CatchStack
        {
            get => this.inventory[3].Itemstack;
            set => this.inventory[3].Itemstack = value;
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side.IsServer())
            {
                this.particleTick = this.RegisterGameTickListener(this.ParticleUpdate, this.tickSeconds * 1000);
                var updateTick = this.RegisterGameTickListener(this.LimbTrotLineUpdate, (int)(this.updateMinutes * 60000));
            }
            this.wetPickupSound = new AssetLocation("game", "sounds/environment/smallsplash");
            this.dryPickupSound = new AssetLocation("game", "sounds/block/cloth");
        }


        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            this.UnregisterGameTickListener(this.particleTick);
        }


        private void GenerateWaterParticles(BlockPos pos, IWorldAccessor world)
        {
            float minQuantity = 1;
            float maxQuantity = 8;
            var color = ColorUtil.ToRgba(40, 125, 185, 255);
            var minPos = new Vec3d();
            var addPos = new Vec3d();
            var minVelocity = new Vec3f(0.1f, 0.0f, 0.1f);
            var maxVelocity = new Vec3f(0.6f, 0.1f, 0.6f);
            var lifeLength = 1f;
            var gravityEffect = 0f;
            var minSize = 0.1f;
            var maxSize = 0.3f;

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
            waterParticles.MinPos.Set(pos.ToVec3d().AddCopy(0.5, 0.2, 0.5));
            waterParticles.AddPos.Set(new Vec3d(0.1, -0.6, 0));
            waterParticles.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARINCREASE, 0.3f);
            waterParticles.ShouldDieInAir = true;
            waterParticles.SelfPropelled = true;
            world.SpawnParticles(waterParticles);
        }


        public void ParticleUpdate(float par)
        {
            if (!this.CatchSlot.Empty)
            {
                var belowblock = new BlockPos(this.Pos.X, this.Pos.Y - 1, this.Pos.Z);
                var belowBlock = this.Api.World.BlockAccessor.GetBlock(belowblock);
                if ((belowBlock.LiquidCode == "water") && (!belowBlock.Code.Path.Contains("inwater")))
                {
                    var rando = Rnd.Next(3);
                    if (rando == 0)
                    { this.GenerateWaterParticles(this.Pos, this.Api.World); }
                }
            }
        }


        private void LimbTrotLineUpdate(float par)
        {
            if (!this.HookSlot.Empty && this.CatchSlot.Empty)
            {
                var belowblock = new BlockPos(this.Pos.X, this.Pos.Y - 1, this.Pos.Z);
                var belowBlock = this.Api.World.BlockAccessor.GetBlock(belowblock);
                if ((belowBlock.LiquidCode == "water") && (!belowBlock.Code.Path.Contains("inwater")))
                {
                    var caught = Rnd.Next(100);
                    if (!this.BaitSlot.Empty)
                    {
                        var rando = Rnd.Next(100);
                        if (rando < this.baitStolenPercent)
                        {
                            if (!this.BaitSlot.Empty)
                            {
                                this.BaitSlot.TakeOutWhole();
                                this.GenerateWaterParticles(this.Pos, this.Api.World);
                                this.MarkDirty();
                                //Api.World.BlockAccessor.MarkBlockDirty(Pos);
                                //Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
                                //MarkDirty(true);
                            }
                        }
                        else
                        {
                            var toCatch = this.baitedCatchPercent;
                            if (!this.LureSlot.Empty)
                            { toCatch = this.baitedLuredCatchPercent; }
                            //System.Diagnostics.Debug.WriteLine("catch %" + toCatch);
                            if (caught < toCatch)
                            {
                                if (this.CatchSlot.Empty)
                                {
                                    /*********************************************/
                                    //depletion check
                                    var rate = PrimitiveSurvivalSystem.FishDepletedPercent(this.Api as ICoreServerAPI, this.Pos);
                                    rando = Rnd.Next(100);
                                    if (rando < rate) //depleted!
                                    { return; }
                                    else
                                    {
                                        // deplete
                                        PrimitiveSurvivalSystem.UpdateChunkInDictionary(this.Api as ICoreServerAPI, this.Pos, ModConfig.Loaded.FishChunkDepletionRate);
                                    }
                                    /*********************************************/
                                    Debug.WriteLine("fish on hook");
                                    this.CatchStack = new ItemStack(this.Api.World.GetItem(new AssetLocation("primitivesurvival:psfish-" + this.fishTypes[Rnd.Next(this.fishTypes.Count())] + "-raw")), 1);
                                    rando = Rnd.Next(2);
                                    if (rando == 0)
                                    { this.BaitSlot.TakeOutWhole(); }
                                    this.MarkDirty();
                                    //Api.World.BlockAccessor.MarkBlockDirty(Pos);
                                    //Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
                                    //MarkDirty(true);
                                }
                            }
                        }
                    }
                    else //hook
                    {
                        var toCatch = this.catchPercent;
                        if (!this.LureSlot.Empty)
                        { toCatch = this.luredCatchPercent; }
                        //System.Diagnostics.Debug.WriteLine("catch %" + toCatch);
                        if (caught < toCatch)
                        {
                            if (this.CatchSlot.Empty)
                            {
                                /*********************************************/
                                //depletion check
                                var rate = PrimitiveSurvivalSystem.FishDepletedPercent(this.Api as ICoreServerAPI, this.Pos);
                                var rando = Rnd.Next(100);
                                if (rando < rate) //depleted!
                                { return; }
                                else
                                {
                                    // deplete
                                    PrimitiveSurvivalSystem.UpdateChunkInDictionary(this.Api as ICoreServerAPI, this.Pos, ModConfig.Loaded.FishChunkDepletionRate);
                                }
                                /*********************************************/
                                Debug.WriteLine("fish on hook");
                                this.CatchStack = new ItemStack(this.Api.World.GetItem(new AssetLocation("primitivesurvival:psfish-" + this.fishTypes[Rnd.Next(this.fishTypes.Count())] + "-raw")), 1);
                                this.MarkDirty();
                                //Api.World.BlockAccessor.MarkBlockDirty(Pos);
                                //Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
                                //MarkDirty(true);
                            }
                        }
                    }
                }
            }
            if (!this.CatchSlot.Empty)
            {
                //remove rot?
                if (this.CatchStack.Item.Code.Path == "rot")
                {
                    var rando = Rnd.Next(100);
                    if (rando < this.rotRemovedPercent)
                    { this.CatchSlot.TakeOutWhole(); }
                    this.MarkDirty();
                    //Api.World.BlockAccessor.MarkBlockDirty(Pos);
                    //Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
                    //MarkDirty(true);
                }
            }
        }


        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            var belowblock = new BlockPos(this.Pos.X, this.Pos.Y - 1, this.Pos.Z);
            var belowBlock = this.Api.World.BlockAccessor.GetBlock(belowblock);
            if (playerSlot.Empty)
            {
                if (this.TryTake(byPlayer, blockSel))
                {
                    if ((belowBlock.LiquidCode == "water") && (!belowBlock.Code.Path.Contains("inwater")))
                    {
                        this.Api.World.PlaySoundAt(this.wetPickupSound, blockSel.Position.X, blockSel.Position.Y - 1, blockSel.Position.Z, byPlayer);
                    }
                    else
                    {
                        this.Api.World.PlaySoundAt(this.dryPickupSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                    }
                    return true;
                }
                return false;
            }
            else
            {
                var colObj = playerSlot.Itemstack.Collectible;
                //Debug.WriteLine("colobj:" + colObj.Attributes);
                if (colObj.Attributes != null)
                {
                    if (this.TryPut(playerSlot))
                    {
                        if ((belowBlock.LiquidCode == "water") && (!belowBlock.Code.Path.Contains("inwater")))
                        {
                            this.Api.World.PlaySoundAt(this.wetPickupSound, blockSel.Position.X, blockSel.Position.Y - 1, blockSel.Position.Z, byPlayer);
                        }
                        else
                        {
                            this.Api.World.PlaySoundAt(this.dryPickupSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                        }
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        internal void OnBreak(IPlayer byPlayer, BlockPos pos)
        {
            for (var index = 3; index >= 0; index--)
            {
                if (!this.inventory[index].Empty)
                {
                    var stack = this.inventory[index].TakeOut(1);
                    if (stack.StackSize > 0)
                    { this.Api.World.SpawnItemEntity(stack, this.Pos.ToVec3d().Add(0.5, 0.5, 0.5)); }
                    this.MarkDirty(true);
                }
            }
        }


        private bool TryPut(ItemSlot playerSlot)
        {
            var index = -1;
            var playerStack = playerSlot.Itemstack;
            if (this.inventory != null)
            {
                var stacks = this.inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
                if (stacks.Count() >= this.maxSlots)
                { return false; }
            }

            if (playerStack.Item != null)
            {
                //Debug.WriteLine("item:" + playerStack.Item.Code.Path);
                if (playerStack.Item.Code.Path.Contains("fishinghook") && this.HookSlot.Empty)
                {
                    if (!this.BaitSlot.Empty || !this.CatchSlot.Empty)
                    { return false; } //must be bare line
                    index = 0;
                }
                else if (playerStack.Item.Code.Path.Contains("fishinglure") && this.LureSlot.Empty)
                {
                    if (this.HookSlot.Empty)
                    { return false; } //must be a hook
                    index = 2;
                }
                else if (Array.IndexOf(this.baitTypes, playerStack.Item.FirstCodePart()) >= 0 && this.BaitSlot.Empty)
                {
                    if (this.HookSlot.Empty || !this.CatchSlot.Empty)
                    { return false; } //needs a hook and no fish
                    index = 1;
                }
                else if (playerStack.Item.Code.Path.Contains("psfish") && this.CatchSlot.Empty)
                {
                    if (this.HookSlot.Empty)
                    { return false; } //needs a hook
                    index = 3;
                }

                if (index > -1)
                {
                    var moved = playerSlot.TryPutInto(this.Api.World, this.inventory[index]);
                    if (moved > 0)
                    {
                        this.MarkDirty(true);
                        return moved > 0;
                    }
                }
            }
            else if (playerStack.Block != null)
            {
                //Debug.WriteLine("block:" + playerStack.Block.Code.Path);
                if (Array.IndexOf(this.baitTypes, playerStack.Block.FirstCodePart()) >= 0 && this.BaitSlot.Empty)
                {
                    if (this.HookSlot.Empty || !this.CatchSlot.Empty)
                    { return false; } //needs a hook and no fish
                    else
                    { index = 1; }
                }
                if (index > -1)
                {
                    var moved = playerSlot.TryPutInto(this.Api.World, this.inventory[index]);
                    if (moved > 0)
                    {
                        this.MarkDirty(true);
                        return moved > 0;
                    }
                }
            }
            return false;
        }



        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!this.CatchSlot.Empty)
            {
                //Debug.WriteLine("Grabbed a " + catchStack.Item.Code.Path);
                var rando = Rnd.Next(3);
                if (rando < 2)
                {
                    byPlayer.InventoryManager.TryGiveItemstack(this.CatchStack);
                    //ItemStack drop = catchStack.Clone();
                    //drop.StackSize = 1;
                    //Api.World.SpawnItemEntity(drop, new Vec3d(Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5), null); //slippery
                    //Api.World.SpawnItemEntity(catchStack, Pos.ToVec3d()); //slippery
                }
                else
                {
                    byPlayer.InventoryManager.TryGiveItemstack(this.CatchStack);
                }
                this.CatchSlot.TakeOutWhole();
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
            else if (!this.LureSlot.Empty)
            {
                byPlayer.InventoryManager.TryGiveItemstack(this.LureStack);
                this.LureSlot.TakeOutWhole();
                this.MarkDirty(true);
                return true;
            }
            else if (!this.HookSlot.Empty)
            {
                byPlayer.InventoryManager.TryGiveItemstack(this.HookStack);
                this.HookSlot.TakeOutWhole();
                this.MarkDirty(true);
                return true;
            }
            return false;
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


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (!this.CatchSlot.Empty)
            {
                sb.Append(Lang.Get("There's something on your hook."));
                if (!this.CatchStack.Item.Code.Path.Contains("psfish"))
                { sb.Append(" " + Lang.Get("Unfortunately, it smells a little funky.")); }
            }
            else
            {
                if (!this.HookSlot.Empty)
                {
                    var hookmsg = "-";
                    var baitmsg = "-";
                    var luremsg = "-";

                    if (!this.HookSlot.Empty)
                    { hookmsg = this.HookStack.GetName().Split('(', ')')[1]; }
                    if (!this.BaitSlot.Empty)
                    { baitmsg = this.BaitStack.GetName(); }
                    if (!this.LureSlot.Empty)
                    { luremsg = this.LureStack.GetName().Split('(', ')')[1]; }

                    sb.AppendLine(Lang.Get("Hook type") + ": " + hookmsg);
                    sb.AppendLine(Lang.Get("Bait type") + ": " + baitmsg);
                    sb.AppendLine(Lang.Get("Lure type") + ": " + luremsg);

                    sb.AppendLine();
                }

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
                else if (this.BaitSlot.Empty && !this.HookSlot.Empty)
                { sb.Append(Lang.Get("Bait it with some food to increase your odds of catching something.")); }
                else if (this.HookSlot.Empty)
                {
                    sb.Append(Lang.Get("Put a hook on that line if you expect to catch something.")).AppendLine().AppendLine();
                }
            }
            sb.AppendLine().AppendLine();
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh;
            var shapeBase = "primitivesurvival:shapes/";
            var shapePath = "";
            string hookType;
            string lureType;
            var alive = false;

            var block = this.Api.World.BlockAccessor.GetBlock(this.Pos) as BlockLimbTrotLineLure;
            Block tmpBlock;
            var texture = tesselator.GetTexSource(block);
            var tmpTextureSource = texture;

            if (block.Code.Path.Contains("-middle"))
            {
                shapePath = "block/limbtrotline/limbtrotline-middle";
                mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }
            if (block.Code.Path.Contains("small"))
            {
                shapePath = "block/limbtrotline/limbtrotline-end-small";
                mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }
            else if (block.Code.Path.Contains("medium"))
            {
                shapePath = "block/limbtrotline/limbtrotline-end-medium";
                mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }
            else if (block.Code.Path.Contains("large"))
            {
                shapePath = "block/limbtrotline/limbtrotline-end-large";
                mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }
            if (block.Code.Path.Contains("-withmiddle"))
            {
                shapePath = "block/limbtrotline/limbtrotline-end-extension";
                mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }

            if (this.inventory != null)
            {
                if (!this.HookSlot.Empty)
                {
                    hookType = this.HookStack.Item.LastCodePart().ToString();
                    var newPath = "texture" + hookType; //don't judge me!!!
                    var tempblock = this.Api.World.GetBlock(block.CodeWithPath(newPath));
                    tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tempblock);

                    shapePath = "item/fishing/fishinghook";
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }
                if (!this.LureSlot.Empty)
                {
                    lureType = this.LureStack.Item.LastCodePart().ToString();
                    var newPath = "texture" + lureType; //don't judge me!!!
                    var tempblock = this.Api.World.GetBlock(block.CodeWithPath(newPath));
                    tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tempblock);

                    shapePath = "item/fishing/fishinglure";
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }
                if (!this.BaitSlot.Empty)
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
                    shapePath = "item/fishing/hookbait"; //baited (for now)
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }
                if (!this.CatchSlot.Empty) //fish or rot
                {
                    if (!this.CatchStack.Item.Code.Path.Contains("psfish"))
                    {
                        tmpBlock = this.Api.World.GetBlock(block.CodeWithPath("texturerot"));
                        tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tmpBlock);
                        shapePath = "primitivesurvival:shapes/item/fishing/fish-pike";
                    }
                    else
                    {
                        shapePath = "primitivesurvival:shapes/item/fishing/fish-" + this.CatchStack.Item.LastCodePart(1).ToString();
                        if (this.CatchStack.Item.Code.Path.Contains("cooked"))
                        {
                            tmpBlock = this.Api.World.GetBlock(block.CodeWithPath("texturecooked"));
                            tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tmpBlock);
                        }
                        else
                        {
                            tmpTextureSource = texture;
                            alive = true;
                        }
                    }
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapePath, tmpTextureSource, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }
            }
            return true;
        }
    }
}

