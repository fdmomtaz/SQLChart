using System.Text;

namespace SqlChart;

public class SqlChartOptions
{
    public enum DatabaseType
    {
        MySQL,
        PostgreSQL,
        SQLServer,
        SQLLite
    }

    public enum ColorScheme {
        Light, 
        Dark
    }

    public enum ChartType
    {
        Bar,
        Line,
        Histogram
    }

    String[,] connectionStringPart = {
        {"server=", "database=", "uid=", "pwd="},
        {"Server=", "Database=", "User Id=", "Password="},
        {"Server=", "Database=", "User Id=", "Password="},
        {"Data Source=", "", "", "Password="}
    };

    public string? Query { get; set; }
    public DatabaseType? DbType { get; set; }
    public string? ConnectionString { get; set; }
    public ChartType? Chart { get; set; }
    public string? Output { get; set; }
    public string Title { get; set; } = "";
    public string XAxis { get; set; } = "";
    public string YAxis { get; set; } = "";
    public int Width { get; set; } = 960;
    public int Height { get; set; } = 540;
    public ColorScheme Color { get; set; } = ColorScheme.Light;
    public string DbServer { get; set; }
    public string DbName { get; set; }
    public string DbUser { get; set; }
    public string DbPassword { get; set; }
    public bool Help { get; set; }

    public string DbConnectionString 
    { 
        get {
            if (!string.IsNullOrWhiteSpace(ConnectionString))
                return ConnectionString;

            if (DbType == null)
                return string.Empty;

            StringBuilder cs = new StringBuilder();

            cs.AppendFormat("{0}{1}; ", connectionStringPart[(int)DbType, 0], DbServer);

            if (!string.IsNullOrWhiteSpace(DbName))
                cs.AppendFormat("{0}{1}; ", connectionStringPart[(int)DbType, 0], DbName);

            if (!string.IsNullOrWhiteSpace(DbUser))
                cs.AppendFormat("{0}{1}; ", connectionStringPart[(int)DbType, 0], DbUser);

            if (!string.IsNullOrWhiteSpace(DbPassword))
                cs.AppendFormat("{0}{1}; ", connectionStringPart[(int)DbType, 0], DbPassword);

            return cs.ToString();
        }
    }

    public void Validate()
    {
        var exceptions = new List<Exception>();

        // Validate Query
        if (string.IsNullOrWhiteSpace(Query))
            exceptions.Add(new ArgumentException("Query is required."));

        // Validate Database Connection
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            if (string.IsNullOrWhiteSpace(DbServer))
                exceptions.Add(new ArgumentException("Database server is required when connection string is not provided."));

            if (string.IsNullOrWhiteSpace(DbName) && DbType != DatabaseType.SQLLite)
                exceptions.Add(new ArgumentException("Database name is required when connection string is not provided."));
        }

        // Validate Chart Type
        if (Chart == null)
            exceptions.Add(new ArgumentException("Chart type is required."));
            
        // Validate Chart Type
        if (DbType == null)
            exceptions.Add(new ArgumentException("Database type is required."));

        // Validate Output Path
        if (string.IsNullOrWhiteSpace(Output))
            exceptions.Add(new ArgumentException("Output file path is required."));
            
        if (File.Exists(Output))
            exceptions.Add(new ArgumentException("Output file already exists."));

        // Validate Dimensions
        if (Width <= 0)
            exceptions.Add(new ArgumentException("Width must be a positive number."));

        if (Height <= 0)
            exceptions.Add(new ArgumentException("Height must be a positive number."));

        if (exceptions.Count > 0)
            throw new AggregateException("Was unable to parse the input options", exceptions);
    }
}
