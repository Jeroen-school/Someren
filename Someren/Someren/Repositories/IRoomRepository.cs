using Someren.Models;
namespace Someren.Repositories
{
    public interface IRoomRepository
    {
        List<Room> GetAll();
        List<Room> GetBySize(int size);
        List<Student> GetStudentInRoomByRoomId(int roomId);
        List<Student> GetStudentNotInRoomByRoomId(int roomId);
        Room? GetById(int roomId);
        bool Add(Room room, out string errorMessage);
        bool Update(Room room, out string errorMessage);
        void Delete(int roomId);
        void AssignStudentToRoom(int studentId, int roomId);
        void RemoveStudentToRoom(int studentId, int roomId);
    }
}
