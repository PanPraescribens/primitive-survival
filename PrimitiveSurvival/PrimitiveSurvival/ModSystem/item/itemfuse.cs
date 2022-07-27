namespace PrimitiveSurvival.ModSystem
{
    //using System;
    using Vintagestory.API.Common;
    //using Vintagestory.API.MathTools;
    //using System.Diagnostics;

    public class ItemFuse : Item
    {

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.PreventDefaultAction;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel == null || byEntity == null)
            { return; }
            var world = byEntity.World;
            if (world == null)
            { return; }
            var block = world.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Default);
            var face = blockSel.Face.ToString();

            //this will need to change if I add more fuse types!
            var blockNew = world.GetBlock(new AssetLocation("primitivesurvival:bfuse-blackmatch-empty"));

            if (face == "down")
            {
                var blockSelBelow = blockSel.Clone();
                blockSelBelow.Position.Y -= 1;
                var blockAbove = world.BlockAccessor.GetBlock(blockSelBelow.Position, BlockLayersAccess.Default);
                if (blockAbove.BlockId == 0 || blockAbove.Code.Path.Contains("tallgrass-") || block.Code.Path.Contains("tallgrass-"))
                {
                    var blockAccessor = world.BlockAccessor;
                    if (block.Code.Path.Contains("tallgrass-"))
                    {
                        blockAccessor.SetBlock(blockNew.BlockId, blockSel.Position);
                        blockAccessor.TriggerNeighbourBlockUpdate(blockSel.Position);
                    }
                    else
                    {
                        blockAccessor.SetBlock(blockNew.BlockId, blockSelBelow.Position);
                        blockAccessor.TriggerNeighbourBlockUpdate(blockSelBelow.Position);
                    }
                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
            }
            else if (face == "up")
            {
                if (block.FirstCodePart() == "bfuse") //prevent fuse on top for now
                { return; }
                var blockSelAbove = blockSel.Clone();
                blockSelAbove.Position.Y += 1;
                var blockAbove = world.BlockAccessor.GetBlock(blockSelAbove.Position, BlockLayersAccess.Default);
                if (blockAbove.BlockId == 0 || blockAbove.Code.Path.Contains("tallgrass-") || block.Code.Path.Contains("tallgrass-"))
                {
                    var blockAccessor = world.BlockAccessor;
                    var posFinal = blockSelAbove.Position;
                    if (block.Code.Path.Contains("tallgrass-"))
                    { posFinal = blockSel.Position; }
                    blockAccessor.SetBlock(blockNew.BlockId, posFinal);
                    blockAccessor.TriggerNeighbourBlockUpdate(posFinal);

                    //update fuse next to firework
                    var neib = blockSelAbove.Clone();
                    neib.Position.Z += 1;
                    var testBlock = world.BlockAccessor.GetBlock(neib.Position, BlockLayersAccess.Default);
                    blockAccessor.TriggerNeighbourBlockUpdate(neib.Position);

                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
            }
            else //wall
            {
                var blockSelBeside = blockSel.Clone();
                if (face == "east")
                { blockSelBeside.Position.X += 1; }
                else if (face == "west")
                { blockSelBeside.Position.X -= 1; }
                else if (face == "north")
                { blockSelBeside.Position.Z -= 1; }
                else
                { blockSelBeside.Position.Z += 1; }

                var blockBeside = world.BlockAccessor.GetBlock(blockSelBeside.Position, BlockLayersAccess.Default);
                if (blockBeside.BlockId == 0 || blockBeside.Code.Path.Contains("tallgrass-") || block.Code.Path.Contains("tallgrass-"))
                {
                    var blockAccessor = world.BlockAccessor;
                    var posFinal = blockSelBeside.Position;
                    if (block.Code.Path.Contains("tallgrass-"))
                    { posFinal = blockSel.Position; }
                    blockAccessor.SetBlock(blockNew.BlockId, posFinal);
                    blockAccessor.TriggerNeighbourBlockUpdate(posFinal);

                    //update fuse next to firework
                    var neib = blockSelBeside.Clone();
                    neib.Position.Z += 1;
                    var testBlock = world.BlockAccessor.GetBlock(neib.Position, BlockLayersAccess.Default);
                    blockAccessor.TriggerNeighbourBlockUpdate(neib.Position);

                    slot.TakeOut(1);
                    slot.MarkDirty();
                }
            }

        }
    }
}
