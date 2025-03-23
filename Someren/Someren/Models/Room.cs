namespace Someren.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
        
        public bool Deleted { get; set; }

        public Room()
        {
            RoomNumber = "";
            Type = "";
            Size = 0;
            Deleted = false;
        }
        public Room(int roomId, string roomNumber, string type, int size, bool deleted)
        {
            RoomId = roomId;
            RoomNumber = roomNumber;
            Type = type;
            Size = size;
            Deleted = deleted;
        }
    }
}
