using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace new_anom
{
    public static class SensorEvents
    {
        public static DataTable outlier_to_event(string sensor_name, DataTable csvData, double event_timegap, float min_event_duration)
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
            double max_mnf = 0;
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                if (csvData.Rows[i]["Warning"].ToString() != "")
                {
                    event_start = i == 0? i + 1 : i;
                    event_end = i - (int)gap_coef - 2 >= 0 ? i - (int)gap_coef - 2 : 0;
                    last_warning = csvData.Rows[i]["Warning"].ToString();
                    break;
                }
            }
            csvData = BasicFunction.DailyAverageFlow(csvData);

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
                            max_mnf = 0;
                        }
                        string current_warning = csvData.Rows[i]["Warning"].ToString();
                        while (i < csvData.Rows.Count && csvData.Rows[i]["Warning"].ToString() == current_warning)
                        {
                            if(Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString()) - Convert.ToDateTime(csvData.Rows[i - 1]["Timestamp"].ToString()) > ts)
                            {
                                break;
                            }
                            double out_diff = Convert.ToDouble(csvData.Rows[i]["out difference"].ToString());
                            double current_mnf = Convert.ToDouble(csvData.Rows[i]["Minimal night flow"].ToString());
                            max_out_diff = Math.Abs(max_out_diff) > Math.Abs(out_diff) ? max_out_diff : out_diff;
                            max_mnf = max_mnf > current_mnf ? max_mnf : current_mnf;
                            i++;
                        }
                        last_warning = csvData.Rows[i - 1]["Warning"].ToString();
                        event_end = i - 1;
                        csvData.Rows[event_start]["duration"] = (event_end - event_start) * ts.TotalMinutes;
                        csvData.Rows[event_start]["Name"] = sensor_name;
                        csvData.Rows[event_start]["out difference"] = max_out_diff;
                        csvData.Rows[event_start]["Minimal night flow"] = max_mnf;
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
                            max_mnf = 0;
                        }
                        string current_warning = csvData.Rows[i]["Warning"].ToString();
                        while (i < csvData.Rows.Count && csvData.Rows[i]["Warning"].ToString() == current_warning)
                        {
                            if (Convert.ToDateTime(csvData.Rows[i]["Timestamp"].ToString()) - Convert.ToDateTime(csvData.Rows[i - 1]["Timestamp"].ToString()) > ts)
                            {
                                break;
                            }
                            i++;
                            double current_mnf = Convert.ToDouble(csvData.Rows[i]["Minimal night flow"].ToString());
                            max_mnf = max_mnf > current_mnf ? max_mnf : current_mnf;
                        }
                        last_warning = csvData.Rows[i - 1]["Warning"].ToString();
                        event_end = i;
                        csvData.Rows[event_start]["duration"] = (event_end - event_start) * ts.TotalMinutes;
                        csvData.Rows[event_start]["Name"] = sensor_name;
                        csvData.Rows[event_start]["Minimal night flow"] = max_mnf;
                    }
                }
            }
            
            //DataView dv = new DataView(csvData);
            //dv.RowFilter = "duration <> ''";
            //string filter_info = "duration > " + min_event_duration;
            //dv.RowFilter = filter_info;
            //return dv.ToTable();
            return csvData;
        }
    }
}
