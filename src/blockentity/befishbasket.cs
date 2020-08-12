using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;

public class BEFishBasket : BlockEntity
{
    protected int timer;
    protected int escapePercent = 30; //30
    protected int catchPercent = 1; //1
    protected int baitedCatchPercent = 3;
    protected static Random rnd = new Random();

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        RegisterGameTickListener(FishBasketUpdate, 6000);
    }

    private void FishBasketUpdate(float par)
    {
        timer++;
        if (timer > 20) //20
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            string blockPath = block.Code.Path;
            string state = blockPath.Split('-')[1];
            string savedstate = state;
            BlockPos nearbyFront = new BlockPos(Pos.X, Pos.Y, Pos.Z + 1);
            BlockPos nearbyBack = new BlockPos(Pos.X, Pos.Y, Pos.Z - 1);
            Block nearbyFrontBlock = Api.World.BlockAccessor.GetBlock(nearbyFront);
            Block nearbyBackBlock = Api.World.BlockAccessor.GetBlock(nearbyBack);
            if ((nearbyFrontBlock.LiquidCode == "water") && (nearbyBackBlock.LiquidCode == "water") && (!nearbyFrontBlock.Code.Path.Contains("fishbasket")))
            {
                int caught;
                int escaped;
                bool isCaught = false;
                caught = rnd.Next(100);
                if ((state == "empty") && (caught < catchPercent))
                { isCaught = true; }
                if ((state == "baited") && (caught < baitedCatchPercent))
                { isCaught = true; }

                if (isCaught)
                {
                    if (state == "baited")
                    {
                        int action = rnd.Next(2);
                        if (action == 0)
                        { state = "empty"; } //a 50% chance that bait is simply stolen
                        else
                        { state = "1fish"; }

                    }
                    else //empty
                    { state = "1fish"; }
                }
                else if (state == "2fish")
                {
                    escaped = rnd.Next(100);
                    if (escaped < escapePercent)
                    { state = "1fish"; }
                }
                else // 1fish
                {
                    caught = rnd.Next(100);
                    if (caught < catchPercent)
                    { state = "2fish"; }
                    else
                    {
                        escaped = rnd.Next(100);
                        if (escaped < escapePercent)
                        { state = "empty"; }
                    }
                }
                if (state != savedstate)
                {
                    block = Api.World.GetBlock(block.CodeWithParts(state));
                    Api.World.BlockAccessor.SetBlock(block.BlockId, Pos);
                }
            }
        }
    }
}
