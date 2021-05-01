using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using System.Diagnostics;

namespace primitiveSurvival
{
    public class BELimbTrotLineLure : BlockEntityDisplay
    {
        public int catchPercent = PrimitiveSurvivalConfig.Loaded.limbTrotlineCatchPercent; //4
        public int baitedCatchPercent = PrimitiveSurvivalConfig.Loaded.limbTrotlineBaitedCatchPercent; //10
        public int luredCatchPercent = PrimitiveSurvivalConfig.Loaded.limbTrotlineLuredCatchPercent; //7
        public int baitedLuredCatchPercent = PrimitiveSurvivalConfig.Loaded.limbTrotlineBaitedLuredCatchPercent; //13
        public int baitStolenPercent = PrimitiveSurvivalConfig.Loaded.limbTrotlineBaitStolenPercent; //5
        public double updateMinutes = PrimitiveSurvivalConfig.Loaded.limbTrotlineUpdateMinutes; //2.4
        public int rotRemovedPercent = PrimitiveSurvivalConfig.Loaded.limbTrotlineRotRemovedPercent; //10

        public int tickSeconds = 5;
        public int maxSlots = 4;
        public string[] baitTypes = { "fruit", "grain", "legume", "meat", "vegetable", "jerky", "mushroom", "bread", "poultry", "pickledvegetable", "redmeat", "bushmeat", "earthworm", "cheese", "fishfillet" };
        public string[] fishTypes = { "trout", "perch", "carp", "bass", "pike", "arcticchar", "catfish", "bluegill" };

        public static Random rnd = new Random();

        public override string InventoryClassName
        {
            get { return "limbtrotlinelure"; }
        }
        protected InventoryGeneric inventory;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public BELimbTrotLineLure()
        {
            inventory = new InventoryGeneric(maxSlots, null, null);
            meshes = new MeshData[maxSlots];
        }


        public ItemSlot hookSlot
        {
            get { return inventory[0]; }
        }

        public ItemSlot baitSlot
        {
            get { return inventory[1]; }
        }

        public ItemSlot lureSlot
        {
            get { return inventory[2]; }
        }

        public ItemSlot catchSlot
        {
            get { return inventory[3]; }
        }

        public ItemStack hookStack
        {
            get { return inventory[0].Itemstack; }
            set { inventory[0].Itemstack = value; }
        }

        public ItemStack baitStack
        {
            get { return inventory[1].Itemstack; }
            set { inventory[1].Itemstack = value; }
        }

        public ItemStack lureStack
        {
            get { return inventory[2].Itemstack; }
            set { inventory[2].Itemstack = value; }
        }

        public ItemStack catchStack
        {
            get { return inventory[3].Itemstack; }
            set { inventory[3].Itemstack = value; }
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side.IsServer())
            {
                RegisterGameTickListener(ParticleUpdate, tickSeconds * 1000);
                RegisterGameTickListener(LimbTrotLineUpdate, (int)(updateMinutes * 60000));
            }
        }


