using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;
using Vintagestory.API.Common.Entities;

public class BlockTemporal : Block
{

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        if (this.Code.Path.Contains("necronomicon"))
            return false;
        string facing = SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
        bool placed;
        placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        if (placed)
        {
            Block block = api.World.BlockAccessor.GetBlock(blockSel.Position);
            string newPath = block.Code.Path;
            newPath = newPath.Replace("north", facing);
            block = api.World.GetBlock(block.CodeWithPath(newPath));
            api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
        }
        return placed;
    }
}
