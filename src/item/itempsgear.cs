using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace primitiveSurvival
{
    public class ItemPSGear : Item
    {
        public SimpleParticleProperties particlesHeld;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            particlesHeld = new SimpleParticleProperties(
                1, 1,
                ColorUtil.ToRgba(50, 220, 220, 220),
                new Vec3d(),
                new Vec3d(),
                new Vec3f(-0.1f, -0.1f, -0.1f),
                new Vec3f(0.1f, 0.1f, 0.1f),
                1.5f,
                0,
                0.5f,
                0.75f,
                EnumParticleModel.Cube
            );

            particlesHeld.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -0.6f);
            particlesHeld.AddPos.Set(0.1f, 0.1f, 0.1f);
            particlesHeld.addLifeLength = 0.5f;
            particlesHeld.RandomVelocityChange = true;
        }

        public override void InGuiIdle(IWorldAccessor world, ItemStack stack)
        {
            GuiTransform.Rotation.Y = GameMath.Mod(world.ElapsedMilliseconds / 50f, 360);
        }

        public override void OnGroundIdle(EntityItem entityItem)
        {
            GroundTransform.Rotation.Y = -GameMath.Mod(entityItem.World.ElapsedMilliseconds / 50f, 360);

            if (entityItem.World is IClientWorldAccessor)
            {
                particlesHeld.MinQuantity = 1;

                Vec3d pos = entityItem.SidedPos.XYZ;

                SpawnParticles(entityItem.World, pos, false);
            }
        }

        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            if (byEntity.World is IClientWorldAccessor)
            {

                FpHandTransform.Rotation.Y = GameMath.Mod(-byEntity.World.ElapsedMilliseconds / 50f, 360);
                TpHandTransform.Rotation.Y = GameMath.Mod(-byEntity.World.ElapsedMilliseconds / 50f, 360);
            }

        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.PreventDefaultAction;
        }


        void SpawnParticles(IWorldAccessor world, Vec3d pos, bool final)
        {
            if (final || world.Rand.NextDouble() > 0.8)
            {
                int h = 110 + world.Rand.Next(15);
                int v = 100 + world.Rand.Next(50);
                particlesHeld.MinPos = pos;
                particlesHeld.Color = ColorUtil.ReverseColorBytes(ColorUtil.HsvToRgba(h, 100, v));

                particlesHeld.MinSize = 0.2f;
                particlesHeld.ParticleModel = EnumParticleModel.Quad;
                particlesHeld.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -150);
                particlesHeld.Color = ColorUtil.ReverseColorBytes(ColorUtil.HsvToRgba(h, 100, v, 150));

                world.SpawnParticles(particlesHeld);
            }
        }
    }
}
