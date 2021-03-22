using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;
using primitiveSurvival;

public class BlockRaft : Block
{

    long handlerId;

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        string facing = SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
        bool placed;
        placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        if (placed)
        {
            Block block = api.World.BlockAccessor.GetBlock(blockSel.Position);
            string newPath = block.Code.Path;
            newPath = newPath.Replace("north", facing);
            block = api.World.GetBlock(block.CodeWithPath(newPath));
            api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
        }
        return placed;
    }


    public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
    {
        if (byEntity.World.Side == EnumAppSide.Client)
        {
            byEntity.World.UnregisterCallback(handlerId);
            handlerId = byEntity.World.RegisterCallback(AfterAwhile, 1350);
        }

        if (byEntity.World is IClientWorldAccessor)
        {
            byEntity.StopAnimation("swim");
            if (byEntity.IsEyesSubmerged() || (byEntity.FeetInLiquid))
            {
                //in water
                byEntity.StartAnimation("swim");

                FpHandTransform.Rotation.X = -120;
                FpHandTransform.Rotation.Y = 44;
                FpHandTransform.Rotation.Z = 180;
                FpHandTransform.Scale = 1.92f;

                TpHandTransform.Translation.X = 0.7f;
                TpHandTransform.Translation.Z = -0.65f;
                TpHandTransform.Rotation.X = -160;
                TpHandTransform.Rotation.Y = -24;
                TpHandTransform.Rotation.Z = 43;
                TpHandTransform.Origin.X = -0.1f;
                TpHandTransform.Origin.Y = 0.5f;
                TpHandTransform.Origin.Z = 0.4f;
                TpHandTransform.Scale = 1.06f;

                if (byEntity.IsEyesSubmerged()) //under water
                {

                    // a bit of forward motion to prevent using waterfalla as elevators
                    // but mostly a floatation device when under water
                    Vec3d pos = byEntity.Pos.HorizontalAheadCopy(0.01f).XYZ;
                    double newX = byEntity.Pos.X - pos.X;
                    double newZ = byEntity.Pos.Z - pos.Z;
                    byEntity.Pos.Motion.X -= newX;
                    byEntity.Pos.Motion.Z -= newZ;
                    byEntity.Pos.Motion.Y += PrimitiveSurvivalConfig.Loaded.raftFlotationModifier;
                }
                else //feet in water
                {
                    Vec3d pos = byEntity.Pos.HorizontalAheadCopy(0.05f).XYZ;
                    double newX = byEntity.Pos.X - pos.X;
                    double newZ = byEntity.Pos.Z - pos.Z;
                    byEntity.Pos.Motion.X -= newX * PrimitiveSurvivalConfig.Loaded.raftWaterSpeedModifier;
                    byEntity.Pos.Motion.Z -= newZ * PrimitiveSurvivalConfig.Loaded.raftWaterSpeedModifier;
                }

            }
            else //on land
            {
                CancelRaft(byEntity);
            }
        }
    }


    private void CancelRaft(EntityAgent byEntity)
    {
        byEntity.StopAnimation("swim");
        FpHandTransform.Rotation.X = -90;
        FpHandTransform.Rotation.Y = 73;
        FpHandTransform.Rotation.Z = 174;
        FpHandTransform.Scale = 1.5f;

        TpHandTransform.Translation.X = -0.3f;
        TpHandTransform.Translation.Z = 0;
        TpHandTransform.Rotation.X = 116;
        TpHandTransform.Rotation.Y = -22;
        TpHandTransform.Rotation.Z = 163;
        TpHandTransform.Origin.X = 0f;
        TpHandTransform.Origin.Y = 0.25f;
        TpHandTransform.Origin.Z = 0f;
        TpHandTransform.Scale = 0.94f;
    }

    private void AfterAwhile(float dt)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;
        IClientPlayer plr = capi.World.Player;
        EntityPlayer byEntity = plr.Entity;
        string stackname = "null";
        if (byEntity.RightHandItemSlot.Itemstack != null)
            stackname = byEntity.RightHandItemSlot.Itemstack.GetName();
        if (stackname != "Raft")
            CancelRaft(byEntity);
        //System.Diagnostics.Debug.WriteLine(stackname);
    }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        handling = EnumHandHandling.PreventDefault;
        base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
    }
}