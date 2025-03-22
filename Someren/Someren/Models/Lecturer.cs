namespace Someren.Models
{
    public class Lecturer
    {
        //fields and properties
        public int LecturerId { get; set; }
        public string RoomNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int Age { get; set; }
        public bool BarDuty { get; set; }
        public bool Deleted { get; set; }

        //constructors
        public Lecturer()
        {

        }

        public Lecturer(int lecturerId, string roomNumber, string firstName, string lastName, string phoneNumber, int age)
        {
            this.LecturerId = lecturerId;
            this.RoomNumber = roomNumber;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PhoneNumber = phoneNumber;
            this.Age = age;
            this.BarDuty = false;
        }

        public Lecturer(string roomNumber, string firstName, string lastName, string phoneNumber, int age, bool balls)
        {
            this.RoomNumber = roomNumber;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PhoneNumber = phoneNumber;
            this.Age = age;
            this.BarDuty = false;
        }

        public Lecturer(int lecturerId, string roomNumber, string firstName, string lastName, string phoneNumber, int age, bool barDuty)
        {
            this.LecturerId = lecturerId;
            this.RoomNumber = roomNumber;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PhoneNumber = phoneNumber;
            this.Age = age;
            this.BarDuty = barDuty;
        }

        public Lecturer(int lecturerId, string roomNumber, string firstName, string lastName, string phoneNumber, int age, bool barDuty, bool deleted)
        {
            this.LecturerId = lecturerId;
            this.RoomNumber = roomNumber;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PhoneNumber = phoneNumber;
            this.Age = age;
            this.BarDuty = barDuty;
            this.Deleted = deleted;
        }

        //methods

    }
}