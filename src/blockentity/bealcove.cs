using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Config;

namespace primitiveSurvival
{
    public class BEAlcove : BlockEntity
    {
        public int tickSeconds = 2;
        public static Random rnd = new Random();


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side.IsServer())
                RegisterGameTickListener(ParticleUpdate, tickSeconds * 1000);
        }

        private void GenerateSmokeParticles(BlockPos pos, IWorldAccessor world)
        {
            float minQuantity = 0;
            float maxQuantity = 3;
            int color = ColorUtil.ToRgba(40, 15, 15, 15);
            Vec3d minPos = new Vec3d();
            Vec3d addPos = new Vec3d();
            Vec3f minVelocity = new Vec3f(0.2f, 0.0f, 0.2f);
            Vec3f maxVelocity = new Vec3f(0.6f, 0.4f, 0.6f);
            float lifeLength = 1f;
            float gravityEffect = -0.05f;
            float minSize = 0.1f;
            float maxSize = 0.5f;

            SimpleParticleProperties smokeParticles = new SimpleParticleProperties(
                minQuantity, maxQuantity,
                color,
                minPos, addPos,
                minVelocity, maxVelocity,
                lifeLength,
                gravityEffect,
                minSize, maxSize,
                EnumParticleModel.Quad
            );
            smokeParticles.MinPos.Set(pos.ToVec3d().AddCopy(0.5, 0.5, 0.5));
            smokeParticles.AddPos.Set(new Vec3d(0.1, 0, 0));
            smokeParticles.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARINCREASE, 0.5f);
            smokeParticles.ShouldDieInAir = false;
            smokeParticles.SelfPropelled = true;
            world.SpawnParticles(smokeParticles);
        }

        public void ParticleUpdate(float par)
        {
            if (Block.Code.Path.Contains("-lit"))
                GenerateSmokeParticles(Pos, Api.World);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            if (block.Code.Path.Contains("-unlit"))
            {
                sb.Append(Lang.Get("Right click with a torch or a candle to light the candle."));
                sb.AppendLine().AppendLine();
            }
            else if (block.Code.Path.Contains("-lit"))
            {
                sb.Append(Lang.Get("Right click with nothing in your hand to extinguish the candle."));
                sb.AppendLine().AppendLine();
            }
        }
    }
}