using Npgsql;

namespace SqlChart;

public class PostgreSqlDatabaseConnector : IDatabaseConnector
{   
    NpgsqlDataSource dbSource;
    NpgsqlConnection dbConnection;

    public async void Connect(string connectionString)
    {
        if (IsConnected())
            return;

        dbSource = NpgsqlDataSource.Create(connectionString);
        dbConnection = await dbSource.OpenConnectionAsync();
    }

    public async void Disconnect()
    {
        if (!IsConnected())
            return;

        await dbConnection.CloseAsync();
        await dbConnection.DisposeAsync();
        await dbSource.DisposeAsync();

        dbConnection = null;
        dbSource = null;
    }
    
    public bool IsConnected()
    {
        return dbSource != null && dbConnection != null && dbConnection.State == System.Data.ConnectionState.Open;
    }

    public async Task<OrderedDictionary<string, List<object>>?> RunQuery(string query)
    {
        if (!IsConnected())
            return null;

        var results = new OrderedDictionary<string, List<object>>();

        using (NpgsqlCommand command = dbSource.CreateCommand(query)) 
        {
            using (NpgsqlDataReader reader = await command.ExecuteReaderAsync()) 
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
