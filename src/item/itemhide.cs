using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

public class ItemHide : Item 
{
   
    protected static Random rnd = new Random();


    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        handling = EnumHandHandling.PreventDefaultAction;

    }

    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {
        if (blockSel == null || byEntity == null) return;
        IWorldAccessor world = byEntity.World;
        if (world.Side == EnumAppSide.Client) return; //Maybe?
        if (world == null) return;

        string itemType = slot.Itemstack.Item.FirstCodePart(1);
        if (itemType != "pelt") return;  //pelts only!

        string itemSize = slot.Itemstack.Item.FirstCodePart(2);
        string outPath = "primitivesurvival:";
        string outHide;

        if (itemSize == "huge") return;
        else if (itemSize == "large")
        {  outHide = "sheephide-bighorn-"; }
        else if (itemSize == "medium")
        {
            string[] hideTypes = { "wolfhide-", "hyenahide-hyena-", "pighide-wild-" };
            outHide = hideTypes[rnd.Next(hideTypes.Count())];
            if (outHide == "wolfhide-")
            {
                string[] mammalTypes = { "grey-", "steppe-", "tundra-" };
                outHide += mammalTypes[rnd.Next(mammalTypes.Count())];
            }
        }
        else //small
        {
            string[] hideTypes = { "foxhide-", "harehide-", "raccoonhide-raccoon-" };
            outHide = hideTypes[rnd.Next(hideTypes.Count())];
            if (outHide == "foxhide-")
            {
                string[] mammalTypes = { "forest-", "arctic-"};
                outHide += mammalTypes[rnd.Next(mammalTypes.Count())];
            }
            else if (outHide == "harehide-")
            {
                string[] mammalTypes = { "arctic-", "ashgrey-", "darkbrown-", "darkgrey-", "desert-", "gold-", "lightbrown-", "lightgrey-", "silver-", "smokegrey-" };
                outHide += mammalTypes[rnd.Next(mammalTypes.Count())];
            }
        }

        string[] genderTypes = { "male-", "female-" };
        string outGender = genderTypes[rnd.Next(genderTypes.Count())];
        outPath += outHide + outGender + "north";

        string facing;
        BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
        double dx = byEntity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
        double dz = byEntity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
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

        outPath = outPath.Replace("north", facing);
        System.Diagnostics.Debug.WriteLine(outPath);  
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        string face = blockSel.Face.ToString();
        if (face == "down") return;

        if (face == "up")
        {
            if (block.Fertility <= 0 && !(block.Code.Path.Contains("tallgrass-"))) return;

            BlockSelection blockSelAbove = blockSel.Clone();
            blockSelAbove.Position.Y += 1;
            Block blockAbove = world.BlockAccessor.GetBlock(blockSelAbove.Position);
            if (blockAbove.BlockId == 0 || blockAbove.Code.Path.Contains("tallgrass-") || block.Code.Path.Contains("tallgrass-"))
            {
                Block blockNew = world.GetBlock(new AssetLocation(outPath));
                IBlockAccessor blockAccessor = world.BlockAccessor;
                if (block.Code.Path.Contains("tallgrass-"))
                { blockAccessor.SetBlock(blockNew.BlockId, blockSel.Position); }
                else
                { blockAccessor.SetBlock(blockNew.BlockId, blockSelAbove.Position); }
                slot.TakeOut(1);
                slot.MarkDirty();
            }
        }
    }
}
