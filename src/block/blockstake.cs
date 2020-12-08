using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System.Collections.Generic;
using Vintagestory.GameContent;

public class BlockStake : Block
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

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
    {
        bool placed;
        bool inwater;
        BlockPos pos = blockSel.Position;
        Block block = world.BlockAccessor.GetBlock(pos);
        if (block.Code.Path.Contains("stakeinwater-") || block.Code.Path.Contains("fishbasket-"))
        { return false; }
        inwater = (block.LiquidCode == "water");
        BlockSelection blockSelBelow = blockSel.Clone();
        blockSelBelow.Position.Y -= 1;
        Block blockBelow = world.BlockAccessor.GetBlock(blockSelBelow.Position);
        if (blockBelow.Fertility <= 0)
        {
            failureCode = "you need more suitable ground to place this stake";
            return false;
        }
        string orientations = GetOrientations(world, pos);
        block = world.BlockAccessor.GetBlock(CodeWithVariant("type", orientations));
        if (block == null) block = this;
        placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        if (placed)
        {
            if (inwater)
            {
                string newPath = block.Code.Path.Replace("stake", "stakeinwater");
                block = api.World.GetBlock(block.CodeWithPath(newPath));
                api.World.BlockAccessor.SetBlock(block.BlockId, pos);
            }
            return true;
        }
        return false;
    }

    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
    {
        string orientations = GetOrientations(world, pos);
        AssetLocation newBlockCode = CodeWithVariant("type", orientations);
        if (!Code.Equals(newBlockCode))
        {
            Block block = world.BlockAccessor.GetBlock(newBlockCode);
            if (block == null) return;
            world.BlockAccessor.SetBlock(block.BlockId, pos);
            world.BlockAccessor.TriggerNeighbourBlockUpdate(pos);
        }
        base.OnNeighbourBlockChange(world, pos, neibpos);
    }

    public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
    {
        return new BlockDropItemStack[] { new BlockDropItemStack(handbookStack) };
    }


    public bool ShouldConnectAt(IWorldAccessor world, BlockPos ownPos, BlockFacing side)
    {
        Block block = world.BlockAccessor.GetBlock(ownPos.AddCopy(side));
        return block.FirstCodePart() == FirstCodePart() || block.SideSolid[side.Opposite.Index];
    }

    static string[] OneDir = new string[] { "n", "e", "s", "w" };
    static string[] TwoDir = new string[] { "ns", "ew" };
    static string[] AngledDir = new string[] { "ne", "es", "sw", "nw" };
    static string[] ThreeDir = new string[] { "nes", "new", "nsw", "esw" };

    static string[] GateLeft = new string[] { "egw", "ngs" };
    static string[] GateRight = new string[] { "gew", "gns" };

    static Dictionary<string, KeyValuePair<string[], int>> AngleGroups = new Dictionary<string, KeyValuePair<string[], int>>();

    static BlockStake()
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
}
