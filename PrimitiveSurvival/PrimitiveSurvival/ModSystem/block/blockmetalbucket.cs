namespace PrimitiveSurvival.ModSystem
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Util;
    using Vintagestory.GameContent;

    public class BlockMetalBucket : BlockLiquidContainerBase
    {

        public override float CapacityLitres => 10;


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (byPlayer.Entity.Controls.Sneak) //sneak place only
            { return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode); }
            return false;
        }


        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel == null || byEntity.Controls.Sneak)
            { return; }
            var bucketPath = slot.Itemstack.Block.Code.Path;
            var pos = blockSel.Position;
            var block = byEntity.World.BlockAccessor.GetBlock(pos);

            var contentStack = this.GetContent(byEntity.World, slot.Itemstack);
            if (block.Code.Path.Contains("lava-") && bucketPath.Contains("-empty") && contentStack == null) //lava block and empty bucket?
            {
                if (block.Code.Path.Contains("-7")) //lots of lava?
                {
                    if (this.api.World.Side == EnumAppSide.Server)
                    {
                        var newblock = byEntity.World.GetBlock(new AssetLocation("primitivesurvival:" + bucketPath.Replace("-empty", "-filled")));
                        var newStack = new ItemStack(newblock);
                        slot.TakeOut(1);
                        slot.MarkDirty();
                        if (!byEntity.TryGiveItemStack(newStack))
                        { this.api.World.SpawnItemEntity(newStack, byEntity.Pos.XYZ.AddCopy(0, 0.5, 0)); }
                        newblock = byEntity.World.GetBlock(new AssetLocation("lava-still-3"));
                        this.api.World.BlockAccessor.SetBlock(newblock.BlockId, pos); //replace lava with less lava
                        this.api.World.BlockAccessor.MarkBlockDirty(pos); //let the server know the lava's gone
                    }
                    handHandling = EnumHandHandling.PreventDefault;
                    if ((byEntity as EntityPlayer) != null)  //bucket scooping lava animation
                    {
                        if (((byEntity as EntityPlayer).Player as IClientPlayer) != null)
                        { ((byEntity as EntityPlayer).Player as IClientPlayer).TriggerFpAnimation(EnumHandInteract.HeldItemInteract); }
                    }
                    return;
                }
            }
            else
            { base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling); }
        }


        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            var val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (val)
            {
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMetalBucket bect)
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
            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
            Dictionary<int, MeshRef> meshrefs = null;
            if (capi.ObjectCache.TryGetValue(this.Code.Path + "MeshRefs", out var obj))
            { meshrefs = obj as Dictionary<int, MeshRef>; }
            else
            { capi.ObjectCache[this.Code.Path + "MeshRefs"] = meshrefs = new Dictionary<int, MeshRef>(); }
            var contentStack = this.GetContent(capi.World, itemstack);
            if (contentStack == null)
            { return; }
            var hashcode = this.GetBucketHashCode(capi.World, contentStack);

            if (!meshrefs.TryGetValue(hashcode, out var meshRef))
            {
                var meshdata = this.GenMesh(capi, contentStack);
                meshrefs[hashcode] = meshRef = capi.Render.UploadMesh(meshdata);
            }
            renderinfo.ModelRef = meshRef;
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
            if (capi.ObjectCache.TryGetValue(this.Code.Path + "MeshRefs", out var obj))
            {
                var meshrefs = obj as Dictionary<int, MeshRef>;
                foreach (var val in meshrefs)
                { val.Value.Dispose(); }
                capi.ObjectCache.Remove(this.Code.Path + "MeshRefs");
            }
        }


        public MeshData GenMesh(ICoreClientAPI capi, ItemStack contentStack, BlockPos forBlockPos = null)
        {
            var shape = capi.Assets.TryGet("primitivesurvival:shapes/block/metalbucket/empty.json").ToObject<Shape>();
            capi.Tesselator.TesselateShape(this, shape, out var bucketmesh);

            if (contentStack != null)
            {
                var props = GetInContainerProps(contentStack);
                if (props != null)
                {
                    var contentSource = new ContainerTextureSource(capi, contentStack, props.Texture);
                    shape = capi.Assets.TryGet("game:shapes/block/wood/bucket/contents.json").ToObject<Shape>();
                    capi.Tesselator.TesselateShape("metalbucket", shape, out var contentMesh, contentSource, new Vec3f(this.Shape.rotateX, this.Shape.rotateY, this.Shape.rotateZ));
                    contentMesh.Translate(0, GameMath.Min(7 / 16f, contentStack.StackSize / props.ItemsPerLitre * 0.7f / 16f), 0);

                    if (props.ClimateColorMap != null)
                    {
                        var col = capi.World.ApplyColorMapOnRgba(props.ClimateColorMap, null, ColorUtil.WhiteArgb, 196, 128, false);
                        if (forBlockPos != null)
                        { col = capi.World.ApplyColorMapOnRgba(props.ClimateColorMap, null, ColorUtil.WhiteArgb, forBlockPos.X, forBlockPos.Y, forBlockPos.Z, false); }
                        var rgba = ColorUtil.ToBGRABytes(col);
                        for (var i = 0; i < contentMesh.Rgba.Length; i++)
                        { contentMesh.Rgba[i] = (byte)(contentMesh.Rgba[i] * rgba[i % 4] / 255); }
                    }
                    for (var i = 0; i < contentMesh.Flags.Length; i++)
                    { contentMesh.Flags[i] = contentMesh.Flags[i] & ~(1 << 12); } // Remove water waving flag
                    bucketmesh.AddMeshData(contentMesh);

                    if (forBlockPos != null) // Water flags
                    {
                        bucketmesh.CustomInts = new CustomMeshDataPartInt(bucketmesh.FlagsCount)
                        {
                            Count = bucketmesh.FlagsCount
                        };
                        bucketmesh.CustomInts.Values.Fill(0x4000000); // light foam only
                        bucketmesh.CustomFloats = new CustomMeshDataPartFloat(bucketmesh.FlagsCount * 2)
                        {
                            Count = bucketmesh.FlagsCount * 2
                        };
                    }
                }
            }
            return bucketmesh;
        }



        public static string PerishableInfoCompact(ICoreAPI api, ItemSlot contentSlot, float ripenRate, bool withStackName = true)
        {
            var dsc = new StringBuilder();
            if (withStackName)
            { dsc.Append(contentSlot.Itemstack.GetName()); }
            TransitionState[] transitionStates = null;
            if (contentSlot.Itemstack != null)
            { transitionStates = contentSlot.Itemstack.Collectible.UpdateAndGetTransitionStates(api.World, contentSlot); }

            if (transitionStates != null)
            {
                for (var i = 0; i < transitionStates.Length; i++)
                {
                    var comma = ", ";
                    var state = transitionStates[i];
                    var prop = state.Props;
                    var perishRate = contentSlot.Itemstack.Collectible.GetTransitionRateMul(api.World, contentSlot, prop.Type);

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
                                double hoursPerday = api.World.Calendar.HoursPerDay;
                                if (freshHoursLeft / hoursPerday >= api.World.Calendar.DaysPerYear)
                                { dsc.Append(comma + Lang.Get("fresh for {0} years", Math.Round(freshHoursLeft / hoursPerday / api.World.Calendar.DaysPerYear, 1))); }
                                else if (freshHoursLeft > hoursPerday)
                                { dsc.Append(comma + Lang.Get("fresh for {0} days", Math.Round(freshHoursLeft / hoursPerday, 1))); }
                                else
                                { dsc.Append(comma + Lang.Get("fresh for {0} hours", Math.Round(freshHoursLeft, 1))); }
                            }
                            break;

                        case EnumTransitionType.Ripen:

                            if (transitionLevel > 0)
                            { dsc.Append(comma + Lang.Get("{1:0.#} days left to ripen ({0}%)", (int)Math.Round(transitionLevel * 100), (state.TransitionHours - state.TransitionedHours) / api.World.Calendar.HoursPerDay / ripenRate)); }
                            else
                            {
                                double hoursPerday = api.World.Calendar.HoursPerDay;
                                if (freshHoursLeft / hoursPerday >= api.World.Calendar.DaysPerYear)
                                { dsc.Append(comma + Lang.Get("will ripen in {0} years", Math.Round(freshHoursLeft / hoursPerday / api.World.Calendar.DaysPerYear, 1))); }
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
