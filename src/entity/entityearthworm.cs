using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Server;
using Vintagestory.API.Datastructures;
using Vintagestory.API;

public class EntityEarthworm : EntityAgent
{
    int cnt = 0;
    public static Random rnd = new Random();
    public int escapePercent = 1; // 1

    public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
    {
        base.Initialize(properties, api, InChunkIndex3d);
    }

    public override void OnInteract(EntityAgent byEntity, ItemSlot slot, Vec3d hitPosition, EnumInteractMode mode)
    {
        if (!Alive || World.Side == EnumAppSide.Client || mode == 0)
        {
            base.OnInteract(byEntity, slot, hitPosition, mode);
            return;
        }

        if (byEntity.Controls.Sneak)
        {
            ItemStack stack = new ItemStack(byEntity.World.GetItem(new AssetLocation("primitivesurvival:earthworm")));
            if (!byEntity.TryGiveItemStack(stack))
            {
                byEntity.World.SpawnItemEntity(stack, ServerPos.XYZ);
            }
            Die(); //remove from the ground
            return;
        }
        base.OnInteract(byEntity, slot, hitPosition, mode);
    }

    public override void OnGameTick(float dt)
    {
        base.OnGameTick(dt);
      
        if (cnt++ > 2000)
        {
            cnt = 0;
            ItemStack castings = new ItemStack(World.GetItem(new AssetLocation("primitivesurvival:earthwormcastings")));
            JsonObject obj = castings.Collectible.Attributes["fertilizerProps"];
            FertilizerProps props = obj.AsObject<FertilizerProps>();

            BlockPos BelowPos = Pos.XYZ.AsBlockPos;
            Block blockBelow = World.BlockAccessor.GetBlock(BelowPos);

            //small aside - get the temperature and kill the worm if necessary
            ClimateCondition conds = World.BlockAccessor.GetClimateAt(BelowPos, EnumGetClimateMode.NowValues);
            int escaped = rnd.Next(100);
            if (conds.Temperature <= 0 || conds.Temperature >= 35 || escaped < escapePercent)
            {
                Die(); //too cold or hot or the worm just left
            }
            else
            {
                if (blockBelow.FirstCodePart() == "farmland")
                {
                    //System.Diagnostics.Debug.WriteLine("firstblock:" + blockBelow.FirstCodePart());
                    BlockEntityFarmland befarmland = World.BlockAccessor.GetBlockEntity(BelowPos) as BlockEntityFarmland;
                    if (befarmland != null)
                    {
                        befarmland.WaterFarmland(0.3f); //aerate 
                        TreeAttribute tree = new TreeAttribute();
                        befarmland.ToTreeAttributes(tree);

                        float slowN = tree.GetFloat("slowN");
                        float slowK = tree.GetFloat("slowK");
                        float slowP = tree.GetFloat("slowP");
                        if (slowN <= 150) slowN += props.N;
                        if (slowK <= 150) slowK += props.K;
                        if (slowP <= 150) slowP += props.P;

                        if (slowN < 150 && slowK < 150 && slowP < 150)
                        {
                            tree.SetFloat("slowN", slowN);
                            tree.SetFloat("slowK", slowK);
                            tree.SetFloat("slowP", slowP);
                            befarmland.FromTreeAttributes(tree, World);
                            befarmland.MarkDirty();
                            World.BlockAccessor.MarkBlockDirty(BelowPos);
                        }
                        else
                        {
                            //For better or worse, you've created a block of Worm Castings
                            Block block = World.BlockAccessor.GetBlock(new AssetLocation("primitivesurvival:earthwormcastings"));
                            World.BlockAccessor.SetBlock(block.BlockId, BelowPos);
                            World.BlockAccessor.MarkBlockDirty(BelowPos);
                        }
                    }
                }
            }
        }
    }
}

