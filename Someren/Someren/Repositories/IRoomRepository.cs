namespace Someren.Repositories
{
    public interface IRoomRepository
    {
        List<Models.Room> GetAll();
        Models.Room? GetById(string roomNumber);
        bool Add(Models.Room room, out string errorMessage);
        bool Update(Models.Room room, out string errorMessage);
        void Delete(string roomNumber);
    }
}
