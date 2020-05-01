using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace new_anom
{
    public static class BasicFunction
    {
        public static DataTable read_csvfile(DataTable csvData, string csv_filepath)
        {
            //DataTable csvData = new DataTable();
            using (TextFieldParser csvReader = new TextFieldParser(csv_filepath))
            {
                csvReader.SetDelimiters(new string[] { "," });
                csvReader.HasFieldsEnclosedInQuotes = true;
                string[] colFields = csvReader.ReadFields();

                foreach (string column in colFields)
                {
                    DataColumn datecolumn;
                    if (column == "Timestamp")
                    {
                        datecolumn = new DataColumn(column, typeof(DateTime));
                    }
                    //else if(column == "Value" ||
                    else if(column.Contains("Value") ||
                            column == "seasonal" ||
                            column == "remainder" ||
                            column == "new remainder" ||
                            column == "out difference" ||
                            column == "trend")
                    {
                        datecolumn = new DataColumn(column, typeof(double));
                    }
                    else
                    {
                        datecolumn = new DataColumn(column);
                    }
                    datecolumn.AllowDBNull = true;
                    csvData.Columns.Add(datecolumn);
                }

                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    //Making empty value as null
                    for (int i = 0; i < fieldData.Length; i++)
                    {
                        if (fieldData[i] == "")
                        {
                            fieldData[i] = null;
                        }
                    }

                    csvData.Rows.Add(fieldData);
                }
            }
            return csvData;
        }
        public static List<double> normal_filter(List<double> data, float coef)
        {
            double data_avg = data.Count > 0 ? data.Average() : 0.0;
            double data_std = data.Count > 0 ? StdDev(data) : 0.0;
            double upper = data_avg + coef * data_std;
            double lower = data_avg - coef * data_std;
            Console.WriteLine("avg: " + data_avg + ", std: " + data_std);
            data = data.Where(x => x < upper && x > lower).ToList();
            return data;
        }
        public static double StdDev(List<double> numberset)
        {
            double mean = numberset.Average();
            return Math.Sqrt(numberset.Sum(x => Math.Pow(x - mean, 2)) / numberset.Count);
        }
        public static DataTable h_stl(DataTable dt)
        {
            dt.Columns.Add("new remainder", typeof(Double));
            double[] dt_value = dt.AsEnumerable().Select(r => Convert.ToDouble(r.Field<double>("Value"))).ToArray();
            double dt_median = GetMedian(dt_value);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["new remainder"] =
                    double.Parse(dt.Rows[i]["Value"].ToString()) - double.Parse(dt.Rows[i]["seasonal"].ToString()) - dt_median;
            }
            return dt;
        }
        public static double GetMedian(double[] sourceNumbers)
        {
            //Framework 2.0 version of this method. there is an easier way in F4        
            if (sourceNumbers == null || sourceNumbers.Length == 0)
                throw new System.Exception("Median of empty array not defined.");

            //make sure the list is sorted, but use a new array
            double[] sortedPNumbers = (double[])sourceNumbers.Clone();
            Array.Sort(sortedPNumbers);

            //get the median
            int size = sortedPNumbers.Length;
            int mid = size / 2;
            double median = (size % 2 != 0) ? (double)sortedPNumbers[mid] : ((double)sortedPNumbers[mid] + (double)sortedPNumbers[mid - 1]) / 2;
            return median;
        }

        public static void WriteToCsvFile(this DataTable dataTable, string filePath)
        {
            StringBuilder fileContent = new StringBuilder();
            foreach (var col in dataTable.Columns)
            {
                fileContent.Append(col.ToString() + ",");
            }
            fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);

            foreach (DataRow dr in dataTable.Rows)
            {
                foreach (var column in dr.ItemArray)
                {
                    fileContent.Append("\"" + column.ToString() + "\",");
                }
                fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);
            }
            System.IO.File.WriteAllText(filePath, fileContent.ToString());
        }
        public static DataTable DailyAverageFlow(DataTable csvData)
        {
            DateTime start_time = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString());
            DateTime anchor_time = new DateTime(2000, 12, 22, 2, 0, 0);
            DateTime current_time;
            TimeSpan ts;
            TimeSpan ts_mnf;
            int steps_counter = 1;
            int mnf_steps_counter = 1;
            double value_acc = Convert.ToDouble(csvData.Rows[0]["Value"].ToString());
            double mnf_value_acc = Convert.ToDouble(csvData.Rows[0]["Value"].ToString());
            double daily_average = 0;
            double mini_night_flow = 0;
            bool is_night_hours = false;
            csvData.Columns.Add("Minimal night flow", typeof(double));
            csvData.Columns.Add("Daily Average", typeof(double));
            //csvData.Columns.Add("Minimal Night Flow", typeof(double));
            for(int i = 1; i < csvData.Rows.Count; i++)
            {
                current_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                ts = current_time - start_time;
                if(ts.TotalHours < 24)
                {
                    steps_counter++;
                    value_acc = value_acc + Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                    ts_mnf = current_time.TimeOfDay - anchor_time.TimeOfDay;
                    if (ts_mnf.TotalHours <= 2 && ts_mnf.TotalHours >= 0)
                    {
                        is_night_hours = true;
                        mnf_steps_counter++;
                        mnf_value_acc = mnf_value_acc + Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                    }
                    else
                    {
                        if(is_night_hours == true)
                        {
                            mini_night_flow = mnf_value_acc / mnf_steps_counter;/*
                            for (int j = i - 1; j > i - mnf_steps_counter; j--)
                            {
                                csvData.Rows[j]["Minimal night flow"] = mini_night_flow;
                            }*/
                            is_night_hours = false;
                        }
                        mnf_steps_counter = 1;
                        mnf_value_acc = Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                    }
                }
                else
                {
                    daily_average = value_acc / steps_counter;
                    for(int j = i - 1; j > i - steps_counter - 1; j--)
                    {
                        csvData.Rows[j]["Daily Average"] = daily_average;
                        csvData.Rows[j]["Minimal night flow"] = mini_night_flow;
                    }
                    steps_counter = 1;
                    value_acc = Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                    start_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                }
            }
            daily_average = value_acc / steps_counter;
            for (int j = csvData.Rows.Count - 1; j > csvData.Rows.Count - steps_counter - 1; j--)
            {
                csvData.Rows[j]["Daily Average"] = daily_average;
                csvData.Rows[j]["Minimal night flow"] = mini_night_flow;
            }
            return csvData;
        }

        public static DataTable MiniNightFlow(DataTable csvData)
        {//dont use this one
            DateTime start_time = new DateTime(2000, 12, 22, 2, 0, 0);
            //DateTime end_time = new DateTime(2000, 12, 22, 4, 0, 0);
            DateTime current_time;
            string night_flag = "off";
            TimeSpan ts;
            int steps_counter = 1;
            double value_acc = Convert.ToDouble(csvData.Rows[0]["Value"].ToString());
            double mini_night_flow = 0;
            csvData.Columns.Add("Minimal night flow", typeof(double));
            //csvData.Columns.Add("Minimal Night Flow", typeof(double));
            for (int i = 1; i < csvData.Rows.Count; i++)
            {
                current_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                ts = current_time.TimeOfDay - start_time.TimeOfDay;
                if (ts.TotalHours <= 2 && ts.TotalHours >= 0)
                {
                    steps_counter++;
                    value_acc = value_acc + Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                }
                else
                {
                    mini_night_flow = value_acc / steps_counter;
                    for (int j = i - 1; j > i - steps_counter; j--)
                    {
                        csvData.Rows[j]["Minimal night flow"] = mini_night_flow;
                    }
                    steps_counter = 1;
                    value_acc = Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                    start_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                }
            }
            return csvData;
        }

        public static DataTable get_unit(DataTable csvData)
        {
            foreach (DataColumn column in csvData.Columns)
            {
                if (column.ColumnName.Contains("."))  //Value.unit., read unit, R cant read "()" in column names and will change them into "."
                {
                    string unit = column.ColumnName.Split('.', '.')[1];
                    WaterEventDetector.current_unit = '(' + unit + ')';
                    //if flow unit
                    if (WaterEventDetector.flow_unit_set.Any(unit.Contains))
                    {
                        WaterEventDetector.flow_unit = unit;
                        column.ColumnName = column.ColumnName.Split('.', '.')[0];
                        //Console.WriteLine("flow unit: {0}", unit);
                        break;
                    }
                    //if pressure unit
                    else if (WaterEventDetector.pressure_unit_set.Any(unit.Contains))
                    {
                        WaterEventDetector.pressure_unit = unit;
                        column.ColumnName = column.ColumnName.Split('.', '.')[0];
                        //Console.WriteLine("pressure unit: {0}", unit);
                        break;
                    }
                }
                else if (column.ColumnName.Contains("("))  //Value(unit), read unit
                {
                    string unit = column.ColumnName.Split('(', ')')[1];
                    WaterEventDetector.current_unit = '(' + unit + ')';
                    //if flow unit
                    if (WaterEventDetector.flow_unit_set.Any(unit.Contains))
                    {
                        WaterEventDetector.flow_unit = unit;
                        column.ColumnName = column.ColumnName.Split('(', ')')[0];
                        //Console.WriteLine("flow unit: {0}", unit);
                        break;
                    }
                    //if pressure unit
                    else if (WaterEventDetector.pressure_unit_set.Any(unit.Contains))
                    {
                        WaterEventDetector.pressure_unit = unit;
                        column.ColumnName = column.ColumnName.Split('(', ')')[0];
                        //Console.WriteLine("pressure unit: {0}", unit);
                        break;
                    }
                }
            }
            return csvData;
        }
        public static DataTable delete_unit(DataTable csvData)
        {
            foreach (DataColumn column in csvData.Columns)
            {
                if (column.ColumnName.Contains("."))  //Value.unit., read unit, R cant read "()" in column names and will change them into "."
                {
                    column.ColumnName = column.ColumnName.Split('.', '.')[0];
                }
                else if (column.ColumnName.Contains("("))  //Value(unit), read unit
                {
                    column.ColumnName = column.ColumnName.Split('(', ')')[0];
                }
            }
            return csvData;
        }
        public static string Get_confidence_level(int score)
        {
            string level;
            switch (score)
            {
                case 1:
                    level = "Very low";
                    break;
                case 2:
                    level = "Low";
                    break;
                case 3:
                    level = "Medium";
                    break;
                case 4:
                    level = "High";
                    break;
                case 5:
                    level = "Very high";
                    break;
                default:
                    level = "Very low";
                    break;
            }
            return level;
        }
    }
}
