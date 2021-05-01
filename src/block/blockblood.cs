using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;
using System.Diagnostics;

namespace primitiveSurvival
{
    public class BlockBlood : Block, VintagestoryAPI.Common.Collectible.Block.IBlockFlowing
    {
        public string Flow { get; set; }
        public Vec3i FlowNormali { get => null; set { } }
        public bool IsLava => true; //removes the "foam" but you also lose the water droplets
        // my own shader would be nice, for best of both worlds


        public BlockBlood() : base()
        { }

        long handlerId;
        long handlerId2;
        long handlerId3;
        BlockPos bloodPos;
        Block blockAtPos;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            //Flow = Variant["flow"] is string f ? string.Intern(f) : null; ;
            //FlowNormali = Flow != null ? Cardinal.FromInitial(Flow)?.Normali : null;
        }

        public override bool ShouldPlayAmbientSound(IWorldAccessor world, BlockPos pos)
        {
            // Play water wave sound when above is air and below is a solid block
            return
                world.BlockAccessor.GetBlock(pos.X, pos.Y + 1, pos.Z).Id == 0 &&
                world.BlockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z).SideSolid[BlockFacing.UP.Index];
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            //Debug.WriteLine("START?");
            if (world.Side == EnumAppSide.Client)
            {
                world.UnregisterCallback(handlerId);
                world.UnregisterCallback(handlerId2);
                world.UnregisterCallback(handlerId3);
                bloodPos = blockSel.Position;
                blockAtPos = api.World.BlockAccessor.GetBlock(bloodPos);
                handlerId = world.RegisterCallback(AfterBloodCollected, 800);
                handlerId2 = world.RegisterCallback(AfterBloodCollected2, 1100);
                handlerId3 = world.RegisterCallback(AfterBloodCollected3, 1400);
            }

            if (api.World.Side == EnumAppSide.Server)
            {
                //Debug.WriteLine("START?");
            }
            return false; // base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        private void AfterBloodCollected(float dt)
        {
            //Debug.WriteLine("Blood collected");
            ICoreClientAPI capi = api as ICoreClientAPI;
            IClientPlayer plr = capi.World.Player;
            EntityPlayer byEntity = plr.Entity;
            string liquidType = blockAtPos.FirstCodePart();
            //Debug.WriteLine(blockAtPos.Code.Path);
            if (api.World.Side == EnumAppSide.Client)
            {
                if (blockAtPos.Code.Path.Contains("-7")) //lots of liquid?
                {
                    string newPath = "game:" + liquidType + "-still-3";
                    Block newblock = api.World.GetBlock(new AssetLocation(newPath));
                    api.World.BlockAccessor.SetBlock(newblock.BlockId, bloodPos); //replace liquid with less
                    api.World.BlockAccessor.MarkBlockDirty(bloodPos);
                    api.World.BlockAccessor.TriggerNeighbourBlockUpdate(bloodPos);
                }
            }



            string stackname = "null";
            if (byEntity.RightHandItemSlot.Itemstack != null)
                stackname = byEntity.RightHandItemSlot.Itemstack.GetName();
            //Debug.WriteLine(stackname);
        }

        private void AfterBloodCollected2(float dt)
        {
            //Debug.WriteLine("Blood collected");
            ICoreClientAPI capi = api as ICoreClientAPI;
            IClientPlayer plr = capi.World.Player;
            EntityPlayer byEntity = plr.Entity;
            string liquidType = blockAtPos.FirstCodePart();
            if (api.World.Side == EnumAppSide.Client)
            {
                string newPath = "game:" + liquidType + "-still-2";
                Block newblock = api.World.GetBlock(new AssetLocation(newPath));
                api.World.BlockAccessor.SetBlock(newblock.BlockId, bloodPos); //replace liquid with less
                api.World.BlockAccessor.MarkBlockDirty(bloodPos);
                api.World.BlockAccessor.TriggerNeighbourBlockUpdate(bloodPos);
            }
        }

        private void AfterBloodCollected3(float dt)
        {
            //Debug.WriteLine("Blood collected");
            ICoreClientAPI capi = api as ICoreClientAPI;
            IClientPlayer plr = capi.World.Player;
            EntityPlayer byEntity = plr.Entity;
            string liquidType = blockAtPos.FirstCodePart();
            if (api.World.Side == EnumAppSide.Client)
            {
                string newPath = "game:" + liquidType + "-still-1";
                Block newblock = api.World.GetBlock(new AssetLocation(newPath));
                api.World.BlockAccessor.SetBlock(0, bloodPos); //replace liquid with less
                api.World.BlockAccessor.MarkBlockDirty(bloodPos);
                api.World.BlockAccessor.TriggerNeighbourBlockUpdate(bloodPos);
            }
        }

        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (api.World.Side == EnumAppSide.Server)
            {
                //Debug.WriteLine("STEP?");
            }
            return false;
        }


        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (api.World.Side == EnumAppSide.Server)
            {
                //Debug.WriteLine("STOP?");
                Block block = world.BlockAccessor.GetBlock(blockSel.Position);
                string liquidType = block.FirstCodePart();
                if (block.Code.Path.Contains("-7")) //lots of liquid?
                {
                    string newPath = "game:" + liquidType + "-still-3";
                    Block newblock = world.GetBlock(new AssetLocation(newPath));
                    world.BlockAccessor.SetBlock(newblock.BlockId, blockSel.Position); //replace liquid with less
                    world.BlockAccessor.MarkBlockDirty(blockSel.Position);
                }
            }
        }
    }
}

