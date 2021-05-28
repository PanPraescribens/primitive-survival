using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using System.Diagnostics;

namespace primitiveSurvival
{
    public class RightClickPickupSpawnWorm : BlockBehavior
    {

        protected static Random rnd = new Random();
        public RightClickPickupSpawnWorm(Block block) : base(block)
        {
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            //Debug.WriteLine("picked up:" + block.Code.Path);
            int wormOdds = 6;
            if (!block.Code.Path.Contains("flint") && !block.Code.Path.Contains("stick")) wormOdds = 16;

            //Debug.WriteLine("odds:" + wormOdds);
            int rando = rnd.Next(wormOdds);
            if (rando < 1)
            {
                if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                {
                    if (world.Side == EnumAppSide.Server)
                    {
                        BlockSelection blockSelBelow = blockSel.Clone();
                        blockSelBelow.Position.Y -= 1;
                        Block blockBelow = world.BlockAccessor.GetBlock(blockSelBelow.Position);

                        //get the temperature too
                        ClimateCondition conds = world.BlockAccessor.GetClimateAt(blockSelBelow.Position, EnumGetClimateMode.NowValues);
                        //Debug.WriteLine("temperature:" + conds.Temperature);
                        string bPath = blockBelow.Code.Path;
                        //Debug.WriteLine("block below:" + bPath);
                        if (bPath.Contains("soil") && conds.Temperature > 0 && conds.Temperature < 35)
                        {
                            //Debug.WriteLine("worm");
                            BlockPos pos = blockSel.Position;
                            EntityProperties type = world.GetEntityType(new AssetLocation("primitivesurvival:earthworm"));
                            Entity entity = world.ClassRegistry.CreateEntity(type);

                            if (entity != null)
                            {
                                entity.ServerPos.X = pos.X + 0.5f;
                                entity.ServerPos.Y = pos.Y + 0f;
                                entity.ServerPos.Z = pos.Z + 0.5f;
                                entity.ServerPos.Yaw = (float)rnd.NextDouble() * 2 * GameMath.PI;
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
