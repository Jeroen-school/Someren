namespace Someren.Models
{
    public class Room
    {
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }

        public Room()
        {
            RoomNumber = "";
            Type = "";
            Size = 1;
        }
        public Room(string roomNumber, string type, int size)
        {
            RoomNumber = roomNumber;
            Type = type;
            Size = size;
        }

    }
}
