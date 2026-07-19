using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class ADO : IADO
{
    private readonly string connectionString;
    public ADO(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("SqlServerConnection");
    }
    public async Task<List<T>> QueryAsync<T>(string sql, CommandType commandType, Func<SqlDataReader, T> mapper
                        , params SqlParameter[] parameters)
    {
        List<T> result = new List<T>();
        await using SqlConnection connection = new SqlConnection(connectionString);
        await using SqlCommand command = new SqlCommand(sql, connection);
        command.CommandType = commandType;

        if (parameters?.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(mapper(reader));
        }

        return result;
    }
}