using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace new_anom
{
    public static class DataPreProcess
    {
        //public static (DataTable detail_dt, DataTable summary_dt) checking_data(string filepath)
        public static (DataTable detail_dt, DataTable summary_dt) checking_data(DataTable csvData, 
                                                                                double max_value, 
                                                                                double min_value, 
                                                                                int min_step)
        {
            //DataTable csvData = new DataTable();
            //csvData = BasicFunction.read_csvfile(csvData, filepath);
            DataTable result_table = csvData.Clone();
            result_table.Columns.Add("Error", typeof(String));
            DataTable summary_dt = new DataTable();

            summary_dt.Columns.Add("Items", typeof(string));
            summary_dt.Columns.Add("Value", typeof(string));
            int missing_num = 0;
            int duplicated_num = 0;
            int irregular_num = 0;
            int sensor_failure = 0;

            DateTime dt1 = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
            DateTime dt2 = Convert.ToDateTime(csvData.Rows[2]["Timestamp"].ToString());
            TimeSpan time_gap = dt2 - dt1;
            DateTime last_time = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString()); ;
            DateTime this_time;
            double last_value = Convert.ToDouble(csvData.Rows[0]["Value"].ToString());
            double this_value;

            for(int i = 1; i < csvData.Rows.Count; i++)
            {
                this_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                if(csvData.Rows[i]["Value"].ToString() == "")
                {
                    csvData.Rows[i]["Value"] = 0;
                }
                this_value = Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                if (this_value == last_value || this_value >= max_value || this_value <= min_value) 
                //if (Math.Abs(this_value - last_value) <= 0.00001 || this_value >= max_value || this_value <= min_value)
                {
                    //sensor failure
                    sensor_failure++;
                    DataRow newrow = result_table.NewRow();
                    newrow["Timestamp"] = this_time;
                    newrow["Value"] = csvData.Rows[i]["Value"].ToString();
                    newrow["Error"] = "Sensor failure";
                    result_table.Rows.Add(newrow);
                }
                if (this_time == last_time + time_gap)
                {
                    last_time = this_time;
                }
                else if(this_time == last_time)//duplicated
                {
                    //Console.WriteLine("dulplicated: i = {0}", i);
                    duplicated_num++;
                    DataRow newrow = result_table.NewRow();
                    newrow["Timestamp"] = this_time;
                    newrow["Value"] = csvData.Rows[i]["Value"].ToString();
                    newrow["Error"] = "Duplicated";
                    result_table.Rows.Add(newrow);
                }
                else if(last_time + time_gap > this_time)//non-periodic(irregular)
                {
                    //Console.WriteLine("non-periodic: i = {0}", i);
                    irregular_num++;
                    DataRow newrow = result_table.NewRow();
                    newrow["Timestamp"] = this_time;
                    newrow["Value"] = csvData.Rows[i]["Value"].ToString();
                    newrow["Error"] = "Irregular";
                    result_table.Rows.Add(newrow);
                    last_time = last_time + time_gap;
                }
                else if(last_time + time_gap < this_time)//missing
                {
                    int missing_count = 1;
                    //Console.WriteLine("missing: i = {0}", i);
                    while(last_time + time_gap < this_time)
                    {
                        //Console.WriteLine("Counting: {0}", missing_num);
                        last_time = last_time + time_gap;
                        DataRow newrow = result_table.NewRow();
                        newrow["Timestamp"] = last_time;
                        newrow["Value"] = Double.NaN;
                        newrow["Error"] = "Missing";
                        result_table.Rows.Add(newrow);
                        missing_count++;
                        missing_num++;
                    }
                    last_time = last_time + time_gap;
                }
                last_value = this_value;
            }
            int data_count = csvData.Rows.Count;
            DataRow smy_row1 = summary_dt.NewRow();
            smy_row1["Items"] = "Start time";
            smy_row1["Value"] = csvData.Rows[0]["Timestamp"].ToString();
            summary_dt.Rows.Add(smy_row1);
            DataRow smy_row2 = summary_dt.NewRow();
            smy_row2["Items"] = "End time";
            smy_row2["Value"] = csvData.Rows[data_count - 1]["Timestamp"].ToString();
            summary_dt.Rows.Add(smy_row2);
            DataRow smy_row3 = summary_dt.NewRow();
            smy_row3["Items"] = "Duplicated time step";
            smy_row3["Value"] = duplicated_num.ToString();
            summary_dt.Rows.Add(smy_row3);
            DataRow smy_row4 = summary_dt.NewRow();
            smy_row4["Items"] = "Irregular time step";
            smy_row4["Value"] = irregular_num.ToString();
            summary_dt.Rows.Add(smy_row4);
            DataRow smy_row5 = summary_dt.NewRow();
            smy_row5["Items"] = "Missing time step";
            smy_row5["Value"] = missing_num.ToString();
            summary_dt.Rows.Add(smy_row5);
            DataRow smy_row6 = summary_dt.NewRow();
            smy_row6["Items"] = "Sensor failure time step";
            smy_row6["Value"] = sensor_failure.ToString();
            summary_dt.Rows.Add(smy_row6);
            DataRow smy_row7 = summary_dt.NewRow();
            smy_row7["Items"] = "Total time steps";
            smy_row7["Value"] = data_count.ToString();
            summary_dt.Rows.Add(smy_row7);
            return (result_table, summary_dt);
        }

        public static DataTable correct_data(DataTable csvData, 
                                             double max_value,
                                             double min_value,
                                             int min_step)
        {
            //DataTable csvData = new DataTable();
            //csvData = BasicFunction.read_csvfile(csvData, filepath);
            DataTable result_table = csvData.Clone();
            DateTime dt1 = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
            DateTime dt2 = Convert.ToDateTime(csvData.Rows[2]["Timestamp"].ToString());
            TimeSpan time_gap = dt2 - dt1;
            double period = 1440 / time_gap.TotalMinutes;  //how many data points per day. 24*60/time_step_in_minutes

            DateTime last_time = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString()); ;
            DateTime this_time;
            double last_value = Convert.ToDouble(csvData.Rows[0]["Value"].ToString());
            double this_value;
            int sensor_failure = 0;
            int first_failure = 0;

            DataRow firstrow = result_table.NewRow();
            firstrow["Timestamp"] = last_time;
            firstrow["Value"] = csvData.Rows[0]["Value"].ToString();
            result_table.Rows.Add(firstrow);

            for (int i = 1; i < csvData.Rows.Count; i++)
            {
                this_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                if (csvData.Rows[i]["Value"].ToString() == "")
                {
                    csvData.Rows[i]["Value"] = 0;
                }
                this_value = Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                if (this_time == last_time + time_gap)
                {
                    last_time = this_time;
                    DataRow newrow = result_table.NewRow();
                    newrow["Timestamp"] = this_time;
                    newrow["Value"] = csvData.Rows[i]["Value"].ToString();
                    result_table.Rows.Add(newrow);
                }
                else if (this_time == last_time)//duplicated
                {
                    continue;
                }
                else if (last_time + time_gap > this_time)//non-periodic(irregular)
                {
                    last_time = last_time + time_gap;
                    Console.WriteLine("non-periodic: i = {0}", i);
                    DataRow newrow = result_table.NewRow();
                    newrow["Timestamp"] = last_time;
                    newrow["Value"] = csvData.Rows[i]["Value"].ToString();
                    result_table.Rows.Add(newrow);
                }
                else if (last_time + time_gap < this_time)//missing
                {
                    sensor_failure = 0;
                    int missing_num = 1;
                    Console.WriteLine("missing: i = {0}", i);
                    while (last_time + time_gap < this_time)
                    {
                        //Console.WriteLine("Counting: {0}", missing_num);
                        last_time = last_time + time_gap;
                        DataRow newrow = result_table.NewRow();
                        newrow["Timestamp"] = last_time;
                        //newrow["Value"] = csvData.Rows[i - (int)period + missing_num - 1]["Value"].ToString();//set missing value equale to last period
                        newrow["Value"] = result_table.Rows[i - (int)period + missing_num - 1]["Value"].ToString();
                        result_table.Rows.Add(newrow);
                        missing_num++;
                    }
                    last_time = this_time;
                    DataRow this_row = result_table.NewRow();
                    this_row["Timestamp"] = this_time;
                    this_row["Value"] = csvData.Rows[i]["Value"].ToString();
                    result_table.Rows.Add(this_row);
                }
                last_value = this_value;
            }
            
            
            DataTable new_result = result_table.Clone();
            int sensor_failure2 = 0;
            int first_failure2 = 0;
            for (int i = 1; i < result_table.Rows.Count; i++)
            {
                //this_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                this_value = Convert.ToDouble(result_table.Rows[i]["Value"].ToString());
                last_value = Convert.ToDouble(result_table.Rows[i - 1]["Value"].ToString());
                if (this_value == last_value && this_value < max_value && this_value > min_value)
                //if (Math.Abs(this_value - last_value) <= 0.00001 && this_value < max_value && this_value > min_value)
                {
                    sensor_failure2++;
                    if (sensor_failure2 == 1)
                    {
                        //first failure
                        first_failure2 = i;
                    }
                    else if (sensor_failure2 >= min_step)
                    //if (sensor_failure2 >= min_step)
                    {
                        i = first_failure2 + (int)period - 1;
                        sensor_failure2 = 0;
                        for (int j = 0; j < min_step - 1; j++)
                        {
                            new_result.Rows[new_result.Rows.Count - 1].Delete();
                        }
                        continue;
                    }
                }
                else
                {
                    sensor_failure2 = 0;
                }
                if (this_value >= max_value || this_value <= min_value)
                {
                    sensor_failure++;
                    result_table.Rows[i]["Value"] = result_table.Rows[i % (int)period]["Value"];
                    if (sensor_failure == 1)
                    {
                        //first failure
                        first_failure = i;
                    }
                    else if (sensor_failure >= min_step)
                    {
                        i = first_failure + (int)period - 1;
                        sensor_failure = 0;
                        for(int j = 0; j < min_step - 1; j++)
                        {
                            new_result.Rows[new_result.Rows.Count - 1].Delete();
                        }
                        continue;
                    }
                }
                else
                {
                    sensor_failure = 0;
                }
                new_result.ImportRow(result_table.Rows[i]);
            }
        return new_result;
        //return result_table;
        }
    }
}
