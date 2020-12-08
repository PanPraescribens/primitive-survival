using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;


public class BlockMetalBucketFilled : Block
{
    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        if (byPlayer.Entity.Controls.Sneak) //sneak place only
        {
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode); 
        }
        return false;
    }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
    {
        if (blockSel == null) return;
        if (byEntity.Controls.Sneak) return;
        string bucketPath = slot.Itemstack.Block.Code.Path;
        BlockPos pos = blockSel.Position;
        Block block = byEntity.World.BlockAccessor.GetBlock(pos);
        
        if (api.World.Side == EnumAppSide.Server)
        {
            Block newblock = byEntity.World.GetBlock(new AssetLocation("primitivesurvival:" + bucketPath.Replace("-filled", "-empty")));
            ItemStack newStack = new ItemStack(newblock);
            slot.TakeOut(1);
            slot.MarkDirty();

            if (!byEntity.TryGiveItemStack(newStack))
            {
                api.World.SpawnItemEntity(newStack, byEntity.Pos.XYZ.AddCopy(0, 0.5, 0));
            }

            newblock = byEntity.World.GetBlock(new AssetLocation("lava-still-7"));
            BlockPos targetPos;
            if (block.IsLiquid()) targetPos = pos;
            else targetPos = blockSel.Position.AddCopy(blockSel.Face);
            api.World.BlockAccessor.SetBlock(newblock.BlockId, targetPos); //put lava above
            api.World.BlockAccessor.MarkBlockDirty(targetPos); //let the server know the lava's there
        }
        handHandling = EnumHandHandling.PreventDefault;
        return;
        
        //base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
    }

}


