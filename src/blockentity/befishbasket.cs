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
using System.Diagnostics;

namespace primitiveSurvival
{
    public class BEFishBasket : BlockEntityDisplay
    {
        public int catchPercent = PrimitiveSurvivalConfig.Loaded.fishBasketCatchPercent; //4
        public int baitedCatchPercent = PrimitiveSurvivalConfig.Loaded.fishBasketBaitedCatchPercent; //10
        public int baitStolenPercent = PrimitiveSurvivalConfig.Loaded.fishBasketBaitStolenPercent; //5
        public int escapePercent = PrimitiveSurvivalConfig.Loaded.fishBasketEscapePercent; //15
        public double updateMinutes = PrimitiveSurvivalConfig.Loaded.fishBasketUpdateMinutes; //2.2
        public int rotRemovedPercent = PrimitiveSurvivalConfig.Loaded.fishBasketRotRemovedPercent; //10

        public int tickSeconds = 4;
        public int maxSlots = 3;
        public string[] baitTypes = { "fruit", "grain", "legume", "meat", "vegetable", "jerky", "mushroom", "bread", "poultry", "pickledvegetable", "redmeat", "bushmeat", "earthworm", "cheese", "fishfillet" };
        public string[] fishTypes = { "trout", "perch", "carp", "bass", "pike", "arcticchar", "catfish", "bluegill" };
        public string[] shellStates = { "scallop", "sundial", "turritella", "clam", "conch", "seastar", "volute" };
        public string[] shellColors = { "latte", "plain", "seafoam", "darkpurple", "cinnamon", "turquoise" };

        public string[] relics = { "psgear-astral", "psgear-ethereal" };

        public static Random rnd = new Random();


        public override string InventoryClassName
        {
            get { return "fishbasket"; }
        }

        protected InventoryGeneric inventory;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }


        public BEFishBasket()
        {
            inventory = new InventoryGeneric(maxSlots, null, null);
            meshes = new MeshData[maxSlots];
        }


        public ItemSlot baitSlot
        {
            get { return inventory[0]; }
        }

        public ItemSlot catch1Slot
        {
            get { return inventory[1]; }
        }

        public ItemSlot catch2Slot
        {
            get { return inventory[2]; }
        }

        public ItemStack baitStack
        {
            get { return inventory[0].Itemstack; }
            set { inventory[0].Itemstack = value; }
        }

        public ItemStack catch1Stack
        {
            get { return inventory[1].Itemstack; }
            set { inventory[1].Itemstack = value; }
        }

