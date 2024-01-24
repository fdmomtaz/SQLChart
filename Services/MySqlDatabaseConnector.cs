
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace SqlChart;

public class MySqlDatabaseConnector : IDatabaseConnector
{
    MySqlConnection? dbConnection;

    public async void Connect(string connectionString)
    {
        if (IsConnected())
            return;

        dbConnection = new MySqlConnection(connectionString);
        await dbConnection.OpenAsync();
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
        return dbConnection != null && dbConnection.State == System.Data.ConnectionState.Open;
    }

    public async Task<OrderedDictionary<string, List<object>>?> RunQuery(string query)
    {
        if (!IsConnected())
            return null;

        var results = new OrderedDictionary<string, List<object>>();

        using  (MySqlCommand command = new MySqlCommand(query, dbConnection))
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
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
