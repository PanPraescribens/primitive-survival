using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

public class ItemWoodSpikeBundle : Item
{
  
    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        handling = EnumHandHandling.PreventDefaultAction;

    }

    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {
        if (blockSel == null || byEntity == null) return;
        IWorldAccessor world = byEntity.World;
        if (world == null) return;
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);

        string face = blockSel.Face.ToString();
        //System.Diagnostics.Debug.WriteLine("Face:" + face);
        if (face == "down") return;

        if (face == "up")
        {
            if ( block.Fertility <= 0 && !(block.Code.Path.Contains("tallgrass-")) ) return;

            BlockSelection blockSelAbove = blockSel.Clone();
            blockSelAbove.Position.Y += 1;
            Block blockAbove = world.BlockAccessor.GetBlock(blockSelAbove.Position);
            if (blockAbove.BlockId == 0 || blockAbove.Code.Path.Contains("tallgrass-") || block.Code.Path.Contains("tallgrass-"))
            {


                Block blockNew = world.GetBlock(new AssetLocation("primitivesurvival:woodspikes"));
                IBlockAccessor blockAccessor = world.BlockAccessor;
                if (block.Code.Path.Contains("tallgrass-"))
                { blockAccessor.SetBlock(blockNew.BlockId, blockSel.Position); }
                else
                { blockAccessor.SetBlock(blockNew.BlockId, blockSelAbove.Position); }
                slot.TakeOut(1);
                slot.MarkDirty();
            }
        }
        else //nsew
        {
            BlockSelection blockSelBeside = blockSel.Clone();
            if (face == "east") blockSelBeside.Position.X += 1;
            else if (face == "west") blockSelBeside.Position.X -= 1;
            else if (face == "north") blockSelBeside.Position.Z -= 1;
            else blockSelBeside.Position.Z += 1;
            Block blockBeside = world.BlockAccessor.GetBlock(blockSelBeside.Position);

            //System.Diagnostics.Debug.WriteLine(block.FirstCodePart());
            
            if (blockBeside.BlockId == 0 || block.FirstCodePart() == "woodsupportspikes")
            {
                bool placeOk = false;
                if (blockBeside.BlockId == 0 && block.Fertility > 0) placeOk = true;
                else
                {
                    string selFace = block.LastCodePart();
                    //System.Diagnostics.Debug.WriteLine("selFace:" + selFace);
                    //System.Diagnostics.Debug.WriteLine("Face:" + face);
                    if ( (face == "east" || face == "west") && (selFace == "north" || selFace == "south")) placeOk = true;
                    else if ((face == "north" || face == "south") && (selFace == "east" || selFace == "west")) placeOk = true;
                }
                if (placeOk)
                {
                    Block blockNew = world.GetBlock(new AssetLocation("primitivesurvival:woodsupportspikes-" + face));
                    IBlockAccessor blockAccessor = world.BlockAccessor;
                    blockAccessor.SetBlock(blockNew.BlockId, blockSelBeside.Position);
                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
            }
        }
    }
}
