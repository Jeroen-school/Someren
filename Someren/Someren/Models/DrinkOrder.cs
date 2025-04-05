namespace Someren.Models
{
    public class DrinkOrder
    {
        public int StudentId { get; set; }
        public int DrinkId { get; set; }
        public int Quantity { get; set; }

        public DrinkOrder() { }

        public  DrinkOrder (int studentId, int drinkId, int quantity)
        {
            StudentId = studentId;
            DrinkId = drinkId;
            Quantity = quantity;
        }
    }
}
