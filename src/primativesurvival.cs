using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;

namespace primitiveSurvival
{
    public partial class PrimitaveSurvivalMod : ModSystem
    {
        ICoreClientAPI capi;
        ICoreServerAPI sapi;

        public void RegisterClasses(ICoreAPI api)
        {
            api.RegisterBlockEntityClass("bedeadfall", typeof(BEDeadfall));
            api.RegisterBlockEntityClass("besnare", typeof(BESnare));
            api.RegisterBlockEntityClass("belimbtrotlinelure", typeof(BELimbTrotLineLure));
            api.RegisterBlockEntityClass("befishbasket", typeof(BEFishBasket));
            api.RegisterBlockEntityClass("beweirtrap", typeof(BEWeirTrap));
            api.RegisterBlockEntityClass("betemporalbase", typeof(BETemporalBase));
            api.RegisterBlockEntityClass("betemporallectern", typeof(BETemporallectern));
            api.RegisterBlockEntityClass("betemporalcube", typeof(BETemporalCube));
            api.RegisterBlockEntityClass("bewoodsupportspikes", typeof(BEWoodSupportSpikes));

            api.RegisterBlockClass("blockstake", typeof(BlockStake));
            api.RegisterBlockClass("blockstakeinwater", typeof(BlockStakeInWater));
            api.RegisterBlockClass("blockdeadfall", typeof(BlockDeadfall));
            api.RegisterBlockClass("blocksnare", typeof(BlockSnare));
            api.RegisterBlockClass("blocklimbtrotlinelure", typeof(BlockLimbTrotLineLure));
            api.RegisterBlockClass("blockfishbasket", typeof(BlockFishBasket));
            api.RegisterBlockClass("blockweirtrap", typeof(BlockWeirTrap));
            api.RegisterBlockClass("blocktemporal", typeof(BlockTemporal));
            api.RegisterBlockClass("blocktemporalbase", typeof(BlockTemporalBase));
            api.RegisterBlockClass("blocktemporallectern", typeof(BlockTemporallectern));
            api.RegisterBlockClass("blocktemporalcube", typeof(BlockTemporalCube));
            api.RegisterBlockClass("blockspiketrap", typeof(BlockSpikeTrap));
            api.RegisterBlockClass("blockwoodsupportspikes", typeof(BlockWoodSupportSpikes));

            api.RegisterItemClass("itemfishinghook", typeof(ItemFishingHook));
            api.RegisterItemClass("itemcordage", typeof(ItemCordage));
            api.RegisterItemClass("itemwoodspikebundle", typeof(ItemWoodSpikeBundle));
            api.RegisterItemClass("itempsgear", typeof(ItemPSGear));
        }

        public override void StartServerSide(ICoreServerAPI Api)
        {
            sapi = Api;
        }

        public override void StartClientSide(ICoreClientAPI Api)
        {
            capi = Api;
            //capi.ShowChatMessage("Primitive Surival mod installed...");
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            RegisterClasses(api);
        }
    }
}



