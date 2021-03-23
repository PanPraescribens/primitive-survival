using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;


public class RightClickPickupRaft : BlockBehavior
{
    public RightClickPickupRaft(Block block) : base(block)
    {
    }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        ItemStack[] stacks = new ItemStack[] { block.OnPickBlock(world, blockSel.Position) };

        if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
        {
            return false;
        }

        if (!byPlayer.Entity.Controls.Sneak && byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
        {
            if (world.Side == EnumAppSide.Server)
            {
                Block raftBlock = world.GetBlock(new AssetLocation("primitivesurvival:raft-north"));
                ItemStack newStack = new ItemStack(raftBlock);
                if (byPlayer.InventoryManager.TryGiveItemstack(newStack, true))
                {
                    world.BlockAccessor.SetBlock(0, blockSel.Position);
                    world.BlockAccessor.TriggerNeighbourBlockUpdate(blockSel.Position);
                    handling = EnumHandling.PreventDefault;
                    return true;
                }
            }
            handling = EnumHandling.PreventDefault;
            return true;
        }
        return false;
    }


    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos, ref EnumHandling handling)
    {
        return base.OnPickBlock(world, pos, ref handling);
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handled)
    {
        return new WorldInteraction[]
        {
                new WorldInteraction()
                {
                    ActionLangCode = "blockhelp-behavior-rightclickpickup",
                    MouseButton = EnumMouseButton.Right,
                    RequireFreeHand = true
                }
        };
    }

}


