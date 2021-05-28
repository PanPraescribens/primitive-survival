using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;


namespace primitiveSurvival
{
    public class BlockTemporallectern : Block
    {
        public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture, int index, ITesselatorAPI tesselator = null)
        {
            Shape shape = null;
            tesselator = capi.Tesselator;
            shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
            MeshData mesh;
            tesselator.TesselateShape(shapePath, shape, out mesh, texture, new Vec3f(0f, 0, 0f));

            if (shapePath.Contains("necronomicon"))
            {
                mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), -22.5f * GameMath.DEG2RAD, 180 * GameMath.DEG2RAD, 0); //tilt
                mesh.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.56f, 0.56f, 0.56f); //shrink
                mesh.Translate(0f, 0.55f, 0f);
            }
            else if (shapePath.Contains("gear-"))
            {
                mesh.Translate(0.15f, 1.3f, 0.06f); //center above
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, 22.5f * GameMath.DEG2RAD, 0); //twist
                mesh.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.48f, 0.48f, 0.48f); //shrink
                mesh.Rotate(new Vec3f(0f, 0.1f, 0f), 0, 0, 270 * GameMath.DEG2RAD); //flip on side
                mesh.Rotate(new Vec3f(0.5f, 1, 0.5f), 0, 90 * GameMath.DEG2RAD, 0);  //orient for pre-facing orient
                if (shapePath.Contains("rusty"))
                    mesh.Translate(0, 0.82f, 0); //up
                else //temporal
                    mesh.Translate(0, 0.90f, 0); //up

                mesh.Translate(0, 0, 0.09f); //in 

            }
            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, Shape.rotateY * GameMath.DEG2RAD, 0); //orient based on direction last
            return mesh;
        }

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

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            BETemporallectern be = world.BlockAccessor.GetBlockEntity(pos) as BETemporallectern;
            if (be != null) be.OnBreak(byPlayer, pos); //empty the inventory onto the ground
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BETemporallectern be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BETemporallectern;
            if (be != null)
            { return be.OnInteract(byPlayer, blockSel); }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}