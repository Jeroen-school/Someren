namespace Someren.Models
{
    public class Drink
    {
        public int DrinkId { get; set; }
        public string DrinkType { get; set; }
        public int DrinkConsumed { get; set; }
        public int DrinkStock { get; set; }
        public decimal DrinkPrice { get; set; }
        public int VatRate { get; set; }

        public Drink() { }

        public Drink(int drinkId, string drinkType, int drinkConsumed, int drinkStock, decimal drinkPrice, int vatRate)
        {
            DrinkId = drinkId;
            DrinkType = drinkType;
            DrinkConsumed = drinkConsumed;
            DrinkStock = drinkStock;
            DrinkPrice = drinkPrice;
            VatRate = vatRate;
        }
    }
}
