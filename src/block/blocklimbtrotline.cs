using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;
using System.Diagnostics;

namespace primitiveSurvival
{
    public class BlockLimbTrotLineLure : Block
    {
        public static Random rnd = new Random();

        public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture, bool alive, ITesselatorAPI tesselator = null)
        {
            Shape shape = null;
            tesselator = capi.Tesselator;
            shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
            MeshData mesh;
            tesselator.TesselateShape(shapePath, shape, out mesh, texture, new Vec3f(Shape.rotateX, Shape.rotateY, Shape.rotateZ));
            if (shapePath.Contains("catfish"))
            { mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.7f, 0.7f, 0.7f); }
            if (shapePath.Contains("lure"))
            {
                int rando = rnd.Next(10);
                if (rando == 0)
                    mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 0, 5 * GameMath.DEG2RAD);
                else if (rando == 1)
                    mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 0, 355 * GameMath.DEG2RAD);
            }

            if (alive) //let's animate these fishes
            {
                double flength = 0.7;
                if (shapePath.Contains("catfish"))
                { flength = 0.8; }
                else if (shapePath.Contains("bluegill"))
                { flength = 0.25; }

                int fishWave = VertexFlags.LeavesWindWaveBitMask | VertexFlags.WeakWaveBitMask;
                for (int vertexNum = 0; vertexNum < mesh.GetVerticesCount(); vertexNum++)
                {
                    //tail only
                    if (mesh.xyz[3 * vertexNum + 1] < -0.2 - flength) mesh.Flags[vertexNum] |= fishWave;
                    else mesh.Flags[vertexNum] |= 6144;
                }
            }
            else
            {
                for (int vertexNum = 0; vertexNum < mesh.GetVerticesCount(); vertexNum++)
                {
                    mesh.Flags[vertexNum] |= 6144;
                }
            }

            if ((Shape.rotateY == 0 || Shape.rotateY == 90) && !shapePath.Contains("-end") && !shapePath.Contains("-middle"))
                mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 180 * GameMath.DEG2RAD, 0);
            return mesh;
        }


        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            Block block = world.BlockAccessor.GetBlock(neibpos);
            if (block.BlockId <= 0) //block removed
            {
                block = world.BlockAccessor.GetBlock(neibpos.NorthCopy());
                if (block.FirstCodePart() == "limbtrotlinelure" && block.LastCodePart() != "east" && block.LastCodePart() != "west")
                    world.BlockAccessor.BreakBlock(neibpos.NorthCopy(), null);
                block = world.BlockAccessor.GetBlock(neibpos.SouthCopy());
                if (block.FirstCodePart() == "limbtrotlinelure" && block.LastCodePart() != "east" && block.LastCodePart() != "west")
                    world.BlockAccessor.BreakBlock(neibpos.SouthCopy(), null);
                block = world.BlockAccessor.GetBlock(neibpos.EastCopy());
                if (block.FirstCodePart() == "limbtrotlinelure" && block.LastCodePart() != "north" && block.LastCodePart() != "south")
                    world.BlockAccessor.BreakBlock(neibpos.EastCopy(), null);
                block = world.BlockAccessor.GetBlock(neibpos.WestCopy());
                if (block.FirstCodePart() == "limbtrotlinelure" && block.LastCodePart() != "north" && block.LastCodePart() != "south")
                    world.BlockAccessor.BreakBlock(neibpos.WestCopy(), null);
            }
        }


        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            BELimbTrotLineLure bedc = world.BlockAccessor.GetBlockEntity(pos) as BELimbTrotLineLure;
            if (bedc != null) bedc.OnBreak(byPlayer, pos); //empty the inventory onto the ground
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockSelection blockSrc = blockSel.Clone();
            blockSel = blockSel.Clone();
            bool placed;
            Block block = api.World.BlockAccessor.GetBlock(blockSel.Position);
            placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            string newPath = block.Code.Path;
            block = api.World.GetBlock(block.CodeWithPath(newPath));
            api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
            return false;
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BELimbTrotLineLure bedc = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELimbTrotLineLure;
            if (bedc != null)
            { return bedc.OnInteract(byPlayer, blockSel); }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}