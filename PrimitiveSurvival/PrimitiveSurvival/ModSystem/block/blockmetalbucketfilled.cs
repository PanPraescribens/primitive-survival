namespace PrimitiveSurvival.ModSystem
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.MathTools;

    public class BlockMetalBucketFilled : Block
    {

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
            if (blockSel == null)
            { return; }
            if (byEntity.Controls.Sneak)
            { return; }
            var bucketPath = slot.Itemstack.Block.Code.Path;
            var pos = blockSel.Position;
            var block = byEntity.World.BlockAccessor.GetBlock(pos);

            if (byEntity.Controls.Sprint && (this.api.World.Side == EnumAppSide.Server))
            {
                var newblock = byEntity.World.GetBlock(new AssetLocation("primitivesurvival:" + bucketPath.Replace("-filled", "-empty")));
                var newStack = new ItemStack(newblock);
                slot.TakeOut(1);
                slot.MarkDirty();
                if (!byEntity.TryGiveItemStack(newStack))
                {
                    this.api.World.SpawnItemEntity(newStack, byEntity.Pos.XYZ.AddCopy(0, 0.5, 0));
                }
                newblock = byEntity.World.GetBlock(new AssetLocation("lava-still-7"));
                BlockPos targetPos;
                if (block.IsLiquid())
                { targetPos = pos; }
                else
                { targetPos = blockSel.Position.AddCopy(blockSel.Face); }
                this.api.World.BlockAccessor.SetBlock(newblock.BlockId, targetPos); //put lava above
                this.api.World.BlockAccessor.MarkBlockDirty(targetPos); //let the server know the lava's there
            }

            if ((byEntity as EntityPlayer) != null)  //bucket dumping lava animation
                if (((byEntity as EntityPlayer).Player as IClientPlayer) != null)
                { ((byEntity as EntityPlayer).Player as IClientPlayer).TriggerFpAnimation(EnumHandInteract.HeldItemInteract); }

            handHandling = EnumHandHandling.PreventDefault;
            return;
        }


        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            var val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (val)
            {
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMetalBucketFilled bect)
                {
                    var targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                    var dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                    var dz = byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                    var angleHor = (float)Math.Atan2(dx, dz);
                    var deg22dot5rad = GameMath.PIHALF / 4;
                    var roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
                    bect.MeshAngle = roundRad;
                }
            }
            return val;
        }


        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            Dictionary<int, MeshRef> meshrefs = null;
            if (capi.ObjectCache.TryGetValue("bucketMeshRefs", out var obj))
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
            var s = contentStack.StackSize + "x" + contentStack.Collectible.Code.ToShortString();
            return s.GetHashCode();
        }


        public override void OnUnloaded(ICoreAPI api)
        {
            if (!(api is ICoreClientAPI capi))
            { return; }
            if (capi.ObjectCache.TryGetValue("bucketMeshRefs", out var obj))
            {
                var meshrefs = obj as Dictionary<int, MeshRef>;
                foreach (var val in meshrefs)
                {
                    val.Value.Dispose();
                }
                capi.ObjectCache.Remove("bucketMeshRefs");
            }
        }

        public MeshData GenMesh(ICoreClientAPI capi, ItemStack contentStack, BlockPos forBlockPos = null)
        {
            var shape = capi.Assets.TryGet("primitivesurvival:shapes/block/metalbucket/filled.json").ToObject<Shape>();
            capi.Tesselator.TesselateShape(this, shape, out var bucketmesh);
            return bucketmesh;
        }


        public static string PerishableInfoCompact(ICoreAPI Api, ItemSlot contentSlot, float ripenRate, bool withStackName = true)
        {
            var dsc = new StringBuilder();
            if (withStackName)
            {
                dsc.Append(contentSlot.Itemstack.GetName());
            }
            TransitionState[] transitionStates = null;
            if (contentSlot.Itemstack != null)
            {
                transitionStates = contentSlot.Itemstack.Collectible.UpdateAndGetTransitionStates(Api.World, contentSlot);
            }

            if (transitionStates != null)
            {
                for (var i = 0; i < transitionStates.Length; i++)
                {
                    var comma = ", ";
                    var state = transitionStates[i];
                    var prop = state.Props;
                    var perishRate = contentSlot.Itemstack.Collectible.GetTransitionRateMul(Api.World, contentSlot, prop.Type);
                    if (perishRate <= 0)
                    { continue; }
                    var transitionLevel = state.TransitionLevel;
                    var freshHoursLeft = state.FreshHoursLeft / perishRate;
                    switch (prop.Type)
                    {
                        case EnumTransitionType.Perish:
                            if (transitionLevel > 0)
                            { dsc.Append(comma + Lang.Get("{0}% spoiled", (int)Math.Round(transitionLevel * 100))); }
                            else
                            {
                                double hoursPerday = Api.World.Calendar.HoursPerDay;
                                if (freshHoursLeft / hoursPerday >= Api.World.Calendar.DaysPerYear)
                                { dsc.Append(comma + Lang.Get("fresh for {0} years", Math.Round(freshHoursLeft / hoursPerday / Api.World.Calendar.DaysPerYear, 1))); }
                                else if (freshHoursLeft > hoursPerday)
                                { dsc.Append(comma + Lang.Get("fresh for {0} days", Math.Round(freshHoursLeft / hoursPerday, 1))); }
                                else
                                { dsc.Append(comma + Lang.Get("fresh for {0} hours", Math.Round(freshHoursLeft, 1))); }
                            }
                            break;

                        case EnumTransitionType.Ripen:

                            if (transitionLevel > 0)
                            { dsc.Append(comma + Lang.Get("{1:0.#} days left to ripen ({0}%)", (int)Math.Round(transitionLevel * 100), (state.TransitionHours - state.TransitionedHours) / Api.World.Calendar.HoursPerDay / ripenRate)); }
                            else
                            {
                                double hoursPerday = Api.World.Calendar.HoursPerDay;
                                if (freshHoursLeft / hoursPerday >= Api.World.Calendar.DaysPerYear)
                                { dsc.Append(comma + Lang.Get("will ripen in {0} years", Math.Round(freshHoursLeft / hoursPerday / Api.World.Calendar.DaysPerYear, 1))); }
                                else if (freshHoursLeft > hoursPerday)
                                { dsc.Append(comma + Lang.Get("will ripen in {0} days", Math.Round(freshHoursLeft / hoursPerday, 1))); }
                                else
                                { dsc.Append(comma + Lang.Get("will ripen in {0} hours", Math.Round(freshHoursLeft, 1))); }
                            }
                            break;
                    }
                }
            }
            return dsc.ToString();
        }
    }
}
