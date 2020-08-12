using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;

public class BBCollectFish : BlockBehavior
{
    public BBCollectFish(Block block) : base(block)
    { }
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        BlockPos pos = blockSel.Position;
        Block block = world.BlockAccessor.GetBlock(pos);
        string ttype = block.FirstCodePart();
        string contents = block.FirstCodePart(1);
        if (ttype == "trotline" || ttype == "fishbasket")
        {
            if (contents == "1fish" || contents == "2fish")
            {
                int fishCount = 1;
                if (contents == "2fish")
                { fishCount = 2; }
                string toreplace = fishCount.ToString() + "fish";
                string newPath = block.Code.Path;
                if (ttype == "trotline")
                { newPath = newPath.Replace(toreplace, "hook"); }
                else
                { newPath = newPath.Replace(toreplace, "empty"); }
                block = world.GetBlock(block.CodeWithPath(newPath));
                world.BlockAccessor.SetBlock(block.BlockId, pos);

                string fishCaught = "primitivesurvival:psfish-bass-raw";
                Random rnd = new Random();
                int fishType = rnd.Next(2);
                if (fishType == 0)
                { fishCaught = fishCaught.Replace("bass", "trout"); }
                ItemStack drop = new ItemStack(world.GetItem(new AssetLocation(fishCaught)), fishCount);
                world.SpawnItemEntity(drop, new Vec3d(byPlayer.WorldData.EntityPlayer.Pos.X, byPlayer.WorldData.EntityPlayer.Pos.Y + 1, byPlayer.WorldData.EntityPlayer.Pos.Z), null);
                handling = EnumHandling.PreventDefault;
            }
        }
        return true;
    }
}