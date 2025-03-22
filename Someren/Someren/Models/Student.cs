namespace Someren.Models
{
    public class Student
    {
        public int StudentNum { get; set; }
        public string RoomNum { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TelNum { get; set; }
        public string StudentClass { get; set; }

        public Student() { }

        public Student(int studentNum, string roomNUm, string firstName, string lastName, string telNum, string studentClass)
        {
            StudentNum = studentNum;
            RoomNum = roomNUm;
            FirstName = firstName;
            LastName = lastName;
            TelNum = telNum;
            StudentClass = studentClass;
        }
    }
}
