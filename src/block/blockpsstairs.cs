using System;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

public class BlockPSStairs : Block
{
    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode)) return false;

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

            string horVer = "-";
            if (blockSel.Face.IsVertical)
            { horVer += blockSel.Face.ToString(); }
            else
            { horVer += (blockSel.HitPosition.Y < 0.5 ? BlockFacing.UP : BlockFacing.DOWN).ToString(); }

            string newPath = blockToPlace.Code.Path;
            newPath = newPath.Replace("north", facing);
            newPath = newPath.Replace("-up", horVer);

            string newfacing = "";
            if (halfQuarter % 2 != 0)
            {
                string facebase = blockSel.Face.ToString();
                if (!blockSel.DidOffset) //Grass fix?  WTF
                {
                    if (facebase == "up") targetPos.Y -= 1;
                    else if (facebase == "east") targetPos.X -= 1;
                    else if (facebase == "west") targetPos.X += 1;
                    else if (facebase == "north") targetPos.Z += 1;
                    else if (facebase == "south") targetPos.Z -= 1;
                }
                BlockPos destPos = targetPos;
    
                if (facebase == "up") destPos.Y += 1;
                else if (facebase == "down") destPos.Y -= 1;
                else if (facebase == "east") destPos.X += 1;
                else if (facebase == "west") destPos.X -= 1;
                else if (facebase == "north") destPos.Z -= 1;
                else if (facebase == "south") destPos.Z += 1;

                //check block below and then above, if a corner or sideways make this block sideways too
                BlockPos thisneib = destPos.DownCopy();
                Block testBlock = api.World.BlockAccessor.GetBlock(thisneib);
                string inOutVer = "-normal";

                if (testBlock.Code.Path.Contains("-inside-up") || testBlock.Code.Path.Contains("-sideways"))
                {
                    newPath = newPath.Replace(horVer, "-sideways");
                } 
                else
                {
                    thisneib = destPos.UpCopy();
                    testBlock = api.World.BlockAccessor.GetBlock(thisneib);
                    if (testBlock.Code.Path.Contains("-inside-down") || testBlock.Code.Path.Contains("-sideways"))
                    {
                        newPath = newPath.Replace(horVer, "-sideways");
                    }
                }
                
                if (newPath.Contains("-sideways"))
                {
                    if (halfQuarter == 7) newfacing = "north";
                    else if (halfQuarter == 1) newfacing = "west";
                    else if (halfQuarter == 5) newfacing = "east";
                    else if (halfQuarter == 3) newfacing = "south";
                    else newfacing = facing;
                }
                else
                {
                    newPath = newPath.Replace("-normal", "-outside");
                    inOutVer = "-outside";
                }

                BlockPos[] neibPos;

                if (facing == "northwest") neibPos = new BlockPos[] { destPos.NorthCopy(), destPos.WestCopy() };
                else if (facing == "northeast") neibPos = new BlockPos[] { destPos.NorthCopy(), destPos.EastCopy() };
                else if (facing == "southwest") neibPos = new BlockPos[] { destPos.SouthCopy(), destPos.WestCopy() };
                else neibPos = new BlockPos[] { destPos.SouthCopy(), destPos.EastCopy() };

                int ncnt = 0; int scnt = 0; int ecnt = 0; int wcnt = 0;
                foreach (BlockPos neib in neibPos)
                {
                    testBlock = api.World.BlockAccessor.GetBlock(neib);
                    if (testBlock.BlockId != 0)
                    {
                        if (!testBlock.Code.Path.Contains("outside"))
                        {
                            if (testBlock.Code.Path.Contains("north")) ncnt++;
                            if (testBlock.Code.Path.Contains("south")) scnt++;
                            if (testBlock.Code.Path.Contains("east")) ecnt++;
                            if (testBlock.Code.Path.Contains("west")) wcnt++;
                        }
                    }

                    //System.Diagnostics.Debug.WriteLine("nsew: " + ncnt + " " + scnt + " " + ecnt + " " + wcnt);
                    if (newPath.Contains("northwest") && (ncnt > scnt || wcnt > ecnt)) newPath = newPath.Replace(inOutVer, "-inside");
                    else if (newPath.Contains("southwest") && (scnt > ncnt || wcnt > ecnt)) newPath = newPath.Replace(inOutVer, "-inside");
                    else if (newPath.Contains("northeast") && (ncnt > scnt || ecnt > wcnt)) newPath = newPath.Replace(inOutVer, "-inside");
                    else if (scnt > ncnt || ecnt > wcnt) newPath = newPath.Replace(inOutVer, "-inside");

                    //System.Diagnostics.Debug.WriteLine(newPath);
                    if (newPath.Contains("inside-sideways"))
                        newPath = newPath.Replace("-sideways", horVer);
                    else if (newPath.Contains("normal-sideways"))
                        newPath = newPath.Replace(facing, newfacing);
                }
            }
            blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(newPath));
            world.BlockAccessor.SetBlock(blockToPlace.BlockId, blockSel.Position);
            return true;
        }
        return false;
    }
}