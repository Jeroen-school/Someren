using Microsoft.Data.SqlClient;
using Someren.Models;
using System.Text.RegularExpressions;
namespace Someren.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly string? _connectionString;

        public RoomRepository(IConfiguration configuration)
        {  
            _connectionString = configuration.GetConnectionString("SomerenDb");
        }

        //validation for room number
        private bool IsValidRoomNumber(string roomNumber)
        {
            string pattern = @"(?i)^[AB]\d-\d{2}$"; // Regex pattern
            return Regex.IsMatch(roomNumber, pattern);
        }

        private bool IsRoomNumberForLecturers(string roomNumber)
        {
            string pattern = @"(?i)^A1-\d{2}$"; // Regex pattern
            return Regex.IsMatch(roomNumber, pattern);
        }
        private bool RoomExists(string roomNumber)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(room_number) FROM room WHERE room_number = @roomNumber AND Deleted = @deleted";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomNumber", roomNumber);
                command.Parameters.AddWithValue("@deleted", false);

                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private bool IsRoomDeleted(string roomNumber)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(room_number) FROM room WHERE room_number = @roomNumber AND Deleted = @deleted";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomNumber", roomNumber);
                command.Parameters.AddWithValue("@deleted", true);

                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
        private Room ReadRoom(SqlDataReader reader)
        {
            int roomId = (int)reader["room_id"];
            string roomNumber = (string)reader["room_number"];
            string type = (string)reader["room_type"];
            int size = (int)reader["room_size"];
            bool deleted = (bool)reader["Deleted"];

            return new Room(roomId, roomNumber, type, size, deleted);
        }

        private Student ReadStudent(SqlDataReader reader)
        {
            int studentNum = (int)reader["student_number"];
            string roomNum = reader["room_id"].ToString();
            string firstName = (string)reader["first_name"];
            string lastName = (string)reader["last_name"];
            string telNum = (string)reader["telephone_number"];
            string studentClass = (string)reader["class"];
            return new Student(studentNum, roomNum, firstName, lastName, telNum, studentClass);
        }

        public List<Room> GetBySize(int size)
        {
            List<Room> rooms = new List<Room>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM room WHERE room_size = @roomSize AND Deleted = @deleted ORDER BY room_number";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomSize", size);
                command.Parameters.AddWithValue("@deleted", false);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Room room = ReadRoom(reader);
                    rooms.Add(room);
                }
                reader.Close();
            }
            return rooms;
        }

        public List<Room> GetAll(bool deleted)
        {
            List<Room> rooms = new List<Room>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM room WHERE Deleted = @deleted ORDER BY room_number";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@deleted", deleted);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Room room = ReadRoom(reader);
                    rooms.Add(room);
                }
                reader.Close();
            }
            return rooms;
        }

        public Room? GetById(int roomId, bool deleted)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM room WHERE room_id = @roomId AND Deleted = @deleted";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomId", roomId);
                command.Parameters.AddWithValue("@deleted", deleted);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    return ReadRoom(reader);
                }
                reader.Close();
                return null;
            }
        }

        public bool Add(Room room, out string errorMessage)
        {
            errorMessage = "";

            // Validate Room Number Format
            if (!IsValidRoomNumber(room.RoomNumber))
            {
                errorMessage = "Invalid room number format. Format must be A1-02 or B2-05.";
                return false;
            }

            // Check if Room Already Exists
            if (RoomExists(room.RoomNumber))
            {
                errorMessage = "Room already exists.";
                return false;
            }

            // Return error message if room number should be for lecturers
            if (room.Type == "Student" && IsRoomNumberForLecturers(room.RoomNumber))
            {
                errorMessage = "Students can not sleep in building A ground floor (A1).";
                return false;
            }

            // Return error message if room number should be for students
            if (room.Type == "Lecturer" && !IsRoomNumberForLecturers(room.RoomNumber))
            {
                errorMessage = "Lecturers can only sleep in building A ground floor (A1).";
                return false;
            }

            // If validation passes, insert the room
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query;
                if (IsRoomDeleted(room.RoomNumber))
                {
                    query = "UPDATE room SET room_number = @roomNumber, room_size = @roomSize, room_type = @roomType, Deleted = @deleted WHERE room_number = @roomNumber";
                }
                else
                {
                    query = "INSERT INTO room (room_number, room_size, room_type, Deleted) " +
                                   "VALUES (@roomNumber, @roomSize, @roomType, @deleted)";
                }
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomNumber", room.RoomNumber.ToUpper());
                command.Parameters.AddWithValue("@roomSize", room.Size);
                command.Parameters.AddWithValue("@roomType", room.Type);
                command.Parameters.AddWithValue("@deleted", false);

                connection.Open();
                command.ExecuteNonQuery();
            }
            return true;
        }

        public List<Student> GetStudentInRoomByRoomId(int roomId)
        {
            List<Student> students = new List<Student>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM student WHERE room_id = @roomId AND Deleted = @deleted ORDER BY last_name";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomId", roomId);
                command.Parameters.AddWithValue("@deleted", false);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Student student = ReadStudent(reader);
                    students.Add(student);
                }
                reader.Close();
            }
            return students;
        }

        public void AssignStudentToRoom(int studentNum, int roomId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE student SET room_id = @roomId WHERE student_number = @studentNum";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomId", roomId);
                command.Parameters.AddWithValue("@studentNum", studentNum);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void RemoveStudentToRoom(int studentNum, int roomId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE student SET room_id = NULL WHERE student_number = @studentNum";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentNum", studentNum);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<Student> GetStudentNotInRoomByRoomId(int roomId)
        {
            List<Student> students = new List<Student>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM student WHERE (room_id IS NULL OR room_id != @roomId) AND Deleted = @deleted ORDER BY last_name";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomId", roomId);
                command.Parameters.AddWithValue("@deleted", false);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Student student = ReadStudent(reader);
                    students.Add(student);
                }
                reader.Close();
            }
            return students;
        }

        public bool Update(Room room, out string errorMessage)
        {
            errorMessage = "";

            // Validate Room Number Format
            if (!IsValidRoomNumber(room.RoomNumber))
            {
                errorMessage = "Invalid room number format. Format must be A1-02 or B2-05.";
                return false;
            }

            Room? savedRoom = GetById(room.RoomId, false);

            // Check if Room Already Exists
            if (savedRoom != null && savedRoom.RoomNumber != room.RoomNumber && RoomExists(room.RoomNumber))
            {
                errorMessage = "Room already exists.";
                return false;
            }

            // Return error message if room number should be for lecturers
            if (room.Type == "Student" && IsRoomNumberForLecturers(room.RoomNumber))
            {
                errorMessage = "Students can not sleep in building A ground floor (A1).";
                return false;
            }

            // Return error message if room number should be for students
            if (room.Type == "Lecturer" && !IsRoomNumberForLecturers(room.RoomNumber))
            {
                errorMessage = "Lecturers can only sleep in building A ground floor (A1).";
                return false;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE room SET room_number = @roomNumber, room_size = @roomSize, room_type = @roomType WHERE room_id = @roomId AND Deleted = @deleted";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomNumber", room.RoomNumber.ToUpper());
                command.Parameters.AddWithValue("@roomId", room.RoomId);
                command.Parameters.AddWithValue("@roomSize", room.Size);
                command.Parameters.AddWithValue("@roomType", room.Type);
                command.Parameters.AddWithValue("@deleted", false);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            return true;
        }

        public bool SoftDelete(int roomId, out string errorMessage)
        {
            errorMessage = "";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                UPDATE room
                SET Deleted = @deleted
                WHERE room_id = @roomId
                  AND room_id NOT IN (SELECT room_id FROM student WHERE room_id IS NOT NULL)
                  AND room_id NOT IN (SELECT room_id FROM lecturer WHERE room_id IS NOT NULL);
                ";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@roomId", roomId);
                command.Parameters.AddWithValue("@deleted", true);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    errorMessage = "Room could not be deleted because it belongs to a student or lecturer.";
                    return false;
                }
            }
            errorMessage = "Room was successfully deleted.";
            return true;
        }


        public void Restore(int roomId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE room SET Deleted = @deleted WHERE room_id = @roomId";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomId", roomId);
                command.Parameters.AddWithValue("@deleted", false);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Erase(int roomId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM room WHERE room_id = @roomId";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@roomId", roomId);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
