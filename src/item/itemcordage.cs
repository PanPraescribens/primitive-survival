using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;
using System.Diagnostics;

namespace primitiveSurvival
{
    public class ItemCordage : Item
    {
        public static BlockPos[] areaAround(BlockPos Pos)
        {
            return new BlockPos[]
            {  Pos.NorthCopy(),Pos.SouthCopy(),Pos.EastCopy(),Pos.WestCopy(),Pos.NorthCopy().EastCopy(), Pos.SouthCopy().WestCopy(),Pos.SouthCopy().EastCopy(), Pos.NorthCopy().WestCopy() };
        }

        public static string BlockHeight(IBlockAccessor blockAccessor, BlockPos pos)
        {
            string heightStr = "small";
            Block blockChk = blockAccessor.GetBlock(pos);
            if (blockChk.BlockId > 0)
            {
                Cuboidf[] sbs = blockChk.GetSelectionBoxes(blockAccessor, pos);
                if (sbs == null) return heightStr;
                foreach (Cuboidf sb in sbs)
                {
                    if (Math.Abs(sb.Y2 - sb.Y1) >= 0.5) heightStr = "large";
                    else if ((Math.Abs(sb.Y2 - sb.Y1) >= 0.3) && heightStr != "large") heightStr = "medium";
                }
            }
            return heightStr;
        }

        public static string BlockWidth(IBlockAccessor blockAccessor, BlockPos pos)
        {
            string widthStr = "small";
            Block blockChk = blockAccessor.GetBlock(pos);
            if (blockChk.BlockId > 0)
            {
                if (blockChk.Code.Path.Contains("stake-") || blockChk.Code.Path.Contains("stakeinwater-")) widthStr = "small";
                else if (blockChk.Code.GetName().Contains("woodenfence-")) widthStr = "medium";
                else
                {
                    Cuboidf[] sbs = blockChk.GetSelectionBoxes(blockAccessor, pos);
                    foreach (Cuboidf sb in sbs)
                    { if (Math.Abs(sb.X2 - sb.X1) > 0.5) widthStr = "large"; }
                }
            }
            return widthStr;
        }

        public static bool ValidEndpoint(IBlockAccessor blockAccessor, BlockPos testpos)
        {
            Block blockChk = blockAccessor.GetBlock(testpos);
            bool validEnd = false;
            if (blockChk.Code.GetName().Contains("woodenfence-") || blockChk.Code.Path.Contains("stake-") || blockChk.Code.Path.Contains("stakeinwater-"))
            { validEnd = true; }
            else if (blockChk.Code.GetName().Contains("limbtrotlinelure"))
            { validEnd = false; }
            else
            {
                Cuboidf[] sb = blockChk.GetSelectionBoxes(blockAccessor, testpos);
                validEnd = BlockHeight(blockAccessor, testpos) == "large";
                BlockPos aroundPos = new BlockPos(testpos.X, testpos.Y, testpos.Z);
                BlockPos[] around = areaAround(aroundPos);
                foreach (BlockPos neighbor in around)
                {
                    blockChk = blockAccessor.GetBlock(neighbor);
                    if (blockChk.BlockId > 0)
                    {
                        if (BlockHeight(blockAccessor, neighbor) != "small" && !blockChk.Code.Path.Contains("limbtrotlinelure"))
                        {
                            if ((!blockChk.Code.GetName().Contains("woodenfence-")) && !blockChk.Code.Path.Contains("stake-") && !blockChk.Code.Path.Contains("stakeinwater-"))
                            { validEnd = false; }
                        }
                    }
                }
            }
            return validEnd;
        }

        public virtual int GetLineLength(IBlockAccessor blockAccessor, BlockSelection blockDest, BlockFacing facing)
        {
            int maxLength = 20;
            int count = 0;
            bool foundEnd = false;
            BlockPos testpos = blockDest.Position.Copy();
            Block blockChk;
            string testPath = "primitivesurvival:limbtrotlinelure-middle-north";
            Block testBlock = blockAccessor.GetBlock(new AssetLocation(testPath));
            do
            {
                count++;
                testpos = testpos.Offset(facing);
                blockChk = blockAccessor.GetBlock(testpos);
                if (!(blockChk.IsReplacableBy(testBlock)))
                {
                    if ((BlockHeight(blockAccessor, testpos) != "small") || blockChk.Code.GetName().Contains("limbtrotlinelure-"))
                    {
                        foundEnd = true;
                        if (count == 1) return 0;
                        if (count == 2) return 1;
                        if (!ValidEndpoint(blockAccessor, testpos)) return 1;
                    }
                }
            }
            while ((foundEnd == false) && (count < maxLength));
            if (!foundEnd)
            { return 1; }
            return count;
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.PreventDefaultAction;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
            if (blockSel == null || byEntity.World == null || byPlayer == null) return;

            BlockFacing facing = byPlayer.CurrentBlockSelection.Face.Opposite;
            IBlockAccessor blockAccessor = byEntity.World.BlockAccessor;
            BlockPos currPos = blockSel.Position.Copy();
            bool validStart = ValidEndpoint(blockAccessor, currPos);

            if (facing.IsHorizontal && validStart)
            {
                int linelength = GetLineLength(blockAccessor, blockSel, facing);
                //Debug.WriteLine("Line length " + linelength);
                ItemStack stack = slot.Itemstack;
                string tempStr = "";
                if ((slot.StackSize < linelength - 1) && (linelength > 0)) linelength = 999;
                if (linelength > 0)
                {
                    //every once in a while linelength loses its s*&t
                    //maybe this prevents that???
                    if (linelength > 20)
                    { linelength = 1; }

                    string blockSize = BlockWidth(blockAccessor, currPos);
                    string newPath;
                    for (int count = 0; count < linelength; count++)
                    {

                        newPath = "primitivesurvival:limbtrotlinelure-";
                        if (count == 0)
                        {
                            currPos = currPos.AddCopy(facing);
                            if (linelength > 1) tempStr = "withmiddle-";
                            newPath += "end-" + blockSize + "-" + tempStr + facing.ToString();
                            Block blocknew = byEntity.World.GetBlock(new AssetLocation(newPath));
                            blockAccessor.SetBlock(blocknew.BlockId, currPos);
                        }
                        else if (count < linelength - 1)
                        {
                            currPos = currPos.AddCopy(facing);
                            newPath += "middle-" + facing.ToString();
                            Block blocknew = byEntity.World.GetBlock(new AssetLocation(newPath));
                            blockAccessor.SetBlock(blocknew.BlockId, currPos);
                        }
                    }
                    //the last block
                    if (linelength > 1)
                    {
                        BlockPos endPos = currPos.AddCopy(facing);
                        blockSize = BlockWidth(blockAccessor, endPos);
                        newPath = "primitivesurvival:limbtrotlinelure-end-" + blockSize + "-withmiddle-" + byPlayer.CurrentBlockSelection.Face.ToString();
                        Block blocknew = byEntity.World.GetBlock(new AssetLocation(newPath));
                        blockAccessor.SetBlock(blocknew.BlockId, currPos);
                        linelength = linelength - 1; //fix to ensure we're removing the correct amount of cordage
                    }
                    slot.TakeOut(linelength);
                    slot.MarkDirty();
                }
            }
        }
    }
}