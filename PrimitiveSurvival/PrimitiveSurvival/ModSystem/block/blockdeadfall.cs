namespace PrimitiveSurvival.ModSystem
{
    using System;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Common.Entities;
    using PrimitiveSurvival.ModConfig;

    public class BlockDeadfall : Block
    {

        private readonly AssetLocation tickSound = new AssetLocation("game", "tick");
        public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
        {
            if (isImpact)
            {
                var block = this.api.World.BlockAccessor.GetBlock(pos);
                var state = block.FirstCodePart(1);
                double maxanimalheight = ModConfig.Loaded.DeadfallMaxAnimalHeight;
                var maxdamage = ModConfig.Loaded.DeadfallMaxDamageBaited;
                if (state == "set")
                { maxdamage = ModConfig.Loaded.DeadfallMaxDamageSet; }
                if (state != "tripped")
                {
                    var dmg = 3;
                    if (entity.Properties.EyeHeight < maxanimalheight)
                    {
                        var rnd = new Random();
                        dmg = rnd.Next(5, maxdamage);
                    }

                    entity.ReceiveDamage(new DamageSource { SourceEntity = null, Type = EnumDamageType.BluntAttack }, dmg);
                    if (world.BlockAccessor.GetBlockEntity(pos) is BEDeadfall bedc)
                    { bedc.TripTrap(pos); }
                    world.PlaySoundAt(this.tickSound, entity.Pos.X, entity.Pos.Y, entity.Pos.Z);
                }
            }
        }


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


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var block = world.BlockAccessor.GetBlock(blockSel.Position);
            var path = block.Code.Path;
            if (path.Contains("-tripped"))
            {
                path = path.Replace("-tripped", "-set");
                block = world.GetBlock(block.CodeWithPath(path));
                world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
                return true;
            }

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEDeadfall bedc)
            { return bedc.OnInteract(byPlayer, blockSel); }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }


        public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture, int slot, bool tripped, ITesselatorAPI tesselator = null)
        {
            tesselator = capi.Tesselator;
            var shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
            tesselator.TesselateShape(shapePath, shape, out var mesh, texture, new Vec3f(0, 0, 0));
            if (slot == 0) //bait
            {
                if (tripped)
                { mesh.Translate(-0.1f, 0f, -0.3f); }
                else
                { mesh.Translate(-0.1f, 0f, -0.2f); }
            }
            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, this.Shape.rotateY * GameMath.DEG2RAD, 0); //orient based on direction last
            return mesh;
        }
    }
}
