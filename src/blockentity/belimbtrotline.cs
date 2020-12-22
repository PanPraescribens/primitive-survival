using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using System.Collections.Generic;

public class BELimbTrotLineLure : BlockEntityDisplay
{
    public int catchPercent = 4; //2
    public int baitedCatchPercent = 10; //10

    public int luredCatchPercent = 7; //6
    public int baitedLuredCatchPercent = 13; //12

    public int baitStolenPercent = 5; //5
    public double updateMinutes = 2.4; //2.4

    public int rotRemovedPercent = 30; //30

    public int tickSeconds = 5;
    public int maxSlots = 4;
    public string[] baitTypes = { "fruit", "grain", "legume", "meat", "vegetable", "jerky", "mushroom", "bread", "poultry", "pickledvegetable", "redmeat", "bushmeat", "cheese" };
    public string[] fishTypes = { "trout", "bass", "pike", "arcticchar", "catfish", "bluegill" };
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
                {   GenerateWaterParticles(Pos, Api.World); }
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
                            Api.World.BlockAccessor.MarkBlockDirty(Pos);
                            MarkDirty();
                            GenerateWaterParticles(Pos, Api.World);
                        }
                    }
                    else
                    {
                        if (!catchSlot.Empty)
                        {
                            if (catchStack.Item.Code.Path.Contains("-rot"))
                            {
                                int escaped = rnd.Next(100);
                                if (escaped < rotRemovedPercent)
                                {
                                    ItemStack stack = catchSlot.TakeOut(1);
                                    //System.Diagnostics.Debug.WriteLine("rotten fish removed");
                                }
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
                                    Api.World.BlockAccessor.MarkBlockDirty(Pos);
                                    MarkDirty();
                                }
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
                            Api.World.BlockAccessor.MarkBlockDirty(Pos);
                            MarkDirty();
                        }
                    }
                }
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

    private void AddSelectionBox()
    {
        Block block = this.Api.World.BlockAccessor.GetBlock(Pos);
        Cuboidf[] selectionBoxes = block.GetSelectionBoxes(Api.World.BlockAccessor, Pos);
        selectionBoxes[1].X1 = 0;
        selectionBoxes[1].Y1 = -1;
        selectionBoxes[1].Z1 = 0;
        selectionBoxes[1].X2 = 1;
        selectionBoxes[1].Y2 = 0.2f;
        selectionBoxes[1].Z2 = 1;
    }

    private void ClearSelectionBox()
    {
        Block block = this.Api.World.BlockAccessor.GetBlock(Pos);
        Cuboidf[] selectionBoxes = block.GetSelectionBoxes(Api.World.BlockAccessor, Pos);
        selectionBoxes[1].X1 = 0;
        selectionBoxes[1].Y1 = 0;
        selectionBoxes[1].Z1 = 0;
        selectionBoxes[1].X2 = 0;
        selectionBoxes[1].Y2 = 0;
        selectionBoxes[1].Z2 = 0;
    }
    
    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        if (!catchSlot.Empty)
        {
            //System.Diagnostics.Debug.WriteLine("Grabbed a " + catchStack.Item.Code.Path);
            int rando = rnd.Next(3);
            if (rando < 2)
                Api.World.SpawnItemEntity(catchStack, Pos.ToVec3d().Add(0.5, 1, 0.5)); //slippery
            else
                byPlayer.InventoryManager.TryGiveItemstack(catchStack);
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


    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        if (!catchSlot.Empty)
        {
            //AddSelectionBox();
            sb.Append("There's something on your hook.");
            if (!catchStack.Item.Code.Path.Contains("psfish"))
            { sb.Append(" Unfortunately, it smells a little funky."); }
        }
        else
        {
            if (!hookSlot.Empty)
            {
                string hookmsg = "None";
                string baitmsg = "None";
                string luremsg = "None";

                if (!hookSlot.Empty) hookmsg = hookStack.GetName().Split('(', ')')[1];
                if (!baitSlot.Empty) baitmsg = baitStack.GetName();
                if (!lureSlot.Empty) luremsg = lureStack.GetName().Split('(', ')')[1];

                sb.AppendLine("Hook type: " + hookmsg);
                sb.AppendLine("Bait type: " + baitmsg);
                sb.AppendLine("Lure type: " + luremsg);

                sb.AppendLine();
            }

            //ClearSelectionBox();
            if (!baitSlot.Empty)
            {
                if (baitStack.Item != null)
                {
                    if (Array.IndexOf(baitTypes, baitStack.Item.FirstCodePart()) < 0)
                    { sb.Append("Your bait has gone rotten. Replace it with fresh bait."); }
                    else
                    { sb.Append("It's baited so your odds of catching something are pretty good."); }
                }
                else if (baitStack.Block != null)
                {
                    if (Array.IndexOf(baitTypes, baitStack.Block.FirstCodePart()) < 0)
                    { sb.Append("Your bait has gone rotten. Replace it with fresh bait."); }
                    else
                    { sb.Append("It's baited so your odds of catching something are pretty good."); }
                }

            }
            else if (baitSlot.Empty && !hookSlot.Empty)
            { sb.Append("Bait it with something to increase your odds of catching something."); }
            else if (hookSlot.Empty)
            {
                sb.Append("Put a hook on that line if you expect to catch something.").AppendLine().AppendLine();
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


