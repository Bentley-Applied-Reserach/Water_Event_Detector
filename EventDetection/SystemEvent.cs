using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace new_anom
{
    public static class SystemEvent
    {
        public static DataTable all_events(string dt_flow_path, string[] dt_pressure_path, 
                                      bool flow_high,
                                      bool flow_low,
                                      bool pressure_high,
                                      bool pressure_low,
                                      bool correlation,
                                      bool mnf_flag,
                                      double mnf_threshold)
        {
            DataTable csvData = new DataTable();
            DataTable csvData2 = new DataTable();
            csvData = BasicFunction.read_csvfile(csvData, dt_flow_path);
            DataTable flow_dt = csvData.Copy();
            string flow_sensor_name = "Original name is flow";
            int first_event_row;
            for(first_event_row = 0; first_event_row < csvData.Rows.Count; first_event_row++)
            {
                if(csvData.Rows[first_event_row]["Name"].ToString() != "")
                {
                    break;
                }
            }
            if(csvData.Rows[first_event_row]["Name"].ToString() != "flow")
            {
                flow_sensor_name = csvData.Rows[first_event_row]["Name"].ToString();
                WaterEventDetector.flow_sensor_name = flow_sensor_name;
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    csvData.Rows[i]["Name"] = "flow";
                }
            }
            DataView dv_flow = new DataView(csvData);
            string dv_flow_filter = "duration <> ''";

            if (flow_high == false)
            {
                dv_flow_filter = dv_flow_filter + "AND Warning <> 'High'";
            }
            if (flow_low == false)
            {
                dv_flow_filter = dv_flow_filter + "AND Warning <> 'Low'";
            }
            dv_flow.RowFilter = dv_flow_filter;

            if (csvData.Columns.Contains("out difference"))
            {
                if (csvData.Columns.Contains("Minimal night flow"))
                {
                    csvData = dv_flow.ToTable("Selected", false, "No.", "Timestamp", "Name", "Value", "Warning", "duration", "out difference", "Minimal night flow");

                }
                else
                {
                    csvData = dv_flow.ToTable("Selected", false, "No.", "Timestamp", "Name", "Value", "Warning", "duration", "out difference");
                }
            }
            else
            {
                if (csvData.Columns.Contains("Minimal night flow"))
                {
                    csvData = dv_flow.ToTable("Selected", false, "No.", "Timestamp", "Name", "Value", "Warning", "duration", "Minimal night flow");

                }
                else
                {
                    csvData = dv_flow.ToTable("Selected", false, "No.", "Timestamp", "Name", "Value", "Warning", "duration");
                }
            }

            foreach (string path in dt_pressure_path)
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
                if(pressure_low == false)
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
                if(csvData2 != null)
                {
                    csvData2.Merge(temp);
                }
                else
                {
                    csvData2 = temp;
                }
            }
            DataView dv_sort = new DataView(csvData);
            dv_sort.Sort = "Timestamp";
            csvData2.Columns.Add("Confidence Level");
            DataView dv_sort2 = new DataView(csvData2);
            dv_sort2.Sort = "Timestamp";
            DataTable copy_data = csvData.Copy(); ;
            DataView dv_delete = new DataView(copy_data);
            dv_delete.Sort = "Timestamp";
            //csvData = dv_delete.ToTable();
            //Xbar_class.WriteToCsvFile(csvData, "D:\\Work\\NYSR data\\Xbar\\report\\before.csv");
            if (correlation != true)
            {
                csvData = dv_delete.ToTable();
                if (flow_sensor_name != "Original name is flow")
                {
                    for (int i = 0; i < csvData.Rows.Count; i++)
                    {
                        if (csvData.Rows[i]["Name"].ToString() == "flow")
                        {
                            csvData.Rows[i]["Name"] = flow_sensor_name;
                        }
                    }
                    WaterEventDetector.flow_sensor_name = flow_sensor_name;
                }
                csvData.Merge(csvData2);
                DataView dv = new DataView(csvData);
                dv.Sort = "Timestamp";
                csvData = dv.ToTable();
                csvData.Columns.Remove("No.");
                csvData.Columns.Remove("Minimal night flow");
                return csvData;
            }      
            
            //Compute mnf average
            double mnf_acc = Convert.ToDouble(flow_dt.Rows[0]["Minimal night flow"].ToString());
            int mnf_counter = 1;
            for (int i = 1; i < flow_dt.Rows.Count; i++)
            {
                if (flow_dt.Rows[i]["Minimal night flow"].ToString() != flow_dt.Rows[i - 1]["Minimal night flow"].ToString())
                {
                    mnf_counter++;
                    mnf_acc = mnf_acc + Convert.ToDouble(flow_dt.Rows[i]["Minimal night flow"].ToString());
                }
            }
            double mnf_avg = mnf_acc / mnf_counter;
            WaterEventDetector.mnf_avg = mnf_avg;
            csvData.Columns.Add("Confidence level");

            DateTime flow_event_start = Convert.ToDateTime(dv_sort[0]["Timestamp"].ToString()); ;
            DateTime flow_event_end = flow_event_start.AddMinutes(Convert.ToDouble(dv_sort[0]["duration"].ToString()));
            DataTable sys_event = csvData.Clone();
            csvData = dv_sort.ToTable();
            csvData2 = dv_sort2.ToTable();

            if (mnf_flag == true)
            {//use minimal night flow as one of the judegment 
                int mnf_cfd_score = 0;
                int od_cfd_score = 0;
                for (int i = 0; i < dv_sort.Count; i++) //dv_sort: flow events list, dv_sort2: pressure events list
                {
                    //compute system event confidence level
                    double current_mnf = Convert.ToDouble(dv_sort[i]["Minimal night flow"].ToString());
                    double sys_event_confidence_lvl = (current_mnf - mnf_avg) / mnf_avg;
                    if (sys_event_confidence_lvl > Convert.ToDouble(WaterEventDetector.mnf_vh))//cfd lvl: very high
                    {
                        //csvData.Rows[i]["Confidence level"] = "Very high";
                        mnf_cfd_score = 5;
                    }
                    else if(sys_event_confidence_lvl > Convert.ToDouble(WaterEventDetector.mnf_h))//cfd lvl: high
                    {
                        //csvData.Rows[i]["Confidence level"] = "High";
                        mnf_cfd_score = 4;
                    }
                    else if (sys_event_confidence_lvl > Convert.ToDouble(WaterEventDetector.mnf_m))//cfd lvl: medium
                    {
                        //csvData.Rows[i]["Confidence level"] = "Medium";
                        mnf_cfd_score = 3;
                    }
                    else if (sys_event_confidence_lvl > Convert.ToDouble(WaterEventDetector.mnf_l))//cfd lvl: low
                    {
                        //csvData.Rows[i]["Confidence level"] = "Low";
                        mnf_cfd_score = 2;
                    }
                    else //cfd lvl: very low
                    {
                        //csvData.Rows[i]["Confidence level"] = "Very low";
                        mnf_cfd_score = 1;
                    }
                    //out difference confidence level score
                    if (csvData.Columns.Contains("out difference"))
                    {
                        //compute system event confidence level
                        double flow_od = Convert.ToDouble(csvData.Rows[i]["out difference"].ToString());
                        double flow_value = Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                        double flow_event_cfl = Math.Abs(flow_od / (flow_value - flow_od));
                        if (flow_event_cfl > Convert.ToDouble(WaterEventDetector.mnf_vh))//cfd lvl: very high
                        {
                            //csvData.Rows[i]["Confidence level"] = "Very high";
                            od_cfd_score = 5;
                        }
                        else if (flow_event_cfl > Convert.ToDouble(WaterEventDetector.mnf_h))//cfd lvl: high
                        {
                            //csvData.Rows[i]["Confidence level"] = "High";
                            od_cfd_score = 4;
                        }
                        else if (flow_event_cfl > Convert.ToDouble(WaterEventDetector.mnf_m))//cfd lvl: medium
                        {
                            //csvData.Rows[i]["Confidence level"] = "Medium";
                            od_cfd_score = 3;
                        }
                        else if (flow_event_cfl > Convert.ToDouble(WaterEventDetector.mnf_l))//cfd lvl: low
                        {
                            //csvData.Rows[i]["Confidence level"] = "Low";
                            od_cfd_score = 2;
                        }
                        else //cfd lvl: very low
                        {
                            //csvData.Rows[i]["Confidence level"] = "Very low";
                            od_cfd_score = 1;
                        }
                        csvData.Rows[i]["Confidence level"] = BasicFunction.Get_confidence_level(Math.Max(od_cfd_score, mnf_cfd_score));
                        //end computition
                    }
                    //end computition
                    flow_event_start = Convert.ToDateTime(dv_sort[i]["Timestamp"].ToString());
                    flow_event_end = flow_event_start.AddMinutes(Convert.ToDouble(dv_sort[i]["duration"].ToString()));
                    bool event_correlation = false;
                    DataRow new_row = csvData.Rows[i];
                    sys_event.ImportRow(new_row);
                    for (int j = 0; j < dv_sort2.Count; j++)
                    {
                        DateTime pressure_event_start = Convert.ToDateTime(dv_sort2[j]["Timestamp"].ToString());
                        DateTime pressure_event_end = pressure_event_start.AddMinutes(Convert.ToDouble(dv_sort2[j]["duration"].ToString()));
                        if ((pressure_event_start >= flow_event_start && pressure_event_start <= flow_event_end) ||
                            (pressure_event_end >= flow_event_start && pressure_event_end <= flow_event_end) ||
                            (pressure_event_start <= flow_event_start && pressure_event_end >= flow_event_end))
                        {
                            if (csvData2.Columns.Contains("out difference") && WaterEventDetector.pre_od_cfd_level == true)//pressure event confidence level base on out difference
                            {
                                double pre_od = Convert.ToDouble(csvData2.Rows[j]["out difference"].ToString());
                                double pre_value = Convert.ToDouble(csvData2.Rows[j]["Value"].ToString());
                                double pre_event_cfl = Math.Abs(pre_od / (pre_value - pre_od));
                                if (pre_event_cfl > Convert.ToDouble(WaterEventDetector.pre_od_vh))//cfd lvl: very high
                                {
                                    csvData2.Rows[j]["Confidence level"] = "Very high";
                                }
                                else if (pre_event_cfl > Convert.ToDouble(WaterEventDetector.pre_od_h))//cfd lvl: high
                                {
                                    csvData2.Rows[j]["Confidence level"] = "High";
                                }
                                else if (pre_event_cfl > Convert.ToDouble(WaterEventDetector.pre_od_m))//cfd lvl: medium
                                {
                                    csvData2.Rows[j]["Confidence level"] = "Medium";
                                }
                                else if (pre_event_cfl > Convert.ToDouble(WaterEventDetector.pre_od_l))//cfd lvl: low
                                {
                                    csvData2.Rows[j]["Confidence level"] = "Low";
                                }
                                else //cfd lvl: very low
                                {
                                    csvData2.Rows[j]["Confidence level"] = "Very low";
                                }
                                //end computition
                            }
                            event_correlation = true;
                            //new_row = csvData2.Rows[j];
                            sys_event.ImportRow(csvData2.Rows[j]);
                        }
                    }
                    if (event_correlation == false)//identify events by minimal night flow when no correlated pressure events are found
                    {
                        if(dv_sort[i]["Minimal night flow"].ToString() != "")
                        {
                            double largest_event_mnf = Convert.ToDouble(dv_sort[i]["Minimal night flow"].ToString());
                            int flow_data_index = Convert.ToInt32(dv_sort[i]["No."].ToString()) - 1;
                            for (int j = flow_data_index; j < flow_dt.Rows.Count; j++)//find the largest minimal night flow during the event
                            {
                                if(Convert.ToDateTime(flow_dt.Rows[j]["Timestamp"].ToString()) > flow_event_end)
                                {
                                    break;
                                }
                                current_mnf = Convert.ToDouble(flow_dt.Rows[j]["Minimal night flow"].ToString());
                                if (current_mnf > largest_event_mnf)
                                {
                                    largest_event_mnf = current_mnf;
                                }
                            }
                            dv_sort[i]["Minimal night flow"] = largest_event_mnf;
                            if (largest_event_mnf < mnf_avg * mnf_threshold)
                            {
                                sys_event.Rows.Remove(sys_event.Rows[sys_event.Rows.Count - 1]);
                            }
                        }
                        else
                        {
                            sys_event.Rows.Remove(sys_event.Rows[sys_event.Rows.Count - 1]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < dv_sort.Count; i++) //dv_sort: flow events list, dv_sort2: pressure events list
                {
                    int od_cfd_score = 0;
                    if(csvData.Columns.Contains("out difference"))
                    {
                        //compute system event confidence level
                        double flow_od = Convert.ToDouble(csvData.Rows[i]["out difference"].ToString());
                        double flow_value = Convert.ToDouble(csvData.Rows[i]["Value"].ToString());
                        double flow_event_cfl = Math.Abs(flow_od / (flow_value - flow_od));
                        if (flow_event_cfl > Convert.ToDouble(WaterEventDetector.mnf_vh))//cfd lvl: very high
                        {
                            csvData.Rows[i]["Confidence level"] = "Very high";
                            od_cfd_score = 5;
                        }
                        else if (flow_event_cfl > Convert.ToDouble(WaterEventDetector.mnf_h))//cfd lvl: high
                        {
                            csvData.Rows[i]["Confidence level"] = "High";
                            od_cfd_score = 4;
                        }
                        else if (flow_event_cfl > Convert.ToDouble(WaterEventDetector.mnf_m))//cfd lvl: medium
                        {
                            csvData.Rows[i]["Confidence level"] = "Medium";
                            od_cfd_score = 3;
                        }
                        else if (flow_event_cfl > Convert.ToDouble(WaterEventDetector.mnf_l))//cfd lvl: low
                        {
                            csvData.Rows[i]["Confidence level"] = "Low";
                            od_cfd_score = 2;
                        }
                        else //cfd lvl: very low
                        {
                            csvData.Rows[i]["Confidence level"] = "Very low";
                            od_cfd_score = 1;
                        }
                        //end computition
                    }
                    flow_event_start = Convert.ToDateTime(dv_sort[i]["Timestamp"].ToString());
                    flow_event_end = flow_event_start.AddMinutes(Convert.ToDouble(dv_sort[i]["duration"].ToString()));
                    bool event_correlation = false;
                    DataRow new_row = csvData.Rows[i];
                    sys_event.ImportRow(new_row);
                    for (int j = 0; j < dv_sort2.Count; j++)
                    {
                        DateTime pressure_event_start = Convert.ToDateTime(dv_sort2[j]["Timestamp"].ToString());
                        DateTime pressure_event_end = pressure_event_start.AddMinutes(Convert.ToDouble(dv_sort2[j]["duration"].ToString()));
                        if ((pressure_event_start >= flow_event_start && pressure_event_start <= flow_event_end) ||
                            (pressure_event_end >= flow_event_start && pressure_event_end <= flow_event_end) ||
                            (pressure_event_start <= flow_event_start && pressure_event_end >= flow_event_end))
                        {
                            if(csvData2.Columns.Contains("out difference") && WaterEventDetector.pre_od_cfd_level == true)//pressure event confidence level base on out difference
                            {
                                double pre_od = Convert.ToDouble(csvData2.Rows[j]["out difference"].ToString());
                                double pre_value = Convert.ToDouble(csvData2.Rows[j]["Value"].ToString());
                                double pre_event_cfl = Math.Abs(pre_od / (pre_value - pre_od));
                                if (pre_event_cfl > Convert.ToDouble(WaterEventDetector.pre_od_vh))//cfd lvl: very high
                                {
                                    csvData2.Rows[j]["Confidence level"] = "Very high";
                                }
                                else if (pre_event_cfl > Convert.ToDouble(WaterEventDetector.pre_od_h))//cfd lvl: high
                                {
                                    csvData2.Rows[j]["Confidence level"] = "High";
                                }
                                else if (pre_event_cfl > Convert.ToDouble(WaterEventDetector.pre_od_m))//cfd lvl: medium
                                {
                                    csvData2.Rows[j]["Confidence level"] = "Medium";
                                }
                                else if (pre_event_cfl > Convert.ToDouble(WaterEventDetector.pre_od_l))//cfd lvl: low
                                {
                                    csvData2.Rows[j]["Confidence level"] = "Low";
                                }
                                else //cfd lvl: very low
                                {
                                    csvData2.Rows[j]["Confidence level"] = "Very low";
                                }
                                //end computition
                            }
                            event_correlation = true;
                            //new_row = csvData2.Rows[j];
                            sys_event.ImportRow(csvData2.Rows[j]);
                        }
                    }
                    if (event_correlation == false)
                    {
                        sys_event.Rows.Remove(sys_event.Rows[sys_event.Rows.Count - 1]);
                    }
                }
            }
            
            
            if (flow_sensor_name != "Original name is flow")
            {
                for (int i = 0; i < sys_event.Rows.Count; i++)
                {
                    if(sys_event.Rows[i]["Name"].ToString() == "flow")
                    {
                        sys_event.Rows[i]["Name"] = flow_sensor_name;
                    }
                }
            }
            sys_event.Columns.Remove("No.");
            if(sys_event.Columns.Contains("Minimal night flow"))
            {
                sys_event.Columns.Remove("Minimal night flow");
            }
            return sys_event;
        }
        public static DataTable pressure_only_event(DataTable dt, double timegap)
        {
            dt.Columns.Add("Pressure Event", typeof(String));
            DataTable result_dt = dt.Clone();
            int event_count = 0;
            DateTime event_start = Convert.ToDateTime(dt.Rows[0]["Timestamp"].ToString());
            DateTime event_end = event_start.AddMinutes(Convert.ToDouble(dt.Rows[0]["duration"].ToString()));
            bool record_first_event = false;
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["Name"].ToString() == WaterEventDetector.flow_sensor_name) // if it is a flow event record
                {
                    dt.Rows[i]["Pressure Event"] = ++event_count;
                    result_dt.ImportRow(dt.Rows[i]);
                    continue;
                }

                DateTime next_event_start = Convert.ToDateTime(dt.Rows[i]["Timestamp"].ToString());
                DateTime next_event_end = next_event_start.AddMinutes(Convert.ToDouble(dt.Rows[i]["duration"].ToString()));

                if ((next_event_start - event_start).TotalMinutes <= timegap)
                {
                    if (dt.Rows[i - 1]["Name"].ToString() == WaterEventDetector.flow_sensor_name) // if it is a flow event record
                    {
                        continue;
                    }
                    if (record_first_event == false) // record the first pressure event if it wasn't recorded
                    {
                        dt.Rows[i - 1]["Pressure Event"] = ++event_count;
                        result_dt.ImportRow(dt.Rows[i - 1]);
                        record_first_event = true;
                    }
                    result_dt.ImportRow(dt.Rows[i]);
                }
                else
                {
                    record_first_event = false;
                }
                event_start = next_event_start;
            }
            return result_dt;
        }
    }
}
