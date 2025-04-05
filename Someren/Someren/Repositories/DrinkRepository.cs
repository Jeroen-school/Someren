using Someren.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;

namespace Someren.Repositories
{
    public sealed class DrinkRepository : IDrinkRepository
    {
        private static readonly string s_baseColumns = "D.[drink_id], D.[drink_type], D.[amount_consumed], D.[stock], D.[price], D.[vat_rate]";
        private static readonly string s_baseSelectQuery = $"SELECT {s_baseColumns} FROM type_of_drink AS D";

        private readonly string? _connectionString;

        public DrinkRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SomerenDB") ??
                throw new ArgumentNullException(nameof(configuration), "Database connection string cannot be null");
        }

        private static Drink ReadDrink(SqlDataReader reader)
        {
            return new Drink(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetDecimal(4),
                reader.GetInt32(5)
            );
        }

        public List<Drink> GetAll()
        {
            var drinks = new List<Drink>();
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"{s_baseSelectQuery} ORDER BY D.drink_type";
            cmd.CommandType = CommandType.Text;

            connection.Open();
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

            while (reader.Read())
            {
                drinks.Add(ReadDrink(reader));
            }

            return drinks;
        }

        public Drink GetById(int drinkId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"{s_baseSelectQuery} WHERE D.drink_id = @DrinkId";
            cmd.CommandType = CommandType.Text;

            var param = new SqlParameter("@DrinkId", SqlDbType.Int) { Value = drinkId };
            cmd.Parameters.Add(param);

            connection.Open();
            using var reader = cmd.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SequentialAccess);

            return reader.Read() ? ReadDrink(reader) : null;
        }

        public void UpdateStock(int drinkId, int numberOfDrinks)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            cmd.CommandText = "UPDATE type_of_drink SET amount_consumed = amount_consumed + @NumberOfDrinks, " +
                             "stock = stock - @NumberOfDrinks WHERE drink_id = @DrinkId";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(new SqlParameter("@DrinkId", SqlDbType.Int) { Value = drinkId });
            cmd.Parameters.Add(new SqlParameter("@NumberOfDrinks", SqlDbType.Int) { Value = numberOfDrinks });

            connection.Open();
            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception($"Drink with ID {drinkId} not found for stock update");
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error updating drink stock for drink {drinkId}: {ex.Message}", ex);
            }
        }
    }
}