using Vintagestory.API.Common;
using System;

public class ItemFishingHook : Item
{
    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        if (blockSel == null) return;
        handling = EnumHandHandling.PreventDefault;
    }

    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {
        if (blockSel == null) return;
        IWorldAccessor world = byEntity.World;
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        if (block.FirstCodePart() == "trotline")
        {
            if (block.LastCodePart(1) == "middle")
            {
                String newPath = block.Code.Path;
                if (slot.GetStackName().Contains("Baited"))
                { newPath = newPath.Replace("middle", "hookbaited"); }
                else
                { newPath = newPath.Replace("middle", "hook"); }
                block = world.GetBlock(block.CodeWithPath(newPath));
                world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
                slot.TakeOut(1);
                slot.MarkDirty();
            }
        }
    }
}
