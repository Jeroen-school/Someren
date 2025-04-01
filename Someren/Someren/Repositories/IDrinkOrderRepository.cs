using Someren.Models;

namespace Someren.Repositories
{
    public interface IDrinkOrderRepository
    {
        void AddOrder(DrinkOrder order);
        List <DrinkOrder> GetOrdersByStudent(int studentId);
    }
}
