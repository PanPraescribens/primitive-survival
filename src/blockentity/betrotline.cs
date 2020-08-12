using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;

public class BETrotline : BlockEntity
{
    protected int timer;
    protected int catchPercent = 1; //1
    protected int baitedCatchPercent = 2; //but a 50% chance that bait is simply stolen
    protected static Random rnd = new Random();

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        RegisterGameTickListener(TrotlineUpdate, 6000);
    }

    private void TrotlineUpdate(float par)
    {
        timer++;
        if (timer > 25) // 25
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string blockPath = block.Code.Path;
            string savedPath = blockPath;
            if (blockPath.Contains("hook"))
            {
                BlockPos belowblock = new BlockPos(Pos.X, Pos.Y - 1, Pos.Z);
                Block belowBlock = Api.World.BlockAccessor.GetBlock(belowblock);
                if ((belowBlock.LiquidCode == "water") && (!belowBlock.Code.Path.Contains("fishbasket")))
                {
                    int caught = rnd.Next(100);
                    if (blockPath.Contains("hookbaited"))
                    {
                        if (caught < baitedCatchPercent)
                        {
                            int action = rnd.Next(2);
                            if (action == 0)
                            { blockPath = blockPath.Replace("hookbaited", "hook"); }
                            else
                            { blockPath = blockPath.Replace("hookbaited", "1fish"); }
                        }
                    }
                    else //hook
                    {
                        if (caught < catchPercent)
                        { blockPath = blockPath.Replace("hook", "1fish"); }
                    }
                    if (blockPath != savedPath)
                    {
                        block = Api.World.GetBlock(block.CodeWithPath(blockPath));
                        Api.World.BlockAccessor.SetBlock(block.BlockId, Pos);
                    }
                }
            }
        }
    }
}
