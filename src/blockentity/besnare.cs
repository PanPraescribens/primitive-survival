using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.GameContent;
using Vintagestory.API.Config;
using Vintagestory.API.Common.Entities;


public class BESnare : BlockEntityDisplay, IAnimalFoodSource
{
    public string[] baitTypes = { "fruit", "grain", "legume", "meat", "vegetable", "jerky", "mushroom", "bread", "poultry", "pickledvegetable", "redmeat", "bushmeat", "cheese" };
    protected static Random rnd = new Random();
    public int maxSlots = 1;


    public ItemSlot baitSlot
    {
        get { return inventory[0]; }
    }

    public ItemStack baitStack
    {
        get { return inventory[0].Itemstack; }
        set { inventory[0].Itemstack = value; }
    }


    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (inventory != null)
        {
            if (!baitSlot.Empty)
            {
                if (baitStack.Item != null)
                {
                    if (Array.IndexOf(baitTypes, baitStack.Item.FirstCodePart()) >= 0)
                        if (Api.Side == EnumAppSide.Server)
                        { Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                }
                else if (baitStack.Block != null)
                {
                    if (Array.IndexOf(baitTypes, baitStack.Block.FirstCodePart()) >= 0)
                        if (Api.Side == EnumAppSide.Server)
                        { Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                }
            }
        }
    }

    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        if (Api.Side == EnumAppSide.Server)
        { Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        if (Api.Side == EnumAppSide.Server)
        { Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
    }

    #region IAnimalFoodSource impl
    public bool IsSuitableFor(Entity entity)
    { return true; }

    public float ConsumeOnePortion()
    {
        TryClearContents();
        return 0f;
    }

    public string Type
    { get { return "food"; } }

    public Vec3d Position
    { get { return base.Pos.ToVec3d().Add(0.5, 0.5, 0.5); } }
    #endregion

    
    public override string InventoryClassName
    {
        get { return "snare"; }
    }
    protected InventoryGeneric inventory;

    public override InventoryBase Inventory
    {
        get { return inventory; }
    }


    public BESnare()
    {
        inventory = new InventoryGeneric(maxSlots, null, null);
        meshes = new MeshData[maxSlots];
    }


    public bool TryClearContents()
    {
        if (!baitSlot.Empty)
        {
            baitSlot.TakeOutWhole();
            if (Api.Side == EnumAppSide.Server)
            { 
                Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this);
                MarkDirty();
            }
            //MarkDirty(true);
            return true;
        }
        return false;
    }


    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (playerSlot.Empty)
        {
            if (TryTake(byPlayer, blockSel))
            {
                if (Api.Side == EnumAppSide.Server)
                { Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                return true;
            }
            return false;
        }
        else
        {
            CollectibleObject colObj = playerSlot.Itemstack.Collectible;
            if (colObj.Attributes != null)
            {
                if (TryPut(playerSlot, blockSel))
                {
                    if (Api.Side == EnumAppSide.Server)
                    { Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                    return true;
                }
                return false;
            }
        }
        return false;
    }


    private bool TryPut(ItemSlot playerSlot, BlockSelection blockSel)
    {
        int index = -1;
        ItemStack playerStack = playerSlot.Itemstack;
        if (inventory != null)
        {
            ItemStack[] stacks = inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
            index = stacks.Count();
            if (index >= maxSlots)
            { return false; }
        }

        if (playerStack.Item != null)
        {
            if (Array.IndexOf(baitTypes, playerStack.Item.FirstCodePart()) >= 0 && baitSlot.Empty)
            {
                index = 0;
            }
            else return false;
        }
        else if (playerStack.Block != null)
        {
            if (Array.IndexOf(baitTypes, playerStack.Block.FirstCodePart()) >= 0 && baitSlot.Empty)
            {
                index = 0;
            }
            else return false;
        }
        if (index != -1)
        {
            int moved = playerSlot.TryPutInto(Api.World, inventory[index]);
            if (moved > 0)
            {
                updateMesh(index);
                MarkDirty(true);
                return moved > 0;
            }
            else return false;
        }
        return false;
    }


    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        if (!baitSlot.Empty)
        {
            ItemStack stack = baitSlot.TakeOut(1);
            if (stack.StackSize > 0)
            { Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5)); }
            updateMesh(0);
            MarkDirty(true);
            return true;
        }
        return false;
    }

    public void tripTrap(BlockPos Pos)
    {
        BlockSnare block = Api.World.BlockAccessor.GetBlock(Pos) as BlockSnare;
        ItemStack stack = baitSlot.TakeOut(1);
        if (stack != null)
        {
            Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            if (Api.Side == EnumAppSide.Server)
            { Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
        }
        string blockPath = block.Code.Path;
        blockPath = blockPath.Replace("set", "tripped");
        block = Api.World.BlockAccessor.GetBlock(block.CodeWithPath(blockPath)) as BlockSnare;
        Api.World.BlockAccessor.SetBlock(block.BlockId, Pos);
        MarkDirty(true);
        updateMesh(0);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        Block block = Api.World.BlockAccessor.GetBlock(Pos);
        if (block.Code.Path.Contains("tripped"))
        { sb.Append(Lang.Get("It's tripped. Sneak click to set it back up.")); }
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
        }
        sb.AppendLine().AppendLine();
    }


    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        MeshData mesh;
        string shapeBase = "primitivesurvival:shapes/";
        string shapePath;
        BlockSnare block = Api.World.BlockAccessor.GetBlock(Pos) as BlockSnare;
        ITexPositionSource texture = tesselator.GetTexSource(block);
        ITexPositionSource tmpTextureSource = texture;

        if (block.FirstCodePart(1) == "set")
            shapePath = "block/snare/snare-set";
        else
            shapePath = "block/snare/snare-tripped";
        mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, -1, false, tesselator);
        mesher.AddMeshData(mesh);

        if (inventory != null)
        {
            if (!baitSlot.Empty) //bait or rot
            {
                bool tripped = true;
                if (block.FirstCodePart(1) == "set")
                    tripped = false;
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
                shapePath = "block/trapbait"; //baited (for now)
                mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, 0, tripped, tesselator);
                mesher.AddMeshData(mesh);
            }
        }
        return true;
    }
}
