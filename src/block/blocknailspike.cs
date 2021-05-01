using Vintagestory.API.Common;

namespace primitiveSurvival
{
    public class BlockNailSpike : Block
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            return false;
        }
    }
}