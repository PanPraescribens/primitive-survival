namespace PrimitiveSurvival.ModSystem
{
    using System.Text;
    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using Vintagestory.API.Config;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Util;
    using Vintagestory.GameContent;
    //using System.Diagnostics;


    public class BlockIrrigationVessel : BlockLiquidContainerTopOpened
    {
        public override float CapacityLitres => 50;
        protected new WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            if (api.Side != EnumAppSide.Client)
            { return; }
            var capi = api as ICoreClientAPI;

            this.interactions = ObjectCacheUtil.GetOrCreate(api, "metalbucketfilled", () =>
            {
                var liquidContainerStacks = new List<ItemStack>();
                foreach (var obj in api.World.Collectibles)
                {
                    if (obj is ILiquidSource || obj is ILiquidSink || obj is BlockWateringCan)
                    {
                        var stacks = obj.GetHandBookStacks(capi);
                        if (stacks == null)
                        { continue; }

                        foreach (var stack in stacks)
                        {
                            stack.StackSize = 1;
                            liquidContainerStacks.Add(stack);
                        }
                    }
                }
                var lcstacks = liquidContainerStacks.ToArray();
                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-bucket-rightclick",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = lcstacks
                    }
                };
            });
        }


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (byPlayer.Entity.Controls.Sneak) //sneak place only
            { return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode); }
            return false;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (!playerSlot.Empty)
            {
                var block = world.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Default);
                var path = block.Code.Path;
                var playerStack = playerSlot.Itemstack;
                if (playerStack.Collectible.Code.Path.Contains("soil-") && path.Contains("-normal"))
                {
                    var be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEIrrigationVessel;
                    be.Buried = true;
                    be.MarkDirty();
                    playerSlot.TakeOut(1);
                    playerSlot.MarkDirty();
                    return true;
                }
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel == null || byEntity.Controls.Sneak)
            { return; }
            var bucketPath = slot.Itemstack.Block.Code.Path;
            var pos = blockSel.Position;
            var block = byEntity.World.BlockAccessor.GetBlock(pos, BlockLayersAccess.Default);

            var contentStack = this.GetContent(slot.Itemstack);

            var onlywater = true;
            if (onlywater)
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
            }

        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            if (blockAccessor.GetBlockEntity(pos) is BEIrrigationVessel be)
            {
                if (be.Buried)
                {
                    var box = new Cuboidf[1];
                    box[0] = new Cuboidf(0f, 0f, 0f, 1f, 1f, 1f).RotatedCopy(0, 0, 0, new Vec3d(0.5, 0.5, 0.5));
                    return box;
                }
            }
            return base.GetCollisionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            if (blockAccessor.GetBlockEntity(pos) is BEIrrigationVessel be)
            {
                if (be.Buried)
                {
                    var box = new Cuboidf[1];
                    box[0] = new Cuboidf(0f, 0f, 0f, 1f, 1f, 1f).RotatedCopy(0, 0, 0, new Vec3d(0.5, 0.5, 0.5));
                    return box;
                }
            }
            return base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            var val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            /*  NO ROTATE
            if (val)
            {
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEIrrigationVessel bect)
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
            */
            return val;
        }


        public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture)
        {
            var tesselator = capi.Tesselator;
            var shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
            tesselator.TesselateShape(shapePath, shape, out var mesh, texture, new Vec3f(0, 0, 0));
            return mesh;

        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return this.interactions;
        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = "heldhelp-fill",
                    MouseButton = EnumMouseButton.Right,
                    ShouldApply = (wi, bs, es) => this.GetCurrentLitres(inSlot.Itemstack) < this.CapacityLitres
                },
                new WorldInteraction()
                {
                    ActionLangCode = "heldhelp-place",
                    HotKeyCode = "sneak",
                    MouseButton = EnumMouseButton.Right,
                    ShouldApply = (wi, bs, es) => true
                }
            };
        }


        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            var dsc = new StringBuilder();
            dsc.AppendLine(base.GetPlacedBlockInfo(world, pos, forPlayer));

            if (world.BlockAccessor.GetBlockEntity(pos) is BEIrrigationVessel be)
            {
                if (be.Buried)
                {
                    if (be.Inventory.Empty)
                    {
                        dsc.AppendLine().AppendLine(Lang.GetMatching("primitivesurvival:fill-irrigationvessel"));
                    }
                    else if (be.Inventory[0].Itemstack.Collectible.Code.Path.Contains("water"))
                    {
                        dsc.AppendLine().AppendLine(Lang.GetMatching("primitivesurvival:working-irrigationvessel"));
                    }
                    else
                    {
                        dsc.AppendLine().AppendLine(Lang.GetMatching("primitivesurvival:wrongliquid-irrigationvessel"));
                    }
                }
                else
                {
                    dsc.AppendLine().AppendLine(Lang.GetMatching("primitivesurvival:bury-irrigationvessel"));
                }
            }
            return dsc.AppendLine().ToString();
        }
    }
}
