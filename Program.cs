using Mono.Options;

namespace SqlChart;
class Program
{
    static async Task Main(string[] args)
    {
        IDatabaseConnector? databaseConnector = null;

        try
        {
            var options = new SqlChartOptions();
            var optionSet = new OptionSet
            {
                { "q|query=", "The SQL query to be executed", v => options.Query = v },
                { "d|db-type=", "Type of the database. Possible Values: MySQL, PostgreSQL, SQLServer, SQLite", 
                    v => options.DbType = Enum.TryParse(v, true, out SqlChartOptions.DatabaseType dbType) ? dbType : throw new ArgumentException("Invalid database type") },
                { "c|connection-string=", "The connection string to the database", v => options.ConnectionString = v },
                { "t|chart-type=", "Type of the chart. Possible Values: Bar, Line, Histogram", 
                    v => options.Chart = Enum.TryParse(v, true, out SqlChartOptions.ChartType chartType) ? chartType : throw new ArgumentException("Invalid chart type") },
                { "o|output=", "Output file path for the chart", v => options.Output = v },
                { "title=", "Title of the chart. Default Value is empty", v => options.Title = v },
                { "x-axis=", "Label for the X-axis. Default Value is empty", v => options.XAxis = v },
                { "y-axis=", "Label for the Y-axis. Default Value is empty", v => options.YAxis = v },
                { "width=", "Width of the chart in pixels. Default Value: 960", (int v) => options.Width = v },
                { "height=", "Height of the chart in pixels; Default Value: 540", (int v) => options.Height = v },
                { "color-scheme=", "Color scheme for the chart. Possible Values: Light, Dark. Default Value: Light",
                v => options.Color = Enum.TryParse(v, true, out SqlChartOptions.ColorScheme colorScheme) ? colorScheme : throw new ArgumentException("Invalid color scheme") },
                { "ds|db-server=", "Database server address", v => options.DbServer = v },
                { "dn|db-name=", "Database name", v => options.DbName = v },
                { "du|db-user=", "Database user name", v => options.DbUser = v },
                { "dpw|db-password=", "Database password", v => options.DbPassword = v },
                { "h|help",  "show this message and exit",  v => options.Help = v != null },
            };

            // parse the options
            optionSet.Parse(args);

            // Validate the options after parsing
            options.Validate(); 

            // show help
            if (options.Help)
                optionSet.WriteOptionDescriptions (Console.Out);

            // assign the database
            switch (options.DbType)
            {
                case SqlChartOptions.DatabaseType.MySQL: 
                    databaseConnector = new MySqlDatabaseConnector();
                    break;
                
                case SqlChartOptions.DatabaseType.PostgreSQL: 
                    databaseConnector = new PostgreSqlDatabaseConnector();
                    break;
                
                case SqlChartOptions.DatabaseType.SQLServer: 
                    databaseConnector = new SqlServerDatabaseConnector();
                    break;
                
                case SqlChartOptions.DatabaseType.SQLLite: 
                    databaseConnector = new SqlLiteDatabaseConnector();
                    break;
                
                default:
                    throw new ArgumentException("Invalid/ Unsupported SQL Database");
            }

            // connect to database
            databaseConnector.Connect(options.DbConnectionString);

            // create instace of chart
            Charter chart = new Charter(options);

            // get data from database
            OrderedDictionary<string, List<object>>? data = await databaseConnector.RunQuery(options.Query);

            if (data == null || data.Count == 0)
                throw new ArgumentException("The provided query didn't generate any output");
            
            chart.AddData(data);

            // discountect from database
            databaseConnector.Disconnect();

            // save chart picture
            chart.Save(options.Output, options.Width, options.Height);
        }
        catch (Exception e)
        {

            if (e.GetType() == typeof(AggregateException))
            {
                foreach (Exception innerException in ((AggregateException) e).InnerExceptions)
                {
                    Console.WriteLine(innerException.Message);
                }
            }
            else 
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.WriteLine("Try 'sqlchart --help' for more information.");

            if (databaseConnector != null)
                databaseConnector.Disconnect();

            return;
        }
    }
}