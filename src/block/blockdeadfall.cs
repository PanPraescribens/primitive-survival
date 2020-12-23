using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;
using Vintagestory.API.Common.Entities;

public class BlockDeadfall : Block
{

    public AssetLocation tickSound = new AssetLocation("game", "tick");
    public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
    {
        if (isImpact)
        {
            Block block = api.World.BlockAccessor.GetBlock(pos);
            string blockPath = block.Code.Path;
            string state = block.FirstCodePart(1);
            double maxanimalheight = Attributes["maxAnimalHeight"].AsDouble();
            int maxdamage = Attributes["maxDamageBaited"].AsInt();
            if (state == "set")
            { maxdamage = Attributes["maxDamageSet"].AsInt(); }
            if (state != "tripped")
            {
                int dmg = 3;
                //System.Diagnostics.Debug.WriteLine("Eye height " + entity.Properties.EyeHeight);

                if (entity.Properties.EyeHeight < maxanimalheight)
                {
                    Random rnd = new Random();
                    dmg = rnd.Next(5, maxdamage);
                }

                entity.ReceiveDamage(new DamageSource { SourceEntity = null, Type = EnumDamageType.BluntAttack }, dmg);
                BEDeadfall bedc = world.BlockAccessor.GetBlockEntity(pos) as BEDeadfall;
                if (bedc != null) bedc.tripTrap(pos);
                world.PlaySoundAt(tickSound, entity.Pos.X, entity.Pos.Y, entity.Pos.Z);
            }
        }
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


    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (byPlayer.Entity.Controls.Sneak)
        {
            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            string path = block.Code.Path;
            if (path.Contains("-tripped"))
            {
                path = path.Replace("-tripped", "-set");
                block = world.GetBlock(block.CodeWithPath(path));
                world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
            }
            return true;
        }

        BEDeadfall bedc = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEDeadfall;
        if (bedc != null) return bedc.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }


    public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture, int slot, bool tripped, ITesselatorAPI tesselator = null)
    {
        Shape shape = null;
        tesselator = capi.Tesselator;
        shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
        MeshData mesh;
        tesselator.TesselateShape(shapePath, shape, out mesh, texture, new Vec3f(0, 0, 0));
        if (slot == 0) //bait
        {
            if (tripped)
                mesh.Translate(-0.1f, 0f, -0.3f);
            else
                mesh.Translate(-0.1f, 0f, -0.2f);
        }
        mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, Shape.rotateY * GameMath.DEG2RAD, 0); //orient based on direction last
        return mesh;
    }







}