        private void GenerateWaterParticles(BlockPos pos, IWorldAccessor world)
        {
            float minQuantity = 1;
            float maxQuantity = 8;
            int color = ColorUtil.ToRgba(40, 125, 185, 255);
            Vec3d minPos = new Vec3d();
            Vec3d addPos = new Vec3d();
            Vec3f minVelocity = new Vec3f(0.1f, 0.0f, 0.1f);
            Vec3f maxVelocity = new Vec3f(0.6f, 0.1f, 0.6f);
            float lifeLength = 1f;
            float gravityEffect = 0f;
            float minSize = 0.1f;
            float maxSize = 0.3f;

            SimpleParticleProperties waterParticles = new SimpleParticleProperties(
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
            if (!catchSlot.Empty)
            {
                BlockPos belowblock = new BlockPos(Pos.X, Pos.Y - 1, Pos.Z);
                Block belowBlock = Api.World.BlockAccessor.GetBlock(belowblock);
                if ((belowBlock.LiquidCode == "water") && (!belowBlock.Code.Path.Contains("inwater")))
                {
                    int rando = rnd.Next(3);
                    if (rando == 0)
                    { GenerateWaterParticles(Pos, Api.World); }
                }
            }
        }


        private void LimbTrotLineUpdate(float par)
        {
            if (!hookSlot.Empty && catchSlot.Empty)
            {
                BlockPos belowblock = new BlockPos(Pos.X, Pos.Y - 1, Pos.Z);
                Block belowBlock = Api.World.BlockAccessor.GetBlock(belowblock);
                if ((belowBlock.LiquidCode == "water") && (!belowBlock.Code.Path.Contains("inwater")))
                {
                    int caught = rnd.Next(100);
                    if (!baitSlot.Empty)
                    {
                        int rando = rnd.Next(100);
                        if (rando < baitStolenPercent)
                        {
                            if (!baitSlot.Empty)
                            {
                                baitSlot.TakeOutWhole();
                                GenerateWaterParticles(Pos, Api.World);
                                MarkDirty();
                                //Api.World.BlockAccessor.MarkBlockDirty(Pos);
                                //Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
                                //MarkDirty(true);
                            }
                        }
                        else
                        {
                            int toCatch = baitedCatchPercent;
                            if (!lureSlot.Empty)
                            { toCatch = baitedLuredCatchPercent; }
                            //System.Diagnostics.Debug.WriteLine("catch %" + toCatch);
                            if (caught < toCatch)
                            {
                                if (catchSlot.Empty)
                                {
                                    catchStack = new ItemStack(Api.World.GetItem(new AssetLocation("primitivesurvival:psfish-" + fishTypes[rnd.Next(fishTypes.Count())] + "-raw")), 1);
                                    rando = rnd.Next(2);
                                    if (rando == 0)
                                    { baitSlot.TakeOutWhole(); }
                                    MarkDirty();
                                    //Api.World.BlockAccessor.MarkBlockDirty(Pos);
                                    //Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
                                    //MarkDirty(true);
                                }
                            }
                        }
                    }
                    else //hook
                    {
                        int toCatch = catchPercent;
                        if (!lureSlot.Empty)
                        { toCatch = luredCatchPercent; }
                        //System.Diagnostics.Debug.WriteLine("catch %" + toCatch);
                        if (caught < toCatch)
                        {
                            if (catchSlot.Empty)
                            {
                                catchStack = new ItemStack(Api.World.GetItem(new AssetLocation("primitivesurvival:psfish-" + fishTypes[rnd.Next(fishTypes.Count())] + "-raw")), 1);
                                MarkDirty();
                                //Api.World.BlockAccessor.MarkBlockDirty(Pos);
                                //Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
                                //MarkDirty(true);
                            }
                        }
                    }
                }
            }
            if (!catchSlot.Empty)
            {
                //remove rot?
                if (catchStack.Item.Code.Path == "rot")
                {
                    int rando = rnd.Next(100);
                    if (rando < rotRemovedPercent)
                    { catchSlot.TakeOutWhole(); }
                    MarkDirty();
                    //Api.World.BlockAccessor.MarkBlockDirty(Pos);
                    //Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
                    //MarkDirty(true);
                }
            }
        }




        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (playerSlot.Empty)
            {
                if (TryTake(byPlayer, blockSel))
                { return true; }
                return false;
            }
            else
            {
                CollectibleObject colObj = playerSlot.Itemstack.Collectible;
                //Debug.WriteLine("colobj:" + colObj.Attributes);
                if (colObj.Attributes != null)
                {
                    if (TryPut(playerSlot, blockSel))
                    { return true; }
                    return false;
                }
            }
            return false;
        }

