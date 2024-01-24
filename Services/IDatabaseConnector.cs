namespace SqlChart;

public interface IDatabaseConnector
{
    void Connect(string connectionString);
    void Disconnect();
    bool IsConnected();
    Task<OrderedDictionary<string, List<object>>?> RunQuery(string query);
}