using Vintagestory.API.Common;
using System.Diagnostics;

namespace primitiveSurvival
{

    //This isn't going to work here.
    //Move the whole thing into blockblood.cs

    public class FiniteLiquid : BlockBehavior
    {
        public FiniteLiquid(Block block) : base(block)
        { }

        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            System.Diagnostics.Debug.WriteLine("CANCEL FINITE?");
            handling = EnumHandling.PreventDefault;
            string newPath = "game:blood-still-3";
            Block newblock = world.GetBlock(new AssetLocation(newPath));
            world.BlockAccessor.SetBlock(newblock.BlockId, blockSel.Position); //replace liquid with less
            world.BlockAccessor.MarkBlockDirty(blockSel.Position);
            return false;
            //return base.OnBlockInteractCancel(secondsUsed, world, byPlayer, blockSel, ref handling);
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            System.Diagnostics.Debug.WriteLine("STOP FINITE?");
            /*handling = EnumHandling.PreventDefault;
            string newPath = "game:blood-still-3";
            Block newblock = byEntity.Api.World.GetBlock(new AssetLocation(newPath));
            byEntity.Api.World.BlockAccessor.SetBlock(newblock.BlockId, blockSel.Position); //replace liquid with less
            byEntity.Api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);
            */
            //base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
        }




        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            /*
            Debug.WriteLine("START FINITE?");
            string newPath = "game:blood-still-3";
            Block newblock = world.GetBlock(new AssetLocation(newPath));
            world.BlockAccessor.SetBlock(newblock.BlockId, blockSel.Position); //replace liquid with less
            */
            //this dont help
            //base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            string liquidType = block.FirstCodePart();
            if (block.Code.Path.Contains("-7")) //lots of liquid?
            {
                handling = EnumHandling.PreventDefault;
                //string newPath = "game:" + liquidType + "-still-3";
                //Block newblock = world.GetBlock(new AssetLocation(newPath));
                //world.BlockAccessor.SetBlock(newblock.BlockId, blockSel.Position); //replace liquid with less
                //this seems optional
                //world.BlockAccessor.MarkBlockDirty(blockSel.Position);
            }
            return true; //makes the remaining liquid disappear
            //return false; //actually fills the bucket
        }
    }
}
