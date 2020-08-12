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
            api.RegisterBlockBehaviorClass("bbcollectfish", typeof(BBCollectFish));
            api.RegisterBlockBehaviorClass("bbrebait", typeof(BBRebait));
            api.RegisterBlockEntityClass("befishbasket", typeof(BEFishBasket));
            api.RegisterBlockEntityClass("betrotline", typeof(BETrotline));
            api.RegisterBlockClass("blocktrap", typeof(BlockTrap));
            api.RegisterBlockClass("blocktrotline", typeof(BlockTrotline));
            api.RegisterBlockClass("blockropebridge", typeof(BlockRopeBridge));
            api.RegisterItemClass("itemfishinghook", typeof(ItemFishingHook));
        }

        public override void StartServerSide(ICoreServerAPI Api)
        {
            sapi = Api;
        }

        public override void StartClientSide(ICoreClientAPI Api)
        {
            capi = Api;
            capi.ShowChatMessage("Primitive Surival mod enabled...");
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            RegisterClasses(api);
        }
    }
}




