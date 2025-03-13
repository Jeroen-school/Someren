using Microsoft.Data.SqlClient;
using Someren.Models;

namespace Someren.Repositories
{
    public class DbLecturersRepository : ILecturersRepository
    {
        //fields and properties
        private readonly string? _connectionString;

        //constructors
        public DbLecturersRepository(IConfiguration configuration)
        {
            //get the connection string from the appsettings
            _connectionString = configuration.GetConnectionString("SomerenDB");
        }

        //methods
        public List<Lecturer> GetAll()
        {
            List<Lecturer> lecturers = new List<Lecturer>();
            
            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {
                string query = "SELECT [lecturer_id], [first_name], [last_name], [telephone_number], [age], [Deleted] FROM lecturer ORDER BY [last_name]";         //the sql query to execute
                SqlCommand command = new SqlCommand(query, connection);

                command.Connection.Open();                                                  //connects to the database
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Lecturer lecturer = ReadLecturerBasic(reader);
                    if (!lecturer.Deleted)
                    {
                        lecturers.Add(lecturer);
                    }
                }

                reader.Close();
            }
            
            return lecturers;
        }

        //Reads basic information from the Lecturer database
        private Lecturer ReadLecturerBasic(SqlDataReader reader)
        {
            int lecturerId = (int)reader["lecturer_id"];
            string firstName = (string)reader["first_name"];
            string lastName = (string)reader["last_name"];
            string telephone_number = (string)reader["telephone_number"];
            int age = (int)reader["age"];
            bool deleted = (bool)reader["Deleted"];

            return new Lecturer(lecturerId, firstName, lastName, telephone_number, age, deleted);
        }

        public Lecturer? GetById(int lecturerId)
        {
            Lecturer lecturer = new Lecturer();



            return lecturer;
        }


        public void Add(Lecturer lecturer)
        {

        }

        public void Update(Lecturer lecturer)
        {

        }

        public void Delete(Lecturer lecturer)
        {

        }

    }
}
