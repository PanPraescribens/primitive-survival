using System;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;

public class BlockNailSpike : Block
{

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
        { return false; }

        string face = blockSel.Face.ToString();
        if (face != "up" && face != "down")
        { return false; }
        

        Block blockToPlace = this;
        if (blockToPlace != null)
        {
            string facing;
            BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
            double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
            double dz = byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
            double angle = Math.Atan2(dx, dz);
            angle += Math.PI;
            angle /= Math.PI / 4;
            int halfQuarter = Convert.ToInt32(angle);
            halfQuarter %= 8;

            if (halfQuarter == 4) facing = "s";
            else if (halfQuarter == 6) facing = "e";
            else if (halfQuarter == 2) facing = "w";
            else facing = "n";

            string newPath = blockToPlace.Code.Path;
            newPath = newPath.Replace("-n", facing);
            blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(newPath));
            if (blockToPlace != null)
            {
                world.BlockAccessor.SetBlock(blockToPlace.BlockId, blockSel.Position);
                return true;
            }
        }
        return false;
    }

}