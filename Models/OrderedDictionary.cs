namespace SqlChart;

public class OrderedDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
{
    List<TKey> OrderedKeys = new List<TKey>();

    public List<TKey> GetOrderedKeys() {
        return OrderedKeys;
    } 

    public void AddOrdered(TKey key, TValue value)
    {
        Add(key, value);
        OrderedKeys.Add(key);
    }
}
