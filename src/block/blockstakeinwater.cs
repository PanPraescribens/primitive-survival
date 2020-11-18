using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System.Collections.Generic;
using Vintagestory.GameContent;

public class BlockStakeInWater : BlockWaterPlant
{

    public string GetOrientations(IWorldAccessor world, BlockPos pos)
    {
        string orientations =
            GetFenceCode(world, pos, BlockFacing.NORTH) +
            GetFenceCode(world, pos, BlockFacing.EAST) +
            GetFenceCode(world, pos, BlockFacing.SOUTH) +
            GetFenceCode(world, pos, BlockFacing.WEST);
        if (orientations.Length == 0) orientations = "empty";
        return orientations;
    }

    private string GetFenceCode(IWorldAccessor world, BlockPos pos, BlockFacing facing)
    {
        if (ShouldConnectAt(world, pos, facing)) return "" + facing.Code[0];
        return "";
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        Block block = world.BlockAccessor.GetBlock(pos);
        if (block.Code.Path.Contains("stakeinwater") && block.Code.Path.Contains("open"))
        {
            //scan around this neighbor for a weirtrap and break it
            BlockPos[] weirSidesPos = new BlockPos[] { pos.EastCopy(), pos.WestCopy(), pos.NorthCopy(), pos.SouthCopy() };
            Block testBlock;
            foreach (BlockPos neighbor in weirSidesPos)
            {
                testBlock = world.BlockAccessor.GetBlock(neighbor);
                if (testBlock.Code.Path.Contains("weirtrap"))
                {
                    world.BlockAccessor.BreakBlock(neighbor, null);
                }
            }
        }
        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("water-still-7")).BlockId, pos);
        world.BlockAccessor.GetBlock(pos).OnNeighbourBlockChange(world, pos, pos);
    }

    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
    {
        Block neibBlock = world.BlockAccessor.GetBlock(neibpos);
        Block block = world.BlockAccessor.GetBlock(pos);

        if (block.Code.Path.Contains("stakeinwater") && block.Code.Path.Contains("open"))
        {
            if (neibBlock.Code.Path.Contains("stakeinwater"))
                return;
            else
            {
                //scan around this neighbor for a weirtrap and break it
                BlockPos[] weirSidesPos = new BlockPos[] { pos.EastCopy(), pos.WestCopy(), pos.NorthCopy(), pos.SouthCopy() };
                Block testBlock;
                foreach (BlockPos neighbor in weirSidesPos)
                {
                    testBlock = world.BlockAccessor.GetBlock(neighbor);
                    if (testBlock.Code.Path.Contains("weirtrap"))
                    {
                        world.BlockAccessor.BreakBlock(neighbor, null);
                    }
                }
            }
        }

        //if (neibBlock.Code.Path.Contains("weirtrap")) //not sure about this...
        //    return;

        string orientations = GetOrientations(world, pos);
        AssetLocation newBlockCode = CodeWithVariant("type", orientations);
        if (!Code.Equals(newBlockCode))
        {
            block = world.BlockAccessor.GetBlock(newBlockCode);
            if (block == null) return;
            world.BlockAccessor.SetBlock(block.BlockId, pos);
            world.BlockAccessor.TriggerNeighbourBlockUpdate(pos);
        }
        //base.OnNeighbourBlockChange(world, pos, neibpos);
    }

    public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
    {
        return new BlockDropItemStack[] { new BlockDropItemStack(handbookStack) };
    }


    public bool ShouldConnectAt(IWorldAccessor world, BlockPos ownPos, BlockFacing side)
    {
        Block block = world.BlockAccessor.GetBlock(ownPos.AddCopy(side));
        return block.FirstCodePart() == FirstCodePart() || block.SideSolid[side.GetOpposite().Index];
    }

    static string[] OneDir = new string[] { "n", "e", "s", "w" };
    static string[] TwoDir = new string[] { "ns", "ew" };
    static string[] AngledDir = new string[] { "ne", "es", "sw", "nw" };
    static string[] ThreeDir = new string[] { "nes", "new", "nsw", "esw" };

    static string[] GateLeft = new string[] { "egw", "ngs" };
    static string[] GateRight = new string[] { "gew", "gns" };

    static Dictionary<string, KeyValuePair<string[], int>> AngleGroups = new Dictionary<string, KeyValuePair<string[], int>>();

    static BlockStakeInWater()
    {
        AngleGroups["n"] = new KeyValuePair<string[], int>(OneDir, 0);
        AngleGroups["e"] = new KeyValuePair<string[], int>(OneDir, 1);
        AngleGroups["s"] = new KeyValuePair<string[], int>(OneDir, 2);
        AngleGroups["w"] = new KeyValuePair<string[], int>(OneDir, 3);

        AngleGroups["ns"] = new KeyValuePair<string[], int>(TwoDir, 0);
        AngleGroups["ew"] = new KeyValuePair<string[], int>(TwoDir, 1);

        AngleGroups["ne"] = new KeyValuePair<string[], int>(AngledDir, 0);
        AngleGroups["nw"] = new KeyValuePair<string[], int>(AngledDir, 1);
        AngleGroups["es"] = new KeyValuePair<string[], int>(AngledDir, 2);
        AngleGroups["sw"] = new KeyValuePair<string[], int>(AngledDir, 3);

        AngleGroups["nes"] = new KeyValuePair<string[], int>(ThreeDir, 0);
        AngleGroups["new"] = new KeyValuePair<string[], int>(ThreeDir, 1);
        AngleGroups["nsw"] = new KeyValuePair<string[], int>(ThreeDir, 2);
        AngleGroups["esw"] = new KeyValuePair<string[], int>(ThreeDir, 3);


        AngleGroups["egw"] = new KeyValuePair<string[], int>(GateLeft, 0);
        AngleGroups["ngs"] = new KeyValuePair<string[], int>(GateLeft, 1);

        AngleGroups["gew"] = new KeyValuePair<string[], int>(GateRight, 0);
        AngleGroups["gns"] = new KeyValuePair<string[], int>(GateRight, 1);
    }

    public override AssetLocation GetRotatedBlockCode(int angle)
    {
        string type = Variant["type"];
        if (type == "empty" || type == "nesw") return Code;
        int angleIndex = angle / 90;
        var val = AngleGroups[type];
        string newFacing = val.Key[(angleIndex + val.Value) % val.Key.Length];
        return CodeWithVariant("type", newFacing);
    }


    public AssetLocation tickSound = new AssetLocation("game", "tick");

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        Block waterBlock;
        BlockPos waterPos;
        Block testBlock;
        BlockPos[] weirSidesPos;
        BlockPos[] weirBasesPos;
        if (byPlayer.Entity.Controls.Sneak)
        {
            BlockPos Pos = blockSel.Position;
            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            string path = block.Code.Path;
            BlockFacing facing = byPlayer.CurrentBlockSelection.Face;
            if (facing.IsHorizontal && (path.EndsWith("-ew") || path.EndsWith("-ns")))
            {
                if (facing.ToString() == "north")
                {
                    path = path.Replace("-ew", "-we");
                    waterPos = Pos.NorthCopy();
                    weirSidesPos = new BlockPos[] { waterPos.EastCopy(), waterPos.WestCopy() };
                    weirBasesPos = new BlockPos[] { waterPos.NorthCopy(), waterPos.NorthCopy().EastCopy(), waterPos.NorthCopy().WestCopy() };

                }
                else if (facing.ToString() == "west")
                {
                    path = path.Replace("-ns", "-sn");
                    waterPos = Pos.WestCopy();
                    weirSidesPos = new BlockPos[] { waterPos.NorthCopy(), waterPos.SouthCopy() };
                    weirBasesPos = new BlockPos[] { waterPos.WestCopy(), waterPos.WestCopy().NorthCopy(), waterPos.WestCopy().SouthCopy() };
                }
                else if (facing.ToString() == "south")
                {
                    waterPos = Pos.SouthCopy();
                    weirSidesPos = new BlockPos[] { waterPos.EastCopy(), waterPos.WestCopy() };
                    weirBasesPos = new BlockPos[] { waterPos.SouthCopy(), waterPos.SouthCopy().EastCopy(), waterPos.SouthCopy().WestCopy() };
                }
                else //east
                {
                    waterPos = Pos.EastCopy();
                    weirSidesPos = new BlockPos[] { waterPos.NorthCopy(), waterPos.SouthCopy() };
                    weirBasesPos = new BlockPos[] { waterPos.EastCopy(), waterPos.EastCopy().NorthCopy(), waterPos.EastCopy().SouthCopy() };
                }
                waterBlock = world.BlockAccessor.GetBlock(waterPos);
                bool areaOK = true;

                // Examine sides 
                foreach (BlockPos neighbor in weirSidesPos)
                {
                    testBlock = world.BlockAccessor.GetBlock(neighbor);
                    if (testBlock.FirstCodePart() != "stakeinwater")
                    { areaOK = false; }
                }

                // Examine bases
                foreach (BlockPos neighbor in weirBasesPos)
                {
                    testBlock = world.BlockAccessor.GetBlock(neighbor);
                    // This might need more work
                    if (testBlock.BlockId == 0 || (testBlock.LiquidCode == "water" && (testBlock.FirstCodePart() != "stakeinwater")))
                    { areaOK = false; }
                }

                if (waterBlock.LiquidCode == "water" && areaOK)
                {
                    path = path + "open";
                    block = world.GetBlock(block.CodeWithPath(path));
                    world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);

                    testBlock = world.BlockAccessor.GetBlock(new AssetLocation("primitivesurvival:weirtrap-" + facing.ToString()));
                    world.BlockAccessor.SetBlock(testBlock.BlockId, waterPos);

                }
            }
        }
        return true;
    }
}
