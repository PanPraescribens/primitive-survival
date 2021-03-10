using System;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

public class BlockPSPillar : Block
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

            if (halfQuarter == 4) facing = "south";
            else if (halfQuarter == 6) facing = "east";
            else if (halfQuarter == 2) facing = "west";
            else if (halfQuarter == 7) facing = "northeast";
            else if (halfQuarter == 1) facing = "northwest";
            else if (halfQuarter == 5) facing = "southeast";
            else if (halfQuarter == 3) facing = "southwest";
            else facing = "north";

            string newPath = blockToPlace.Code.Path;
            
            bool done = false; 
            string neibPath;
            BlockPos belowPos = blockSel.Position.DownCopy();
            Block belowBlock = api.World.BlockAccessor.GetBlock(belowPos);
            BlockPos abovePos = blockSel.Position.UpCopy();
            Block aboveBlock = api.World.BlockAccessor.GetBlock(abovePos);
            if (face == "up")
            {
                if (belowBlock.FirstCodePart() != "steatitepillar")
                {
                    newPath = newPath.Replace("-middle", "-bottom");
                   
                    if (aboveBlock.FirstCodePart() != "steatitepillar") done = true;

                }
                else
                {
                    newPath = newPath.Replace("north", belowBlock.LastCodePart()); //orient like block below
                    
                    if (belowBlock.FirstCodePart(2) == "top") //if block below is a top, make it a middle
                    {
                        neibPath = belowBlock.Code.Path;
                        neibPath = neibPath.Replace("-top", "-middle");
                        blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(neibPath));
                        world.BlockAccessor.SetBlock(blockToPlace.BlockId, belowPos);
                    }

                }
                if (!done) //check above last
                {
                    if (aboveBlock.FirstCodePart() != "steatitepillar") newPath = newPath.Replace("-middle", "-top");
                    else
                    {
                        newPath = newPath.Replace("north", aboveBlock.LastCodePart()); //orient like block above
                        neibPath = aboveBlock.Code.Path;
                        neibPath = neibPath.Replace("-bottom", "-middle");
                        blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(neibPath));
                        world.BlockAccessor.SetBlock(blockToPlace.BlockId, abovePos);
                    }

                }
                else newPath = newPath.Replace("north", facing);
            }
            else //face == down
            {
                if (aboveBlock.FirstCodePart() != "steatitepillar")
                {
                    newPath = newPath.Replace("-middle", "-top");
                    if (belowBlock.FirstCodePart() != "steatitepillar") done = true;
                }
                else
                {
                    newPath = newPath.Replace("north", aboveBlock.LastCodePart()); //orient like block above 
                    //if above block is a bottom, make it a middle
                    if (aboveBlock.FirstCodePart(2) == "bottom")
                    {
                        neibPath = aboveBlock.Code.Path;
                        neibPath = neibPath.Replace("-bottom", "-middle");
                        blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(neibPath));
                        world.BlockAccessor.SetBlock(blockToPlace.BlockId, abovePos);
                    }

                }
                if (!done) //check below last
                {
                    if (belowBlock.FirstCodePart() != "steatitepillar") newPath = newPath.Replace("-middle", "-bottom");
                    else
                    {
                        newPath = newPath.Replace("north", belowBlock.LastCodePart()); //orient like block below
                        neibPath = belowBlock.Code.Path;
                        neibPath = neibPath.Replace("-top", "-middle");
                        blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(neibPath));
                        world.BlockAccessor.SetBlock(blockToPlace.BlockId, belowPos);
                    }

                }
                else newPath = newPath.Replace("north", facing);
            }

            //System.Diagnostics.Debug.WriteLine(newPath);
            newPath = newPath.Replace("westwest", "west"); //don't even f*&^k ask
            newPath = newPath.Replace("easteast", "east");
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