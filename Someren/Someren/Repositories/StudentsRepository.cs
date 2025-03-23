// Necessary imports with minimal inclusions
using Someren.Models;                 // For Student model
using Microsoft.Data.SqlClient;       // For SQL Server access
using System.Data;                    // For ADO.NET data types
using System.Threading.Tasks;         // Prepared for potential async operations
using System.Collections.Generic;     // For List<> collections
using System;                         // For core .NET types


/*
Optimization techniques added:
- 'sealed' class: Enables compiler optimizations by preventing inheritance
- Static readonly fields: Reduces string allocations and memory pressure
- CommandBehavior flags: SequentialAccess for streaming data, SingleRow for single-record optimization
- Direct index access: reader.GetInt32(0) eliminates string lookup overhead vs reader["column_name"]
- Explicit parameter typing: SqlParameter(name, SqlDbType.X, size) improves query plan caching
- Early null checking: Prevents cascading failures with descriptive exceptions
- String interpolation with cached parameters: Maintains readability with parameterized security
- ID inclusion in errors: Creates more actionable logs for troubleshooting
- Parameter size specification: Prevents parameter sniffing issues for consistent performance
*/
namespace Someren.Repositories
{
    // 'sealed' prevents inheritance, which allows compiler optimizations
    public sealed class StudentsRepository : IStudentsRepository
    {
        // Static readonly fields for SQL fragments to reduce string allocations
        // These are computed once at class load time, not per-instance
        private static readonly string s_baseColumns = "S.[student_number], R.[room_number], S.[first_name], S.[last_name], S.[telephone_number], S.[class]";
        private static readonly string s_notDeletedClause = "(S.Deleted = 0 OR S.Deleted IS NULL)";
        private static readonly string s_baseSelectQuery = $"SELECT {s_baseColumns} FROM student AS S JOIN room AS R ON S.room_id = R.room_id WHERE";
        private static readonly string s_deletedClause = "S.Deleted = 1";

        // Cache parameter names to avoid string allocations in methods
        // 's_' prefix indicates static field 
        private static readonly string s_paramStudentNum = "@StudentNum";
        private static readonly string s_paramRoomNum = "@RoomNum";
        private static readonly string s_paramFirstName = "@FirstName";
        private static readonly string s_paramLastName = "@LastName";
        private static readonly string s_paramTelNum = "@TelNum";
        private static readonly string s_paramClass = "@Class";

        // Connection string stored as readonly field, never changes after initialization
        private readonly string? _connectionString;

        // Constructor with null check using null-coalescing operator for early failure
        public StudentsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SomerenDB") ??
                throw new ArgumentNullException(nameof(configuration), "Database connection string cannot be null");
        }

