namespace Someren.Models
{
    public class Lecturer
    {
        //fields and properties
        public int LecturerId { get; }
        public string RoomNum { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TelNum { get; set; }
        public int age { get; set; }
        public bool barDuty { get; set; }
        public bool Deleted { get; set; }

        //constructors
        public Lecturer() {
            
        }

        public Lecturer(int lecturerNum, string firstName, string lastName, string telNum, int age, bool deleted)
        {
            this.LecturerId = lecturerNum;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.TelNum = telNum;
            this.age = age;
            this.Deleted = deleted;
        }

        public Lecturer(int lecturerNum, string roomNUm, string firstName, string lastName, string telNum, int age, bool barDuty)
        {
            this.LecturerId = lecturerNum;
            this.RoomNum = roomNUm;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.TelNum = telNum;
            this.age = age;
            this.barDuty = barDuty;
            this.Deleted = false;
        }

        //methods

    }
}
