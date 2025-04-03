namespace Someren.Models
{
    public class RoomDetail
    {
        public List<Student> StudentsInRoom { get; set; }
        public List<Student> StudentsNotInRoom { get; set; }
        public Room Room { get; set; }
    }
}
