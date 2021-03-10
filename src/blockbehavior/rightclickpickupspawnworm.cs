using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

public class RightClickPickupSpawnWorm : BlockBehavior
{

    protected static Random rnd = new Random();
    public RightClickPickupSpawnWorm(Block block) : base(block)
    {
    }


    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        //System.Diagnostics.Debug.WriteLine("picked up:" + block.Code.Path);
        int wormOdds = 6;
        if (!block.Code.Path.Contains("flint") && !block.Code.Path.Contains("stick")) wormOdds = 16;
        
        //System.Diagnostics.Debug.WriteLine("odds:" + wormOdds);
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
                    //System.Diagnostics.Debug.WriteLine("temperature:" + conds.Temperature);
                    string bPath = blockBelow.Code.Path;
                    //System.Diagnostics.Debug.WriteLine("block below:" + bPath);
                    if (bPath.Contains("soil") && conds.Temperature > 0 && conds.Temperature < 35)
                    {
                        //System.Diagnostics.Debug.WriteLine("worm");
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

