using System.Data;
using Microsoft.Data.SqlClient;

public interface IADO
{
    Task<List<T>> QueryAsync<T>(string sql, CommandType commandType, Func<SqlDataReader, T> mapper, params SqlParameter[] parameters);
}