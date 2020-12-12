using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;


public class BlockMetalBucketFilled : Block
{

    /*public override float CapacityLitres
    {
        get { return 10; }
    }*/

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        if (byPlayer.Entity.Controls.Sneak) //sneak place only
        {
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode); 
        }
        return false;
    }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
    {
        if (blockSel == null) return;
        if (byEntity.Controls.Sneak) return;
        string bucketPath = slot.Itemstack.Block.Code.Path;
        BlockPos pos = blockSel.Position;
        Block block = byEntity.World.BlockAccessor.GetBlock(pos);

        if ((byEntity.Controls.Sprint) &&(api.World.Side == EnumAppSide.Server))
        {
            Block newblock = byEntity.World.GetBlock(new AssetLocation("primitivesurvival:" + bucketPath.Replace("-filled", "-empty")));
            ItemStack newStack = new ItemStack(newblock);
            slot.TakeOut(1);
            slot.MarkDirty();

            if (!byEntity.TryGiveItemStack(newStack))
            {
                api.World.SpawnItemEntity(newStack, byEntity.Pos.XYZ.AddCopy(0, 0.5, 0));
            }

            newblock = byEntity.World.GetBlock(new AssetLocation("lava-still-7"));
            BlockPos targetPos;
            if (block.IsLiquid()) targetPos = pos;
            else targetPos = blockSel.Position.AddCopy(blockSel.Face);
            api.World.BlockAccessor.SetBlock(newblock.BlockId, targetPos); //put lava above
            api.World.BlockAccessor.MarkBlockDirty(targetPos); //let the server know the lava's there
        }

        if (byEntity as EntityPlayer != null)  //bucket dumping lava animation
            if ((byEntity as EntityPlayer).Player as IClientPlayer != null)
                ((byEntity as EntityPlayer).Player as IClientPlayer).TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

        handHandling = EnumHandHandling.PreventDefault;
        return;
        
        //base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
    }




    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
    {
        bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        if (val)
        {
            //BlockEntityBucket bect = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityBucket;
            BEMetalBucketFilled bect = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEMetalBucketFilled;
            if (bect != null)
            {
                BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                double dz = byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                float angleHor = (float)Math.Atan2(dx, dz);

                float deg22dot5rad = GameMath.PIHALF / 4;
                float roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
                bect.MeshAngle = roundRad;
            }
        }
        return val;
    }


    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        Dictionary<int, MeshRef> meshrefs = null;

        object obj;
        if (capi.ObjectCache.TryGetValue("bucketMeshRefs", out obj))
        {
            meshrefs = obj as Dictionary<int, MeshRef>;
        }
        else
        {
            capi.ObjectCache["bucketMeshRefs"] = meshrefs = new Dictionary<int, MeshRef>();
        }
    }



    public int GetBucketHashCode(IClientWorldAccessor world, ItemStack contentStack)
    {
        string s = contentStack.StackSize + "x" + contentStack.Collectible.Code.ToShortString();
        return s.GetHashCode();
    }



    public override void OnUnloaded(ICoreAPI api)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;
        if (capi == null) return;

        object obj;
        if (capi.ObjectCache.TryGetValue("bucketMeshRefs", out obj))
        {
            Dictionary<int, MeshRef> meshrefs = obj as Dictionary<int, MeshRef>;

            foreach (var val in meshrefs)
            {
                val.Value.Dispose();
            }

            capi.ObjectCache.Remove("bucketMeshRefs");
        }
    }

    public MeshData GenMesh(ICoreClientAPI capi, ItemStack contentStack, BlockPos forBlockPos = null)
    {
        Shape shape = capi.Assets.TryGet("primitivesurvival:shapes/block/metalbucket/filled.json").ToObject<Shape>();
        MeshData bucketmesh;
        capi.Tesselator.TesselateShape(this, shape, out bucketmesh);
        return bucketmesh;
    }



    public static string PerishableInfoCompact(ICoreAPI Api, ItemSlot contentSlot, float ripenRate, bool withStackName = true)
    {
        StringBuilder dsc = new StringBuilder();

        if (withStackName)
        {
            dsc.Append(contentSlot.Itemstack.GetName());
        }

        TransitionState[] transitionStates = null;
        if (contentSlot.Itemstack != null)
            transitionStates = contentSlot.Itemstack.Collectible.UpdateAndGetTransitionStates(Api.World, contentSlot);

        if (transitionStates != null)
        {
            for (int i = 0; i < transitionStates.Length; i++)
            {
                string comma = ", ";

                TransitionState state = transitionStates[i];

                TransitionableProperties prop = state.Props;
                float perishRate = contentSlot.Itemstack.Collectible.GetTransitionRateMul(Api.World, contentSlot, prop.Type);

                if (perishRate <= 0) continue;

                float transitionLevel = state.TransitionLevel;
                float freshHoursLeft = state.FreshHoursLeft / perishRate;

                switch (prop.Type)
                {
                    case EnumTransitionType.Perish:


                        if (transitionLevel > 0)
                        {
                            dsc.Append(comma + Lang.Get("{0}% spoiled", (int)Math.Round(transitionLevel * 100)));
                        }
                        else
                        {
                            double hoursPerday = Api.World.Calendar.HoursPerDay;

                            if (freshHoursLeft / hoursPerday >= Api.World.Calendar.DaysPerYear)
                            {
                                dsc.Append(comma + Lang.Get("fresh for {0} years", Math.Round(freshHoursLeft / hoursPerday / Api.World.Calendar.DaysPerYear, 1)));
                            }
                            else if (freshHoursLeft > hoursPerday)
                            {
                                dsc.Append(comma + Lang.Get("fresh for {0} days", Math.Round(freshHoursLeft / hoursPerday, 1)));
                            }
                            else
                            {
                                dsc.Append(comma + Lang.Get("fresh for {0} hours", Math.Round(freshHoursLeft, 1)));
                            }
                        }
                        break;

                    case EnumTransitionType.Ripen:

                        if (transitionLevel > 0)
                        {
                            dsc.Append(comma + Lang.Get("{1:0.#} days left to ripen ({0}%)", (int)Math.Round(transitionLevel * 100), (state.TransitionHours - state.TransitionedHours) / Api.World.Calendar.HoursPerDay / ripenRate));
                        }
                        else
                        {
                            double hoursPerday = Api.World.Calendar.HoursPerDay;

                            if (freshHoursLeft / hoursPerday >= Api.World.Calendar.DaysPerYear)
                            {
                                dsc.Append(comma + Lang.Get("will ripen in {0} years", Math.Round(freshHoursLeft / hoursPerday / Api.World.Calendar.DaysPerYear, 1)));
                            }
                            else if (freshHoursLeft > hoursPerday)
                            {
                                dsc.Append(comma + Lang.Get("will ripen in {0} days", Math.Round(freshHoursLeft / hoursPerday, 1)));
                            }
                            else
                            {
                                dsc.Append(comma + Lang.Get("will ripen in {0} hours", Math.Round(freshHoursLeft, 1)));
                            }
                        }
                        break;
                }
            }

        }
        return dsc.ToString();
    }

}


