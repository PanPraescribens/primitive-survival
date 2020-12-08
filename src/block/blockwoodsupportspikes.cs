using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common.Entities;

public class BlockWoodSupportSpikes : Block
{
    public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture, int slot, ITesselatorAPI tesselator = null)
    {
        Shape shape = null;
        tesselator = capi.Tesselator;
        shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
        MeshData mesh;
        tesselator.TesselateShape(shapePath, shape, out mesh, texture, new Vec3f(0, 0, 0));
        if (slot == -1) //spikes
        {
            mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), Shape.rotateX * GameMath.DEG2RAD, Shape.rotateY * GameMath.DEG2RAD, Shape.rotateZ * GameMath.DEG2RAD);
        }
        else
        { 
            //foilage
        }
        return mesh;
    }


    public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
    {
        base.OnEntityCollide(world, entity, pos, facing, collideSpeed, isImpact);
        if (world.Side == EnumAppSide.Server) // && isImpact)// && facing.Axis == EnumAxis.Y)
        {
            //System.Diagnostics.Debug.WriteLine("break");
            world.BlockAccessor.BreakBlock(pos, null);

        }
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        BEWoodSupportSpikes be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEWoodSupportSpikes;
        if (be != null)
        {
            bool result = be.OnInteract(byPlayer, blockSel);
            return result;
        }
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
        
    }


    public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
    {
        BEWoodSupportSpikes be = api.World.BlockAccessor.GetBlockEntity(pos) as BEWoodSupportSpikes;
        if (be != null)
        {
            return be.GetBlockName(world, pos);
        }
        return base.GetPlacedBlockName(world, pos);
    }




    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
    {
        Block block = world.BlockAccessor.GetBlock(neibpos);
        if (block.BlockId <= 0) //block removed
        {
            block = world.BlockAccessor.GetBlock(neibpos.NorthCopy());
            if (block.FirstCodePart() == "woodsupportspikes")
                world.BlockAccessor.BreakBlock(neibpos.NorthCopy(), null);
            block = world.BlockAccessor.GetBlock(neibpos.SouthCopy());
            if (block.FirstCodePart() == "woodsupportspikes")
                world.BlockAccessor.BreakBlock(neibpos.SouthCopy(), null);
            block = world.BlockAccessor.GetBlock(neibpos.EastCopy());
            if (block.FirstCodePart() == "woodsupportspikes")
                world.BlockAccessor.BreakBlock(neibpos.EastCopy(), null);
            block = world.BlockAccessor.GetBlock(neibpos.WestCopy());
            if (block.FirstCodePart() == "woodsupportspikes")
                world.BlockAccessor.BreakBlock(neibpos.WestCopy(), null);
        }
        else //block added
        {
            block = world.BlockAccessor.GetBlock(neibpos.DownCopy());
            if (block.FirstCodePart() == "woodsupportspikes")
            {
                world.BlockAccessor.BreakBlock(neibpos.DownCopy(), null);
                world.BlockAccessor.BreakBlock(neibpos, null);
            }
        }
    }
}
