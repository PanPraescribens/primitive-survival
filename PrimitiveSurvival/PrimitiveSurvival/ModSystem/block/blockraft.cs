namespace PrimitiveSurvival.ModSystem
{
    using Vintagestory.API.Common;
    using Vintagestory.API.Client;
    using PrimitiveSurvival.ModConfig;

    public class BlockRaft : Block
    {

        private long handlerId;


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            var facing = SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
            bool placed;
            placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            if (placed)
            {
                var block = this.api.World.BlockAccessor.GetBlock(blockSel.Position);
                var newPath = block.Code.Path;
                newPath = newPath.Replace("north", facing);
                block = this.api.World.GetBlock(block.CodeWithPath(newPath));
                this.api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
            }
            return placed;
        }


        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            if (byEntity.World.Side == EnumAppSide.Client)
            {
                byEntity.World.UnregisterCallback(this.handlerId);
                this.handlerId = byEntity.World.RegisterCallback(this.AfterAwhile, 1350);
            }

            if (byEntity.World is IClientWorldAccessor)
            {
                byEntity.StopAnimation("swim");
                if (byEntity.IsEyesSubmerged() || byEntity.FeetInLiquid)
                {
                    //in water
                    byEntity.StartAnimation("swim");

                    this.FpHandTransform.Rotation.X = -120;
                    this.FpHandTransform.Rotation.Y = 44;
                    this.FpHandTransform.Rotation.Z = 180;
                    this.FpHandTransform.Scale = 1.92f;

                    this.TpHandTransform.Translation.X = 0.7f;
                    this.TpHandTransform.Translation.Z = -0.65f;
                    this.TpHandTransform.Rotation.X = -160;
                    this.TpHandTransform.Rotation.Y = -24;
                    this.TpHandTransform.Rotation.Z = 43;
                    this.TpHandTransform.Origin.X = -0.1f;
                    this.TpHandTransform.Origin.Y = 0.5f;
                    this.TpHandTransform.Origin.Z = 0.4f;
                    this.TpHandTransform.Scale = 1.06f;

                    if (byEntity.IsEyesSubmerged()) //under water
                    {
                        // a bit of forward motion to prevent using waterfalls as elevators
                        // but mostly a floatation device when under water
                        var pos = byEntity.Pos.HorizontalAheadCopy(0.01f).XYZ;
                        var newX = byEntity.Pos.X - pos.X;
                        var newZ = byEntity.Pos.Z - pos.Z;
                        byEntity.Pos.Motion.X -= newX;
                        byEntity.Pos.Motion.Z -= newZ;
                        byEntity.Pos.Motion.Y += ModConfig.Loaded.RaftFlotationModifier;
                    }
                    else //feet in water
                    {
                        var pos = byEntity.Pos.HorizontalAheadCopy(0.05f).XYZ;
                        var newX = byEntity.Pos.X - pos.X;
                        var newZ = byEntity.Pos.Z - pos.Z;
                        byEntity.Pos.Motion.X -= newX * ModConfig.Loaded.RaftWaterSpeedModifier;
                        byEntity.Pos.Motion.Z -= newZ * ModConfig.Loaded.RaftWaterSpeedModifier;
                    }

                }
                else //on land
                {
                    this.CancelRaft(byEntity);
                }
            }
        }


        private void CancelRaft(EntityAgent byEntity)
        {
            byEntity.StopAnimation("swim");
            this.FpHandTransform.Rotation.X = -90;
            this.FpHandTransform.Rotation.Y = 73;
            this.FpHandTransform.Rotation.Z = 174;
            this.FpHandTransform.Scale = 1.5f;

            this.TpHandTransform.Translation.X = -0.3f;
            this.TpHandTransform.Translation.Z = 0;
            this.TpHandTransform.Rotation.X = 116;
            this.TpHandTransform.Rotation.Y = -22;
            this.TpHandTransform.Rotation.Z = 163;

            this.TpHandTransform.Origin.X = 0f;
            this.TpHandTransform.Origin.Y = 0.25f;
            this.TpHandTransform.Origin.Z = 0f;
            this.TpHandTransform.Scale = 0.94f;
        }

        private void AfterAwhile(float dt)
        {
            var capi = this.api as ICoreClientAPI;
            var plr = capi.World.Player;
            var byEntity = plr.Entity;
            var stackname = "null";
            if (byEntity.RightHandItemSlot.Itemstack != null)
            { stackname = byEntity.RightHandItemSlot.Itemstack.GetName(); }
            if (stackname != "Raft")
            { this.CancelRaft(byEntity); }
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.PreventDefault;
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }
    }
}
