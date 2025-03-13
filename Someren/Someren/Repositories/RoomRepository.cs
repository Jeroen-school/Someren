using Microsoft.Data.SqlClient;
using Someren.Models;
namespace Someren.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly string? _connectionString;

        public RoomRepository(IConfiguration configuration)
        {  
            _connectionString = configuration.GetConnectionString("SomerenDb");
        }

        private Room ReadRoom(SqlDataReader reader)
        {
            //retrieve data from fields
            string roomNumber = (string)reader["RoomNumber"];
            string type = (string)reader["Type"];
            int size = (int)reader["Size"];

            //return new User object
            return new Room(roomNumber, type, size);
        }

        public List<Room> GetAll()
        {
            List<Room> rooms = new List<Room>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM room";
                SqlCommand command = new SqlCommand(query, connection);

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
    }
}
