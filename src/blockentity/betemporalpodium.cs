using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;

public class BETemporallectern : BlockEntityDisplay
{
    public int maxSlots = 2;

    public override string InventoryClassName
    {
        get { return "temporallectern"; }
    }
    protected InventoryGeneric inventory;

    public override InventoryBase Inventory
    {
        get { return inventory; }
    }

    public BETemporallectern()
    {
        inventory = new InventoryGeneric(maxSlots, null, null);
        meshes = new MeshData[maxSlots];
    }

    public ItemSlot topSlot
    {
        get { return inventory[0]; }
    }

    public ItemSlot gearSlot
    {
        get { return inventory[1]; }
    }

    public ItemStack topStack
    {
        get { return inventory[0].Itemstack; }
        set { inventory[0].Itemstack = value; }
    }

    public ItemStack gearStack
    {
        get { return inventory[1].Itemstack; }
        set { inventory[1].Itemstack = value; }
    }


    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
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
            if (TryPut(byPlayer, blockSel))
            { return true; }
            return false;
        }
    }


    internal void OnBreak(IPlayer byPlayer, BlockPos pos)
    {
        for (int index = maxSlots - 1; index >= 0; index--)
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


    private bool TryPut(IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        ItemStack playerStack = playerSlot.Itemstack;

        if (inventory != null)
        {
            ItemStack[] stacks = inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
            if (stacks.Count() >= maxSlots)
            { return false; }
        }
        if (playerStack.Block != null)
        {
            if (playerStack.Block.Code.Path.Contains("necronomicon") && topSlot.Empty)
            {
                int moved = playerSlot.TryPutInto(Api.World, topSlot);
                if (moved > 0)
                {
                    MarkDirty(true);
                    return moved > 0;
                }
            }
        }

        else if (playerStack.Item != null)
        {
            //System.Diagnostics.Debug.WriteLine("Putting item: " + playerStack.Item.Code.Path);
            string path = playerStack.Item.Code.Path;
            if (path.Contains("gear-"))
            {
                Block tmpblock = Api.World.BlockAccessor.GetBlock(Pos);
                string blockFacing = tmpblock.LastCodePart();

                BlockFacing facing = byPlayer.CurrentBlockSelection.Face.Opposite;
                string playerFacing = facing.ToString();

                if (playerFacing == blockFacing && gearSlot.Empty)
                {
                    int moved = playerSlot.TryPutInto(Api.World, gearSlot);
                    if (moved > 0)
                    {
                        MarkDirty(true);
                        return moved > 0;
                    }
                }
            }
        }
        return false;
    }


    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        BlockFacing facing = byPlayer.CurrentBlockSelection.Face.Opposite;
        string playerFacing = facing.ToString();
        Block tmpblock = Api.World.BlockAccessor.GetBlock(Pos);
        string blockFacing = tmpblock.LastCodePart();

        if (playerFacing == blockFacing && !gearSlot.Empty)
        {
            byPlayer.InventoryManager.TryGiveItemstack(gearStack);
            gearSlot.TakeOutWhole();
            MarkDirty(true);
            return true;
        }
        else if (!topSlot.Empty)
        {
            byPlayer.InventoryManager.TryGiveItemstack(topStack);
            topSlot.TakeOutWhole();
            MarkDirty(true);
            return true;
        }
        return false;
    }


    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        sb.Append("It looks like something is missing.");
        sb.AppendLine().AppendLine();
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        MeshData mesh;
        string shapeBase = "primitivesurvival:shapes/";
        string shapePath = "";
        int index = -1;

        BlockTemporallectern block = Api.World.BlockAccessor.GetBlock(Pos) as BlockTemporallectern;
        Block tmpBlock;
        ITexPositionSource texture = tesselator.GetTexSource(block);

        string newPath = "temporallectern";
        shapePath = "block/relic/" + newPath;
        mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, index, tesselator);
        mesher.AddMeshData(mesh);

        if (inventory != null)
        {
            if (!gearSlot.Empty) //gear - temporal or rusty
            {
                string gearType = gearStack.Item.FirstCodePart(1);
                tmpBlock = Api.World.GetBlock(block.CodeWithPath("texture" + gearType));
                if (gearType != "rusty")
                    gearType = "temporal";
                shapePath = "game:shapes/item/gear-" + gearType;
                texture = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                mesh = block.GenMesh(Api as ICoreClientAPI, shapePath, texture, 1, tesselator);
                mesher.AddMeshData(mesh);
            }

            if (!topSlot.Empty)
            {
                newPath = topStack.Block.FirstCodePart();
                if (newPath.Contains("necronomicon"))
                {
                    tmpBlock = Api.World.GetBlock(block.CodeWithPath("necronomicon-north"));
                    texture = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);

                    shapePath = "block/relic/" + newPath + "-closed";
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, index, tesselator);
                    mesher.AddMeshData(mesh);
                }
            }
        }
        return true;
    }
}


