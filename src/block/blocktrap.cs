using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common.Entities;

public class BlockTrap : Block
{
    public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
    {
        if (isImpact)
        {
            Block block = api.World.BlockAccessor.GetBlock(pos);
            string blockPath = block.Code.Path;
            string state = block.FirstCodePart(1);
            double maxAnimalHeight = Attributes["maxAnimalHeight"].AsDouble();
            int maxDamage = Attributes["maxDamageBaited"].AsInt();
            if (state == "set")
            { maxDamage = Attributes["maxDamageSet"].AsInt(); }

            if (state != "tripped")
            {
                int dmg = 1;
                if (entity.Properties.EyeHeight < maxAnimalHeight)
                {
                    Random rnd = new Random();
                    dmg = rnd.Next(3, maxDamage);
                }
                entity.ReceiveDamage(new DamageSource { SourceEntity = null, Type = EnumDamageType.BluntAttack }, dmg);
                blockPath = blockPath.Replace(state, "tripped");
                block = api.World.GetBlock(block.CodeWithPath(blockPath));
                entity.Pos.Motion.Y *= -0.3;
                api.World.BlockAccessor.SetBlock(block.BlockId, pos);
            }
        }
    }
}
