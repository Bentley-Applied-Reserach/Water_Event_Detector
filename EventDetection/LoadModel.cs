using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace new_anom
{
    class LoadModel
    {
        public static DataTable Data_pre_process(DataTable csvData,
                                                  bool missing_ts,
                                                  bool duplicated_ts,
                                                  bool irregular_ts,
                                                  bool sensor_failure,
                                                  int min_step,
                                                  double min_value,
                                                  double max_value)
        {
            DataTable result_table = csvData.Clone();
            int sensor_failure_num = 0;
            int first_failure = 0;
            double current_value = Convert.ToDouble(csvData.Rows[0]["Value"]);
            if(current_value < max_value && current_value > min_value)
            {
                result_table.ImportRow(csvData.Rows[0]);
            }
            for (int i = 1; i < csvData.Rows.Count; i++)
            {
                DateTime current_date = Convert.ToDateTime(csvData.Rows[i]["Timestamp"]);
                DateTime last_date = Convert.ToDateTime(csvData.Rows[i - 1]["Timestamp"]);
                current_value = Convert.ToDouble(csvData.Rows[i]["Value"]);
                double last_value = Convert.ToDouble(csvData.Rows[i - 1]["Value"]);
                if (last_date == current_date || current_value >= max_value || current_value <= min_value) //check duplicated time step and extreme value
                {
                    csvData.Rows[i].Delete();
                    continue;
                }
                if(current_value == last_value)
                {
                    sensor_failure_num++;
                    if(sensor_failure_num == 1)
                    {//first failure
                        first_failure = i;
                    }
                    else if(sensor_failure_num >= min_step)
                    {
                        continue;
                    }
                }
                else
                {
                    sensor_failure_num = 0;
                }
                result_table.ImportRow(csvData.Rows[i]);
            }
            return result_table;
        }
        public static DataTable Decompose(DataTable csvData, DataTable seasonal_dt, double median, double timegap)
        {
            csvData.Columns.Add("new remainder", typeof(Double));
            int j = 0;
            for(int i = 0; i <csvData.Rows.Count; i++)
            {
                DateTime data_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                //if (data_time.Year == 2016 && data_time.Month == 9 && data_time.Day == 21 && data_time.Hour == 2 && data_time.Minute == 10)
                for(int k = 0; k < seasonal_dt.Rows.Count; k++)
                //while (j < seasonal_dt.Rows.Count)
                {
                    if (j >= seasonal_dt.Rows.Count)
                    {
                        j = 0;
                    }
                    DateTime seasonal_time = Convert.ToDateTime(seasonal_dt.Rows[j]["Timestamp"].ToString());
                    if(data_time.TimeOfDay == seasonal_time.TimeOfDay || (data_time.TimeOfDay - seasonal_time.TimeOfDay).TotalMinutes <= timegap / 2)
                    {
                        double value = Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                        double seasonal = Convert.ToDouble(seasonal_dt.Rows[j]["seasonal"].ToString());
                        csvData.Rows[i]["new remainder"] = value - seasonal - median;
                        j++;
                        break;
                    }
                    j++;
                }
            }
            return csvData;
        }
        public static DataTable XbarOutlier(DataTable csvData,
                                            bool nmfilter,
                                            double nmfilter_coef,
                                            double xbar_coef,
                                            double timegap,
                                            double flow_remainder_mean,
                                            double flow_remainder_std)
        {
            /*List<double> filtered_flow_remainder =
                csvData.AsEnumerable().Select(r => r.Field<double>("new remainder")).ToList();
            if (nmfilter == true)
            {
                filtered_flow_remainder = BasicFunction.normal_filter(filtered_flow_remainder, (float)nmfilter_coef);
            }

            double flow_remainder_mean = filtered_flow_remainder.Average();
            double flow_remainder_std = BasicFunction.StdDev(filtered_flow_remainder);
            Console.WriteLine("mean: " + flow_remainder_mean + ", Std: " + flow_remainder_std);*/

            double upper_lmt = flow_remainder_mean + xbar_coef * flow_remainder_std;
            double lower_lmt = flow_remainder_mean - 1.5 * xbar_coef * flow_remainder_std;
            Console.WriteLine("upper limit: " + upper_lmt + ", lower limit: " + lower_lmt);

            csvData.Columns.Add("out difference", typeof(Double));
            csvData.Columns.Add("duration", typeof(string));
            csvData.Columns.Add("Warning", typeof(String));
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                if (Convert.ToDouble(csvData.Rows[i]["new remainder"]) > upper_lmt)
                {
                    csvData.Rows[i]["out difference"] = Convert.ToDouble(csvData.Rows[i]["new remainder"]) - upper_lmt;
                    csvData.Rows[i]["Warning"] = "High";
                    csvData.Rows[i]["duration"] = timegap;
                }
                else if (Convert.ToDouble(csvData.Rows[i]["new remainder"]) < lower_lmt)
                {
                    csvData.Rows[i]["out difference"] = Convert.ToDouble(csvData.Rows[i]["new remainder"]) - lower_lmt;
                    csvData.Rows[i]["Warning"] = "Low";
                    csvData.Rows[i]["duration"] = timegap;
                }
            }
            // BasicFunction.WriteToCsvFile(csvData, "D:\\Work\\test result\\nysr_model_outlier.csv");
            return csvData;
        }
        public static DataTable EWMA_Outlier(DataTable csvData,
                                             bool nmfilter,
                                             double nmfiler_coef,
                                             double ewma_coef,
                                             double lambda,
                                             double timegap,
                                            double flow_remainder_mean,
                                            double flow_remainder_std)
        {/*
            List<double> filtered_flow_remainder =
                csvData.AsEnumerable().Select(r => r.Field<double>("new remainder")).ToList();
            //csvData.AsEnumerable().Select(r => Convert.ToDouble(r.Field<string>("new remainder"))).ToList();
            if (nmfilter == true)
            {
                filtered_flow_remainder = BasicFunction.normal_filter(filtered_flow_remainder, (float)nmfiler_coef);
            }

            double flow_remainder_mean = filtered_flow_remainder.Average();
            double flow_remainder_std = BasicFunction.StdDev(filtered_flow_remainder);*/

            double poly = Math.Pow(lambda / (2 - lambda), 0.5);
            double upper_lmt = flow_remainder_mean + ewma_coef * flow_remainder_std * poly;
            double lower_lmt = flow_remainder_mean - 1.2 * ewma_coef * flow_remainder_std * poly;

            csvData.Columns.Add("duration", typeof(string));
            csvData.Columns.Add("Warning", typeof(String));
            csvData.Columns.Add("EWMA", typeof(Double));

            double last_zeta;
            double current_value = Convert.ToDouble(csvData.Rows[0]["new remainder"]);
            csvData.Rows[0]["EWMA"] = current_value;
            if (current_value > upper_lmt)
            {
                csvData.Rows[0]["Warning"] = "High";
                csvData.Rows[0]["duration"] = timegap;
            }
            else if (current_value < lower_lmt)
            {
                csvData.Rows[0]["Warning"] = "Low";
                csvData.Rows[0]["duration"] = timegap;
            }

            for (int i = 1; i < csvData.Rows.Count; i++)
            {
                last_zeta = Convert.ToDouble(csvData.Rows[i - 1]["EWMA"]);
                current_value = Convert.ToDouble(csvData.Rows[i]["new remainder"]);
                csvData.Rows[i]["EWMA"] = lambda * current_value + (1 - lambda) * last_zeta;
                if (Convert.ToDouble(csvData.Rows[i]["EWMA"]) > upper_lmt)
                {
                    csvData.Rows[i]["Warning"] = "High";
                    csvData.Rows[i]["duration"] = timegap;
                }
                else if (Convert.ToDouble(csvData.Rows[i]["new remainder"]) < lower_lmt)
                {
                    csvData.Rows[i]["Warning"] = "Low";
                    csvData.Rows[i]["duration"] = timegap;
                }
            }
            return csvData;
        }
        public static DataTable Cusum_Outlier(DataTable csvData,
                                              bool nmfilter,
                                              double nmfilter_coef,
                                              double cusum_coef,
                                              double time_gap,
                                            double flow_remainder_mean,
                                            double flow_remainder_std)
        {/*
            List<double> filtered_flow_remainder =
                csvData.AsEnumerable().Select(r => r.Field<double>("new remainder")).ToList();
            //csvData.AsEnumerable().Select(r => Convert.ToDouble(r.Field<string>("new remainder"))).ToList();
            if (nmfilter == true)
            {
                filtered_flow_remainder = BasicFunction.normal_filter(filtered_flow_remainder, (float)nmfilter_coef);
            }

            double flow_remainder_mean = filtered_flow_remainder.Average();
            double flow_remainder_std = BasicFunction.StdDev(filtered_flow_remainder);*/

            double upper_lmt = flow_remainder_mean + cusum_coef * flow_remainder_std;
            double lower_lmt = flow_remainder_mean - 1.5 * cusum_coef * flow_remainder_std;

            csvData.Columns.Add("duration", typeof(string));
            csvData.Columns.Add("Warning", typeof(String));
            csvData.Columns.Add("SH", typeof(Double));
            csvData.Columns.Add("SL", typeof(Double));

            double current_value = Convert.ToDouble(csvData.Rows[0]["new remainder"]);
            double sh_i = Math.Max(0, current_value - upper_lmt);
            double sl_i = Math.Min(0, current_value - lower_lmt);
            csvData.Rows[0]["SH"] = sh_i;
            csvData.Rows[0]["SL"] = sl_i;
            double upper_diff = sh_i > upper_lmt ? sh_i - upper_lmt : 0;
            double lower_diff = sl_i < lower_lmt ? lower_lmt - sl_i : 0;
            if (lower_diff != 0 || upper_diff != 0)
            {
                csvData.Rows[0]["Warning"] = upper_diff > lower_diff ? "High" : "Low";
                csvData.Rows[0]["duration"] = time_gap;
            }


            for (int i = 1; i < csvData.Rows.Count; i++)
            {
                current_value = Convert.ToDouble(csvData.Rows[i]["new remainder"]);
                sh_i = Math.Max(0, Convert.ToDouble(csvData.Rows[i - 1]["SH"]) + current_value - upper_lmt);
                sl_i = Math.Min(0, Convert.ToDouble(csvData.Rows[i - 1]["SL"]) + current_value - lower_lmt);
                csvData.Rows[i]["SH"] = sh_i;
                csvData.Rows[i]["SL"] = sl_i;
                upper_diff = sh_i > upper_lmt ? sh_i - upper_lmt : 0;
                lower_diff = sl_i < lower_lmt ? lower_lmt - sl_i : 0;
                if (lower_diff != 0 || upper_diff != 0)
                {
                    csvData.Rows[i]["Warning"] = upper_diff > lower_diff ? "High" : "Low";
                    csvData.Rows[i]["duration"] = time_gap;
                }
            }
            return csvData;
        }
        public static DataTable Sensor_Event(DataTable csvData,
                                             string sensor_name,
                                             double event_timegap,
                                             double min_event_duration)
        {
            double gap_coef;  //if this_event_end_time + (gap_coef * timegap) (in Minutes) > next_event_start_time, then these 2 are one event.
            DateTime first_row = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
            DateTime second_row = Convert.ToDateTime(csvData.Rows[2]["Timestamp"].ToString());
            TimeSpan ts = second_row - first_row;
            gap_coef = event_timegap / ts.TotalMinutes;

            int event_start = 0;
            int event_end = 0;
            csvData.Columns.Add("Name", typeof(String));
            string last_warning = null;
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                if (csvData.Rows[i]["Warning"].ToString() != "")
                {
                    event_start = i == 0 ? i + 1 : i;
                    event_end = i - (int)gap_coef - 2 >= 0 ? i - (int)gap_coef - 2 : 0;
                    last_warning = csvData.Rows[i]["Warning"].ToString();
                    break;
                }
            }
            if (csvData.Columns.Contains("out difference"))
            {
                double max_out_diff = 0;
                for (int i = event_start; i < csvData.Rows.Count; i++)
                {
                    if (csvData.Rows[i]["Warning"].ToString() != "")
                    {
                        DateTime current_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                        DateTime event_end_time = Convert.ToDateTime(csvData.Rows[event_end]["Timestamp"].ToString());
                        if (//i - event_end > gap_coef ||  //
                            csvData.Rows[i]["Warning"].ToString() != last_warning ||
                            (current_time - event_end_time).TotalMinutes > event_timegap)
                        {
                            event_start = i;
                            max_out_diff = 0;
                        }
                        string current_warning = csvData.Rows[i]["Warning"].ToString();
                        while (i < csvData.Rows.Count && csvData.Rows[i]["Warning"].ToString() == current_warning)
                        {
                            if (Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString()) - Convert.ToDateTime(csvData.Rows[i - 1]["Timestamp"].ToString()) > ts)
                            {
                                break;
                            }
                            double out_diff = Convert.ToDouble(csvData.Rows[i]["out difference"].ToString());
                            max_out_diff = Math.Abs(max_out_diff) > Math.Abs(out_diff) ? max_out_diff : out_diff;
                            i++;
                        }
                        last_warning = csvData.Rows[i - 1]["Warning"].ToString();
                        event_end = i - 1;
                        csvData.Rows[event_start]["duration"] = (event_end - event_start) * ts.TotalMinutes;
                        csvData.Rows[event_start]["Name"] = sensor_name;
                        csvData.Rows[event_start]["out difference"] = max_out_diff;
                    }
                }
            }
            else
            {
                for (int i = event_start; i < csvData.Rows.Count; i++)
                {
                    if (csvData.Rows[i]["Warning"].ToString() != "")
                    {
                        DateTime current_time = Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString());
                        DateTime event_end_time = Convert.ToDateTime(csvData.Rows[event_end]["Timestamp"].ToString());
                        if (//i - event_end > gap_coef ||  //
                            csvData.Rows[i]["Warning"].ToString() != last_warning ||
                            (current_time - event_end_time).TotalMinutes > event_timegap)
                        {
                            event_start = i;
                        }
                        string current_warning = csvData.Rows[i]["Warning"].ToString();
                        while (i < csvData.Rows.Count && csvData.Rows[i]["Warning"].ToString() == current_warning)
                        {
                            if (Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString()) - Convert.ToDateTime(csvData.Rows[i - 1]["Timestamp"].ToString()) > ts)
                            {
                                break;
                            }
                            i++;
                        }
                        last_warning = csvData.Rows[i - 1]["Warning"].ToString();
                        event_end = i;
                        csvData.Rows[event_start]["duration"] = (event_end - event_start) * ts.TotalMinutes;
                        csvData.Rows[event_start]["Name"] = sensor_name;
                    }
                }
            }
            return csvData;
        }
    }
}
