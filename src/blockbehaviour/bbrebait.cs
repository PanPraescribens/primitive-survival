using Vintagestory.API.Common;

public class BBRebait : BlockBehavior
{
    public BBRebait(Block block) : base(block)
    { }
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
        ItemStack stack = slot.Itemstack;
        if (stack != null)
        {
            if (stack.Item != null)
            {
                if (stack.Item.FirstCodePart() == "fruit")
                {
                    string newPath = block.Code.Path;
                    string savePath = newPath;
                    string ttype = block.FirstCodePart();
                    string state = block.LastCodePart();
                    string stateWhenType = block.LastCodePart(1);

                    //hook on a trotline?
                    if (ttype == "trotline" && stateWhenType == "hook")
                    { newPath = newPath.Replace("hook", "hookbaited"); }
                    //deadfall?
                    else if (ttype == "deadfall" && stateWhenType == "set")
                    { newPath = newPath.Replace("set", "baited"); }
                    //snare?
                    else if (ttype == "snare" && state == "set")
                    { newPath = newPath.Replace("set", "baited"); }
                    //snare?
                    else if (ttype == "fishbasket" && state == "empty")
                    { newPath = newPath.Replace("empty", "baited"); }
                    if (newPath != savePath)
                    {
                        block = world.GetBlock(block.CodeWithPath(newPath));
                        world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
                        slot.TakeOut(1);
                        slot.MarkDirty();
                        handling = EnumHandling.PreventDefault;
                    }
                }
            }
        }
        return true;
    }
}
