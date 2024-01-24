
using System.Reflection.PortableExecutable;
using Microsoft.Identity.Client;
using ScottPlot;

namespace SqlChart;

public class Charter
{
    ScottPlot.Plot myPlot;
    SqlChartOptions.ChartType chartType;

    public Charter(SqlChartOptions options) {
        myPlot = new ScottPlot.Plot();

        myPlot.Axes.Left.Label.Text = options.YAxis;
        myPlot.Axes.Bottom.Label.Text = options.XAxis;
        myPlot.Axes.Title.Label.Text = options.Title;

        if (options.Color == SqlChartOptions.ColorScheme.Dark) {
            myPlot.Style.Background(figure: Color.FromHex("#07263b"), data: Color.FromHex("#0b3049"));
            myPlot.Style.ColorAxes(Color.FromHex("#a0acb5"));
            myPlot.Style.ColorGrids(Color.FromHex("#0e3d54"));
        }

        chartType = options.Chart.Value;
    }

    public void AddData(OrderedDictionary<string, List<object>> data)
    {
        // validate input data
        if (chartType == SqlChartOptions.ChartType.Line && data.Keys.Count % 2 != 0)
            throw new Exception("When creating a line chart the query should return column count that is divisible by two; Given the frist column is the X-value and second is the Y-value.");

        if (chartType == SqlChartOptions.ChartType.Histogram || chartType == SqlChartOptions.ChartType.Bar) {
            if (data.Keys.Count() > 1)
                throw new Exception("When creating a histogram chart the query should return one column.");
        }

        if (chartType == SqlChartOptions.ChartType.Bar) {
            foreach (string key in data.Keys) {
                foreach (object value in data[key]) {
                    if (value.GetType() != typeof(long) && value.GetType() != typeof(int) && value.GetType() != typeof(float) && value.GetType() != typeof(double))
                        throw new Exception("When creating a bar chart the query should return int/long or float/double values as they are the only values that can be graphed");
                }
            }
        }

        // plot line graph
        if (chartType == SqlChartOptions.ChartType.Line) {
            string[] columns = data.GetOrderedKeys().ToArray();

            for (int i = 0; i < columns.Count() ; i += 2) {
                if (data[columns[i]].Count() != data[columns[i+1]].Count())
                    throw new Exception("When plotting line graph the number of values for X and Y should match");

                int numVariables = data[columns[i]].Count();

                object[] xs = new object[numVariables];
                object[] ys = new object[numVariables];

                for(int j = 0; j < numVariables; j++) {
                    xs[j] = data[columns[i]][j];
                    ys[j] = data[columns[i+1]][j];
                }

                myPlot.Add.Scatter(xs, ys);
            } 
        }

        // plot bar graph
        if (chartType == SqlChartOptions.ChartType.Bar) {
            List<object> values = data.Values.ElementAt(0);
            double[] bars = new double[values.Count()];

            for (int i = 0; i < bars.Count(); i++) {
                bars[i] = Convert.ToDouble(values[i]);
            }

            myPlot.Add.Bars(bars);
        }
        
        // plot and calculate histogram graph
        if (chartType == SqlChartOptions.ChartType.Histogram) {
            OrderedDictionary<string, int> histogram =  new OrderedDictionary<string, int>();

            // group by the key
            List<object> values = data.Values.ElementAt(0);
            foreach(object value in values) {
                string valueKey =  value.ToString();

                if (!histogram.ContainsKey(valueKey))
                    histogram.AddOrdered(valueKey, 0);

                histogram[valueKey]++;
            }

            List<string> uniqueValues = histogram.GetOrderedKeys();
            Tick[] names = new Tick[uniqueValues.Count()];

            // populate the chart
            for(int i = 0; i < uniqueValues.Count(); i++) {
                names[i] = new Tick(i, uniqueValues[i]);
                myPlot.Add.Bar(position: i, value: histogram[uniqueValues[i]]);
            }

            myPlot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(names);
            myPlot.Axes.Bottom.MajorTickStyle.Length = 0;
        }
    }

    public void Save(string imagePath, int imageWidth, int imageHeight) {
        if (myPlot == null)
            return;

        myPlot.SavePng(imagePath, imageWidth, imageHeight);
    }

}
