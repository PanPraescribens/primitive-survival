namespace PrimitiveSurvival.ModSystem
{
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;

    public class BlockTemporalCube : Block
    {

        public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture, int index, ITesselatorAPI tesselator = null)
        {
            tesselator = capi.Tesselator;
            var shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
            tesselator.TesselateShape(shapePath, shape, out var mesh, texture, new Vec3f(0f, 0, 0f));

            if (shapePath.Contains("gear-"))
            {
                mesh.Translate(0.15f, 1.3f, 0.06f); //center above
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, 22.5f * GameMath.DEG2RAD, 0); //twist
                mesh.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.48f, 0.48f, 0.48f); //shrink
                mesh.Rotate(new Vec3f(0f, 0.1f, 0f), 0, 0, 270 * GameMath.DEG2RAD); //flip on side
                if (index == 0)
                { mesh.Rotate(new Vec3f(0.5f, 1, 0.5f), 0, 270 * GameMath.DEG2RAD, 0); }
                else if (index == 1)
                { mesh.Rotate(new Vec3f(0.5f, 1, 0.5f), 0, 180 * GameMath.DEG2RAD, 0); }
                else if (index == 2)
                { mesh.Rotate(new Vec3f(0.5f, 1, 0.5f), 0, 90 * GameMath.DEG2RAD, 0); }
                else if (index == 3)
                { mesh.Rotate(new Vec3f(0.5f, 1, 0.5f), 0, 0 * GameMath.DEG2RAD, 0); }

                if (shapePath.Contains("rusty"))
                { mesh.Translate(0, 0.66f, 0); } //up
                else //temporal
                { mesh.Translate(0, 0.75f, 0); }//up
            }
            else
            { mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, this.Shape.rotateY * GameMath.DEG2RAD, 0); } //orient based on direction last
            return mesh;
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


        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is BETemporalCube be)
            { be.OnBreak(byPlayer, pos); } //empty the inventory onto the ground
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BETemporalCube be)
            { return be.OnInteract(byPlayer, blockSel); }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
