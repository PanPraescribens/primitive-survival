using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;

public class BETemporalCube : BlockEntityDisplay
{
    public int maxSlots = 4;
    
    public override string InventoryClassName
    {
        get { return "temporalcube"; }
    }
    protected InventoryGeneric inventory;

    public override InventoryBase Inventory
    {
        get { return inventory; }
    }

    public BETemporalCube()
    {
        inventory = new InventoryGeneric(maxSlots, null, null);
        meshes = new MeshData[maxSlots];
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
        for (int index = maxSlots-1; index >= 0; index--)
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
        int index = -1;
        ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        ItemStack playerStack = playerSlot.Itemstack;

        if (inventory != null)
        {
            ItemStack[] stacks = inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
            if (stacks.Count() >= maxSlots)
            { return false; }
        }
        if (playerStack.Item != null)
        {
            //System.Diagnostics.Debug.WriteLine("Putting item: " + playerStack.Item.Code.Path);
            string path = playerStack.Item.Code.Path;
            if (path.Contains("gear-"))
            {
                 BlockFacing facing = byPlayer.CurrentBlockSelection.Face.Opposite;
                string playerFacing = facing.ToString();

                if (playerFacing == "north")
                    index = 0;
                else if (playerFacing == "east")
                    index = 1;
                else if (playerFacing == "south")
                    index = 2;
                else if (playerFacing == "west")
                    index = 3;

                if (index >= 0)
                {
                    if (inventory[index].Empty)
                    {
                        int moved = playerSlot.TryPutInto(Api.World, inventory[index]);
                        if (moved > 0)
                        {
                            MarkDirty(true);
                            return moved > 0;
                        }
                    }
                }
            }
        }
        return false;
    }


    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        BlockFacing facing = byPlayer.CurrentBlockSelection.Face.Opposite;
        int index = -1;
        //System.Diagnostics.Debug.WriteLine(facing.ToString());
        string playerFacing = facing.ToString();
   
        if (playerFacing == "north")
            index = 0;
        else if (playerFacing == "east")
            index = 1;
        else if (playerFacing == "south")
            index = 2;
        else if (playerFacing == "west")
            index = 3;

        if (index >= 0)
        {
            if (!inventory[index].Empty)
            {
                byPlayer.InventoryManager.TryGiveItemstack(inventory[index].Itemstack);
                inventory[index].TakeOutWhole();
                MarkDirty(true);
                return true;
            }
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

        BlockTemporalCube block = Api.World.BlockAccessor.GetBlock(Pos) as BlockTemporalCube;
        Block tmpBlock;
        ITexPositionSource texture = tesselator.GetTexSource(block);

        string newPath = "temporalcube";
        shapePath = "block/relic/" + newPath;
        mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, index, tesselator);
        mesher.AddMeshData(mesh);

        if (inventory != null)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (!inventory[i].Empty) //gear - temporal or rusty
                {
                    string gearType = inventory[i].Itemstack.Item.FirstCodePart(1);
                    tmpBlock = Api.World.GetBlock(block.CodeWithPath("texture" + gearType));
                    if (gearType != "rusty")
                        gearType = "temporal";
                    shapePath = "game:shapes/item/gear-" + gearType;
                    texture = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapePath, texture, i, tesselator);
                    mesher.AddMeshData(mesh);
                }
            }
        }
        return true;
    }
}


