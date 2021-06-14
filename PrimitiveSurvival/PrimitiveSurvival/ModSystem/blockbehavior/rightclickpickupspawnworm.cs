namespace PrimitiveSurvival.ModSystem
{
    using System;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using PrimitiveSurvival.ModConfig;

    public class RightClickPickupSpawnWorm : BlockBehavior
    {

        protected static readonly Random Rnd = new Random();


        public RightClickPickupSpawnWorm(Block block) : base(block)
        {
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            var block = world.BlockAccessor.GetBlock(blockSel.Position);
            var wormOdds = ModConfig.Loaded.WormFoundPercentRock; //6
            if (!block.Code.Path.Contains("flint") && !block.Code.Path.Contains("stick"))
            { wormOdds = ModConfig.Loaded.WormFoundPercentStickFlint; } //16
            var rando = Rnd.Next(wormOdds);
            if (rando < 1)
            {
                if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                {
                    if (world.Side == EnumAppSide.Server)
                    {
                        var blockSelBelow = blockSel.Clone();
                        blockSelBelow.Position.Y -= 1;
                        var blockBelow = world.BlockAccessor.GetBlock(blockSelBelow.Position);
                        var conds = world.BlockAccessor.GetClimateAt(blockSelBelow.Position, EnumGetClimateMode.NowValues); //get the temperature too
                        if (blockBelow.Code.Path.Contains("soil") && conds.Temperature > 0 && conds.Temperature < 35)
                        {
                            var pos = blockSel.Position;
                            var type = world.GetEntityType(new AssetLocation("primitivesurvival:earthworm"));
                            var entity = world.ClassRegistry.CreateEntity(type);
                            if (entity != null)
                            {
                                entity.ServerPos.X = pos.X + 0.5f;
                                entity.ServerPos.Y = pos.Y + 0f;
                                entity.ServerPos.Z = pos.Z + 0.5f;
                                entity.ServerPos.Yaw = (float)Rnd.NextDouble() * 2 * GameMath.PI;
                                world.SpawnEntity(entity);
                                handling = EnumHandling.PreventDefault;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
