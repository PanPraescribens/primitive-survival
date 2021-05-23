using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace primitiveSurvival
{
    public class BlockBlood : Block, VintagestoryAPI.Common.Collectible.Block.IBlockFlowing
    {
        public string Flow { get; set; }
        public Vec3i FlowNormali { get => null; set { } }
        public bool IsLava => true; //removes the "foam" but you also lose the water droplets
                                    // my own shader would be nice, for best of both worlds

       public BlockBlood() : base()
        { }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }


        public override bool ShouldPlayAmbientSound(IWorldAccessor world, BlockPos pos)
        {
            // Play water wave sound when above is air and below is a solid block
            return
                world.BlockAccessor.GetBlock(pos.X, pos.Y + 1, pos.Z).Id == 0 &&
                world.BlockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z).SideSolid[BlockFacing.UP.Index];
        }
    }
}

