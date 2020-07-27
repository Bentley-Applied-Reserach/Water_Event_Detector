using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using System.Windows.Forms;
using System.IO;

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
                dt.Rows[i]["new remainder"] = dt.Rows[i]["remainder"];
                    // double.Parse(dt.Rows[i]["Value"].ToString()) - double.Parse(dt.Rows[i]["seasonal"].ToString()) - dt_median;
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
        public static void changeDateTimeFormat()
        {
            //open file 
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            string filePath = " ";
            if (result == DialogResult.OK)
            {
                filePath = ofd.FileName;
            }

            DataTable dt = new DataTable();
            dt = BasicFunction.read_csvfile(dt, filePath);
            dt.Columns.Add("Timestamp", typeof(DateTime));
            string[] allowedFormats = { "MM/dd/yyyy h:mm", "M/d/yyyy h:mm", "MM/d/yyyy h:mm", "M/dd/yyyy h:mm",
                                        "MM/dd/yyyy hh:mm", "M/d/yyyy hh:mm", "MM/d/yyyy hh:mm", "M/dd/yyyy hh:mm" };

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string tempTime = dt.Rows[i][0].ToString() + " " + dt.Rows[i][1].ToString();
                DateTime myDate = Convert.ToDateTime(tempTime); /*
                if (!DateTime.TryParseExact(tempTime, allowedFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out myDate))
                {
                    Console.WriteLine("not valid: " + tempTime);
                    myDate = Convert.ToDateTime(tempTime);
                }*/
                //var myDate = DateTime.ParseExact(tempTime, "MM/dd/yy h:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                dt.Rows[i]["Timestamp"] = myDate;
            }

            DataView dv = new DataView(dt);
            dt = dv.ToTable("Selected", false, "Timestamp", "Size", "Type", "Remarks", "Leak Category");
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|.csv";
            DialogResult save_result = sfd.ShowDialog();
            if (save_result == DialogResult.OK)
            {
                string path = sfd.FileName;
                BasicFunction.WriteToCsvFile(dt, path);
            }
        }
        public static void verifyPUBGroundTrue(double timegap)
        {
            //open file 
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            string filePath = " ";
            if (result == DialogResult.OK)
            {
                filePath = ofd.FileName;
            }

            //double timegap = 72;

            DataTable myEvents = new DataTable();
            myEvents = BasicFunction.read_csvfile(myEvents, filePath);
            myEvents.Columns.Add("Status", typeof(string));
            DataTable PUBEvents = new DataTable();
            string PUBPath = "D:\\Work\\WaterEventDetection\\data for cluster based detection\\Sensor Event\\Verify\\Ground True.csv";
            PUBEvents = BasicFunction.read_csvfile(PUBEvents, PUBPath);

            for (int i = 0; i < myEvents.Rows.Count; i++)
            {
                if(myEvents.Rows[i][3].ToString() != "") //start of a event [i]["Pressure Event"]
                {
                    DateTime myStartTime = Convert.ToDateTime(myEvents.Rows[i]["Timestamp"]);
                    for (int j = 0; j < PUBEvents.Rows.Count; j++)
                    {
                        DateTime PUBstartTime = Convert.ToDateTime(PUBEvents.Rows[j]["Timestamp"]);
                        if ((PUBstartTime - myStartTime).TotalHours <= timegap && (PUBstartTime - myStartTime).TotalHours > 0)
                        {
                            myEvents.Rows[i]["Status"] = "True Positive";
                            break;
                        }
                    }
                }
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|.csv";
            DialogResult save_result = sfd.ShowDialog();
            if (save_result == DialogResult.OK)
            {
                string path = sfd.FileName;
                BasicFunction.WriteToCsvFile(myEvents, path);
            }
        }
        public static void reverseVerify() //out-of-date
        {
            string[] myEventPath = {"D:\\Work\\WaterEventDetection\\data for cluster based detection\\Sensor Event\\Verify\\15Pressure.csv",
                                    "D:\\Work\\WaterEventDetection\\data for cluster based detection\\Sensor Event\\Verify\\30Pressure.csv",
                                    "D:\\Work\\WaterEventDetection\\data for cluster based detection\\Sensor Event\\Verify\\45Pressure.csv",
                                    "D:\\Work\\WaterEventDetection\\data for cluster based detection\\Sensor Event\\Verify\\60Pressure.csv" };

            string pubPath = "D:\\Work\\WaterEventDetection\\data for cluster based detection\\Sensor Event\\Verify\\Ground True.csv";
            double timegap = 24;

            DataTable pubEvents = new DataTable();
            pubEvents = BasicFunction.read_csvfile(pubEvents, pubPath);
            pubEvents.Columns.Add("15min", typeof(string));
            pubEvents.Columns.Add("30min", typeof(string));
            pubEvents.Columns.Add("45min", typeof(string));
            pubEvents.Columns.Add("60min", typeof(string));
            int index = 0;
            foreach (string path in myEventPath)
            {
                index++;
                DataTable myEvents = new DataTable();
                myEvents = BasicFunction.read_csvfile(myEvents, path);
                for (int i = 0; i < pubEvents.Rows.Count; i++)
                {
                    DateTime PUBstartTime = Convert.ToDateTime(pubEvents.Rows[i]["Timestamp"]);
                    for (int j = 0; j < myEvents.Rows.Count; j++)
                    {
                        if (myEvents.Rows[j][6].ToString() != "") //start of a event [i]["Pressure Event"]
                        {
                            DateTime myStartTime = Convert.ToDateTime(myEvents.Rows[j]["Timestamp"]);
                            if ((PUBstartTime - myStartTime).TotalHours <= timegap && (PUBstartTime - myStartTime).TotalHours > 0)
                            {
                                pubEvents.Rows[i][4 + index] = "identified";
                                break;
                            }
                        }
                    }
                }
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|.csv";
            DialogResult save_result = sfd.ShowDialog();
            if (save_result == DialogResult.OK)
            {
                string path = sfd.FileName;
                BasicFunction.WriteToCsvFile(pubEvents, path);
            }
        }
        /*
        private void ste_sys_event_identify_pre_bt_Click(object sender, EventArgs e)
        {
            bool pressure_high = ste_pre_high_cb.Checked;
            bool pressure_low = ste_pre_low_cb.Checked;
            bool flow_high = ste_flow_high_cb.Checked;
            bool flow_low = ste_flow_low_cb.Checked;
            string[] pressure_path = ste_pre_path_rtb.Lines;
            string flow_path = ste_flow_path_tb.Text;
            DataTable csvData = new DataTable();
            foreach (string path in pressure_path)
            {
                DataTable temp = new DataTable();
                temp = BasicFunction.read_csvfile(temp, path);
                DataView dv_pressure = new DataView(temp);
                string dv_pressure_filter = "duration <> '' AND Name <> ''";
                //dv_pressure.RowFilter = "Name <> ''";
                if (pressure_high == false)
                {
                    dv_pressure_filter = dv_pressure_filter + "AND Warning <> 'High'";
                }
                if (pressure_low == false)
                {
                    dv_pressure_filter = dv_pressure_filter + "AND Warning <> 'Low'";
                }
                dv_pressure.RowFilter = dv_pressure_filter;

                if (temp.Columns.Contains("out difference"))
                {
                    temp = dv_pressure.ToTable("Selected", false, "Timestamp", "Name", "Value", "Warning", "duration", "out difference");
                }
                else
                {
                    temp = dv_pressure.ToTable("Selected", false, "Timestamp", "Name", "Value", "Warning", "duration");
                }
                csvData.Merge(temp);
            }

            DataTable flow_event = new DataTable();
            flow_event = BasicFunction.read_csvfile(flow_event, flow_path);
            DataView dv_flow = new DataView(flow_event);
            string dv_flow_filter = "duration <> '' AND Name <> ''";
            if (flow_high == false)
            {
                dv_flow_filter = dv_flow_filter + "AND Warning <> 'High'";
            }
            if (flow_low == false)
            {
                dv_flow_filter = dv_flow_filter + "AND Warning <> 'Low'";
            }
            dv_flow.RowFilter = dv_flow_filter;
            flow_event = dv_flow.ToTable("Selected", false, "Timestamp", "Name", "Value", "Warning", "duration", "out difference");
            if (flow_event.Rows[0]["Name"].ToString() != "")
            {
                flow_sensor_name = flow_event.Rows[0]["Name"].ToString();
            }
            csvData.Merge(flow_event);

            DataView dv_sort = new DataView(csvData);
            dv_sort.Sort = "Timestamp";
            csvData = dv_sort.ToTable();

            //
            double timegap = Convert.ToDouble(ste_pre_only_timegap_tb.Text);
            csvData = SystemEvent.pressure_only_event(csvData, timegap);
            // csvData = SystemEvent.cluster_evetns(csvData, timegap);

            DataView dv_result = new DataView(csvData);
            //dv_result.RowFilter = "duration <> ''";
            csvData = dv_result.ToTable();
            ste_result_dgv.DataSource = csvData;

            int event_num = 0;
            if (csvData.Rows.Count == 0)
            {
                MessageBox.Show("Result is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //string flow_name = csvData.Rows[0]["Name"].ToString();
            string flow_name = flow_sensor_name;
            ste_result_dgv.Columns["Value"].DefaultCellStyle.Format = "N2";
            if (ste_result_dgv.Columns.Contains("out difference"))
            {
                ste_result_dgv.Columns["out difference"].DefaultCellStyle.Format = "N2";
            }
            //ste_result_dgv.Columns["out difference"].HeaderText = "Max out difference";

            //set datagridview format
            //row header width and alignment
            ste_result_dgv.RowHeadersWidth = 50;
            ste_result_dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //header context
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                if (csvData.Rows[i]["Name"].ToString() == flow_name)
                {
                    event_num++;
                    ste_result_dgv.Rows[i].HeaderCell.Value = event_num.ToString();
                }
            }
            //cell alignment
            ste_result_dgv.Columns["Timestamp"].Width = 140;
            ste_result_dgv.Columns["Name"].Width = 50;
            ste_result_dgv.Columns["Value"].Width = 50;
            ste_result_dgv.Columns["duration"].Width = 50;
            ste_result_dgv.Columns["Warning"].Width = 50;
            if (ste_result_dgv.Columns.Contains("out difference"))
            {
                ste_result_dgv.Columns["out difference"].Width = 70;
            }
            if (ste_result_dgv.Columns.Contains("Confidence level"))
            {
                ste_result_dgv.Columns["Confidence level"].Width = 70;
            }
            //ste_result_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|.csv";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = sfd.FileName;
                BasicFunction.WriteToCsvFile((DataTable)ste_result_dgv.DataSource, path);
            }
        }*/
        public static void reverseVerify2(double timegap)
        {
            string myEventPath = "";
                //"D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Verify\\event report.csv";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                myEventPath = ofd.FileName;
            }
            //string pubPath = "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Verify\\Ground True.csv"; //location of ground true verification file
            string pubPath = WaterEventDetector.ground_true_file_path;
            if (!File.Exists(pubPath))
            {
                pubPath = myEventPath.Replace(ofd.SafeFileName, "") + "Ground True.csv"; // if the ground true location is not in the above path, try to find it in the verify file folder
            }

            DataTable pubEvents = new DataTable();
            pubEvents = BasicFunction.read_csvfile(pubEvents, pubPath);
            pubEvents.Columns.Add("Status", typeof(string));
            DataTable myEvents = new DataTable();
            myEvents = BasicFunction.read_csvfile(myEvents, myEventPath);
            for (int i = 0; i < pubEvents.Rows.Count; i++)
            {
                DateTime PUBstartTime = Convert.ToDateTime(pubEvents.Rows[i]["Timestamp"]);
                for (int j = 0; j < myEvents.Rows.Count; j++)
                {
                    if (myEvents.Rows[j][3].ToString() != "") //start of a event [i]["Pressure Event"]
                    {
                        DateTime myStartTime = Convert.ToDateTime(myEvents.Rows[j]["Timestamp"]);
                        if ((PUBstartTime - myStartTime).TotalHours <= timegap && (PUBstartTime - myStartTime).TotalHours > 0)
                        {
                            pubEvents.Rows[i]["Status"] = "identified";
                            break;
                        }
                    }
                }
            }
            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|.csv";
            DialogResult save_result = sfd.ShowDialog();
            if (save_result == DialogResult.OK)
            {
                string path = sfd.FileName;
                BasicFunction.WriteToCsvFile(pubEvents, path);
            }
        }
        public static void culmative_event_score()
        {
            //open file 
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            string filePath = " ";
            if (result == DialogResult.OK)
            {
                filePath = ofd.FileName;
            }

            //double timegap = 72;

            DataTable myEvents = new DataTable();
            myEvents = BasicFunction.read_csvfile(myEvents, filePath);
            myEvents.Columns.Add("Culmative Score", typeof(string));

            DateTime currentTime = new DateTime();
            DateTime startTime = new DateTime();
            DateTime firstLineTime = Convert.ToDateTime(myEvents.Rows[0]["Timestamp"].ToString());
            for (int i = 0; i < myEvents.Rows.Count; i++)
            {
                if (myEvents.Rows[i]["Event Score"].ToString() != "")
                {
                    currentTime = Convert.ToDateTime(myEvents.Rows[i]["Timestamp"].ToString());
                    startTime = currentTime.AddDays(-2);
                    double culSocre = 0;
                    for (int j = i; j >= 0; j--)
                    {
                        DateTime temp = Convert.ToDateTime(myEvents.Rows[j]["Timestamp"].ToString());
                        if (myEvents.Rows[j]["Event Score"].ToString() != "")
                        {
                            culSocre = culSocre + Convert.ToDouble(myEvents.Rows[j]["Event Score"].ToString());
                        }                            
                        if ((temp - startTime).TotalMinutes <= 15 || (temp - firstLineTime).TotalMinutes <= 0)
                        {
                            myEvents.Rows[i]["Culmative Score"] = culSocre;
                            break;
                        }
                    }
                }
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|.csv";
            DialogResult save_result = sfd.ShowDialog();
            if (save_result == DialogResult.OK)
            {
                string path = sfd.FileName;
                BasicFunction.WriteToCsvFile(myEvents, path);
            }
        }
    }
}