        internal void OnBreak(IPlayer byPlayer, BlockPos pos)
        {
            for (int index = 3; index >= 0; index--)
            {
                if (!inventory[index].Empty)
                {
                    ItemStack stack = inventory[index].TakeOut(1);
                    if (stack.StackSize > 0)
                    { Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5)); }
                    MarkDirty(true);
                }
            }
        }


        private bool TryPut(ItemSlot playerSlot, BlockSelection blockSel)
        {
            int index = -1;
            ItemStack playerStack = playerSlot.Itemstack;
            if (inventory != null)
            {
                ItemStack[] stacks = inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
                if (stacks.Count() >= maxSlots)
                { return false; }
            }

            if (playerStack.Item != null)
            {
                //Debug.WriteLine("item:" + playerStack.Item.Code.Path);

                if (playerStack.Item.Code.Path.Contains("fishinghook") && hookSlot.Empty)
                {
                    if (!baitSlot.Empty || !catchSlot.Empty) return false; //must be bare line
                    index = 0;
                }
                else if (playerStack.Item.Code.Path.Contains("fishinglure") && lureSlot.Empty)
                {
                    if (hookSlot.Empty) return false; //must be a hook
                    index = 2;
                }
                else if (Array.IndexOf(baitTypes, playerStack.Item.FirstCodePart()) >= 0 && baitSlot.Empty)
                {
                    if (hookSlot.Empty || !catchSlot.Empty) return false; //needs a hook and no fish
                    else index = 1;
                }
                else if (playerStack.Item.Code.Path.Contains("psfish") && catchSlot.Empty)
                {
                    if (hookSlot.Empty) return false; //needs a hook
                    index = 3;
                }

                if (index > -1)
                {
                    int moved = playerSlot.TryPutInto(Api.World, inventory[index]);
                    if (moved > 0)
                    {
                        MarkDirty(true);
                        return moved > 0;
                    }
                }
            }
            else if (playerStack.Block != null)
            {
                //Debug.WriteLine("block:" + playerStack.Block.Code.Path);
                if (Array.IndexOf(baitTypes, playerStack.Block.FirstCodePart()) >= 0 && baitSlot.Empty)
                {
                    if (hookSlot.Empty || !catchSlot.Empty) return false; //needs a hook and no fish
                    else index = 1;
                }
                if (index > -1)
                {
                    int moved = playerSlot.TryPutInto(Api.World, inventory[index]);
                    if (moved > 0)
                    {
                        MarkDirty(true);
                        return moved > 0;
                    }
                }
            }
            return false;
        }



        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!catchSlot.Empty)
            {
                //Debug.WriteLine("Grabbed a " + catchStack.Item.Code.Path);
                int rando = rnd.Next(3);
                if (rando < 2)
                {
                    byPlayer.InventoryManager.TryGiveItemstack(catchStack);
                    //ItemStack drop = catchStack.Clone();
                    //drop.StackSize = 1;
                    //Api.World.SpawnItemEntity(drop, new Vec3d(Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5), null); //slippery
                    //Api.World.SpawnItemEntity(catchStack, Pos.ToVec3d()); //slippery
                }
                else
                {
                    byPlayer.InventoryManager.TryGiveItemstack(catchStack);
                }
                catchSlot.TakeOutWhole();
                MarkDirty(true);
                return true;
            }
            else if (!baitSlot.Empty)
            {
                byPlayer.InventoryManager.TryGiveItemstack(baitStack);
                baitSlot.TakeOutWhole();
                MarkDirty(true);
                return true;
            }
            else if (!lureSlot.Empty)
            {
                byPlayer.InventoryManager.TryGiveItemstack(lureStack);
                lureSlot.TakeOutWhole();
                MarkDirty(true);
                return true;
            }
            else if (!hookSlot.Empty)
            {
                byPlayer.InventoryManager.TryGiveItemstack(hookStack);
                hookSlot.TakeOutWhole();
                MarkDirty(true);
                return true;
            }
            return false;
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
        {
            base.FromTreeAttributes(tree, worldForResolve);
            if (Api != null)
            {
                if (Api.Side == EnumAppSide.Client)
                { Api.World.BlockAccessor.MarkBlockDirty(Pos); }
            }
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (!catchSlot.Empty)
            {
                sb.Append(Lang.Get("There's something on your hook."));
                if (!catchStack.Item.Code.Path.Contains("psfish"))
                { sb.Append(" " + Lang.Get("Unfortunately, it smells a little funky.")); }
            }
            else
            {
                if (!hookSlot.Empty)
                {
                    string hookmsg = "-";
                    string baitmsg = "-";
                    string luremsg = "-";

                    if (!hookSlot.Empty) hookmsg = hookStack.GetName().Split('(', ')')[1];
                    if (!baitSlot.Empty) baitmsg = baitStack.GetName();
                    if (!lureSlot.Empty) luremsg = lureStack.GetName().Split('(', ')')[1];

                    sb.AppendLine(Lang.Get("Hook type") + ": " + hookmsg);
                    sb.AppendLine(Lang.Get("Bait type") + ": " + baitmsg);
                    sb.AppendLine(Lang.Get("Lure type") + ": " + luremsg);

                    sb.AppendLine();
                }

                if (!baitSlot.Empty)
                {
                    if (baitStack.Item != null)
                    {
                        if (Array.IndexOf(baitTypes, baitStack.Item.FirstCodePart()) < 0)
                        { sb.Append(Lang.Get("Your bait has gone rotten. Replace it with fresh bait.")); }
                        else
                        { sb.Append(Lang.Get("It's baited so your odds of catching something are pretty good.")); }
                    }
                    else if (baitStack.Block != null)
                    {
                        if (Array.IndexOf(baitTypes, baitStack.Block.FirstCodePart()) < 0)
                        { sb.Append(Lang.Get("Your bait has gone rotten. Replace it with fresh bait.")); }
                        else
                        { sb.Append(Lang.Get("It's baited so your odds of catching something are pretty good.")); }
                    }

                }
                else if (baitSlot.Empty && !hookSlot.Empty)
                { sb.Append(Lang.Get("Bait it with some food to increase your odds of catching something.")); }
                else if (hookSlot.Empty)
                {
                    sb.Append(Lang.Get("Put a hook on that line if you expect to catch something.")).AppendLine().AppendLine();
                }
            }
            sb.AppendLine().AppendLine();
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh;
            string shapeBase = "primitivesurvival:shapes/";
            string shapePath = "";
            string hookType;
            string lureType;
            bool alive = false;

            BlockLimbTrotLineLure block = Api.World.BlockAccessor.GetBlock(Pos) as BlockLimbTrotLineLure;
            Block tmpBlock;
            ITexPositionSource texture = tesselator.GetTexSource(block);
            ITexPositionSource tmpTextureSource = texture;

            if (block.Code.Path.Contains("-middle"))
            {
                shapePath = "block/limbtrotline/limbtrotline-middle";
                mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }
            if (block.Code.Path.Contains("small"))
            {
                shapePath = "block/limbtrotline/limbtrotline-end-small";
                mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }
            else if (block.Code.Path.Contains("medium"))
            {
                shapePath = "block/limbtrotline/limbtrotline-end-medium";
                mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }
            else if (block.Code.Path.Contains("large"))
            {
                shapePath = "block/limbtrotline/limbtrotline-end-large";
                mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }
            if (block.Code.Path.Contains("-withmiddle"))
            {
                shapePath = "block/limbtrotline/limbtrotline-end-extension";
                mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, alive, tesselator);
                mesher.AddMeshData(mesh);
            }

            if (inventory != null)
            {
                if (!hookSlot.Empty)
                {
                    hookType = hookStack.Item.LastCodePart().ToString();
                    string newPath = "texture" + hookType; //don't judge me!!!
                    Block tempblock = Api.World.GetBlock(block.CodeWithPath(newPath));
                    tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tempblock);

                    shapePath = "item/fishing/fishinghook";
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }
                if (!lureSlot.Empty)
                {
                    lureType = lureStack.Item.LastCodePart().ToString();
                    string newPath = "texture" + lureType; //don't judge me!!!
                    Block tempblock = Api.World.GetBlock(block.CodeWithPath(newPath));
                    tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tempblock);

                    shapePath = "item/fishing/fishinglure";
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }
                if (!baitSlot.Empty)
                {
                    if (baitStack.Item != null)
                    {
                        if (Array.IndexOf(baitTypes, baitStack.Item.FirstCodePart()) < 0)
                        {
                            Block tempblock = Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tempblock);
                        }
                        else tmpTextureSource = texture;
                    }
                    else if (baitStack.Block != null)
                    {
                        if (Array.IndexOf(baitTypes, baitStack.Block.FirstCodePart()) < 0)
                        {
                            Block tempblock = Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tempblock);
                        }
                        else tmpTextureSource = texture;
                    }
                    shapePath = "item/fishing/hookbait"; //baited (for now)
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }
                if (!catchSlot.Empty) //fish or rot
                {
                    if (!catchStack.Item.Code.Path.Contains("psfish"))
                    {
                        tmpBlock = Api.World.GetBlock(block.CodeWithPath("texturerot"));
                        tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                        shapePath = "primitivesurvival:shapes/item/fishing/fish-pike";
                    }
                    else
                    {
                        shapePath = "primitivesurvival:shapes/item/fishing/fish-" + catchStack.Item.LastCodePart(1).ToString();
                        if (catchStack.Item.Code.Path.Contains("cooked"))
                        {
                            tmpBlock = Api.World.GetBlock(block.CodeWithPath("texturecooked"));
                            tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                        }
                        else
                        {
                            tmpTextureSource = texture;
                            alive = true;
                        }
                    }
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapePath, tmpTextureSource, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }
            }
            return true;
        }
    }
}

