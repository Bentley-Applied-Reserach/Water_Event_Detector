using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace new_anom
{
    public partial class SaveModelDetails : Form
    {
        public static WaterEventDetector wed = new WaterEventDetector();
        public static bool missing_time_cb;
        public static bool duplicated_time_cb;
        public static bool irregular_time_cb;
        public static bool dpp_sensorfailure_cb;
        public static string dpp_minstep_tb;
        public static string dpp_minvalue_tb;
        public static string dpp_maxvalue_tb;

        public static string dcp_path_tb;
        public static string dcp_period_tb;

        public static string ol_xbar_timestep_tb;
        public static bool ol_xbar_nmfilter_cb;
        public static string ol_xbar_nmfilter_coef_tb;
        public static string ol_xbar_coef_tb;

        public static string ol_EWMA_timestep_tb;
        public static bool ol_EWMA_nmfilter_cb;
        public static string ol_EWMA_nmfilter_coef_tb;
        public static string ol_EWMA_coef_tb;
        public static string ol_EWMA_lmd_tb;

        public static string ol_Cusum_timestep_tb;
        public static bool ol_cusum_nmfilter_cb;
        public static string ol_cusum_nmfilter_coef_tb;
        public static string ol_cusum_coef_tb;

        public static string sse_sensorname_tb;
        public static string sse_min_duration_tb;
        public static string sse_timegap_tb;

        public SaveModelDetails()
        {
            InitializeComponent();
        }

        private void SaveModelDetails_Load(object sender, EventArgs e)
        {
            //wed = new WaterEventDetector();
            //dpp
            model_missing_time_cb.Checked = missing_time_cb;
            model_duplicated_time_cb.Checked = duplicated_time_cb;
            model_irregular_time_cb.Checked = irregular_time_cb;

            model_dpp_sensorfailure_cb.Checked = dpp_sensorfailure_cb;
            model_dpp_minstep_tb.Text = dpp_minstep_tb;
            model_dpp_minvalue_tb.Text = dpp_minvalue_tb;
            model_dpp_maxvalue_tb.Text = dpp_maxvalue_tb;
            //dcp
            model_dcp_path_tb.Text = dcp_path_tb;
            model_dcp_period_tb.Text = dcp_period_tb;
            //ol
            model_ol_xbar_timestep_tb.Text = ol_xbar_timestep_tb;
            model_ol_xbar_nmfilter_cb.Checked = ol_xbar_nmfilter_cb;
            model_ol_xbar_nmfilter_coef_tb.Text = ol_xbar_nmfilter_coef_tb;
            model_ol_xbar_coef_tb.Text = ol_xbar_nmfilter_coef_tb;

            model_ol_EWMA_timestep_tb.Text = ol_EWMA_timestep_tb;
            model_ol_EWMA_nmfilter_cb.Checked = ol_EWMA_nmfilter_cb;
            model_ol_EWMA_nmfilter_coef_tb.Text = ol_EWMA_nmfilter_coef_tb;
            model_ol_EWMA_coef_tb.Text = ol_EWMA_coef_tb;
            model_ol_EWMA_lmd_tb.Text = ol_EWMA_lmd_tb;

            model_ol_Cusum_timestep_tb.Text = ol_Cusum_timestep_tb;
            model_ol_cusum_nmfilter_cb.Checked = ol_cusum_nmfilter_cb;
            model_ol_cusum_nmfilter_coef_tb.Text = ol_cusum_nmfilter_coef_tb;
            model_ol_cusum_coef_tb.Text = ol_cusum_coef_tb;
            //sse
            model_sse_sensorname_tb.Text = sse_sensorname_tb;
            model_sse_min_duration_tb.Text = sse_min_duration_tb;
            model_sse_timegap_tb.Text = sse_timegap_tb;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void smd_save_bt_Click(object sender, EventArgs e)
        {
            DataTable saveData = new DataTable();
            //////////////save dcp data, seasonality, median
            saveData.Columns.Add("Timestamp", typeof(string));
            saveData.Columns.Add("seasonal", typeof(double));
            DataTable dcpData = new DataTable();
            dcpData = BasicFunction.read_csvfile(dcpData, dcp_path_tb);
            dcpData = BasicFunction.delete_unit(dcpData);

            int start_date_index = 0;
            for (int i = 0; i < dcpData.Rows.Count; i++)
            {
                DateTime current_time = Convert.ToDateTime(dcpData.Rows[i]["Timestamp"].ToString());
                if(current_time.TimeOfDay.Hours == 0 && current_time.TimeOfDay.Minutes == 0)
                {
                    start_date_index = i;
                    break;
                }
            }
            for(int i = 0; i < Convert.ToInt32(dcp_period_tb); i++)
            {
                DateTime current_time = Convert.ToDateTime(dcpData.Rows[i + start_date_index]["Timestamp"].ToString());
                DataRow newrow = saveData.NewRow();
                newrow["Timestamp"] = current_time.TimeOfDay.ToString();
                newrow["seasonal"] = Convert.ToDouble(dcpData.Rows[i + start_date_index]["seasonal"].ToString());
                saveData.Rows.Add(newrow);
            }
            //median, mean, std
            double[] raw_data = dcpData.AsEnumerable().Select(r => Convert.ToDouble(r.Field<double>("Value"))).ToArray();
            double median_value = BasicFunction.GetMedian(raw_data);
            saveData.Columns.Add("Median", typeof(string));
            saveData.Rows[0]["Median"] = median_value;
            //set the current outlier method as the method of this model
            bool nmfilter = true;
            double nmfilter_coef = 3;

            int selected_index = model_method_tab.SelectedIndex;
            switch (selected_index)
            {
                //Xbar
                case 0:
                    nmfilter = ol_xbar_nmfilter_cb;
                    nmfilter_coef = Convert.ToDouble(ol_xbar_nmfilter_coef_tb.ToString());
                    break;
                //EWMA
                case 1:
                    nmfilter = ol_EWMA_nmfilter_cb;
                    nmfilter_coef = Convert.ToDouble(ol_EWMA_nmfilter_coef_tb.ToString());
                    break;
                //Cusum
                case 2:
                    nmfilter = ol_cusum_nmfilter_cb;
                    nmfilter_coef = Convert.ToDouble(ol_cusum_nmfilter_coef_tb.ToString());
                    break;
                default:
                    break;
            }
            dcpData = BasicFunction.h_stl(dcpData);
            List<double> filtered_flow_remainder =
                dcpData.AsEnumerable().Select(r => r.Field<double>("new remainder")).ToList();
            if (nmfilter == true)
            {
                filtered_flow_remainder = BasicFunction.normal_filter(filtered_flow_remainder, (float)nmfilter_coef);
            }

            double flow_remainder_mean = filtered_flow_remainder.Average();
            double flow_remainder_std = BasicFunction.StdDev(filtered_flow_remainder);
            saveData.Rows[1]["Median"] = "Method";
            saveData.Rows[2]["Median"] = selected_index;
            saveData.Rows[3]["Median"] = "mean";
            saveData.Rows[4]["Median"] = flow_remainder_mean;
            saveData.Rows[5]["Median"] = "STD";
            saveData.Rows[6]["Median"] = flow_remainder_std;
            //
            //save dpp parameter
            saveData.Columns.Add("");
            saveData.Columns.Add("dpp", typeof(string));
            saveData.Columns.Add("dpp value", typeof(string));

            saveData.Rows[0]["dpp"] = "Missing time steps";
            saveData.Rows[1]["dpp"] = "Duplicated time steps";
            saveData.Rows[2]["dpp"] = "Irregular time steps";
            saveData.Rows[3]["dpp"] = "Sensor failure";
            saveData.Rows[4]["dpp"] = "Minimal consecutive steps";
            saveData.Rows[5]["dpp"] = "Minimal value";
            saveData.Rows[6]["dpp"] = "Maximal value";

            saveData.Rows[0]["dpp value"] = model_missing_time_cb.Checked == true ? "true" : "false";
            saveData.Rows[1]["dpp value"] = model_duplicated_time_cb.Checked == true ? "true" : "false";
            saveData.Rows[2]["dpp value"] = model_irregular_time_cb.Checked == true ? "true" : "false";
            saveData.Rows[3]["dpp value"] = model_dpp_sensorfailure_cb.Checked == true ? "true" : "false";
            saveData.Rows[4]["dpp value"] = model_dpp_minstep_tb.Text;
            saveData.Rows[5]["dpp value"] = model_dpp_minvalue_tb.Text;
            saveData.Rows[6]["dpp value"] = model_dpp_maxvalue_tb.Text;
            //save outlier data
            saveData.Columns.Add("");
            saveData.Columns.Add("outlier", typeof(string));
            saveData.Columns.Add("outlier value", typeof(string));

            saveData.Rows[0]["outlier"] = "X-bar";
            saveData.Rows[1]["outlier"] = "Apply normal filter";
            saveData.Rows[2]["outlier"] = "Normal filter coefficient";
            saveData.Rows[3]["outlier"] = "Xbar coefficient";
            
            saveData.Rows[1]["outlier value"] = model_ol_xbar_nmfilter_cb.Checked == true ? "true" : "false";
            saveData.Rows[2]["outlier value"] = model_ol_xbar_nmfilter_coef_tb.Text;
            saveData.Rows[3]["outlier value"] = model_ol_xbar_coef_tb.Text;

            saveData.Rows[4]["outlier"] = "EWMA";
            saveData.Rows[5]["outlier"] = "Apply normal filter";
            saveData.Rows[6]["outlier"] = "Normal filter coefficient";
            saveData.Rows[7]["outlier"] = "EWMA coefficient";
            saveData.Rows[8]["outlier"] = "Weight";
            
            saveData.Rows[5]["outlier value"] = model_ol_EWMA_nmfilter_cb.Checked == true ? "true" : "false";
            saveData.Rows[6]["outlier value"] = model_ol_EWMA_nmfilter_coef_tb.Text;
            saveData.Rows[7]["outlier value"] = model_ol_EWMA_coef_tb.Text;
            saveData.Rows[8]["outlier value"] = model_ol_EWMA_lmd_tb.Text;

            saveData.Rows[9]["outlier"] = "Cusum";
            saveData.Rows[10]["outlier"] = "Apply normal filter";
            saveData.Rows[11]["outlier"] = "Normal filter coefficient";
            saveData.Rows[12]["outlier"] = "Cusum coefficient";

            saveData.Rows[10]["outlier value"] = model_ol_cusum_nmfilter_cb.Checked == true ? "true" : "false";
            saveData.Rows[11]["outlier value"] = model_ol_cusum_nmfilter_coef_tb.Text;
            saveData.Rows[12]["outlier value"] = model_ol_cusum_coef_tb.Text;
            //save sensor event data
            saveData.Columns.Add("");
            saveData.Columns.Add("sse", typeof(string));
            saveData.Columns.Add("sse value", typeof(string));

            saveData.Rows[0]["sse"] = "Sensor name";
            saveData.Rows[1]["sse"] = "Minimum duration";
            saveData.Rows[2]["sse"] = "Time gap";

            saveData.Rows[0]["sse value"] = model_sse_sensorname_tb.Text;
            saveData.Rows[1]["sse value"] = model_sse_min_duration_tb.Text;
            saveData.Rows[2]["sse value"] = model_sse_timegap_tb.Text;
            //////////////save model file
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|*.csv";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                BasicFunction.WriteToCsvFile(saveData, sfd.FileName);
            }
        }
    }
}
