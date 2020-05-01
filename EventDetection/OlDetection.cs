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
    public static class OlDetection
    {
        public static DataTable Xbar_ol(DataTable csvData, bool nmfilter, float nom_coef,
                                            float xbar_coef, float timegap)
        {
            csvData = BasicFunction.h_stl(csvData);

            List<double> filtered_flow_remainder =
                csvData.AsEnumerable().Select(r => r.Field<double>("new remainder")).ToList();
            //csvData.AsEnumerable().Select(r => Convert.ToDouble(r.Field<string>("new remainder"))).ToList();
            if (nmfilter == true)
            {
                filtered_flow_remainder = BasicFunction.normal_filter(filtered_flow_remainder, nom_coef);
            }

            double flow_remainder_mean = filtered_flow_remainder.Average();
            double flow_remainder_std = BasicFunction.StdDev(filtered_flow_remainder);
            //Console.WriteLine("mean: " + flow_remainder_mean + ", Std: " + flow_remainder_std);

            double upper_lmt = flow_remainder_mean + xbar_coef * flow_remainder_std;
            double lower_lmt = flow_remainder_mean - xbar_coef * flow_remainder_std;
            Console.WriteLine("upper limit: " + upper_lmt + ", lower limit: " + lower_lmt);

            csvData.Columns.Add("out difference", typeof(Double));
            csvData.Columns.Add("duration", typeof(Double));
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
            return csvData;
        }
        public static DataTable EWMA_ol(DataTable csvData, bool nmfilter, float nom_coef,
                                        float EWMA_coef, float lamda, float timegap)
        {
            csvData = BasicFunction.h_stl(csvData);

            List<double> filtered_flow_remainder =
                csvData.AsEnumerable().Select(r => r.Field<double>("new remainder")).ToList();
            //csvData.AsEnumerable().Select(r => Convert.ToDouble(r.Field<string>("new remainder"))).ToList();
            if (nmfilter == true)
            {
                filtered_flow_remainder = BasicFunction.normal_filter(filtered_flow_remainder, nom_coef);
            }

            double flow_remainder_mean = filtered_flow_remainder.Average();
            double flow_remainder_std = BasicFunction.StdDev(filtered_flow_remainder);

            double poly = Math.Pow(lamda / (2 - lamda), 0.5);
            double upper_lmt = flow_remainder_mean + EWMA_coef * flow_remainder_std * poly;
            double lower_lmt = flow_remainder_mean - 1.2 * EWMA_coef * flow_remainder_std * poly;

            csvData.Columns.Add("duration", typeof(Double));
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
                csvData.Rows[i]["EWMA"] = lamda * current_value + (1 - lamda) * last_zeta;
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
        public static DataTable Cusum_ol(DataTable csvData, bool nmfilter, float nom_coef,
                                        float csm_coef, float timegap)
        {
            csvData = BasicFunction.h_stl(csvData);

            List<double> filtered_flow_remainder =
                csvData.AsEnumerable().Select(r => r.Field<double>("new remainder")).ToList();
            //csvData.AsEnumerable().Select(r => Convert.ToDouble(r.Field<string>("new remainder"))).ToList();
            if (nmfilter == true)
            {
                filtered_flow_remainder = BasicFunction.normal_filter(filtered_flow_remainder, nom_coef);
            }

            double flow_remainder_mean = filtered_flow_remainder.Average();
            double flow_remainder_std = BasicFunction.StdDev(filtered_flow_remainder);

            double upper_lmt = flow_remainder_mean + csm_coef * flow_remainder_std;
            double lower_lmt = flow_remainder_mean - 1.5 * csm_coef * flow_remainder_std;

            csvData.Columns.Add("duration", typeof(Double));
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
                csvData.Rows[0]["duration"] = timegap;
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
                    csvData.Rows[i]["duration"] = timegap;
                }
            }
            return csvData;
        }
        public static DataTable SH_ESD_ol(DataTable raw_dt, string anom_data_path)
        {
            //DataTable raw_dt = new DataTable();
            //raw_dt = BasicFunction.read_csvfile(raw_dt, raw_data_path);
            DataTable anom_dt = new DataTable();
            anom_dt = BasicFunction.read_csvfile(anom_dt, anom_data_path);
            raw_dt.Columns.Add("Expected value", typeof(double));
            raw_dt.Columns.Add("out_diff", typeof(double));
            raw_dt.Columns.Add("Warning", typeof(string));
            raw_dt.Columns.Add("duration", typeof(Double));
            for (int i = 0; i < anom_dt.Rows.Count; i++)
            {
                int this_anom = int.Parse(anom_dt.Rows[i]["index"].ToString()) - 1;
                double anom_value = Convert.ToDouble(anom_dt.Rows[i]["anoms"].ToString());
                double expected_value = Convert.ToDouble(anom_dt.Rows[i]["expected_value"].ToString());
                double out_diff = anom_value - expected_value;
                raw_dt.Rows[this_anom]["Expected value"] = expected_value;
                raw_dt.Rows[this_anom]["out_diff"] = out_diff;
                if (out_diff > 0)
                {
                    raw_dt.Rows[this_anom]["Warning"] = "High";
                }
                else
                {
                    raw_dt.Rows[this_anom]["Warning"] = "Low";
                }
            }
            return raw_dt;
        }

        public static DataTable EKF_DBN_ol(DataTable csvData)
        {
            DateTime dt_start = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString());
            DateTime dt_next = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
            string time_step = (dt_next - dt_start).TotalMinutes.ToString();

            csvData.Columns.Add("out difference", typeof(Double));
            csvData.Columns.Add("duration", typeof(Double));
            csvData.Columns.Add("Warning", typeof(String));
            csvData.Columns.Add("trend", typeof(String));
            csvData.Columns.Add("seasonal", typeof(String));
            csvData.Columns.Add("remainder", typeof(String));

            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                if (Convert.ToDouble(csvData.Rows[i]["Value"]) > Convert.ToDouble(csvData.Rows[i]["upper bound"]))
                {
                    csvData.Rows[i]["out difference"] = Convert.ToDouble(csvData.Rows[i]["Value"]) - Convert.ToDouble(csvData.Rows[i]["upper bound"]);
                    csvData.Rows[i]["Warning"] = "High";
                    csvData.Rows[i]["duration"] = time_step;
                }
                else if (Convert.ToDouble(csvData.Rows[i]["Value"]) < Convert.ToDouble(csvData.Rows[i]["lower bound"]))
                {
                    csvData.Rows[i]["out difference"] = Convert.ToDouble(csvData.Rows[i]["Value"]) - Convert.ToDouble(csvData.Rows[i]["lower bound"]);
                    csvData.Rows[i]["Warning"] = "Low";
                    csvData.Rows[i]["duration"] = time_step;
                }
            }
            return csvData;
        }
        public static string dgv_filter_cmd(bool normal, bool high, bool low)
        {
            string cmd_line = "";

            if (normal != true)
            {
                cmd_line = "Warning <> ''";
            }

            if (high != true)
            {
                cmd_line += cmd_line == "" ? "Warning <> 'High'" : " AND Warning <> 'High'";
            }
            if (low != true)
            {
                cmd_line += cmd_line == "" ? "Warning <> 'Low'" : " AND Warning <> 'Low'";
            }/*
            if (normal == true)
            {
                cmd_line = "IsNull(Warning,'') = ''";
            }
            else
            {
                cmd_line = "Warning <> ''";
            }

            if (high == true)
            {
                cmd_line += cmd_line == "" ? "Warning = 'High'" : " OR Warning = 'High'";
            }
            if (low == true)
            {
                cmd_line += cmd_line == "" ? "Warning = 'Low'" : " OR Warning = 'Low'";
            }*/
            return cmd_line;
        }
        public static void Save_MeanAndStd(DataTable csvData, bool nmfilter, float nom_coef)
        {
            csvData = BasicFunction.h_stl(csvData);

            List<double> filtered_flow_remainder =
                csvData.AsEnumerable().Select(r => r.Field<double>("new remainder")).ToList();
            //csvData.AsEnumerable().Select(r => Convert.ToDouble(r.Field<string>("new remainder"))).ToList();
            if (nmfilter == true)
            {
                filtered_flow_remainder = BasicFunction.normal_filter(filtered_flow_remainder, nom_coef);
            }

            double flow_remainder_mean = filtered_flow_remainder.Average();
            double flow_remainder_std = BasicFunction.StdDev(filtered_flow_remainder);

            DataTable parameter_dt = new DataTable();
            parameter_dt.Columns.Add("Items", typeof(string));
            parameter_dt.Columns.Add("Value", typeof(string));
            DataRow row1 = parameter_dt.NewRow();
            row1["Items"] = "Mean";
            row1["Value"] = flow_remainder_mean.ToString();
            parameter_dt.Rows.Add(row1);
            DataRow row2 = parameter_dt.NewRow();
            row2["Items"] = "Std";
            row2["Value"] = flow_remainder_std.ToString();
            parameter_dt.Rows.Add(row2);

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|.csv";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = sfd.FileName;
                BasicFunction.WriteToCsvFile(parameter_dt, path);
            }
        }
    }
}
