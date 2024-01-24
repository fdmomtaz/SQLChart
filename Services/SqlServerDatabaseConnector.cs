using System.Data;
using Microsoft.Data.SqlClient;

namespace SqlChart;

public class SqlServerDatabaseConnector : IDatabaseConnector
{
    private SqlConnection dbConnection;

    public void Connect(string connectionString)
    {
        if (IsConnected())
            return;

        dbConnection = new SqlConnection(connectionString);
        dbConnection.Open();
    }

    public async void Disconnect()
    {
        if (!IsConnected())
            return;

        await dbConnection.CloseAsync();
        await dbConnection.DisposeAsync();

        dbConnection = null;
    }

    public bool IsConnected()
    {
        return dbConnection != null && dbConnection.State == ConnectionState.Open;
    }

    async Task<OrderedDictionary<string, List<object>>?> IDatabaseConnector.RunQuery(string query)
    {
        if (!IsConnected())
            return null;

        var results = new OrderedDictionary<string, List<object>>();

        using (SqlCommand command = new SqlCommand(query, dbConnection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (!results.ContainsKey(reader.GetName(i)))
                            results.AddOrdered(reader.GetName(i), new List<object>());

                        results[reader.GetName(i)].Add(reader.GetValue(i));
                    }                    
                }
            }
        }

        return results;
    }
}
