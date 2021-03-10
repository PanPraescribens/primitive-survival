using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

public class BlockRaft : Block 
{

    ILoadedSound sound;

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
        { return false; }
        return false;
    }


    public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
    {
        if (byEntity.World is IClientWorldAccessor)
        {
            //these two lines rotate the raft in hand when idle
            

            if(byEntity.IsEyesSubmerged())
            {
                FpHandTransform.Rotation.X = -120;
                FpHandTransform.Rotation.Y = 44;
                FpHandTransform.Rotation.Z = 180;
                FpHandTransform.Scale = 1.92f;

                TpHandTransform.Rotation.X = 20;
                TpHandTransform.Rotation.Y = 35;
                TpHandTransform.Rotation.Z = 125; //90;
                
                //TpHandTransform.Translation.Y = 0.5f; 
                TpHandTransform.Translation.Z = 0.2f; 

                Vec3d pos = byEntity.Pos.HorizontalAheadCopy(0.05f).XYZ;
                //System.Diagnostics.Debug.WriteLine("submerged");
                //byEntity.Pos.Motion.Y = GameMath.Clamp(-byEntity.Pos.Motion.Y * 0.8, -0.5, 0.5);
                byEntity.Pos.Motion.Y += 0.03;
                //System.Diagnostics.Debug.WriteLine("X:" + byEntity.Pos.X + "   to X:" + pos.X);
                double newX = byEntity.Pos.X - pos.X;
                double newZ = byEntity.Pos.Z - pos.Z;
                byEntity.Pos.Motion.X -= (newX * 2f);
                byEntity.Pos.Motion.Z -= (newZ * 2f);
                byEntity.StartAnimation("swim");
            }
            else
            {
                if (byEntity.FeetInLiquid)
                {
                    byEntity.StartAnimation("swim");
                }
                else
                { 
                    //finish
                    byEntity.StopAnimation("swim");

                    FpHandTransform.Rotation.X = -90;
                    FpHandTransform.Rotation.Y = 73;
                    FpHandTransform.Rotation.Z = 174;
                    FpHandTransform.Scale = 1.5f;

                    FpHandTransform.Rotation.X = -90;
                    
                    TpHandTransform.Rotation.X = 0;
                    TpHandTransform.Rotation.Y = 0; 
                    TpHandTransform.Rotation.Z = 45;
                    
                    //TpHandTransform.Translation.Y = 0;
                    TpHandTransform.Translation.Z = 0;
                }
            }

            /*IRenderAPI rapi = (byEntity.World.Api as ICoreClientAPI).Render;
            Vec3d aboveHeadPos = byEntity.Pos.XYZ.Add(0, byEntity.EyeHeight() - 0.1f, 0);
            Vec3d pos = MatrixToolsd.Project(aboveHeadPos, rapi.PerspectiveProjectionMat, rapi.PerspectiveViewMat, rapi.ScreenWidth, rapi.ScreenHeight);


            particlesHeld.minSize = 0.05f;
            particlesHeld.minSize = 0.15f;

            SpawnParticles(byEntity.World, pos);*/
        }

    }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        handling = EnumHandHandling.PreventDefault;

        if (!firstEvent)
        {
            return;
        }


        if (!byEntity.FeetInLiquid && api.Side == EnumAppSide.Client)
        {
            (api as ICoreClientAPI).TriggerIngameError(this, "notinwater", Lang.Get("Must stand in water to use a raft"));
            return;
        }
    }


    public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {
        if (( byEntity.Controls.Jump) && !byEntity.Controls.Sneak) return false; // Cancel if the player jumps or sneaks
        //byEntity.Controls.TriesToMove ||

        //string blockMaterialCode = GetBlockMaterialCode(slot.Itemstack);
        //if (blockMaterialCode == null || !slot.Itemstack.TempAttributes.GetBool("canpan")) return false;
        Vec3d pos = byEntity.Pos.AheadCopy(0.4f).XYZ;
        pos.Y += byEntity.LocalEyePos.Y - 0.4f;

        if (secondsUsed > 0.5f && api.World.Rand.NextDouble() > 0.5)
        {
            //Block block = api.World.GetBlock(new AssetLocation(blockMaterialCode));
            //Vec3d particlePos = pos.Clone();

            //particlePos.X += GameMath.Sin(-secondsUsed * 20) / 5f;
            //particlePos.Z += GameMath.Cos(-secondsUsed * 20) / 5f;
            //particlePos.Y -= 0.07f;

            //byEntity.World.SpawnCubeParticles(particlePos, new ItemStack(block), 0.3f, (int)(1.5f + (float)api.World.Rand.NextDouble()), 0.3f + (float)api.World.Rand.NextDouble() / 6f, (byEntity as EntityPlayer)?.Player);
        }


        if (byEntity.World is IClientWorldAccessor)
        {
            ModelTransform tf = new ModelTransform();

            tf.EnsureDefaultValues();

            tf.Translation.Set(-1f * (float)Easings.EaseOutBack(Math.Min(secondsUsed * 1.5f, 1)), (float)Easings.EaseOutBack(Math.Min(1, secondsUsed * 1.5f)) * 0.3f, -1 * (float)Easings.EaseOutBack(Math.Min(secondsUsed * 1.5f, 1)));

            if (secondsUsed > 0.5f)
            {
                tf.Translation.X += (float)GameMath.MurmurHash3Mod((int)(secondsUsed * 3), 0, 0, 10) / 600f;
                tf.Translation.Y += (float)GameMath.MurmurHash3Mod(0, (int)(secondsUsed * 3), 0, 10) / 600f;

                if (sound == null)
                {
                    sound = (api as ICoreClientAPI).World.LoadSound(new SoundParams()
                    {
                        Location = new AssetLocation("sounds/player/swim.ogg"),
                        ShouldLoop = false,
                        RelativePosition = true,
                        Position = new Vec3f(),
                        DisposeOnFinish = true,
                        Volume = 0.5f,
                        Range = 8
                    });

                    sound.Start();
                }
            }

       
            tf.Scale = 1 + Math.Min(0.6f, 2 * secondsUsed);


            byEntity.Controls.UsingHeldItemTransformBefore = tf;

            return secondsUsed <= 1f;
        }

        // Let the client decide when he is done rafting
        return true;
    }


    public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
    {
        if (cancelReason == EnumItemUseCancelReason.ReleasedMouse)
        {
            return false;
        }

        if (api.Side == EnumAppSide.Client)
        {
            if (sound != null)
                sound.Stop();
            sound = null;
        }
        return true;
    }


    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {
        if (sound != null)
            sound.Stop();
        sound = null;

        if (secondsUsed >= 3.4f)
        {
            //string code = GetBlockMaterialCode(slot.Itemstack);

            if (api.Side == EnumAppSide.Server)
            {
                //CreateDrop(byEntity, code);
            }

            //RemoveMaterial(slot);
            //slot.MarkDirty();
            //(byEntity as EntityPlayer)?.Player?.InventoryManager.BroadcastHotbarSlot();

            //byEntity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(4f);
        }
    }
}