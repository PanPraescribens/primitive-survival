using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Datastructures;

public class ItemFishingHook : Item
{
    /*
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
        if (block.FirstCodePart() == "limbtrotlinelure")
        {
            string path = block.Code.Path;
            if (!path.Contains("hook"))
            {
                string lastPart = path.Split('-').Last(); //direction
                string hookType = "hook-" + slot.Itemstack.Item.Code.Path.Split('-').Last() + "-" + lastPart;
                path = path.Replace(lastPart, hookType);
                block = world.GetBlock(block.CodeWithPath(path));

                // Nope dont change the block anymore
                //world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);

                // Example In case we need to touch the entity
                //BELimbTrotLine bec = byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BELimbTrotLine;
                //bec = byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BELimbTrotLine;
                //if (bec != null) bec.UpdateTree(tree);

                slot.TakeOut(1);
                slot.MarkDirty();
            }
        }
    } */
}

