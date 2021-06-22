namespace PrimitiveSurvival.ModSystem
{
    using System;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using PrimitiveSurvival.ModConfig;
 
    public class ItemPSFish : Item
    {
        private readonly int eggsPercent = ModConfig.Loaded.FishChanceOfEggs; //10
        private static readonly Random Rnd = new Random();
        public override void OnConsumedByCrafting(ItemSlot[] allInputSlots, ItemSlot stackInSlot, GridRecipe gridRecipe, CraftingRecipeIngredient fromIngredient, IPlayer byPlayer, int quantity)
        {
            base.OnConsumedByCrafting(allInputSlots, stackInSlot, gridRecipe, fromIngredient, byPlayer, quantity);

            var rando = Rnd.Next(100);
            Item item;
            if (rando < this.eggsPercent)
            {
                rando = Rnd.Next(2);
                if (rando == 0)
                {
                    item = this.api.World.GetItem(new AssetLocation("primitivesurvival:fisheggs-raw-normal"));
                }
                else
                {
                    item = this.api.World.GetItem(new AssetLocation("primitivesurvival:fisheggs-raw-ovulated"));
                }
                var outStack = new ItemStack(item);
                this.api.World.SpawnItemEntity(outStack, new Vec3d(byPlayer.Entity.Pos.X + 0.5, byPlayer.Entity.Pos.Y + 0.5, byPlayer.Entity.Pos.Z + 0.5), null);
            }
        }
    }
}
