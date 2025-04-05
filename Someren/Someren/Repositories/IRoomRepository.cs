using Someren.Models;
namespace Someren.Repositories
{
    public interface IRoomRepository
    {
        List<Room> GetAll(bool deleted);
        List<Room> GetBySize(int size);
        List<Student> GetStudentInRoomByRoomId(int roomId);
        List<Student> GetStudentNotInRoomByRoomId(int roomId);
        Room? GetById(int roomId, bool deleted);
        bool Add(Room room, out string errorMessage);
        bool Update(Room room, out string errorMessage);
        bool SoftDelete(int roomId, out string errorMessage);
        void Restore(int roomId);
        void Erase(int roomId);
        void AssignStudentToRoom(int studentId, int roomId);
        void RemoveStudentToRoom(int studentId, int roomId);
    }
}
