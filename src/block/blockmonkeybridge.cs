using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common.Entities;
using System;

public class BlockMonkeyBridge : Block
{
    /*
    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer player, 1f)
    {
        base.OnBlockBroken(world, pos, player, 1f);
    }
    */

    public void BreakAbove(IWorldAccessor world, BlockPos neibpos)
    {

        Block block = world.BlockAccessor.GetBlock(neibpos.UpCopy());
        //System.Diagnostics.Debug.WriteLine(block.Code.Path);
        if (block.FirstCodePart() == "monkeybridge" && block.FirstCodePart(1) == "null")
            world.BlockAccessor.SetBlock(0, neibpos.UpCopy());  //remove the null block with no drop
    }

    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
    {

        Block block = world.BlockAccessor.GetBlock(neibpos);
        Block thisblock = world.BlockAccessor.GetBlock(pos);
        //BreakAbove(world, neibpos);

        float dropQty;

        

        if (block.BlockId <= 0) //block removed
        {
            block = world.BlockAccessor.GetBlock(neibpos.NorthCopy());
            if (block.FirstCodePart() == "monkeybridge" && block.LastCodePart() != "east" && block.LastCodePart() != "west")
            {
                if (block.FirstCodePart(1) != "null") dropQty = 1f;
                else dropQty = 0f;
                world.BlockAccessor.BreakBlock(neibpos.NorthCopy(), null, dropQty);
                BreakAbove(world, neibpos.NorthCopy());
            }

            block = world.BlockAccessor.GetBlock(neibpos.SouthCopy());
            if (block.FirstCodePart() == "monkeybridge" && block.LastCodePart() != "east" && block.LastCodePart() != "west")
            {
                if (block.FirstCodePart(1) != "null") dropQty = 1f;
                else dropQty = 0f;
                world.BlockAccessor.BreakBlock(neibpos.SouthCopy(), null, dropQty);
                BreakAbove(world, neibpos.SouthCopy());

            }
            block = world.BlockAccessor.GetBlock(neibpos.EastCopy());
            if (block.FirstCodePart() == "monkeybridge" && block.LastCodePart() != "north" && block.LastCodePart() != "south")
            {
                if (block.FirstCodePart(1) != "null") dropQty = 1f;
                else dropQty = 0f;
                world.BlockAccessor.BreakBlock(neibpos.EastCopy(), null, dropQty);
                BreakAbove(world, neibpos.EastCopy());

            }
            block = world.BlockAccessor.GetBlock(neibpos.WestCopy());
            if (block.FirstCodePart() == "monkeybridge" && block.LastCodePart() != "north" && block.LastCodePart() != "south")
            {
                if (block.FirstCodePart(1) != "null") dropQty = 1f;
                else dropQty = 0f;
                world.BlockAccessor.BreakBlock(neibpos.WestCopy(), null, dropQty);
                BreakAbove(world, neibpos.WestCopy());

            }
            block = world.BlockAccessor.GetBlock(neibpos.DownCopy());
            if (block.FirstCodePart() == "monkeybridge" && thisblock.FirstCodePart() == "monkeybridge")
            {
                if (block.FirstCodePart(1) != "null") dropQty = 1f;
                else dropQty = 0f;
                world.BlockAccessor.BreakBlock(neibpos.DownCopy(), null, dropQty);
                BreakAbove(world, neibpos.DownCopy());
            }
        }
        // base.OnNeighbourBlockChange(world, pos, neibpos);
    }

    public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
    {
        //need to override this, otherwise sometimes the bridge just vanishes when you jump on it (and reappears when you reload the game)
        //other times, it breaks when you jump on it

        //to-do: based on collision speed, I could have it break when you jump on it...
        //System.Diagnostics.Debug.WriteLine("speed " + collideSpeed.ToString());
        //base.OnEntityCollide(world, entity, pos, facing, collideSpeed, isImpact);
    }
}