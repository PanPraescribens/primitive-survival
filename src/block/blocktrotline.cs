using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
public class BlockTrotline : Block
{
    public virtual int GetTrotlineLength(IBlockAccessor blockAccessor, BlockPos pos, string facing)
    {
        int length = 0;
        Block blockSel = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
        Block blockAbove = blockAccessor.GetBlock(pos.X, pos.Y, pos.Z);
        if ((blockSel.Fertility <= 0) || ((blockAbove.BlockId > 0) && (blockAbove.LastCodePart() != "free")))
        { return length; }
        else
        {
            int maxLength = 20;
            int count = 0;
            bool foundEnd = false;
            do
            {
                count++;
                if (facing == "east")
                {
                    blockSel = blockAccessor.GetBlock(pos.X + count, pos.Y - 1, pos.Z);
                    blockAbove = blockAccessor.GetBlock(pos.X + count, pos.Y, pos.Z);
                }
                else if (facing == "west")
                {
                    blockSel = blockAccessor.GetBlock(pos.X - count, pos.Y - 1, pos.Z);
                    blockAbove = blockAccessor.GetBlock(pos.X - count, pos.Y, pos.Z);
                }
                else if (facing == "south")
                {
                    blockSel = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z + count);
                    blockAbove = blockAccessor.GetBlock(pos.X, pos.Y, pos.Z + count);
                }
                else //north
                {
                    blockSel = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z - count);
                    blockAbove = blockAccessor.GetBlock(pos.X, pos.Y, pos.Z - count);
                }
                if ((((blockSel.BlockId > 0) && (blockSel.LastCodePart() != "free")) && (!(blockSel.IsLiquid() && blockSel.LiquidLevel >= 4 && blockSel.LiquidCode.Contains("water")))) || (blockAbove.BlockId > 0))
                {
                    foundEnd = true; //check for water or interference along the way
                    length = 0;
                }
                if ((blockSel.Fertility > 0) && ((blockAbove.BlockId <= 0) || (blockAbove.LastCodePart() == "free")))
                {
                    foundEnd = true; //found a valid endpoint
                    length = count;
                }
            }
            while ((foundEnd == false) && (count < maxLength - 1));
        }
        return length;
    }

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        string facing = SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
        int lineLength = GetTrotlineLength(world.BlockAccessor, blockSel.Position.Copy(), facing);
        if (itemstack.StackSize <= (lineLength - 2))
        {
            failureCode = "you will need more trotline to span this area";
            return false;
        }
        if (lineLength > 1)
        {
            BlockSelection blockSrc = blockSel.Clone();
            blockSel = blockSel.Clone();
            bool placed = false;
            Block block = null;
            string newPath = "";
            for (int i = 0; i <= lineLength; i++)
            {
                if (facing == "east")
                { blockSel.Position.X = blockSrc.Position.X + i; }
                else if (facing == "west")
                { blockSel.Position.X = blockSrc.Position.X - i; }
                else if (facing == "south")
                { blockSel.Position.Z = blockSrc.Position.Z + i; }
                else //north
                { blockSel.Position.Z = blockSrc.Position.Z - i; }
                placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);

                block = api.World.BlockAccessor.GetBlock(blockSel.Position);
                newPath = block.Code.Path;
                newPath = newPath.Replace("north", facing);
                if (i == 0)
                { newPath = newPath.Replace("middle", "closeend"); }
                if (i == lineLength)
                { newPath = newPath.Replace("middle", "farend"); }
                block = api.World.GetBlock(block.CodeWithPath(newPath));
                api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
            }
            itemstack.StackSize -= (lineLength - 2); //ends dont count
            return true;
        }
        else
        {
            failureCode = "you need suitable ground at both ends of trotline and a clear line of sight";
            return false;
        }
    }

    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
    {
        Block block = api.World.BlockAccessor.GetBlock(pos);
        if (block.Code.Path.Contains("end-"))
        {
            if (!CanTrotlineStay(world.BlockAccessor, pos))
            {
                world.BlockAccessor.BreakBlock(pos, null);
            }
        }
    }

    public virtual bool CanTrotlineStay(IBlockAccessor blockAccessor, BlockPos pos)
    {
        Block block = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
        return block.Fertility > 0;
    }

    public virtual int removeTrotline(IWorldAccessor world, BlockPos pos, string dir, bool fromMiddle, int mult)
    {
        int count = 1;
        bool thisDirDone = false;
        BlockPos bPos = null;
        Block bBlock = null;
        do
        {
            if ((dir == "east") || (dir == "west"))
            { bPos = new BlockPos(pos.X + (count * mult), pos.Y, pos.Z); }
            else
            { bPos = new BlockPos(pos.X, pos.Y, pos.Z + (count * mult)); }
            bBlock = world.BlockAccessor.GetBlock(bPos);
            if (bBlock.FirstCodePart() == "trotline")
            {
                //if (bBlock.LastCodePart(1) != "middle")
                if (bBlock.LastCodePart(1) == "farend" || bBlock.LastCodePart(1) == "nearend")
                {
                    thisDirDone = true;
                    if (((count > 1) && (!fromMiddle)) || fromMiddle)
                    { world.BlockAccessor.SetBlock(0, bPos); }
                }
                else
                {
                    world.BlockAccessor.SetBlock(0, bPos);
                }
                count++;
            }
            else { thisDirDone = true; }
        }
        while (!thisDirDone);
        return count;
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        string dir = LastCodePart();
        string type = LastCodePart(1);
        int removed = 0;
        bool fromMiddle = false;
        if (type != "farend" && type == "nearend")
        { fromMiddle = true; }
        removed += removeTrotline(world, pos, dir, fromMiddle, 1);
        removed += removeTrotline(world, pos, dir, fromMiddle, -1);
        removed -= 3;
        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier * removed); //dont count endpoints
    }
}