        // Reader that uses column indexes instead of names for faster access
        private static Student ReadStudent(SqlDataReader reader)
        {
            return new Student(
                reader.GetInt32(0),            // Direct index access instead of string lookup
                reader.GetString(1),           // Much faster than reader["column_name"]
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5)
            );
        }

        // GetAll method optimized
        public List<Student> GetAll()
        {
            var students = new List<Student>(50); // Preallocate for typical result set
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"{s_baseSelectQuery} {s_notDeletedClause} ORDER BY S.last_name";
            cmd.CommandType = CommandType.Text;

            connection.Open();
            // SequentialAccess improves performance for forward-only reading
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

            while (reader.Read())
            {
                students.Add(ReadStudent(reader));
            }

            return students;
        }

        // Returns a list of all students where the LAST NAME CONTAINS THE SEARCHED STRING
        public List<Student> GetFiltered(string lastName)
        {
            var students = new List<Student>(50);
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"{s_baseSelectQuery} S.[last_name] LIKE {s_paramLastName} AND {s_notDeletedClause} ORDER BY S.[last_name]";
            cmd.CommandType = CommandType.Text;

            // Sanitize input by removing any SQL wildcards before adding our own
            string sanitizedName = lastName?.Replace("%", "").Replace("_", "") ?? "";
            cmd.Parameters.Add(new SqlParameter(s_paramLastName, SqlDbType.NVarChar, 50) { Value = $"%{sanitizedName}%" });

            connection.Open();
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

            while (reader.Read())
            {
                students.Add(ReadStudent(reader));
            }

            return students;
        }

        // Get all soft-deleted students
        public List<Student> GetDeleted()
        {
            var students = new List<Student>(50);
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"{s_baseSelectQuery} {s_deletedClause} ORDER BY S.last_name";
            cmd.CommandType = CommandType.Text;

            connection.Open();
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

            while (reader.Read())
            {
                students.Add(ReadStudent(reader));
            }

            return students;
        }

        // Returns deleted students filtered by last name
        public List<Student> GetFilteredDeleted(string lastName)
        {
            var students = new List<Student>(50);
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"{s_baseSelectQuery} S.last_name LIKE {s_paramLastName} AND {s_deletedClause} ORDER BY S.last_name";
            cmd.CommandType = CommandType.Text;

            // Sanitize input by removing any SQL wildcards before adding our own
            string sanitizedName = lastName?.Replace("%", "").Replace("_", "") ?? "";
            cmd.Parameters.Add(new SqlParameter(s_paramLastName, SqlDbType.NVarChar, 50) { Value = $"%{sanitizedName}%" });

            connection.Open();
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

            while (reader.Read())
            {
                students.Add(ReadStudent(reader));
            }

            return students;
        }

        // GetByNum optimized for single record retrieval
        public Student? GetByNum(int studentNum)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"{s_baseSelectQuery} S.student_number = {s_paramStudentNum} AND {s_notDeletedClause}";
            cmd.CommandType = CommandType.Text;

            // Create parameter directly with proper type specification
            var param = new SqlParameter(s_paramStudentNum, SqlDbType.Int) { Value = studentNum };
            cmd.Parameters.Add(param);

            connection.Open();
            // SingleRow + SequentialAccess for optimal performance on single record fetch
            using var reader = cmd.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SequentialAccess);

            return reader.Read() ? ReadStudent(reader) : null;
        }

        // Get deleted student by ID
        public Student? GetDeletedByNum(int studentNum)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"{s_baseSelectQuery} S.student_number = {s_paramStudentNum} AND {s_deletedClause}";
            cmd.CommandType = CommandType.Text;

            var param = new SqlParameter(s_paramStudentNum, SqlDbType.Int) { Value = studentNum };
            cmd.Parameters.Add(param);

            connection.Open();
            using var reader = cmd.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SequentialAccess);

            return reader.Read() ? ReadStudent(reader) : null;
        }

        public void Add(Student student)
        {
            // Early null check to prevent NullReferenceException
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            // Check if the student number already exists
            if (StudentNumExists(student.StudentNum))
                throw new InvalidOperationException($"Student with number {student.StudentNum} already exists in the system.");

            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            // String interpolation with static fields for better readability
            cmd.CommandText = $"INSERT INTO student (student_number, room_id, first_name, last_name, telephone_number, class) " +
                             $"VALUES ({s_paramStudentNum}, (SELECT room_id FROM room WHERE room_number = {s_paramRoomNum}), {s_paramFirstName}, {s_paramLastName}, {s_paramTelNum}, {s_paramClass})";
            cmd.CommandType = CommandType.Text;

            // Use helper method to add all parameters with proper types
            AddParametersWithValues(cmd, student);

            connection.Open();
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                // Include student ID in error message for better diagnostics
                throw new Exception($"Database error adding student {student.StudentNum}: {ex.Message}", ex);
            }
        }

        public bool StudentNumExists(int studentNum)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            // Check both active and deleted students for the student number
            cmd.CommandText = $"SELECT COUNT(1) FROM student WHERE student_number = {s_paramStudentNum}";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(new SqlParameter(s_paramStudentNum, SqlDbType.Int) { Value = studentNum });

            connection.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        // Update method with checks and optimized parameter handling
        public void Update(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            // Formatted for better readability while still using static fields
            cmd.CommandText = $"UPDATE student SET " +
                             $"room_id = (SELECT room_id FROM room WHERE room_number = {s_paramRoomNum}), " +
                             $"first_name = {s_paramFirstName}, " +
                             $"last_name = {s_paramLastName}, " +
                             $"telephone_number = {s_paramTelNum}, " +
                             $"class = {s_paramClass} " +
                             $"WHERE student_number = {s_paramStudentNum}";
            cmd.CommandType = CommandType.Text;

            AddParametersWithValues(cmd, student);

            connection.Open();
            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    // More descriptive error message with student ID
                    throw new Exception($"Student {student.StudentNum} not found for update");
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error updating student {student.StudentNum}: {ex.Message}", ex);
            }
        }

        // Delete method
        public void Delete(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            // First check if the student exists and is not already deleted
            using (var checkConnection = new SqlConnection(_connectionString))
            {
                using var checkCmd = checkConnection.CreateCommand();
                // Fix: Use proper table alias here
                checkCmd.CommandText = $"SELECT COUNT(1) FROM student AS S WHERE S.student_number = {s_paramStudentNum} AND {s_notDeletedClause}";
                checkCmd.CommandType = CommandType.Text;
                checkCmd.Parameters.Add(new SqlParameter(s_paramStudentNum, SqlDbType.Int) { Value = student.StudentNum });

                checkConnection.Open();
                int exists = (int)checkCmd.ExecuteScalar();

                if (exists == 0)
                {
                    throw new Exception($"Student {student.StudentNum} not found or is already deleted");
                }
            }

            // Proceed with deletion (soft delete by setting Deleted flag)
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"UPDATE student SET Deleted = 1 WHERE student_number = {s_paramStudentNum}";
            cmd.CommandType = CommandType.Text;

            // Only need one parameter for deletion
            cmd.Parameters.Add(new SqlParameter(s_paramStudentNum, SqlDbType.Int) { Value = student.StudentNum });

            connection.Open();
            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception($"Student {student.StudentNum} not found for deletion or is already deleted");
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error deleting student {student.StudentNum}: {ex.Message}", ex);
            }
        }

        // Permanently delete a student record
        public void PermaDel(int studentNum)
        {
            if (studentNum < 0)
                throw new ArgumentException("Student number must be positive", nameof(studentNum));

            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"DELETE FROM student WHERE student_number = {s_paramStudentNum} AND Deleted = 1";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(new SqlParameter(s_paramStudentNum, SqlDbType.Int) { Value = studentNum });

            connection.Open();
            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception($"Student {studentNum} not found for permanent deletion or is not marked as deleted");
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error permanently deleting student {studentNum}: {ex.Message}", ex);
            }
        }

        // Restore a soft-deleted student
        public void Restore(int studentNum)
        {
            if (studentNum < 0)
                throw new ArgumentException("Invalid student number", nameof(studentNum));

            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"UPDATE student SET Deleted = 0 WHERE student_number = {s_paramStudentNum} AND Deleted = 1";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(new SqlParameter(s_paramStudentNum, SqlDbType.Int) { Value = studentNum });

            connection.Open();
            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception($"Student {studentNum} not found for restoration or is not deleted");
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error restoring student {studentNum}: {ex.Message}", ex);
            }
        }

        // Helper method with size-specified parameter creation
        private static void AddParametersWithValues(SqlCommand cmd, Student student)
        {
            // Explicitly specify sizes for string parameters to avoid parameter sniffing issues
            cmd.Parameters.Add(new SqlParameter(s_paramStudentNum, SqlDbType.Int) { Value = student.StudentNum });
            cmd.Parameters.Add(new SqlParameter(s_paramRoomNum, SqlDbType.NVarChar, 50) { Value = student.RoomNum });
            cmd.Parameters.Add(new SqlParameter(s_paramFirstName, SqlDbType.NVarChar, 50) { Value = student.FirstName });
            cmd.Parameters.Add(new SqlParameter(s_paramLastName, SqlDbType.NVarChar, 50) { Value = student.LastName });
            cmd.Parameters.Add(new SqlParameter(s_paramTelNum, SqlDbType.NVarChar, 20) { Value = student.TelNum });
            cmd.Parameters.Add(new SqlParameter(s_paramClass, SqlDbType.NVarChar, 10) { Value = student.StudentClass });
        }
    }
}