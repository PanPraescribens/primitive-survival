using Vintagestory.API.Common;

namespace primitiveSurvival
{
    public class BlockAlcove : Block
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            string facing = SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
            bool placed;
            placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            if (placed)
            {
                Block block = api.World.BlockAccessor.GetBlock(blockSel.Position);
                string newPath = block.Code.Path;
                newPath = newPath.Replace("north", facing);
                block = api.World.GetBlock(block.CodeWithPath(newPath));
                api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
            }
            return placed;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (!playerSlot.Empty)
            {
                ItemStack playerStack = playerSlot.Itemstack;
                if (playerStack.Block != null)
                {
                    if (playerStack.Block.Code.Path.Contains("torch-up"))
                    {
                        Block blockToPlace = this;
                        string newPath = blockToPlace.Code.Path;
                        if (newPath.Contains("-unlit"))
                        {
                            newPath = newPath.Replace("-unlit", "-lit");
                            blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(newPath));
                            world.BlockAccessor.SetBlock(blockToPlace.BlockId, blockSel.Position);
                            return true;
                        }
                    }
                }
                else if (playerStack.Item != null)
                {
                    if (playerStack.Item.Code.Path.Contains("candle"))
                    {
                        Block blockToPlace = this;
                        string newPath = blockToPlace.Code.Path;
                        if (newPath.Contains("-unlit"))
                        {
                            newPath = newPath.Replace("-unlit", "-lit");
                            blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(newPath));
                            world.BlockAccessor.SetBlock(blockToPlace.BlockId, blockSel.Position);
                            return true;
                        }
                    }
                }
            }
            else
            {
                Block blockToPlace = this;
                string newPath = blockToPlace.Code.Path;
                if (newPath.Contains("-lit"))
                {
                    newPath = newPath.Replace("-lit", "-unlit");
                    blockToPlace = api.World.GetBlock(blockToPlace.CodeWithPath(newPath));
                    world.BlockAccessor.SetBlock(blockToPlace.BlockId, blockSel.Position);
                    return true;
                }
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}