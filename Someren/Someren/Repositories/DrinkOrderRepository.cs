using Someren.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;

namespace Someren.Repositories
{
    public sealed class DrinkOrderRepository : IDrinkOrderRepository
    {
        private readonly string? _connectionString;

        public DrinkOrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SomerenDB") ??
                throw new ArgumentNullException(nameof(configuration), "Database connection string cannot be null");
        }

        public void AddOrder(DrinkOrder order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "INSERT INTO purchases (student_id, drink_id, number_of_drinks) " +
                             "VALUES (@StudentId, @DrinkId, @NumberOfDrinks)";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.Int) { Value = order.StudentId });
            cmd.Parameters.Add(new SqlParameter("@DrinkId", SqlDbType.Int) { Value = order.DrinkId });
            cmd.Parameters.Add(new SqlParameter("@NumberOfDrinks", SqlDbType.Int) { Value = order.Quantity });

            connection.Open();
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error adding order: {ex.Message}", ex);
            }
        }

        public List<DrinkOrder> GetOrdersByStudent(int studentId)
        {
            var orders = new List<DrinkOrder>();
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "SELECT p.student_id, p.drink_id, p.number_of_drinks " +
                             "FROM purchases p " +
                             "WHERE p.student_id = @StudentId";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.Int) { Value = studentId });

            connection.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var order = new DrinkOrder(
                    reader.GetInt32(0), // student_id
                    reader.GetInt32(1), // drink_id
                    reader.GetInt32(2)  // number_of_drinks
                );

                orders.Add(order);
            }

            return orders;
        }
    }
}