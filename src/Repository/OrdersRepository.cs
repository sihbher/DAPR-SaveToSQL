


using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.Serialization;
using DaprSaveToSql.Entities;

namespace DaprSaveToSql.Repository;

public class OrdersRepository
{
    private readonly string _connectionString;

    public OrdersRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> AddOrUpdate(Orders order)
    {
        return await SaveToDatabase(order) > 0;
    }



    private async Task<int> SaveToDatabase<T>(T entity)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var tableName = typeof(T).Name;
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                       .Where(p => !Attribute.IsDefined(p, typeof(IgnoreDataMemberAttribute)));
            var primaryKeys = GetPrimaryKeys(entity);

            var columnNames = string.Join(",", properties.Select(p => $"[{p.Name}]"));
            var parameterNames = string.Join(",", properties.Select(p => $"@{p.Name}"));

            var mergeSql = $@"MERGE [{tableName}] AS target
                              USING (VALUES ({parameterNames})) AS source ({columnNames})
                              ON {string.Join(" AND ", primaryKeys.Select(p => $"target.[{p.Name}] = source.[{p.Name}]"))}
                              WHEN MATCHED THEN
                                  UPDATE SET {string.Join(",", properties.Select(p => $"target.[{p.Name}] = source.[{p.Name}]"))}
                              WHEN NOT MATCHED THEN
                                  INSERT ({columnNames}) VALUES ({parameterNames});";

            var command = new SqlCommand(mergeSql, connection);

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                command.Parameters.AddWithValue($"@{property.Name}", value ?? DBNull.Value);
            }

            try
            {
                var rowsAffected =await command.ExecuteScalarAsync();
                return (int)rowsAffected;
            }
            catch (SqlException ex)
            {
                // Handle the SQL exception here
                throw new Exception("An error occurred while saving to the database.", ex);
            }
            catch (Exception ex)
            {
                // Handle other exceptions here
                throw new Exception("An error occurred while saving to the database.", ex);
            }
        }
    }

    private static IEnumerable<PropertyInfo> GetPrimaryKeys<T>(T entity)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                   .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute)));
        if (!properties.Any())
        {
            throw new ArgumentException($"Type {typeof(T).Name} does not define any properties with the KeyAttribute.");
        }
        return properties;
    }


}