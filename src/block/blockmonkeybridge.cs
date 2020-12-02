using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;

public class BlockMonkeyBridge : Block
{

    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
    {
        Block block = world.BlockAccessor.GetBlock(neibpos);
        Block thisblock = world.BlockAccessor.GetBlock(pos);
        if (block.BlockId <= 0) //block removed
        {
            block = world.BlockAccessor.GetBlock(neibpos.NorthCopy());
            if (block.FirstCodePart() == "monkeybridge" && block.LastCodePart() != "east" && block.LastCodePart() != "west")
                world.BlockAccessor.BreakBlock(neibpos.NorthCopy(), null);
            block = world.BlockAccessor.GetBlock(neibpos.SouthCopy());
            if (block.FirstCodePart() == "monkeybridge" && block.LastCodePart() != "east" && block.LastCodePart() != "west")
                world.BlockAccessor.BreakBlock(neibpos.SouthCopy(), null);
            block = world.BlockAccessor.GetBlock(neibpos.EastCopy());
            if (block.FirstCodePart() == "monkeybridge" && block.LastCodePart() != "north" && block.LastCodePart() != "south")
                world.BlockAccessor.BreakBlock(neibpos.EastCopy(), null);
            block = world.BlockAccessor.GetBlock(neibpos.WestCopy());
            if (block.FirstCodePart() == "monkeybridge" && block.LastCodePart() != "north" && block.LastCodePart() != "south")
                world.BlockAccessor.BreakBlock(neibpos.WestCopy(), null);

            block = world.BlockAccessor.GetBlock(neibpos.DownCopy());
            if (block.FirstCodePart() == "monkeybridge" && thisblock.FirstCodePart() == "monkeybridge")
            world.BlockAccessor.BreakBlock(neibpos.DownCopy(), null);

        }
    }
}
