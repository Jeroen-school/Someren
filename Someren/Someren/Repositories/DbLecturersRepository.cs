﻿using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Someren.Models;
using System.Data;

namespace Someren.Repositories
{
    //One of the criteria in the grading rubric was: "Methods are no longer than 10 lines of code, so I removed *some* redundancy and adhere to that requirement, unless I forgot how to count"
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

        //returns a list of all NOT DELETED lecturers
        public List<Lecturer> GetAll()
        {
            List<Lecturer> lecturers = new List<Lecturer>();
            
            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [Deleted] = 0 ORDER BY [last_name]";         //the sql query to execute
            
            ExecuteQuery(lecturers, query, null, null);
            
            return lecturers;
        }

        //This is the literal same as the thing above, but it only gets deleted records! Wooooo
        public List<Lecturer> GetAllDeleted()
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [Deleted] = 1 ORDER BY [last_name]";         //the sql query to execute

            ExecuteQuery(lecturers, query, null, null);

            return lecturers;
        }

        //returns a list of all lecturers where the LAST NAME CONTAINS THE SEARCHED STRING
        public List<Lecturer> GetFiltered(string lastName)
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [last_name] LIKE @LastName AND [Deleted] = 0 ORDER BY [last_name];";

            ExecuteQuery(lecturers, query, "@LastName", $"%{lastName}%");

            return lecturers;
        }

        //Does the exact same thing as the get filtered method above, BUT ONLY FOR DELETED RECORDS
        public List<Lecturer> GetFilteredDeleted(string lastName)
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [last_name] LIKE @LastName AND [Deleted] = 1 ORDER BY [last_name];";

            ExecuteQuery(lecturers, query, "@LastName", $"%{lastName}%");

            return lecturers;
        }


        //Search a lecturer by ID, then return a class with the information of that lecturer, starts with a list so I can use the method I made specifically for Reading records from the database
        public Lecturer? GetById(int lecturerId)
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [lecturer_id] = @Id;";

            ExecuteQuery(lecturers, query, "@Id", lecturerId.ToString());

            if(lecturers.Count != 0)
            {
                return lecturers[0];
            }
            else
            {
                throw new Exception("Lecturer not found");
            }
        }

        private Lecturer? GetByLastName(string? lastName)
        {
            List<Lecturer> lecturers = new List<Lecturer>();

            string query = "SELECT [lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty], [Deleted] FROM lecturer WHERE [last_name] LIKE @LastName;";

            ExecuteQuery(lecturers, query, "@LastName", lastName);

            //the lecturers list always returns the first list item, this should not be an issue as you are not allowed to add lecturers with a duplicate last name. TECHNICALLY you are still able to add a lecturer, then change their last name to one that already exists in the database.
            if(!lecturers.IsNullOrEmpty())
            {
                return lecturers[0];
            }
            else
            {
                return null;
            }
        }


        //ADD a lecturer to the database
        public void Add(Lecturer lecturer)
        {
            string query;
            //Check if the lecturer has been soft deleted in the past, if they have been, undelete them. Then check if the last name already exists in the database
            Lecturer? checkLecturer = GetByLastName(lecturer.LastName);

            if (checkLecturer != null)
            {
                if (checkLecturer.LecturerId == lecturer.LecturerId && checkLecturer.FirstName == lecturer.FirstName && checkLecturer.LastName == lecturer.LastName && checkLecturer.Deleted)
                {
                    query = $"UPDATE lecturer SET [telephone_number] = @PhoneNumber, [age] = @Age, [Deleted] = 0 WHERE lecturer.lecturer_id = @Id;";
                }
                //If lecturer name is not null AND the id and first name is not the same AND the lecturer is deleted, then show that the lecturer already exists in the database, as checkLecturer can only have a value if the last names match
                else
                {
                    throw new Exception("Lecturer's last name already exists in the database");
                }
            }
            else
            {
                //If the lecturer is new, create a new lecturer in the database
                query = $"INSERT INTO lecturer ([lecturer_id], [room_number], [first_name], [last_name], [telephone_number], [age], [bar_duty])" +
                                        $"VALUES (@Id, @RoomNumber, @Firstname, @LastName, @PhoneNumber, @Age, @BarDuty);";
            }

            ExecuteQuery(lecturer, query);
        }

        //EDIT a lecturer in the database
        public void Update(Lecturer lecturer)
        {
            string query = $"UPDATE lecturer SET [lecturer_id] = @Id, [room_number] = @RoomNumber, [first_name] = @Firstname, [last_name] = @LastName, [telephone_number] = @PhoneNumber, [age] = @Age, [bar_duty] = @BarDuty WHERE lecturer.[lecturer_id] = @Id;";

            ExecuteQuery(lecturer, query);
        }

        //SOFT DELETE a lecturer from the database
        public void Delete(Lecturer lecturer)
        {
            string query = $"UPDATE lecturer SET Deleted = 1 WHERE lecturer.lecturer_id = @Id;";

            ExecuteQuery(lecturer , query);
        }

        //Restore a deleted lecturer from the database
        public void Restore(Lecturer lecturer)
        {
            string query = $"UPDATE lecturer SET Deleted = 0 WHERE lecturer.lecturer_id = @Id;";

            ExecuteQuery(lecturer, query);
        }

        //HARD DELETE a lecturer from the database
        public void Erase(Lecturer lecturer)
        {
            string query = $"DELETE FROM lecturer WHERE lecturer.lecturer_id = @Id;";

            ExecuteQuery(lecturer, query);
        }


        //This is where I keep the methods that PREVENT REPETITION

        //This is to add parameters to the SQL query to prevent an SQL injection
        private static void AddParametersWithValues(SqlCommand command, Lecturer lecturer)
        {
            command.Parameters.AddWithValue("@Id", lecturer.LecturerId);
            command.Parameters.AddWithValue("@RoomNumber", lecturer.RoomNumber);
            command.Parameters.AddWithValue("@FirstName", lecturer.FirstName);
            command.Parameters.AddWithValue("@LastName", lecturer.LastName);
            command.Parameters.AddWithValue("@PhoneNumber", lecturer.PhoneNumber);
            command.Parameters.AddWithValue("@Age", lecturer.Age);
            command.Parameters.AddWithValue("@BarDuty", lecturer.BarDuty);
        }


        //Reads basic information from the Lecturer database
        private Lecturer ReadLecturer(SqlDataReader reader)
        {
            int lecturerId = (int)reader["lecturer_id"];
            string roomNumber = (string)reader["room_number"];
            string firstName = (string)reader["first_name"];
            string lastName = (string)reader["last_name"];
            string phoneNumber = (string)reader["telephone_number"];
            int age = (int)reader["age"];
            bool barDuty = (bool)reader["bar_duty"];
            bool deleted = (bool)reader["Deleted"];

            return new Lecturer(lecturerId, roomNumber, firstName, lastName, phoneNumber, age, barDuty, deleted);
        }

        //To execute the read methods
        private void ExecuteQuery(List<Lecturer> lecturers, string query, string? parameterName, string? parameterValue)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))         //this sets up the ground rules for the connection
            {
                SqlCommand command = new SqlCommand(query, connection);

                if (parameterName != null && parameterValue != null)
                {
                    command.Parameters.AddWithValue($"{parameterName}", parameterValue);
                }

                command.Connection.Open();                                                  //connects to the database
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    lecturers.Add(ReadLecturer(reader));
                }

                reader.Close();
            }

        }

        //To execute the Create, Update, and Delete methods, uses a Data Reader because I want to add the error codes **later**
        private void ExecuteQuery(Lecturer lecturer, string query)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {

                SqlCommand command = new SqlCommand(query, connection);

                AddParametersWithValues(command, lecturer);

                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                reader.Close();
            }

        }
    }
}
