using Someren.Models;

namespace Someren.Repositories
{
    public interface IDrinkRepository
    {
        List<Drink> GetAll();
        Drink? GetById(int drinkId);
        void UpdateStock(int drinkId, int quantityConsumed);
    }
}