        public ItemStack catch2Stack
        {
            get { return inventory[2].Itemstack; }
            set { inventory[2].Itemstack = value; }
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side.IsServer())
            {
                RegisterGameTickListener(ParticleUpdate, tickSeconds * 1000);
                RegisterGameTickListener(FishBasketUpdate, (int)(updateMinutes * 60000));
            }
        }


        private void GenerateWaterParticles(int slot, string type, BlockPos pos, IWorldAccessor world)
        {
            float minQuantity = 1;
            float maxQuantity = 8;
            if (type == "seashell")
                maxQuantity = 2;
            int color = ColorUtil.ToRgba(40, 125, 185, 255);
            Vec3d minPos = new Vec3d();
            Vec3d addPos = new Vec3d();
            Vec3f minVelocity = new Vec3f(0.2f, 0.0f, 0.2f);
            Vec3f maxVelocity = new Vec3f(0.6f, 0.4f, 0.6f);
            float lifeLength = 5f;
            float gravityEffect = -0.1f;
            float minSize = 0.1f;
            float maxSize = 0.5f;

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
            if (Block.Code.Path.Contains("inwater"))
            {
                int rando = rnd.Next(2);
                if (rando == 0)
                {
                    for (int slot = 1; slot <= 2; slot++)
                    {
                        if (!inventory[slot].Empty)
                        {
                            if (inventory[slot].Itemstack.Item != null) //fish
                                GenerateWaterParticles(slot, "fish", Pos, Api.World);
                            else if (inventory[slot].Itemstack.Block != null) //seashell
                                GenerateWaterParticles(slot, "seashell", Pos, Api.World);
                        }
                    }
                }
            }
        }


        public void FishBasketUpdate(float par)
        {
            bool waterAllAround = true;
            int rando;
            Block testBlock;
            BlockPos[] neibPos = new BlockPos[] { Pos.EastCopy(), Pos.NorthCopy(), Pos.SouthCopy(), Pos.WestCopy() };

            // Examine sides 
            foreach (BlockPos neib in neibPos)
            {
                testBlock = Api.World.BlockAccessor.GetBlock(neib);
                if (testBlock.LiquidCode != "water")
                { waterAllAround = false; }
            }
            if (waterAllAround)
            {
                int escaped;
                int caught = rnd.Next(100);
                if (!baitSlot.Empty)
                {
                    rando = rnd.Next(100);
                    if (rando < baitStolenPercent)
                    {
                        if (WorldTake(0, Pos))
                            GenerateWaterParticles(1, "fish", Pos, Api.World);
                    }
                }
                bool caughtOk = false;
                if ((!baitSlot.Empty && caught < baitedCatchPercent) || (baitSlot.Empty && caught < catchPercent))
                {
                    rando = rnd.Next(2);
                    caughtOk = WorldPut(rando + 1, Pos);
                    if (!caughtOk)
                    {
                        rando = 1 - rando;
                        caughtOk = WorldPut(rando + 1, Pos);
                    }
                }
                if (!caughtOk && (!catch1Slot.Empty || !catch2Slot.Empty))
                {
                    escaped = rnd.Next(100);
                    if (escaped < escapePercent)
                    {
                        bool escapedOk = false;
                        rando = rnd.Next(2);
                        escapedOk = WorldTake(rando + 1, Pos);
                        if (!escapedOk)
                        {
                            rando = 1 - rando;
                            escapedOk = WorldTake(rando + 1, Pos);
                        }
                        if (escapedOk)
                            GenerateWaterParticles(2, "fish", Pos, Api.World);
                    }
                }

                if (!catch1Slot.Empty)
                {
                    if (catch1Stack.Item != null)
                    {
                        if (catch1Stack.Item.Code.Path == "rot")
                        {
                            escaped = rnd.Next(100);
                            if (escaped < rotRemovedPercent)
                            { WorldTake(1, Pos); } //remove rot from slot 1
                        }
                    }
                }
                if (!catch2Slot.Empty)
                {
                    if (catch2Stack.Item != null)
                    {
                        if (catch2Stack.Item.Code.Path == "rot")
                        {
                            escaped = rnd.Next(100);
                            if (escaped < rotRemovedPercent)
                            { WorldTake(2, Pos); } //remove rot from slot 2
                        }
                    }
                }
            }
        }


        public bool WorldPut(int slot, BlockPos pos)
        {
            ItemStack newStack = null;
            int rando = rnd.Next(5);
            //rando = 0;  //debug
            if (rando < 1) //20% chance of a seashell or relic
            {
                rando = rnd.Next(10); //10

                if (rando < 1) //10% chance of a relic
                {
                    string thisRelic = relics[rnd.Next(relics.Count())];
                    newStack = new ItemStack(Api.World.GetItem(new AssetLocation("primitivesurvival:" + thisRelic)), 1);
                }
                else
                    newStack = new ItemStack(Api.World.GetBlock(new AssetLocation("game:seashell-" + shellStates[rnd.Next(shellStates.Count())] + "-" + shellColors[rnd.Next(shellColors.Count())])), 1);
            }
            else
            {
                newStack = new ItemStack(Api.World.GetItem(new AssetLocation("primitivesurvival:psfish-" + fishTypes[rnd.Next(fishTypes.Count())] + "-raw")), 1);
            }
            if (inventory[slot].Empty)
            {
                if (newStack.Collectible.Code.Path.Contains("psfish"))
                {
                    /*********************************************/
                    //depletion check last
                    int rate = PrimitiveSurvivalMod.fishDepletedPercent(Api as ICoreServerAPI, Pos);
                    rando = rnd.Next(100);
                    if (rando < rate) //depleted!
                    { return false; }
                    else
                    {
                        // deplete
                        PrimitiveSurvivalMod.UpdateChunkInDictionary(Api as ICoreServerAPI, Pos, PrimitiveSurvivalConfig.Loaded.fishChunkDepletionRate);
                    }
                    /*********************************************/
                }

                inventory[slot].Itemstack = newStack;
                //Api.World.BlockAccessor.MarkBlockDirty(pos);
                MarkDirty(true);
                return true;
            }
            return false;
        }


        public bool WorldTake(int slot, BlockPos pos)
        {
            if (!inventory[slot].Empty)
            {
                /*********************************************/
                //Debug.WriteLine("Escaped: " + inventory[slot].Itemstack.Collectible.Code.Path);
                if (inventory[slot].Itemstack.Collectible.Code.Path.Contains("psfish"))
                {
                    //replete (at deplete rate)
                    PrimitiveSurvivalMod.UpdateChunkInDictionary(Api as ICoreServerAPI, Pos, -PrimitiveSurvivalConfig.Loaded.fishChunkDepletionRate);
                }
                /*********************************************/

                inventory[slot].TakeOutWhole();

                Api.World.BlockAccessor.MarkBlockDirty(pos);
                MarkDirty();
                return true;
            }
            return false;
        }


        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty)
            {
                if (TryTake(byPlayer, blockSel))
                { return true; }
                return false;
            }
            else
            {
                CollectibleObject colObj = slot.Itemstack.Collectible;
                if (colObj.Attributes != null)
                {
                    if (TryPut(slot, blockSel))
                    { return true; }
                    return false;
                }
            }
            return false;
        }


        private bool TryPut(ItemSlot playerSlot, BlockSelection blockSel)
        {
            int index = -1;
            int moved = 0;
            ItemStack playerStack = playerSlot.Itemstack;
            if (inventory != null)
            {
                ItemStack[] stacks = inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
                if (stacks.Count() >= maxSlots)
                { return false; }
            }

            if (playerStack.Item != null)
            {
                //Debug.WriteLine("Putting a " + playerStack.Item.FirstCodePart());
                if (Array.IndexOf(baitTypes, playerStack.Item.FirstCodePart()) >= 0 && baitSlot.Empty)
                { index = 0; }
                else if (playerStack.Item.Code.Path.Contains("psfish"))
                {
                    if (catch1Slot.Empty) index = 1;
                    else if (catch2Slot.Empty) index = 2;
                }
            }
            else if (playerStack.Block != null)
            {
                if (Array.IndexOf(baitTypes, playerStack.Block.FirstCodePart()) >= 0 && baitSlot.Empty)
                { index = 0; }
                else if (playerStack.Block.Code.Path.Contains("seashell"))
                {
                    if (catch1Slot.Empty) index = 1;
                    else if (catch2Slot.Empty) index = 2;
                }
            }
            if (index >= 0)
            {
                moved = playerSlot.TryPutInto(Api.World, inventory[index]);
                if (moved > 0)
                {
                    MarkDirty(true);
                    return moved > 0;
                }
            }
            return false;
        }


        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!catch2Slot.Empty)
            {
                int rando = rnd.Next(3);
                if (rando < 2 && catch2Stack.Item != null) //fish
                {
                    //ItemStack drop = catch2Stack.Clone();
                    //drop.StackSize = 1;
                    //Api.World.SpawnItemEntity(drop, new Vec3d(Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5), null);
                    byPlayer.InventoryManager.TryGiveItemstack(catch2Stack);
                }
                else
                    byPlayer.InventoryManager.TryGiveItemstack(catch2Stack);
                catch2Slot.TakeOutWhole();
                MarkDirty(true);
                return true;
            }
            else if (!catch1Slot.Empty)
            {
                int rando = rnd.Next(3);
                if (rando < 2 && catch1Stack.Item != null) //fish
                {
                    //ItemStack drop = catch1Stack.Clone();
                    //drop.StackSize = 1;
                    //Api.World.SpawnItemEntity(drop, new Vec3d(Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5), null);
                    byPlayer.InventoryManager.TryGiveItemstack(catch1Stack);
                }
                else
                {
                    byPlayer.InventoryManager.TryGiveItemstack(catch1Stack);
                }
                catch1Slot.TakeOutWhole();
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
            return false;
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            if (block.Code.Path.Contains("inwater"))
            {
                bool rot = false;
                if (!catch1Slot.Empty || !catch2Slot.Empty)
                {
                    sb.Append(Lang.Get("There's something in your basket."));
                    if (!catch1Slot.Empty)
                    {
                        if (catch1Stack.Block != null)
                        { }
                        else if (!catch1Stack.Item.Code.Path.Contains("psfish"))
                        { sb.Append(" " + Lang.Get("It smells a little funky in there.")); }
                        rot = true;
                    }
                    if (!catch2Slot.Empty && !rot)
                    {
                        if (catch2Stack.Block != null)
                        { }
                        else if (!catch2Stack.Item.Code.Path.Contains("psfish"))
                        { sb.Append(" " + Lang.Get("It smells a little funky in there.")); }
                    }
                    sb.AppendLine().AppendLine();
                }
                else
                {
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
                    else if (baitSlot.Empty)
                    { sb.Append(Lang.Get("Bait it with some food to increase your odds of catching something.")); }
                    sb.AppendLine().AppendLine();
                }
            }
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




        // Note: There's a bug "of sorts" if the water isn't full block fishbasket is land type not water type...

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh;
            string shapePath;
            bool alive = false;
            Block tmpBlock;
            BlockFishBasket block = Api.World.BlockAccessor.GetBlock(Pos) as BlockFishBasket;
            ITexPositionSource texture = tesselator.GetTexSource(block);
            ITexPositionSource tmpTextureSource = texture;
            shapePath = "primitivesurvival:shapes/block/fishbasket/fishbasket";
            mesh = block.GenMesh(Api as ICoreClientAPI, shapePath, texture, -1, alive, tesselator);
            mesher.AddMeshData(mesh);

            if (inventory != null)
            {
                if (!baitSlot.Empty) //bait or rot
                {
                    if (baitStack.Item != null)
                    {
                        if (Array.IndexOf(baitTypes, baitStack.Item.FirstCodePart()) < 0)
                        {
                            Block tempblock = Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tempblock);
                        }
                        else
                            tmpTextureSource = texture;
                    }
                    else if (baitStack.Block != null)
                    {
                        if (Array.IndexOf(baitTypes, baitStack.Block.FirstCodePart()) < 0)
                        {
                            Block tempblock = Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tempblock);
                        }
                        else
                            tmpTextureSource = texture;
                    }
                    shapePath = "primitivesurvival:shapes/item/fishing/hookbait"; //baited (for now)
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapePath, tmpTextureSource, 0, alive, tesselator);
                    mesher.AddMeshData(mesh);
                }

                for (int i = 1; i <= 2; i++)
                {
                    shapePath = "";
                    alive = false;
                    if (!inventory[i].Empty) //fish, shell, or rot
                    {
                        if (inventory[i].Itemstack.Block != null) //shell
                        {
                            shapePath = "game:shapes/block/seashell/" + inventory[i].Itemstack.Block.FirstCodePart(1);
                            tmpTextureSource = tesselator.GetTexSource(inventory[i].Itemstack.Block);
                        }
                        else if (inventory[i].Itemstack.Item.Code.Path.Contains("gear"))
                        {
                            string gearType = inventory[i].Itemstack.Item.FirstCodePart(1);
                            tmpBlock = Api.World.GetBlock(block.CodeWithPath("texture" + gearType));
                            if (gearType != "rusty")
                            { gearType = "temporal"; }
                            shapePath = "game:shapes/item/gear-" + gearType;
                            tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                            //Debug.WriteLine(shapePath);
                        }


                        else if (!inventory[i].Itemstack.Item.Code.Path.Contains("psfish"))
                        {
                            tmpBlock = Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                            shapePath = "primitivesurvival:shapes/item/fishing/fish-pike";
                        }
                        else if (inventory[i].Itemstack.Item.Code.Path.Contains("psfish"))
                        {
                            shapePath = "primitivesurvival:shapes/item/fishing/fish-" + inventory[i].Itemstack.Item.LastCodePart(1).ToString();
                            if (inventory[i].Itemstack.Item.Code.Path.Contains("cooked"))
                            {
                                tmpBlock = Api.World.GetBlock(block.CodeWithPath("texturecooked"));
                                tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                            }
                            else
                            {
                                alive = true;
                                tmpTextureSource = texture;
                            }
                        }
                        if (shapePath != "")
                        {
                            mesh = block.GenMesh(Api as ICoreClientAPI, shapePath, tmpTextureSource, i, alive, tesselator);
                            mesher.AddMeshData(mesh);
                        }
                    }
                }
            }
            return true;
        }
    }
}