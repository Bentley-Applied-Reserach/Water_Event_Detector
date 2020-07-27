using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RDotNet;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace new_anom
{
    public partial class WaterEventDetector : Form
    {
        //unit
        public static string[] flow_unit_set = { "MGD", "Mgd", "mgd" };
        public static string flow_unit = "mgd";
        public static string[] pressure_unit_set = { "PSI", "Psi", "psi", "Meter", "METER", "M", "m" };
        public static string pressure_unit = "Meter";
        public static string current_unit = "(null)";
        //R engine
        // REngine engine;
        //ste
        int ste_flow_start_index = -1;
        int ste_flow_end_index = -1;
        int ste_current_pre_index;
        DataSet ds_pressure;
        public static double mnf_avg;
        //system event confidence level
        public static double mnf_vh = 0.25;
        public static double mnf_h = 0.20;
        public static double mnf_m = 0.15;
        public static double mnf_l = 0.10;
        public static double mnf_vl = 0.05;

        public static double flow_od_vh = 0.25;
        public static double flow_od_h = 0.2;
        public static double flow_od_m = 0.15;
        public static double flow_od_l = 0.1;
        public static double flow_od_vl = 0.05;

        public static double pre_od_vh = 0.25;
        public static double pre_od_h = 0.20;
        public static double pre_od_m = 0.15;
        public static double pre_od_l = 0.10;
        public static double pre_od_vl = 0.05;

        public static bool mnf_cfd_level = true;
        public static bool flow_od_cfd_level = true;
        public static bool pre_od_cfd_level = true;
        //Pressure cluster classification verification
        public static string ground_true_file_path = "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Verify\\Ground True.csv";
        public WaterEventDetector()
        {
            InitializeComponent();
            //initialize R engine
            // REngine.SetEnvironmentVariables();
            // engine = REngine.GetInstance();
            // engine.Initialize();
            //data pre-processing tab initiation
            dpp_file_path_tb.Text = "E:\\WaterDistribution\\EventDetection\\C-Work\\improved stl\\flow and pressure data\\NYSR.csv";
            dpp_minstep_tb.Text = "10";
            dpp_minvalue_tb.Text = "1";
            dpp_maxvalue_tb.Text = "100";
            //decomposition tab initiation
            dcp_path_tb.Text = "E:\\WaterDistribution\\EventDetection\\D-Work\\NYSR data\\fixed data\\NYSR.csv";
            dcp_period_tb.Text = "288";
            //outlier tab initiation
            ol_path_tb.Text = "E:\\WaterDistribution\\EventDetection\\D-Work\\NYSR data\\0713\\dcp\\dcp-NYSR.csv";
            ol_xbar_timestep_tb.Text = "00:05:00";
            ol_xbar_coef_tb.Text = "3";
            ol_xbar_nmfilter_coef_tb.Text = "3";
            ol_EWMA_nmfilter_coef_tb.Enabled = false;
            ol_EWMA_nmfilter_coef_tb.Text = "3";
            ol_EWMA_coef_tb.Text = "3";
            ol_EWMA_lmd_tb.Text = "0.5";
            ol_cusum_nmfilter_coef_tb.Enabled = false;
            ol_cusum_nmfilter_coef_tb.Text = "3";
            ol_cusum_coef_tb.Text = "3";
            //ol_SHESD_nmfilter_coef_tb.Enabled = false;
            ol_SHESD_longterm_tb.Text = "30";
            ol_shesd_confi_lvl_tb.Text = "0.95";
            ol_shesd_max_anom.Text = "0.2";
            //sensor event tab initiation
            sse_path_tb.Text = "E:\\WaterDistribution\\EventDetection\\D-Work\\NYSR data\\Xbar\\flow outlier.csv";
            //system event tab initiation
            ste_flow_path_tb.Text = "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Flow.csv";
            string[] ste_rtb = { "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn12.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn13.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn14.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn15.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn16.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn20.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn21.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn29.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn30.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn31.csv",
                                 "D:\\Work\\WaterEventDetection\\data for cluster based detection\\multi-decompose\\Stn55.csv"};
            ste_pre_path_rtb.Lines = ste_rtb;
            ste_mnf_tb.Text = "30";
            //real time
            rtd_datapath_tb.Text = "E:\\WaterDistribution\\EventDetection\\D-Work\\NYSR data\\flow and pressure data\\NYSR.csv";
            rtd_parameterpath_tb.Text = "E:\\WaterDistribution\\EventDetection\\D-Work\\test result\\model.csv";
            //EKF
            EKF_file_path_tb.Text = " E:\\WaterDistribution\\EKF-DBN\\EKF-DBN\\flow1\\";

            EKF_train_cfgfile_name_tb.Text = "EKF.txt";
            EKF_EKFtrain_weight_tb.Text = "training_weightEKF.txt";
            EKF_DBNtrain_weight_tb.Text = "training_weightDBN.txt";
            EKF_train_s_cov_tb.Text = "s_covariance.txt";
            EKF_train_p_cov_tb.Text = "p_covariance.txt";
            EKF_train_lowerbound_tb.Text = "lowerboundM.txt";
            EKF_train_upperbound_tb.Text = "upperBoundM.txt";
            EKF_train_prediction_tb.Text = "predict_train.txt";

            EKF_predict_cfg_tb.Text = "EKF.txt";
            EKF_predict_weight_tb.Text = "training_weightEKF.txt";
            EKF_predict_s_cov_tb.Text = "s_covariance.txt";
            EKF_predict_p_cov_tb.Text = "p_covariance.txt";
            EKF_predict_new_weight_tb.Text = "updated_training_weightEKF.txt";
            EKF_predict_new_s_cov_tb.Text = "s_updated_covariance.txt";
            EKF_predict_new_p_cov_tb.Text = "p_updated_covariance.txt";
            EKF_predict_lowerbound_tb.Text = "lowerboundMP.txt";
            EKF_predict_upperbound_tb.Text = "upperBoundMP.txt";
            EKF_predict_predict_tb.Text = "predict_test.txt";
        }

        private void file_path_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                dpp_file_path_tb.Text = ofd.FileName; //get path

                DataTable csvData = new DataTable();
                csvData = BasicFunction.read_csvfile(csvData, ofd.FileName);
                DateTime dt_start = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString());
                DateTime dt_next = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
                TimeSpan ts = dt_next - dt_start;
                dpp_timestep_tb.Text = ts.ToString();

                foreach (DataColumn column in csvData.Columns)
                {
                    if (column.ColumnName.Contains("("))  //Value(unit), read unit
                    {
                        string unit = column.ColumnName.Split('(', ')')[1];
                        current_unit = '(' + unit + ')';
                        //if flow unit
                        if (flow_unit_set.Any(unit.Contains))
                        {
                            flow_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("flow unit: {0}", unit);
                            break;
                        }
                        //if pressure unit
                        else if (pressure_unit_set.Any(unit.Contains))
                        {
                            pressure_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("pressure unit: {0}", unit);
                            break;
                        }
                    }
                }
                foreach (var series in dpp_before_chart.Series)
                {
                    series.Points.Clear();
                }
                foreach (var series in dpp_after_chart.Series)
                {
                    series.Points.Clear();
                }


                string[] file_split = dpp_file_path_tb.Text.Split('\\');
                string sensor_id = file_split[file_split.Length - 1].Split('.')[0];
                dpp_before_chart.Series[0].Name = sensor_id + current_unit;
                dpp_before_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                dpp_before_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                dpp_before_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                dpp_before_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                dpp_before_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                dpp_before_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                dpp_before_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    dpp_before_chart.Series[0].Points.AddXY
                        (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);
                }
            }
        }

        private void tabPage1_Enter(object sender, EventArgs e)
        {
            missing_time_cb.Checked = true;
            duplicated_time_cb.Checked = true;
            irregular_time_cb.Checked = true;
        }

        private void check_data_bt_Click(object sender, EventArgs e)
        {
            try
            {
                dpp_detail_gv.Refresh();
                dpp_summary_dgv.Refresh();
                string path = dpp_file_path_tb.Text;
                //DataTable dt = new DataTable();
                DataTable csvData = new DataTable();
                csvData = BasicFunction.read_csvfile(csvData, path);
                foreach (DataColumn column in csvData.Columns)
                {
                    if (column.ColumnName.Contains("("))  //Value(unit), read unit
                    {
                        string unit = column.ColumnName.Split('(', ')')[1];
                        current_unit = '(' + unit + ')';
                        //if flow unit
                        if (flow_unit_set.Any(unit.Contains))
                        {
                            flow_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("flow unit: {0}", unit);
                            break;
                        }
                        //if pressure unit
                        else if (pressure_unit_set.Any(unit.Contains))
                        {
                            pressure_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("pressure unit: {0}", unit);
                            break;
                        }
                    }
                }
                double max_value = Convert.ToDouble(dpp_maxvalue_tb.Text);
                double min_value = Convert.ToDouble(dpp_minvalue_tb.Text);
                int min_failstep = Convert.ToInt32(dpp_minstep_tb.Text);
                var dt_result = DataPreProcess.checking_data(csvData, max_value, min_value, min_failstep);
                DataView dv_detail = new DataView(dt_result.detail_dt);
                DataView dv_summary = new DataView(dt_result.summary_dt);

                string[] file_split = dpp_file_path_tb.Text.Split('\\');
                string sensor_id = file_split[file_split.Length - 1].Split('.')[0];
                DataRow smy_row0 = dt_result.summary_dt.NewRow();
                smy_row0["Items"] = "Sensor name";
                smy_row0["Value"] = sensor_id;
                dt_result.summary_dt.Rows.InsertAt(smy_row0, 0);

                string detail_filter = "";
                string smry_filter = "";
                //DataTable dt_filter = dt.Copy();
                if (missing_time_cb.Checked == false)
                {
                    //dv_detail.RowFilter = "NOT Error = 'Missing'";
                    //dv_summary.RowFilter = "NOT Items = 'Missing time step'";
                    detail_filter = "NOT Error = 'Missing'";
                    smry_filter = "NOT Items = 'Missing time step'";
                }
                if (duplicated_time_cb.Checked == false)
                {
                    //dv_detail.RowFilter = "NOT Error = 'Duplicated'";
                    //dv_summary.RowFilter = "NOT Items = 'Duplicated time step'";
                    detail_filter = detail_filter == "" ? "NOT Error = 'Duplicated'" : detail_filter + " AND NOT Error = 'Duplicated'";
                    smry_filter = smry_filter == "" ?
                        "NOT Items = 'Duplicated time step'" : smry_filter + " AND NOT Items = 'Duplicated time step'";
                }
                if (irregular_time_cb.Checked == false)
                {
                    //dv_detail.RowFilter = "NOT Error = 'Irregular'";
                    //dv_summary.RowFilter = "NOT Items = 'Irregular time step'";
                    detail_filter = detail_filter == "" ? "NOT Error = 'Irregular'" : detail_filter + " AND NOT Error = 'Irregular'";
                    smry_filter = smry_filter == "" ?
                        "NOT Items = 'Irregular time step'" : smry_filter + " AND NOT Items = 'Irregular time step'";
                }
                if (dpp_sensorfailure_cb.Checked == false)
                {
                    //dv_detail.RowFilter = "NOT Error = 'Sensor failure'";
                    //dv_summary.RowFilter = "NOT Items = 'Sensor failure time step'";
                    detail_filter = detail_filter == "" ? "NOT Error = 'Sensor failure'" : detail_filter + " AND NOT Error = 'Sensor failure'";
                    smry_filter = smry_filter == "" ?
                        "NOT Items = 'Sensor failure time step'" : smry_filter + " AND NOT Items = 'Sensor failure time step'";
                }
                dv_detail.RowFilter = detail_filter;
                dv_summary.RowFilter = smry_filter;
                //dt_filter = dv_detail.ToTable();
                dpp_detail_gv.DataSource = dv_detail.ToTable();
                dpp_summary_dgv.DataSource = dv_summary.ToTable();
                //dpp_detail_gv.DefaultCellStyle.Format = "N2";
                dpp_detail_gv.Columns["Value"].DefaultCellStyle.Format = "N2";
                dpp_detail_gv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dpp_summary_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                //data_summary_gv.Refresh();

                //chart
                /*
                foreach (var series in dpp_before_chart.Series)
                {
                    series.Points.Clear();
                }
                foreach (var series in dpp_after_chart.Series)
                {
                    series.Points.Clear();
                }
                //get sensor name(file name)

                dpp_before_chart.Series[0].Name = sensor_id + current_unit;
                dpp_before_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                dpp_before_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                dpp_before_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                dpp_before_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                dpp_before_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                dpp_before_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                dpp_before_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    dpp_before_chart.Series[0].Points.AddXY
                        (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);
                }*/
            }
            catch (FormatException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }/*
            catch (DataException)
            {
                Console.WriteLine("Ah oh");
            }*/
        }

        private void correct_save_bt_Click(object sender, EventArgs e)
        {
            string save_path;
            DataTable dt = new DataTable();
            dt = BasicFunction.read_csvfile(dt, dpp_file_path_tb.Text);
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.Contains("("))  //Value(unit), read unit
                {
                    string unit = column.ColumnName.Split('(', ')')[1];
                    current_unit = '(' + unit + ')';
                    //if flow unit
                    if (flow_unit_set.Any(unit.Contains))
                    {
                        flow_unit = unit;
                        column.ColumnName = column.ColumnName.Split('(', ')')[0];
                        //Console.WriteLine("flow unit: {0}", unit);
                        break;
                    }
                    //if pressure unit
                    else if (pressure_unit_set.Any(unit.Contains))
                    {
                        pressure_unit = unit;
                        column.ColumnName = column.ColumnName.Split('(', ')')[0];
                        //Console.WriteLine("pressure unit: {0}", unit);
                        break;
                    }
                }
            }
            double max_value = Convert.ToDouble(dpp_maxvalue_tb.Text);
            double min_value = Convert.ToDouble(dpp_minvalue_tb.Text);
            int min_failstep = Convert.ToInt32(dpp_minstep_tb.Text);
            dt = DataPreProcess.correct_data(dt, max_value, min_value, min_failstep);
            //chart
            foreach (var series in dpp_after_chart.Series)
            {
                series.Points.Clear();
            }
            dpp_after_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            dpp_after_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            dpp_after_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            dpp_after_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            dpp_after_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
            dpp_after_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            dpp_after_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            dpp_after_chart.Series[0].Name = dpp_before_chart.Series[0].Name;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dpp_after_chart.Series[0].Points.AddXY
                    (dt.Rows[i]["Timestamp"], dt.Rows[i]["Value"]);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|*.csv";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                save_path = sfd.FileName;
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ColumnName == "Value" && current_unit != "(null)")
                    {
                        column.ColumnName = column.ColumnName + current_unit;
                    }
                }
                BasicFunction.WriteToCsvFile(dt, save_path);
            }
        }

        private void dcp_file_path_bt_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "CSV|*.csv";
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK) // Test result.
                {
                    dcp_path_tb.Text = ofd.FileName;
                    DataTable csvData = new DataTable();
                    csvData = BasicFunction.read_csvfile(csvData, ofd.FileName);
                    DateTime dt_start = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString());
                    DateTime dt_end = Convert.ToDateTime(csvData.Rows[csvData.Rows.Count - 1]["Timestamp"].ToString());
                    DateTime dt_next = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
                    string time_span = csvData.Rows.Count.ToString();
                    dcp_timespan_tb.Text = time_span;
                    TimeSpan ts = dt_next - dt_start;
                    string time_step = ts.ToString();
                    dcp_timestep_tb.Text = time_step;
                    dcp_period_tb.Text = (1440 / ts.TotalMinutes).ToString();
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show("Selected file is not in the right format. "
                    + error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void decompose_bt_Click(object sender, EventArgs e)
        {
            foreach (var series in dcp_chart.Series)
            {
                series.Points.Clear();
            }
            try
            {
                string read_path = dcp_path_tb.Text;
                /*
                
                //init the R engine            
                //REngine.SetEnvironmentVariables();
                //engine = REngine.GetInstance();
                //engine.Initialize();
                CharacterVector ReadPath = engine.CreateCharacterVector(new[] { read_path });
                engine.SetSymbol("read_filepath", ReadPath);
                engine.Evaluate("flow_data <- read.csv(read_filepath, header = TRUE)");

                string start_time = "2016-06-01 00:00";
                string frequency = dcp_period_tb.Text;
                CharacterVector StartTime = engine.CreateCharacterVector(new[] { start_time });
                engine.SetSymbol("StartTime", StartTime);
                string dcp_cmd = "dataf <- ts(flow_data$Value, start = as.Date(StartTime), frequency = " + frequency + ")";
                engine.Evaluate(dcp_cmd);
                //engine.Evaluate("dataf <- ts(flow_data$Value, start = as.Date(StartTime), frequency = 288)");
                engine.Evaluate("stlflow = stl(dataf, s.window = 'periodic', robust = TRUE)");
                engine.Evaluate("flow_data$remainder <- stlflow$time.series[,3]");
                engine.Evaluate("flow_data$trend <- stlflow$time.series[,2]");
                engine.Evaluate("flow_data$seasonal <- stlflow$time.series[,1]");

                string save_path;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV|*.csv";
                DialogResult result = sfd.ShowDialog();
                if (result == DialogResult.OK) // Test result.
                {
                    save_path = sfd.FileName;
                    CharacterVector SavePath = engine.CreateCharacterVector(new[] { save_path });
                    engine.SetSymbol("SavePath", SavePath);
                    engine.Evaluate("write.csv(flow_data, SavePath)");
                    //clean up
                    //engine.Dispose();
                    /***   Close R engine after saving file  **

                    DataTable csvData = new DataTable();
                    csvData = BasicFunction.read_csvfile(csvData, save_path);
                    //unit
                    foreach (DataColumn column in csvData.Columns)
                    {
                        if (column.ColumnName.Contains("."))  //Value(unit), read unit
                        {
                            string unit = column.ColumnName.Split('.', '.')[1];
                            current_unit = '(' + unit + ')';
                            //if flow unit
                            if (flow_unit_set.Any(unit.Contains))
                            {
                                flow_unit = unit;
                                column.ColumnName = column.ColumnName.Split('.', '.')[0];
                                //Console.WriteLine("flow unit: {0}", unit);
                                break;
                            }
                            //if pressure unit
                            else if (pressure_unit_set.Any(unit.Contains))
                            {
                                pressure_unit = unit;
                                column.ColumnName = column.ColumnName.Split('.', '.')[0];
                                //Console.WriteLine("pressure unit: {0}", unit);
                                break;
                            }
                            if (column.ColumnName.Contains("Value"))
                            {
                                column.ColumnName = "Value";
                            }
                        }
                    }
                    string[] file_split = dcp_path_tb.Text.Split('\\');
                    string sensor_id = file_split[file_split.Length - 1].Split('.')[0];
                    dcp_chart.Titles[0].Text = "Sensor: " + sensor_id + current_unit;
                    //
                    csvData = BasicFunction.h_stl(csvData);
                    foreach (var series in dcp_chart.Series)
                    {
                        series.Points.Clear();
                    }

                    dcp_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                    dcp_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                    dcp_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                    dcp_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                    dcp_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                    dcp_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                    dcp_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                    for (int i = 0; i < csvData.Rows.Count; i++)
                    {
                        dcp_chart.Series["Original value"].Points.AddXY
                            (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);

                        dcp_chart.Series["Seasonal"].Points.AddXY
                            (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["seasonal"]);

                        dcp_chart.Series["Trend"].Points.AddXY
                            (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["trend"]);

                        dcp_chart.Series["Remainder"].Points.AddXY
                            (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["remainder"]);

                        dcp_chart.Series["Remainder with median"].Points.AddXY
                            (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["new remainder"]);
                    }
                }*/
            }
            catch (FormatException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string save_path = "D:\\Work\\test result\\eee.csv";
            DataTable csvData = new DataTable();
            csvData = BasicFunction.read_csvfile(csvData, save_path);

            //chart1.Series[0].XAxisType = 
            dcp_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            dcp_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            dcp_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            dcp_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                dcp_chart.Series["Original value"].Points.AddXY
                    (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);

                dcp_chart.Series["Seasonal"].Points.AddXY
                    (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["seasonal"]);

                dcp_chart.Series["Trend"].Points.AddXY
                    (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["trend"]);

                dcp_chart.Series["Remainder"].Points.AddXY
                    (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["remainder"]);
            }
            dcp_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
        }

        private void value_show_cb_CheckedChanged(object sender, EventArgs e)
        {
            dcp_chart.Series["Original value"].Enabled = value_show_cb.Checked;
        }

        private void seasonal_show_cb_CheckedChanged(object sender, EventArgs e)
        {
            dcp_chart.Series["Seasonal"].Enabled = seasonal_show_cb.Checked;
        }

        private void trend_show_cb_CheckedChanged(object sender, EventArgs e)
        {
            dcp_chart.Series["Trend"].Enabled = trend_show_cb.Checked;
        }

        private void remainder_show_cb_CheckedChanged(object sender, EventArgs e)
        {
            dcp_chart.Series["Remainder"].Enabled = remainder_show_cb.Checked;
        }

        private void save_chart_Click(object sender, EventArgs e)
        {
            string save_path;
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "PNG|*.Png";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                save_path = sfd.FileName;
                this.dcp_chart.SaveImage(save_path, ChartImageFormat.Png);
            }
        }

        private void ol_path_bt_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "CSV|*.csv";
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK) // Test result.
                {
                    ol_path_tb.Text = ofd.FileName;
                    DataTable csvData = new DataTable();
                    csvData = BasicFunction.read_csvfile(csvData, ofd.FileName);
                    DateTime dt_start = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString());
                    DateTime dt_next = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
                    string time_step = (dt_next - dt_start).ToString();
                    ol_xbar_timestep_tb.Text = time_step;
                    ol_EWMA_timestep_tb.Text = time_step;
                    ol_Cusum_timestep_tb.Text = time_step;
                    ol_SHESD_timestep_tb.Text = time_step;
                    ol_shesd_period_tb.Text = (1440 / (dt_next - dt_start).TotalMinutes).ToString();
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show("Selected file is not in the right format. "
                    + error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ol_identify_bt_Click(object sender, EventArgs e)
        {
            foreach (var series in ol_result_chart.Series)
            {
                series.Points.Clear();
            }
            foreach (var series in ol_result_remainder_chart.Series)
            {
                series.Points.Clear();
            }
            try
            {
                DataTable csvData = new DataTable();
                csvData = BasicFunction.read_csvfile(csvData, ol_path_tb.Text);
                //unit
                foreach (DataColumn column in csvData.Columns)
                {
                    if (column.ColumnName.Contains("."))  //Value.unit., read unit, R cant read "()" in column names and will change them into "."
                    {
                        string unit = column.ColumnName.Split('.', '.')[1];
                        current_unit = '(' + unit + ')';
                        //if flow unit
                        if (flow_unit_set.Any(unit.Contains))
                        {
                            flow_unit = unit;
                            column.ColumnName = column.ColumnName.Split('.', '.')[0];
                            //Console.WriteLine("flow unit: {0}", unit);
                            break;
                        }
                        //if pressure unit
                        else if (pressure_unit_set.Any(unit.Contains))
                        {
                            pressure_unit = unit;
                            column.ColumnName = column.ColumnName.Split('.', '.')[0];
                            //Console.WriteLine("pressure unit: {0}", unit);
                            break;
                        }
                    }
                    else if (column.ColumnName.Contains("("))  //Value(unit), read unit
                    {
                        string unit = column.ColumnName.Split('(', ')')[1];
                        current_unit = '(' + unit + ')';
                        //if flow unit
                        if (flow_unit_set.Any(unit.Contains))
                        {
                            flow_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("flow unit: {0}", unit);
                            break;
                        }
                        //if pressure unit
                        else if (pressure_unit_set.Any(unit.Contains))
                        {
                            pressure_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("pressure unit: {0}", unit);
                            break;
                        }
                    }
                }
                //
                ol_result_chart.Show();
                ol_result_remainder_chart.Show();
                ol_result_dgv.Show();
                TimeSpan ts = TimeSpan.Parse(ol_xbar_timestep_tb.Text);
                float time_step = float.Parse(ts.TotalMinutes.ToString());
                int selected_index = ol_method_tab.SelectedIndex;
                bool nmfilter;
                float nmfilter_coef;
                switch (selected_index)
                {
                    //Xbar
                    case 0:
                        /* remainder = remainder + trend */
                        for (int i = 0; i < csvData.Rows.Count; i++)
                        {
                            csvData.Rows[i]["remainder"] = Convert.ToDouble(csvData.Rows[i]["remainder"].ToString()) + Convert.ToDouble(csvData.Rows[i]["trend"].ToString());
                        }
                        ol_methodname_lb.Text = "Xbar";
                        float xbar_coef = float.Parse(ol_xbar_coef_tb.Text);
                        if (ol_xbar_nmfilter_cb.Checked == true)
                        {
                            nmfilter = true;
                            nmfilter_coef = float.Parse(ol_xbar_nmfilter_coef_tb.Text);
                        }
                        else
                        {
                            nmfilter = false;
                            nmfilter_coef = 0;
                        }
                        csvData = OlDetection.Xbar_ol(csvData, nmfilter, nmfilter_coef, xbar_coef, time_step);
                        break;
                    //EWMA
                    case 1:
                        ol_methodname_lb.Text = "EWMA";
                        float EWMA_coef = float.Parse(ol_EWMA_coef_tb.Text);
                        if (ol_EWMA_nmfilter_cb.Checked == true)
                        {
                            nmfilter = true;
                            nmfilter_coef = float.Parse(ol_EWMA_nmfilter_coef_tb.Text);
                        }
                        else
                        {
                            nmfilter = false;
                            nmfilter_coef = 0;
                        }
                        float lamda = float.Parse(ol_EWMA_lmd_tb.Text);
                        csvData = OlDetection.EWMA_ol(csvData, nmfilter, nmfilter_coef, EWMA_coef, lamda, time_step);
                        break;
                    //Cusum
                    case 2:
                        ol_methodname_lb.Text = "Cusum";
                        float csm_coef = float.Parse(ol_cusum_coef_tb.Text);
                        if (ol_cusum_nmfilter_cb.Checked == true)
                        {
                            nmfilter = true;
                            nmfilter_coef = float.Parse(ol_cusum_nmfilter_coef_tb.Text);
                        }
                        else
                        {
                            nmfilter = false;
                            nmfilter_coef = 0;
                        }
                        csvData = OlDetection.Cusum_ol(csvData, nmfilter, nmfilter_coef, csm_coef, time_step);
                        break;
                    //SH-ESD
                    case 3:
                        ol_methodname_lb.Text = "SH-ESD";
                        float long_term = float.Parse(ol_SHESD_longterm_tb.Text);/*
                        if (ol_SHESD_nmfilter_cb.Checked == true)
                        {
                            nmfilter = true;
                            nmfilter_coef = float.Parse(ol_SHESD_nmfilter_coef_tb.Text);
                        }
                        else
                        {
                            nmfilter = false;
                            nmfilter_coef = 0;
                        }
                        CharacterVector ReadPath = engine.CreateCharacterVector(new[] { ol_path_tb.Text });
                        engine.SetSymbol("read_filepath", ReadPath);
                        engine.Evaluate("p1 <- read.csv(read_filepath, header = TRUE)");
                        try
                        {
                            engine.Evaluate("library(AnomalyDetection)");
                        }
                        catch (RDotNet.EvaluationException e1)
                        {
                            string cmd1 = "install.packages(\"devtools\", dependencies = TRUE)";
                            string cmd2 = "devtools::install_github(\"twitter/AnomalyDetection\")";
                            engine.Evaluate(cmd1);
                            engine.Evaluate(cmd2);
                            engine.Evaluate("library(AnomalyDetection)");
                        }
                        //string period = (1440 / time_step).ToString();
                        string period = ol_shesd_period_tb.Text;
                        int period_multiple = int.Parse(ol_SHESD_longterm_tb.Text);
                        //string max_ts = ol_SHESD_longterm_tb.Text;
                        string max_ts = (period_multiple * int.Parse(period)).ToString();
                        string max_anoms = ol_shesd_max_anom.Text;
                        string confidence_lvl = (1 - Convert.ToDouble(ol_shesd_confi_lvl_tb.Text)).ToString();
                        string shesd_anom_cmd = $@"p1_anoms = AnomalyDetectionVec(p1$Value, max_anoms={max_anoms}, period={period}, 
                                                  direction='both', only_last = FALSE, threshold = 'None', plot = TRUE, longterm_period = {max_ts}, 
                                                  alpha = {confidence_lvl}, e_value = TRUE)";
                        engine.Evaluate(shesd_anom_cmd);
                        string shesd_save_path;
                        SaveFileDialog save_esd = new SaveFileDialog();
                        save_esd.Filter = "CSV|*.csv";
                        DialogResult esd_result = save_esd.ShowDialog();
                        if (esd_result == DialogResult.OK)
                        {
                            shesd_save_path = save_esd.FileName;
                            CharacterVector RSave_path = engine.CreateCharacterVector(new[] { shesd_save_path });
                            engine.SetSymbol("RSave_path", RSave_path);
                            engine.Evaluate("write.csv(p1_anoms$anoms, RSave_path)");
                        }
                        else
                        {
                            return;
                        }
                        csvData = OlDetection.SH_ESD_ol(csvData, shesd_save_path);*/
                        break;
                    case 4://ml
                        ol_methodname_lb.Text = "EKF-DBN";
                        DataTable csvData2 = new DataTable();
                        csvData = BasicFunction.read_csvfile(csvData2, ol_ekf_filepath_tb.Text);
                        csvData = OlDetection.EKF_DBN_ol(csvData);
                        break;
                    default:
                        break;
                }

                //display result in tablegridview
                //csvData.Columns.Remove("Column 1");
                csvData.Columns[0].ColumnName = "No.";
                ol_result_dgv.Refresh();


                ol_result_dgv.DataSource = csvData;
                ol_result_dgv.Columns["Value"].DefaultCellStyle.Format = "N2";
                ol_result_dgv.Columns["trend"].DefaultCellStyle.Format = "N2";
                ol_result_dgv.Columns["seasonal"].DefaultCellStyle.Format = "N2";
                if (ol_result_dgv.Columns.Contains("new remainder"))
                {//sh-esd does not have new remainder column
                    ol_result_dgv.Columns["new remainder"].DefaultCellStyle.Format = "N2";
                    ol_result_dgv.Columns["remainder"].Visible = false;
                }
                else
                {
                    csvData.Columns["remainder"].ColumnName = "new remainder";
                }
                if (ol_result_dgv.Columns.Contains("out difference"))
                {//ewma and cusum and sh-esd does not have out diff column
                    ol_result_dgv.Columns["out difference"].DefaultCellStyle.Format = "N2";
                }
                //ol_result_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                foreach (DataGridViewRow row in ol_result_dgv.Rows)
                {
                    row.HeaderCell.Value = (row.Index + 1).ToString();
                }
                //display result in chart
                string[] file_split = ol_path_tb.Text.Split('\\');
                string sensor_id = file_split[file_split.Length - 1].Split('.')[0];
                ol_result_chart.Titles[0].Text = "Sensor: " + sensor_id + current_unit + " Original";
                ol_result_remainder_chart.Titles[0].Text = "Sensor: " + sensor_id + current_unit + " Residual";

                ol_result_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                ol_result_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                ol_result_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                ol_result_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                ol_result_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                ol_result_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                ol_result_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

                ol_result_remainder_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                ol_result_remainder_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                ol_result_remainder_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                ol_result_remainder_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                ol_result_remainder_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                ol_result_remainder_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                ol_result_remainder_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    if (csvData.Rows[i]["Warning"].ToString() == "")
                    {
                        ol_result_chart.Series["Time series"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);

                        ol_result_remainder_chart.Series["Time series"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["new remainder"]);

                    }
                    else if (csvData.Rows[i]["Warning"].ToString() == "High")
                    {
                        ol_result_chart.Series["High outliers"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);

                        ol_result_remainder_chart.Series["High outliers"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["new remainder"]);
                    }
                    else
                    {
                        ol_result_chart.Series["Low outliers"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);

                        ol_result_remainder_chart.Series["Low outliers"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["new remainder"]);
                    }
                }

                string save_path;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV|*.csv";
                DialogResult result = sfd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    save_path = sfd.FileName;
                    foreach (DataColumn column in csvData.Columns)
                    {
                        if (column.ColumnName == "Value" && current_unit != "(null)")
                        {
                            column.ColumnName = column.ColumnName + current_unit;
                        }
                    }
                    BasicFunction.WriteToCsvFile(csvData, save_path);
                }
            }
            catch (NullReferenceException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }/*
            catch (NullReferenceException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
        }

        private void ol_xbar_nmfilter_cb_CheckedChanged(object sender, EventArgs e)
        {
            if (this.ol_xbar_nmfilter_cb.Checked == true)
            {
                ol_xbar_nmfilter_coef_tb.Text = "3";
                ol_xbar_nmfilter_coef_tb.Enabled = true;
            }
            else
            {
                ol_xbar_nmfilter_coef_tb.Text = "";
                ol_xbar_nmfilter_coef_tb.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {//outlier detection test button
            DataTable csvData = new DataTable();
            csvData = OlDetection.SH_ESD_ol(csvData, "D:\\Work\\test result\\SH-ESD flow.csv");
            //display result in tablegridview
            ol_result_dgv.DataSource = csvData;
            //display result in chart
            ol_result_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            ol_result_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            ol_result_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            ol_result_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            ol_result_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                if (csvData.Rows[i]["Warning"].ToString() == "")
                {
                    ol_result_chart.Series["Original value"].Points.AddXY
                          (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);
                }
                else if (csvData.Rows[i]["Warning"].ToString() == "High")
                {
                    ol_result_chart.Series["High outliers"].Points.AddXY
                          (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);
                }
                else
                {
                    ol_result_chart.Series["Low outliers"].Points.AddXY
                          (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);
                }
            }

            string save_path;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|*.csv";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                save_path = sfd.FileName;
                BasicFunction.WriteToCsvFile(csvData, save_path);
            }
        }

        private void sse_path_bt_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "CSV|*.csv";
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    sse_path_tb.Text = ofd.FileName;
                    string[] file_split = sse_path_tb.Text.Split('\\');
                    string sensor_id = file_split[file_split.Length - 1].Split('.')[0];
                    sse_sensorname_tb.Text = sensor_id;
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show("Selected file is not in the right format. "
                    + error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sse_identify_bt_Click(object sender, EventArgs e)
        {
            DataTable csvData = new DataTable();
            foreach (var series in sse_result_chart.Series)
            {
                series.Points.Clear();
            }
            try
            {
                double timegap = Convert.ToDouble(sse_timegap_tb.Text);
                float min_duration = float.Parse(sse_min_duration_tb.Text);
                string sensor_name = sse_sensorname_tb.Text;
                csvData = BasicFunction.read_csvfile(csvData, sse_path_tb.Text);
                //unit
                foreach (DataColumn column in csvData.Columns)
                {
                    if (column.ColumnName.Contains("("))  //Value(unit), read unit
                    {
                        string unit = column.ColumnName.Split('(', ')')[1];
                        current_unit = '(' + unit + ')';
                        //if flow unit
                        if (flow_unit_set.Any(unit.Contains))
                        {
                            flow_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("flow unit: {0}", unit);
                            break;
                        }
                        //if pressure unit
                        else if (pressure_unit_set.Any(unit.Contains))
                        {
                            pressure_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("pressure unit: {0}", unit);
                            break;
                        }
                    }
                }
                //
                DataTable ol_dt = csvData.Copy();
                csvData = SensorEvents.outlier_to_event(sensor_name, csvData, timegap, min_duration);
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    if (csvData.Rows[i]["duration"].ToString() != "")
                    {
                        if (float.Parse(csvData.Rows[i]["duration"].ToString()) < min_duration)
                        //if (float.Parse(csvData.Rows[i]["duration"].ToString()) == 15)
                        {
                            csvData.Rows[i]["duration"] = "";
                        }
                    }
                }
                //daily average and mnf
                if(csvData.Columns.Contains("Minimal night flow") == false)
                {
                    csvData = BasicFunction.DailyAverageFlow(csvData);
                }
                //save event file
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV|*.csv";
                DialogResult result = sfd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    BasicFunction.WriteToCsvFile(csvData, sfd.FileName);
                }

                //display result in chart
                sse_result_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                sse_result_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                sse_result_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                sse_result_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                sse_result_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                sse_result_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                sse_result_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

                for (int i = 0; i < ol_dt.Rows.Count; i++)
                {
                    //normal value
                    sse_result_chart.Series[0].Points.AddXY
                              (ol_dt.Rows[i]["Timestamp"], ol_dt.Rows[i]["Value"]);
                }
                DateTime dt1 = Convert.ToDateTime(ol_dt.Rows[0]["Timestamp"].ToString());
                DateTime dt2 = Convert.ToDateTime(ol_dt.Rows[1]["Timestamp"].ToString());
                TimeSpan ts = dt2 - dt1;
                int gap = (int)ts.TotalMinutes;
                sse_result_chart.Titles[0].Text = "Sensor:" + sse_sensorname_tb.Text + current_unit;

                int ol_pointer = 0;
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    //high and low event
                    if (csvData.Rows[i]["Warning"].ToString() == "High" && csvData.Rows[i]["duration"].ToString() != "")
                    {
                        int event_steps = Convert.ToInt32(csvData.Rows[i]["duration"].ToString()) / gap;
                        string event_start = csvData.Rows[i]["Timestamp"].ToString();
                        while (ol_pointer < ol_dt.Rows.Count)
                        {
                            if (ol_dt.Rows[ol_pointer]["Timestamp"].ToString() == event_start)
                            {
                                break;
                            }
                            else
                            {
                                ol_pointer++;

                            }
                        }
                        for (int j = 0; j < event_steps; j++)
                        {
                            //high events
                            sse_result_chart.Series[1].Points.AddXY
                                  (ol_dt.Rows[ol_pointer + j]["Timestamp"], ol_dt.Rows[ol_pointer + j]["Value"]);
                        }
                    }
                    //low events
                    else if (csvData.Rows[i]["Warning"].ToString() == "Low" && csvData.Rows[i]["duration"].ToString() != "")
                    {
                        int event_steps = Convert.ToInt32(csvData.Rows[i]["duration"].ToString()) / gap;
                        string event_start = csvData.Rows[i]["Timestamp"].ToString();
                        while (ol_pointer < ol_dt.Rows.Count)
                        {
                            if (ol_dt.Rows[ol_pointer]["Timestamp"].ToString() == event_start)
                            {
                                break;
                            }
                            else
                            {
                                ol_pointer++;

                            }
                        }
                        for (int j = 0; j < event_steps; j++)
                        {
                            sse_result_chart.Series[2].Points.AddXY
                                  (ol_dt.Rows[ol_pointer + j]["Timestamp"], ol_dt.Rows[ol_pointer + j]["Value"]);
                        }
                    }
                }

                //diaplay result in datagridview
                DataView dv = new DataView(csvData);
                dv.RowFilter = "duration <> ''";
                //string filter_info = "duration > " + sse_min_duration_tb.Text;
                //dv.RowFilter = filter_info;
                csvData = dv.ToTable();

                csvData.Columns.Remove("Name");
                csvData.Columns[0].ColumnName = "Event No.";
                csvData.Columns["Timestamp"].ColumnName = "Start time";
                csvData.Columns["duration"].SetOrdinal(2);
                csvData.Columns["Warning"].SetOrdinal(3);
                csvData.Columns["duration"].ColumnName = "duration(min)";
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    csvData.Rows[i][0] = i + 1;
                }
                sse_result_dgv.Refresh();
                sse_result_dgv.DataSource = csvData;
                sse_result_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                //sse_result_dgv.Columns["remainder"].Visible = false;
                sse_result_dgv.Columns["Value"].DefaultCellStyle.Format = "N2";
                sse_result_dgv.Columns["trend"].DefaultCellStyle.Format = "N2";
                sse_result_dgv.Columns["seasonal"].DefaultCellStyle.Format = "N2";
                if (sse_result_dgv.Columns.Contains("Daily Average"))
                {
                    sse_result_dgv.Columns["Daily Average"].DefaultCellStyle.Format = "N2";
                    sse_result_dgv.Columns["Minimal night flow"].DefaultCellStyle.Format = "N2";
                }
                //sse_result_dgv.Columns["new remainder"].DefaultCellStyle.Format = "N2";
                if (sse_result_dgv.Columns.Contains("out difference"))
                {
                    sse_result_dgv.Columns["out difference"].DefaultCellStyle.Format = "N2";
                }
                if (sse_result_dgv.Columns.Contains("new remainder"))
                {//sh-esd does not have new remainder column
                    sse_result_dgv.Columns["new remainder"].DefaultCellStyle.Format = "N2";
                    if (sse_result_dgv.Columns.Contains("remainder"))
                    {
                        sse_result_dgv.Columns["remainder"].Visible = false;
                    }
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //catch (FormatException error)
            //{
            //    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void sse_savedata_bt_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|*.csv";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                DataTable csvData = (DataTable)(sse_result_dgv.DataSource);
                BasicFunction.WriteToCsvFile(csvData, sfd.FileName);
            }
        }

        private void ste_flow_path_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                ste_flow_path_tb.Text = ofd.FileName;
            }
        }

        private void ste_pre_path_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                /*string[] filenames = ofd.FileNames;
                string name_show = "";/*
                foreach(string name in filenames)
                {
                    name_show += name + "\n";
                }*/
                ///ste_pre_path_rtb.Text = name_show;
                ste_pre_path_rtb.Lines = ofd.FileNames;
            }
        }

        private void ste_sys_event_identify_bt_Click(object sender, EventArgs e)
        {
            try
            {
                bool flow_high = ste_flow_high_cb.Checked;
                bool flow_low = ste_flow_low_cb.Checked;
                bool pressure_high = ste_pre_high_cb.Checked;
                bool pressure_low = ste_pre_low_cb.Checked;
                bool correlation = ste_fpcorrelation_cb.Checked;
                bool mnf_flag = ste_mnf_threshold_cb.Checked;
                double mnf_threshold = 100;
                if (ste_mnf_tb.Text != "")
                {
                    mnf_threshold = 1 + Convert.ToDouble(ste_mnf_tb.Text) / 100;
                }
                string flow_path = ste_flow_path_tb.Text;
                string[] pressure_path = ste_pre_path_rtb.Lines;
                DataTable csvData = new DataTable();
                if (flow_path == "" && pressure_path.Length > 0)
                {//only show pressure data, flow path is null
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
                    DataView dv_sort = new DataView(csvData);
                    dv_sort.Sort = "Timestamp";
                    csvData = dv_sort.ToTable();
                }
                else if (flow_path != "" && (pressure_path.Length == 0 || pressure_path == null))
                {//only show flow data, pressure is null
                    csvData = BasicFunction.read_csvfile(csvData, flow_path);
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
                        csvData = dv_flow.ToTable("Selected", false, "Timestamp", "Name", "Value", "Warning", "duration", "out difference");
                    }
                    else
                    {
                        csvData = dv_flow.ToTable("Selected", false, "Timestamp", "Name", "Value", "Warning", "duration");
                    }
                    DataView dv_sort = new DataView(csvData);
                    dv_sort.Sort = "Timestamp";
                    csvData = dv_sort.ToTable();
                }
                else
                {
                    csvData = SystemEvent.all_events(flow_path,
                                                     pressure_path,
                                                     flow_high,
                                                     flow_low,
                                                     pressure_high,
                                                     pressure_low,
                                                     correlation,
                                                     mnf_flag,
                                                     mnf_threshold);
                }
                ste_result_dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                ste_result_dgv.Refresh();
                if (mnf_cfd_level == false && flow_od_cfd_level == false)//only show confidence level when they are checked
                {
                    csvData.Columns.Remove("Confidence level");
                }
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
            }
            catch (FormatException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ol_EWMA_nmfilter_cb_CheckedChanged(object sender, EventArgs e)
        {
            if (ol_EWMA_nmfilter_cb.Checked == true)
            {
                ol_EWMA_nmfilter_coef_tb.Enabled = true;
                ol_EWMA_nmfilter_coef_tb.Text = "3";
            }
            else
            {
                ol_EWMA_nmfilter_coef_tb.Text = "";
                ol_EWMA_nmfilter_coef_tb.Enabled = false;
            }
        }

        private void ol_cusum_nmfilter_cb_CheckedChanged(object sender, EventArgs e)
        {
            if (ol_cusum_nmfilter_cb.Checked == true)
            {
                ol_cusum_nmfilter_coef_tb.Enabled = true;
                ol_cusum_nmfilter_coef_tb.Text = "3";
            }
            else
            {
                ol_cusum_nmfilter_coef_tb.Text = "";
                ol_cusum_nmfilter_coef_tb.Enabled = false;
            }
        }

        private void ol_method_tab_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected_index = ol_method_tab.SelectedIndex;
            switch (selected_index)
            {
                case 0:
                    ol_path_tb.Enabled = true;
                    if (ol_methodname_lb.Text != "Xbar")
                    {
                        ol_result_chart.Hide();
                        ol_result_remainder_chart.Hide();
                        ol_result_dgv.Hide();
                    }
                    else
                    {
                        ol_result_chart.Show();
                        ol_result_remainder_chart.Show();
                        ol_result_dgv.Show();
                    }
                    break;
                case 1:
                    ol_path_tb.Enabled = true;
                    if (ol_methodname_lb.Text != "EWMA")
                    {
                        ol_result_chart.Hide();
                        ol_result_remainder_chart.Hide();
                        ol_result_dgv.Hide();
                    }
                    else
                    {
                        ol_result_chart.Show();
                        ol_result_remainder_chart.Show();
                        ol_result_dgv.Show();
                    }
                    break;
                case 2:
                    ol_path_tb.Enabled = true;
                    if (ol_methodname_lb.Text != "Cusum")
                    {
                        ol_result_chart.Hide();
                        ol_result_remainder_chart.Hide();
                        ol_result_dgv.Hide();
                    }
                    else
                    {
                        ol_result_chart.Show();
                        ol_result_remainder_chart.Show();
                        ol_result_dgv.Show();
                    }
                    break;
                case 3:
                    ol_path_tb.Enabled = true;
                    if (ol_methodname_lb.Text != "SH-ESD")
                    {
                        ol_result_chart.Hide();
                        ol_result_remainder_chart.Hide();
                        ol_result_dgv.Hide();
                    }
                    else
                    {
                        ol_result_chart.Show();
                        ol_result_remainder_chart.Show();
                        ol_result_dgv.Show();
                    }
                    break;
                case 4:
                    ol_path_tb.Enabled = false;
                    break;
            }
        }

        private void ol_normalvalue_cb_CheckedChanged(object sender, EventArgs e)
        {
            ol_result_chart.Series[0].Enabled = ol_normalvalue_cb.Checked;
            ol_result_remainder_chart.Series[0].Enabled = ol_normalvalue_cb.Checked;
            if (ol_result_dgv.DataSource != null)
            {
                string filter_cmd = OlDetection.dgv_filter_cmd(ol_normalvalue_cb.Checked, ol_highol_cb.Checked, ol_lowol_cb.Checked);
                (ol_result_dgv.DataSource as DataTable).DefaultView.RowFilter = filter_cmd;
                foreach (DataGridViewRow row in ol_result_dgv.Rows)
                {
                    row.HeaderCell.Value = (row.Index + 1).ToString();
                }
            }
        }

        private void ol_highol_cb_CheckedChanged(object sender, EventArgs e)
        {
            ol_result_chart.Series["High outliers"].Enabled = ol_highol_cb.Checked;
            ol_result_remainder_chart.Series[1].Enabled = ol_highol_cb.Checked;
            if (ol_result_dgv.DataSource != null)
            {
                string filter_cmd = OlDetection.dgv_filter_cmd(ol_normalvalue_cb.Checked, ol_highol_cb.Checked, ol_lowol_cb.Checked);
                (ol_result_dgv.DataSource as DataTable).DefaultView.RowFilter = filter_cmd;
                foreach (DataGridViewRow row in ol_result_dgv.Rows)
                {
                    row.HeaderCell.Value = (row.Index + 1).ToString();
                }
            }
        }

        private void ol_lowol_cb_CheckedChanged(object sender, EventArgs e)
        {
            ol_result_chart.Series["Low outliers"].Enabled = ol_lowol_cb.Checked;
            ol_result_remainder_chart.Series[2].Enabled = ol_lowol_cb.Checked;
            if (ol_result_dgv.DataSource != null)
            {
                string filter_cmd = OlDetection.dgv_filter_cmd(ol_normalvalue_cb.Checked, ol_highol_cb.Checked, ol_lowol_cb.Checked);
                (ol_result_dgv.DataSource as DataTable).DefaultView.RowFilter = filter_cmd;
                foreach (DataGridViewRow row in ol_result_dgv.Rows)
                {
                    row.HeaderCell.Value = (row.Index + 1).ToString();
                }
            }
        }

        private void sse_normal_cb_CheckedChanged(object sender, EventArgs e)
        {
            sse_result_chart.Series[0].Enabled = sse_normal_cb.Checked;
        }

        private void sse_highevent_cb_CheckedChanged(object sender, EventArgs e)
        {
            sse_result_chart.Series[1].Enabled = sse_highevent_cb.Checked;
        }

        private void sse_lowevent_cb_CheckedChanged(object sender, EventArgs e)
        {
            sse_result_chart.Series[2].Enabled = sse_lowevent_cb.Checked;
        }

        private void ol_savechart_bt_Click(object sender, EventArgs e)
        {
            string save_path;
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "PNG|*.Png";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                save_path = sfd.FileName;
                this.ol_result_chart.SaveImage(save_path, ChartImageFormat.Png);
            }
        }
        public static string flow_sensor_name = "flow";
        private void ste_showchart_bt_Click(object sender, EventArgs e)
        {
            ste_flow_chart.Refresh();
            ste_pressure_chart.Refresh();
            try
            {
                int selected_index = ste_result_dgv.CurrentRow.Index;
                string selected_name = ste_result_dgv.Rows[selected_index].Cells["Name"].Value.ToString();
                if (selected_name == flow_sensor_name)
                {
                    SystemEventChart ste_form = new SystemEventChart();
                    SystemEventChart.flow_index = selected_index;
                    SystemEventChart.sys_event_dt = (DataTable)ste_result_dgv.DataSource;
                    SystemEventChart.flow_path = ste_flow_path_tb.Text;
                    SystemEventChart.pressure_path = ste_pre_path_rtb.Lines;
                    ste_form.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please select a flow event.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (NullReferenceException error)
            {
                MessageBox.Show("Event result is empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WaterEventDetector_Resize(object sender, EventArgs e)
        {
            //system event
            ste_result_dgv.Width = ste_result_gb.Size.Width / 4 + 200;
            ste_flow_chart.Left = ste_result_dgv.Right + 10;
            ste_flow_chart.Width = ste_result_gb.Size.Width - ste_result_dgv.Width - 20;
            ste_flow_chart.Height = ste_result_dgv.Height / 2 - 3;
            ste_pressure_chart.Left = ste_flow_chart.Left;
            ste_pressure_chart.Width = ste_flow_chart.Width;
            ste_pressure_chart.Top = ste_flow_chart.Bottom + 3;
            ste_pressure_chart.Height = ste_flow_chart.Height;
            //outlier
            ol_result_chart.Height = tabControl2.Height / 2 - 50;
            ol_result_remainder_chart.Height = ol_result_chart.Height;
            //ol_result_remainder_chart.Top = ol_result_chart.Bottom;
            //data pre-process
            dpp_after_chart.Height = tabControl3.Height / 2 - 20;
            dpp_before_chart.Height = dpp_after_chart.Height;
            dpp_after_chart.Top = dpp_before_chart.Bottom + 1;
            //real time data
            //rt_event_chart.Height = tabControl5.Height / 2 - 30;
            //rtd_remainder_chart.Height = rtd_original_chart.Height;
        }

        private void ste_show_chart_bt_Click(object sender, EventArgs e)
        {
            foreach (var series in ste_pressure_chart.Series)
            {
                series.Points.Clear();
            }

            try
            {
                int flow_index = ste_result_dgv.CurrentRow.Index;
                string selected_name = ste_result_dgv.Rows[flow_index].Cells["Name"].Value.ToString();
                if (selected_name == flow_sensor_name)
                {
                    foreach (var series in ste_flow_chart.Series)
                    {
                        series.Points.Clear();
                    }
                    DataTable sys_event_dt = (DataTable)ste_result_dgv.DataSource;
                    string[] pressure_path = ste_pre_path_rtb.Lines;
                    //prepare data
                    DataTable dt_flow = new DataTable();
                    dt_flow = BasicFunction.read_csvfile(dt_flow, ste_flow_path_tb.Text);
                    double flow_timestep;
                    DateTime dt1 = Convert.ToDateTime(dt_flow.Rows[0]["Timestamp"].ToString());
                    DateTime dt2 = Convert.ToDateTime(dt_flow.Rows[1]["Timestamp"].ToString());
                    TimeSpan ts = dt2 - dt1;
                    flow_timestep = ts.TotalMinutes;

                    //DataSet ds_pressure = new DataSet();
                    ds_pressure = new DataSet();
                    for (int i = 0; i < pressure_path.Length; i++)
                    {
                        DataTable temp = new DataTable();
                        temp = BasicFunction.read_csvfile(temp, pressure_path[i]);
                        ds_pressure.Tables.Add(temp);
                        string sensor_name;
                        for (int j = 0; j < temp.Rows.Count; j++)
                        {
                            if (temp.Rows[j]["Name"].ToString() != "")
                            {
                                sensor_name = temp.Rows[j]["Name"].ToString();
                                ds_pressure.Tables[i].TableName = sensor_name;
                                break;
                            }
                        }
                    }
                    double pressure_timestep = 0;
                    if (pressure_path.Length > 0)
                    {
                        dt1 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[0]["Timestamp"].ToString());
                        dt2 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[1]["Timestamp"].ToString());
                        ts = dt2 - dt1;
                        pressure_timestep = ts.TotalMinutes;
                    }
                    //find flow index
                    string flow_event_start = sys_event_dt.Rows[flow_index]["Timestamp"].ToString();
                    int flow_event_step = (int)(Convert.ToDouble(sys_event_dt.Rows[flow_index]["duration"].ToString()) / flow_timestep);
                    int flow_event_start_index = 0;
                    int flow_start_index = 0;
                    int flow_end_index = dt_flow.Rows.Count - 1;
                    int duration_minute = Convert.ToInt32(ste_timespan_tb.Text) * 60;
                    DateTime chart_start_date = Convert.ToDateTime(ste_starttime_tb.Text);
                    for (int i = 0; i < dt_flow.Rows.Count; i++)
                    {
                        DateTime current_time = Convert.ToDateTime(dt_flow.Rows[i]["Timestamp"].ToString());
                        if (Math.Abs((chart_start_date - current_time).TotalMinutes) < flow_timestep
                                 || ((chart_start_date - current_time).TotalMinutes < 0 && flow_start_index == 0))
                        {
                            flow_start_index = i;
                            flow_end_index = i + Convert.ToInt32(duration_minute / flow_timestep) < dt_flow.Rows.Count ?
                                             i + Convert.ToInt32(duration_minute / flow_timestep) : dt_flow.Rows.Count - 1;
                        }
                        if (dt_flow.Rows[i]["Timestamp"].ToString() == flow_event_start)
                        {
                            int flow_event_duration = Convert.ToInt32(dt_flow.Rows[i]["duration"].ToString());
                            if (flow_event_duration < 180 && ste_timespan_tb.Text == "")//disply 6 hours data when duration less than 180min
                            {
                                flow_event_start_index = i;/*
                                flow_start_index = i - 180 / Convert.ToInt32(flow_timestep) >= 0 ? 
                                                   i - 180 / Convert.ToInt32(flow_timestep) : 0;
                                flow_end_index = flow_start_index + 360 < dt_flow.Rows.Count ?
                                                 flow_start_index + 360 : dt_flow.Rows.Count - 1;*/
                                break;
                            }
                            else if (flow_event_duration >= 180 && ste_timespan_tb.Text == "")   //display 2*event_duration when event lasts for more than 6 hours
                            {
                                flow_event_start_index = i;/*
                                flow_start_index = i - flow_event_step / 2 >= 0 ? i - flow_event_step / 2 : 0;
                                flow_end_index = flow_start_index + 2 * flow_event_step < dt_flow.Rows.Count ?
                                                 flow_start_index + 2 * flow_event_step : dt_flow.Rows.Count - 1;*/
                                break;
                            }
                            else
                            {
                                flow_event_start_index = i;/*
                                flow_start_index = i - (int)(int.Parse(ste_timespan_tb.Text) * 60 / flow_timestep) / 2 >= 0 ? 
                                                   i - (int)(int.Parse(ste_timespan_tb.Text) * 60 / flow_timestep) / 2 : 0;
                                flow_end_index = flow_start_index + 2 * (int)(int.Parse(ste_timespan_tb.Text) * 60 / flow_timestep) < dt_flow.Rows.Count ?
                                                 flow_start_index + 2 * (int)(int.Parse(ste_timespan_tb.Text) * 60 / flow_timestep) : 
                                                 dt_flow.Rows.Count - 1;*/
                                break;
                            }
                        }
                    }
                    //display flow chart
                    ste_flow_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                    ste_flow_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

                    ste_flow_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                    ste_flow_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                    ste_flow_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                    ste_flow_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                    ste_flow_chart.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm";

                    ste_flow_chart.ChartAreas[0].CursorX.Interval = 0;
                    ste_flow_chart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Minutes;
                    ste_flow_chart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 1D;

                    ste_flow_chart.Titles[0].Text = "Flow sensor: " + flow_sensor_name +
                        " (" + flow_unit + ")  Event No." + ste_result_dgv.Rows[flow_index].HeaderCell.Value;

                    ste_flow_start_index = flow_start_index;
                    ste_flow_end_index = flow_end_index;

                    int other_event_flag = 0;
                    if (dt_flow.Columns.Contains("Daily Average"))
                    {
                        for (int i = flow_start_index; i < flow_end_index; i++)
                        {
                            ste_flow_chart.Series[0].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Value"]);
                            ste_flow_chart.Series[2].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["new remainder"]);
                            ste_flow_chart.Series[4].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Daily Average"]);
                            ste_flow_chart.Series[5].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Minimal night flow"]);
                            if (i >= flow_event_start_index && i <= flow_event_start_index + flow_event_step)
                            //if (dt_flow.Rows[i]["Warning"].ToString() != "")
                            {
                                ste_flow_chart.Series[1].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Value"]);
                                ste_flow_chart.Series[3].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["new remainder"]);
                            }
                            else if (dt_flow.Rows[i]["duration"].ToString() != "" || other_event_flag != 0)
                            {
                                ste_flow_chart.Series[6].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Value"]);
                                if (dt_flow.Rows[i]["duration"].ToString() != "")
                                {
                                    other_event_flag = Convert.ToInt32(dt_flow.Rows[i]["duration"].ToString()) / (int)flow_timestep;
                                }
                                else
                                {
                                    other_event_flag--;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = flow_start_index; i < flow_end_index; i++)
                        {
                            ste_flow_chart.Series[0].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Value"]);
                            ste_flow_chart.Series[2].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["new remainder"]);
                            if (i >= flow_event_start_index && i <= flow_event_start_index + flow_event_step)
                            //if (dt_flow.Rows[i]["Warning"].ToString() != "")
                            {
                                ste_flow_chart.Series[1].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Value"]);
                                ste_flow_chart.Series[3].Points.AddXY
                                              (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["new remainder"]);
                            }
                        }
                    }
                    //diaplay pressure chart

                    ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                    ste_pressure_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                    ste_pressure_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                    ste_pressure_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                    ste_pressure_chart.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm";

                    ste_pressure_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                    ste_pressure_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

                    ste_pressure_chart.ChartAreas[0].CursorX.Interval = 0;
                    ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Minutes;
                    ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 1D;
                    //
                    int i_pre = flow_index + 1;
                    ste_current_pre_index = i_pre;
                    if (i_pre >= sys_event_dt.Rows.Count)
                    {
                        MessageBox.Show("No associated pressure events.");
                        return;
                    }
                    string current_sensor_name = sys_event_dt.Rows[i_pre]["Name"].ToString();
                    if (current_sensor_name == selected_name || pressure_path.Length == 0)
                    {
                        MessageBox.Show("No associated pressure events.");
                    }
                    else
                    {
                        ste_pressure_chart.Titles[0].Text = "Pressure sensor: " + current_sensor_name + " (" + pressure_unit + ")";
                        string series_name = current_sensor_name;
                        //find index
                        int pre_event_step = (int)(Convert.ToDouble(sys_event_dt.Rows[i_pre]["duration"].ToString()) / pressure_timestep);
                        int pre_start_index = 0;
                        int pre_end_index = ds_pressure.Tables[current_sensor_name].Rows.Count;
                        DateTime flow_chart_start_date = Convert.ToDateTime(dt_flow.Rows[flow_start_index]["Timestamp"].ToString());
                        DateTime flow_chart_end_date = Convert.ToDateTime(dt_flow.Rows[flow_end_index]["Timestamp"].ToString());

                        for (int j = 0; j < ds_pressure.Tables[current_sensor_name].Rows.Count; j++)
                        {
                            DateTime pre_chart_start_date = Convert.ToDateTime
                                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"].ToString());
                            double total_minutes = (pre_chart_start_date - flow_chart_start_date).TotalMinutes;
                            if (Math.Abs(total_minutes) <= (pressure_timestep / 2) || total_minutes > 0)
                            {
                                pre_start_index = j;
                                break;
                            }
                        }

                        if (ds_pressure.Tables[current_sensor_name].Columns.Contains("Daily Average"))
                        {
                            for (int j = pre_start_index; j < ds_pressure.Tables[current_sensor_name].Rows.Count; j++)
                            {   //all value
                                ste_pressure_chart.Series[0].Points.AddXY
                                               (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                ste_pressure_chart.Series[2].Points.AddXY
                                               (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                ste_pressure_chart.Series[4].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                               ds_pressure.Tables[current_sensor_name].Rows[j]["Daily Average"]);
                                ste_pressure_chart.Series[5].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                               ds_pressure.Tables[current_sensor_name].Rows[j]["Minimal night flow"]);

                                string pre_duration = ds_pressure.Tables[current_sensor_name].Rows[j]["duration"].ToString();
                                //highlight event
                                if (pre_duration != "")
                                {
                                    int pre_duration_step = Convert.ToInt32(pre_duration) / (int)pressure_timestep;
                                    while (j < ds_pressure.Tables[current_sensor_name].Rows.Count &&
                                          //ds_pressure.Tables[current_sensor_name].Rows[j]["Warning"].ToString() != "")
                                          pre_duration_step > 0)
                                    {
                                        ste_pressure_chart.Series[0].Points.AddXY
                                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                        ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                        ste_pressure_chart.Series[2].Points.AddXY
                                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                        ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                        //event
                                        ste_pressure_chart.Series[1].Points.AddXY
                                                 (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                  ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                        ste_pressure_chart.Series[3].Points.AddXY
                                                 (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                  ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                        j++;
                                        pre_duration_step--;
                                    }
                                }

                                DateTime pre_chart_end_date = Convert.ToDateTime
                                                           (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"].ToString());
                                double total_minutes = (pre_chart_end_date - flow_chart_end_date).TotalMinutes;
                                if (Math.Abs(total_minutes) <= (pressure_timestep / 2) || total_minutes > 0)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int j = pre_start_index; j < ds_pressure.Tables[current_sensor_name].Rows.Count; j++)
                            {   //all value
                                ste_pressure_chart.Series[0].Points.AddXY
                                               (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                ste_pressure_chart.Series[2].Points.AddXY
                                               (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);

                                string pre_duration = ds_pressure.Tables[current_sensor_name].Rows[j]["duration"].ToString();
                                //highlight event
                                if (pre_duration != "")
                                {
                                    int pre_duration_step = Convert.ToInt32(pre_duration) / (int)pressure_timestep;
                                    while (j < ds_pressure.Tables[current_sensor_name].Rows.Count &&
                                          //ds_pressure.Tables[current_sensor_name].Rows[j]["Warning"].ToString() != "")
                                          pre_duration_step > 0)
                                    {
                                        ste_pressure_chart.Series[0].Points.AddXY
                                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                        ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                        ste_pressure_chart.Series[2].Points.AddXY
                                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                        ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                        //event
                                        ste_pressure_chart.Series[1].Points.AddXY
                                                 (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                  ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                        ste_pressure_chart.Series[3].Points.AddXY
                                                 (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                  ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                        j++;
                                        pre_duration_step--;
                                    }
                                }

                                DateTime pre_chart_end_date = Convert.ToDateTime
                                                           (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"].ToString());
                                double total_minutes = (pre_chart_end_date - flow_chart_end_date).TotalMinutes;
                                if (Math.Abs(total_minutes) <= (pressure_timestep / 2) || total_minutes > 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //display pressure event on pressure chart
                    MessageBox.Show("Please select a flow event.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if(ds_pressure == null)
                    {
                        ds_pressure = new DataSet();
                        string[] pressure_path = ste_pre_path_rtb.Lines;
                        for (int i = 0; i < pressure_path.Length; i++)
                        {
                            DataTable temp = new DataTable();
                            temp = BasicFunction.read_csvfile(temp, pressure_path[i]);
                            ds_pressure.Tables.Add(temp);
                            string sensor_name;
                            for (int j = 0; j < temp.Rows.Count; j++)
                            {
                                if (temp.Rows[j]["Name"].ToString() != "")
                                {
                                    sensor_name = temp.Rows[j]["Name"].ToString();
                                    ds_pressure.Tables[i].TableName = sensor_name;
                                    break;
                                }
                            }
                        }
                    }
                    int selected_index = ste_result_dgv.CurrentRow.Index;
                    selected_name = ste_result_dgv.Rows[selected_index].Cells["Name"].Value.ToString();
                    ste_current_pre_index = selected_index;

                    foreach (var series in ste_pressure_chart.Series)
                    {
                        series.Points.Clear();
                    }

                    ste_pressure_chart.Titles[0].Text = "Pressure sensor: " + selected_name + " (" + pressure_unit + ")";
                    double pressure_timestep;
                    DateTime dt1 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[0]["Timestamp"].ToString());
                    DateTime dt2 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[1]["Timestamp"].ToString());
                    TimeSpan ts = dt2 - dt1;
                    pressure_timestep = ts.TotalMinutes;
                    DataTable dt_flow = new DataTable();
                    dt_flow = BasicFunction.read_csvfile(dt_flow, ste_flow_path_tb.Text);
                    DataTable sys_event_dt = (DataTable)ste_result_dgv.DataSource;
                    string current_sensor_name = sys_event_dt.Rows[selected_index]["Name"].ToString();
                    string series_name = current_sensor_name;
                    //find index
                    int pre_event_step = (int)(Convert.ToDouble(sys_event_dt.Rows[selected_index]["duration"].ToString()) / pressure_timestep);
                    int pre_start_index = 0;
                    int pre_end_index = ds_pressure.Tables[current_sensor_name].Rows.Count;

                    string pressure_event_start = sys_event_dt.Rows[selected_index]["Timestamp"].ToString();
                    int pressure_event_start_index = 0;
                    int duration_minute = Convert.ToInt32(ste_timespan_tb.Text) * 60;
                    DateTime chart_start_date = Convert.ToDateTime(ste_starttime_tb.Text);
                    for (int i = 0; i < ds_pressure.Tables[current_sensor_name].Rows.Count; i++)
                    {
                        DateTime current_time = Convert.ToDateTime(ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"].ToString());
                        if (Math.Abs((chart_start_date - current_time).TotalMinutes) < pressure_timestep
                                 || ((chart_start_date - current_time).TotalMinutes < 0 && pre_start_index == 0))
                        {
                            pre_start_index = i;
                            pre_end_index = i + Convert.ToInt32(duration_minute / pressure_timestep) < ds_pressure.Tables[current_sensor_name].Rows.Count ?
                                            i + Convert.ToInt32(duration_minute / pressure_timestep) : ds_pressure.Tables[current_sensor_name].Rows.Count - 1;
                        }
                        if (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"].ToString() == pressure_event_start)
                        {
                            int pressure_event_duration = Convert.ToInt32(ds_pressure.Tables[current_sensor_name].Rows[i]["duration"].ToString());
                            pressure_event_start_index = i;
                            break;
                        }
                    }
                    //display pressure chart setting
                    ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                    ste_pressure_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                    ste_pressure_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                    ste_pressure_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                    ste_pressure_chart.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm";

                    ste_pressure_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                    ste_pressure_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

                    ste_pressure_chart.ChartAreas[0].CursorX.Interval = 0;
                    ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Minutes;
                    ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 1D;

                    int other_event_flag = 0;
                    if (dt_flow.Columns.Contains("Daily Average"))
                    {
                        for (int i = pre_start_index; i < pre_end_index; i++)
                        {
                            ste_pressure_chart.Series[0].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["Value"]);
                            ste_pressure_chart.Series[2].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["new remainder"]);
                            ste_pressure_chart.Series[4].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["Daily Average"]);
                            ste_pressure_chart.Series[5].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["Minimal night flow"]);
                            if (i >= pressure_event_start_index && i <= pressure_event_start_index + pre_event_step)
                            //if (dt_flow.Rows[i]["Warning"].ToString() != "")
                            {
                                ste_pressure_chart.Series[1].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["Value"]);
                                ste_pressure_chart.Series[3].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["new remainder"]);
                            }
                            else if (ds_pressure.Tables[current_sensor_name].Rows[i]["duration"].ToString() != "" || other_event_flag != 0)
                            {
                                ste_pressure_chart.Series[6].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["Value"]);
                                if (ds_pressure.Tables[current_sensor_name].Rows[i]["duration"].ToString() != "")
                                {
                                    other_event_flag = Convert.ToInt32(ds_pressure.Tables[current_sensor_name].Rows[i]["duration"].ToString()) / (int)pressure_timestep;
                                }
                                else
                                {
                                    other_event_flag--;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = pre_start_index; i < pre_end_index; i++)
                        {
                            ste_pressure_chart.Series[0].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["Value"]);
                            ste_pressure_chart.Series[2].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["new remainder"]);
                            if (i >= pressure_event_start_index && i <= pressure_event_start_index + pre_event_step)
                            //if (dt_flow.Rows[i]["Warning"].ToString() != "")
                            {
                                ste_pressure_chart.Series[1].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["Value"]);
                                ste_pressure_chart.Series[3].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[i]["Timestamp"], ds_pressure.Tables[current_sensor_name].Rows[i]["new remainder"]);
                            }
                        }
                    }
                }
            }
            catch (ExecutionEngineException)
            {

            }
            /*
            catch(ArgumentException)
            {
                MessageBox.Show("Event result is empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
        }

        private void ste_savetable_bt_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV|.csv";
                DialogResult result = sfd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string path = sfd.FileName;
                    BasicFunction.WriteToCsvFile((DataTable)ste_result_dgv.DataSource, path);
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Event result is empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadtable_bt_Click(object sender, EventArgs e)
        {
            DataTable sys_dt = new DataTable();
            sys_dt = BasicFunction.read_csvfile(sys_dt, "D:\\Work\\NYSR data\\result\\New name result.csv");
            ste_result_dgv.Refresh();
            ste_result_dgv.DataSource = sys_dt;
            ste_result_dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            ste_result_dgv.Columns["Value"].DefaultCellStyle.Format = "N2";
            ste_result_dgv.Columns["out difference"].DefaultCellStyle.Format = "N2";
            //ste_result_dgv.Columns["out difference"].HeaderText = "Max out difference";
            ste_result_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ste_result_dgv.RowHeadersWidth = 50;
            int event_num = 0;
            string flow_name = sys_dt.Rows[0]["Name"].ToString();
            for (int i = 0; i < sys_dt.Rows.Count; i++)
            {
                if (sys_dt.Rows[i]["Name"].ToString() == flow_name)
                {
                    event_num++;
                    ste_result_dgv.Rows[i].HeaderCell.Value = event_num.ToString();
                }
            }
        }


        private void ste_result_dgv_SizeChanged(object sender, EventArgs e)
        {

            ste_result_dgv.Width = ste_result_gb.Size.Width / 4 + 200;
            ste_flow_chart.Left = ste_result_dgv.Right + 10;
            ste_flow_chart.Width = ste_result_gb.Size.Width - ste_result_dgv.Width - 20;
            ste_flow_chart.Height = ste_result_dgv.Height / 2 - 3;
            ste_pressure_chart.Left = ste_flow_chart.Left;
            ste_pressure_chart.Width = ste_flow_chart.Width;
            ste_pressure_chart.Top = ste_flow_chart.Bottom + 3;
            ste_pressure_chart.Height = ste_flow_chart.Height;
        }

        private void ste_previouspre_bt_Click(object sender, EventArgs e)
        {
            try
            {
                int selected_index = ste_current_pre_index - 1;
                string selected_name = ste_result_dgv.Rows[selected_index].Cells["Name"].Value.ToString();
                if (selected_name == flow_sensor_name)
                {
                    MessageBox.Show("No previous pressure event.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    ste_current_pre_index = selected_index;
                    foreach (var series in ste_pressure_chart.Series)
                    {
                        series.Points.Clear();
                    }
                    ste_pressure_chart.Titles[0].Text = "Pressure sensor: " + selected_name + " (" + pressure_unit + ")";
                    double pressure_timestep;
                    DateTime dt1 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[0]["Timestamp"].ToString());
                    DateTime dt2 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[1]["Timestamp"].ToString());
                    TimeSpan ts = dt2 - dt1;
                    pressure_timestep = ts.TotalMinutes;
                    DataTable dt_flow = new DataTable();
                    dt_flow = BasicFunction.read_csvfile(dt_flow, ste_flow_path_tb.Text);
                    DataTable sys_event_dt = (DataTable)ste_result_dgv.DataSource;
                    string current_sensor_name = sys_event_dt.Rows[selected_index]["Name"].ToString();
                    string series_name = current_sensor_name;
                    //find index
                    int pre_event_step = (int)(Convert.ToDouble(sys_event_dt.Rows[selected_index]["duration"].ToString()) / pressure_timestep);
                    int pre_start_index = 0;
                    int pre_end_index = ds_pressure.Tables[current_sensor_name].Rows.Count;
                    DateTime flow_chart_start_date = Convert.ToDateTime(dt_flow.Rows[ste_flow_start_index]["Timestamp"].ToString());
                    DateTime flow_chart_end_date = Convert.ToDateTime(dt_flow.Rows[ste_flow_end_index]["Timestamp"].ToString());

                    for (int j = 0; j < ds_pressure.Tables[current_sensor_name].Rows.Count; j++)
                    {
                        DateTime pre_chart_start_date = Convert.ToDateTime
                                                   (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"].ToString());
                        double total_minutes = (pre_chart_start_date - flow_chart_start_date).TotalMinutes;
                        if (Math.Abs(total_minutes) <= (pressure_timestep / 2))
                        {
                            pre_start_index = j;
                            break;
                        }
                    }

                    for (int j = pre_start_index; j < ds_pressure.Tables[current_sensor_name].Rows.Count; j++)
                    {   //all value
                        ste_pressure_chart.Series[0].Points.AddXY
                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                        ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                        ste_pressure_chart.Series[2].Points.AddXY
                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                        ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                        ste_pressure_chart.Series[4].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                               ds_pressure.Tables[current_sensor_name].Rows[j]["Daily Average"]);
                        ste_pressure_chart.Series[5].Points.AddXY
                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                        ds_pressure.Tables[current_sensor_name].Rows[j]["Minimal night flow"]);

                        string pre_duration = ds_pressure.Tables[current_sensor_name].Rows[j]["duration"].ToString();
                        //highlight event
                        if (pre_duration != "")
                        {
                            int pre_duration_step = Convert.ToInt32(pre_duration) / (int)pressure_timestep;
                            while (j < ds_pressure.Tables[current_sensor_name].Rows.Count &&
                                  //ds_pressure.Tables[current_sensor_name].Rows[j]["Warning"].ToString() != "")
                                  pre_duration_step > 0)
                            {
                                ste_pressure_chart.Series[0].Points.AddXY
                                               (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                ste_pressure_chart.Series[2].Points.AddXY
                                               (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                //event
                                ste_pressure_chart.Series[1].Points.AddXY
                                         (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                          ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                ste_pressure_chart.Series[3].Points.AddXY
                                         (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                          ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                j++;
                                pre_duration_step--;
                            }
                        }

                        DateTime pre_chart_end_date = Convert.ToDateTime
                                                   (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"].ToString());
                        double total_minutes = (pre_chart_end_date - flow_chart_end_date).TotalMinutes;
                        if (Math.Abs(total_minutes) <= (pressure_timestep / 2))
                        {
                            break;
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("No previous pressure event. \n Index out of range.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ste_nextpre_bt_Click(object sender, EventArgs e)
        {
            try
            {
                int selected_index = ste_current_pre_index + 1;
                string selected_name = ste_result_dgv.Rows[selected_index].Cells["Name"].Value.ToString();
                if (selected_name == flow_sensor_name)
                {
                    MessageBox.Show("No previous pressure event.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    ste_current_pre_index = selected_index;
                    foreach (var series in ste_pressure_chart.Series)
                    {
                        series.Points.Clear();
                    }
                    ste_pressure_chart.Titles[0].Text = "Pressure sensor: " + selected_name + " (" + pressure_unit + ")";
                    double pressure_timestep;
                    DateTime dt1 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[0]["Timestamp"].ToString());
                    DateTime dt2 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[1]["Timestamp"].ToString());
                    TimeSpan ts = dt2 - dt1;
                    pressure_timestep = ts.TotalMinutes;
                    DataTable dt_flow = new DataTable();
                    dt_flow = BasicFunction.read_csvfile(dt_flow, ste_flow_path_tb.Text);
                    DataTable sys_event_dt = (DataTable)ste_result_dgv.DataSource;
                    string current_sensor_name = sys_event_dt.Rows[selected_index]["Name"].ToString();
                    string series_name = current_sensor_name;
                    //find index
                    int pre_event_step = (int)(Convert.ToDouble(sys_event_dt.Rows[selected_index]["duration"].ToString()) / pressure_timestep);
                    int pre_start_index = 0;
                    int pre_end_index = ds_pressure.Tables[current_sensor_name].Rows.Count;
                    DateTime flow_chart_start_date = Convert.ToDateTime(dt_flow.Rows[ste_flow_start_index]["Timestamp"].ToString());
                    DateTime flow_chart_end_date = Convert.ToDateTime(dt_flow.Rows[ste_flow_end_index]["Timestamp"].ToString());

                    for (int j = 0; j < ds_pressure.Tables[current_sensor_name].Rows.Count; j++)
                    {
                        DateTime pre_chart_start_date = Convert.ToDateTime
                                                   (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"].ToString());
                        double total_minutes = (pre_chart_start_date - flow_chart_start_date).TotalMinutes;
                        if (Math.Abs(total_minutes) <= (pressure_timestep / 2))
                        {
                            pre_start_index = j;
                            break;
                        }
                    }

                    for (int j = pre_start_index; j < ds_pressure.Tables[current_sensor_name].Rows.Count; j++)
                    {   //all value
                        ste_pressure_chart.Series[0].Points.AddXY
                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                        ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                        ste_pressure_chart.Series[2].Points.AddXY
                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                        ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                        ste_pressure_chart.Series[4].Points.AddXY
                                              (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                               ds_pressure.Tables[current_sensor_name].Rows[j]["Daily Average"]);
                        ste_pressure_chart.Series[5].Points.AddXY
                                       (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                        ds_pressure.Tables[current_sensor_name].Rows[j]["Minimal night flow"]);

                        string pre_duration = ds_pressure.Tables[current_sensor_name].Rows[j]["duration"].ToString();
                        //highlight event
                        if (pre_duration != "")
                        {
                            int pre_duration_step = Convert.ToInt32(pre_duration) / (int)pressure_timestep;
                            while (j < ds_pressure.Tables[current_sensor_name].Rows.Count &&
                                  //ds_pressure.Tables[current_sensor_name].Rows[j]["Warning"].ToString() != "")
                                  pre_duration_step > 0)
                            {
                                ste_pressure_chart.Series[0].Points.AddXY
                                               (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                ste_pressure_chart.Series[2].Points.AddXY
                                               (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                                ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                //event
                                ste_pressure_chart.Series[1].Points.AddXY
                                         (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                          ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                                ste_pressure_chart.Series[3].Points.AddXY
                                         (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                                          ds_pressure.Tables[current_sensor_name].Rows[j]["new remainder"]);
                                j++;
                                pre_duration_step--;
                            }
                        }

                        DateTime pre_chart_end_date = Convert.ToDateTime
                                                   (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"].ToString());
                        double total_minutes = (pre_chart_end_date - flow_chart_end_date).TotalMinutes;
                        if (Math.Abs(total_minutes) <= (pressure_timestep / 2))
                        {
                            break;
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("No previous pressure event. \n Index out of range.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ste_show_value_cb_CheckedChanged(object sender, EventArgs e)
        {
            ste_flow_chart.Series[0].Enabled = ste_show_value_cb.Checked;
            ste_flow_chart.Series[1].Enabled = ste_show_value_cb.Checked;
            ste_flow_chart.ChartAreas[0].RecalculateAxesScale();

            ste_pressure_chart.Series[0].Enabled = ste_show_value_cb.Checked;
            ste_pressure_chart.Series[1].Enabled = ste_show_value_cb.Checked;
            ste_pressure_chart.ChartAreas[0].RecalculateAxesScale();
        }

        private void ste_show_remainder_cb_CheckedChanged(object sender, EventArgs e)
        {
            ste_flow_chart.Series[2].Enabled = ste_show_remainder_cb.Checked;
            ste_flow_chart.Series[3].Enabled = ste_show_remainder_cb.Checked;
            //ste_flow_chart.Series[6].Enabled = ste_show_remainder_cb.Checked;
            ste_flow_chart.ChartAreas[0].RecalculateAxesScale();

            ste_pressure_chart.Series[2].Enabled = ste_show_remainder_cb.Checked;
            ste_pressure_chart.Series[3].Enabled = ste_show_remainder_cb.Checked;
            ste_pressure_chart.ChartAreas[0].RecalculateAxesScale();
        }

        private void ol_result_chart_Resize(object sender, EventArgs e)
        {
            ol_result_chart.Height = tabControl2.Height / 2 - 50;
            ol_result_remainder_chart.Height = ol_result_chart.Height;
        }

        private void dpp_sensorfailure_cb_CheckedChanged(object sender, EventArgs e)
        {
            dpp_minstep_tb.Enabled = dpp_sensorfailure_cb.Checked;
            dpp_minvalue_tb.Enabled = dpp_sensorfailure_cb.Checked;
            dpp_maxvalue_tb.Enabled = dpp_sensorfailure_cb.Checked;
        }

        private void dcp_medianremainder_cv_CheckedChanged(object sender, EventArgs e)
        {
            dcp_chart.Series[4].Enabled = dcp_medianremainder_cb.Checked;
        }

        private void ol_saveparameter_bt_Click(object sender, EventArgs e)
        {
            DataTable csvData = new DataTable();
            csvData = BasicFunction.read_csvfile(csvData, ol_path_tb.Text);
            //unit
            foreach (DataColumn column in csvData.Columns)
            {
                if (column.ColumnName.Contains("."))  //Value.unit., read unit, R cant read "()" in column names and will change them into "."
                {
                    string unit = column.ColumnName.Split('.', '.')[1];
                    current_unit = '(' + unit + ')';
                    //if flow unit
                    if (flow_unit_set.Any(unit.Contains))
                    {
                        flow_unit = unit;
                        column.ColumnName = column.ColumnName.Split('.', '.')[0];
                        //Console.WriteLine("flow unit: {0}", unit);
                        break;
                    }
                    //if pressure unit
                    else if (pressure_unit_set.Any(unit.Contains))
                    {
                        pressure_unit = unit;
                        column.ColumnName = column.ColumnName.Split('.', '.')[0];
                        //Console.WriteLine("pressure unit: {0}", unit);
                        break;
                    }
                }
                else if (column.ColumnName.Contains("("))  //Value(unit), read unit
                {
                    string unit = column.ColumnName.Split('(', ')')[1];
                    current_unit = '(' + unit + ')';
                    //if flow unit
                    if (flow_unit_set.Any(unit.Contains))
                    {
                        flow_unit = unit;
                        column.ColumnName = column.ColumnName.Split('(', ')')[0];
                        //Console.WriteLine("flow unit: {0}", unit);
                        break;
                    }
                    //if pressure unit
                    else if (pressure_unit_set.Any(unit.Contains))
                    {
                        pressure_unit = unit;
                        column.ColumnName = column.ColumnName.Split('(', ')')[0];
                        //Console.WriteLine("pressure unit: {0}", unit);
                        break;
                    }
                }
            }
            bool nmfilter;
            float nmfilter_coef;
            if (ol_xbar_nmfilter_cb.Checked == true)
            {
                nmfilter = true;
                nmfilter_coef = float.Parse(ol_xbar_nmfilter_coef_tb.Text);
            }
            else
            {
                nmfilter = false;
                nmfilter_coef = 0;
            }
            OlDetection.Save_MeanAndStd(csvData, nmfilter, nmfilter_coef);
        }

        private void rtd_datapath_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                rtd_datapath_tb.Text = ofd.FileName;
                DataTable csvData = new DataTable();
                csvData = BasicFunction.read_csvfile(csvData, ofd.FileName);
                DateTime dt_start = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString());
                DateTime dt_next = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
                string time_step = (dt_next - dt_start).ToString();
                rtd_xbar_timestep_tb.Text = time_step;
                rtd_EWMA_timestep_tb.Text = time_step;
                rtd_Cusum_timestep_tb.Text = time_step;
                //rtd_SHESD_timestep_tb.Text = time_step;
            }
        }

        private void rtd_parameterpath_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                rtd_parameterpath_tb.Text = ofd.FileName;
                //initiate 
                DataTable csvParameter = new DataTable();
                csvParameter = BasicFunction.read_csvfile(csvParameter, rtd_parameterpath_tb.Text);
                
                //outlier
                bool xbar_nmfilter = csvParameter.Rows[1]["outlier value"].ToString() == "TRUE" ? true : false;
                double? xbar_nmfilter_coef = Convert.ToDouble(csvParameter.Rows[2]["outlier value"].ToString());
                double? xbar_coef = Convert.ToDouble(csvParameter.Rows[3]["outlier value"].ToString());
                rtd_xbar_nmfilter_cb.Checked = xbar_nmfilter;
                rtd_xbar_nmfilter_coef_tb.Text = xbar_nmfilter_coef.ToString();
                rtd_xbar_coef_tb.Text = xbar_coef.ToString();

                bool EWMA_nmfilter = csvParameter.Rows[5]["outlier value"].ToString() == "TRUE" ? true : false;
                double EWMA_nmfilter_coef = Convert.ToDouble(csvParameter.Rows[6]["outlier value"].ToString());
                double EWMA_coef = Convert.ToDouble(csvParameter.Rows[7]["outlier value"].ToString());
                double EWMA_lmd = Convert.ToDouble(csvParameter.Rows[8]["outlier value"].ToString());
                rtd_EWMA_nmfilter_cb.Checked = EWMA_nmfilter;
                rtd_EWMA_nmfilter_coef_tb.Text = EWMA_nmfilter_coef.ToString();
                rtd_EWMA_coef_tb.Text = EWMA_coef.ToString();
                rtd_EWMA_lmd_tb.Text = EWMA_lmd.ToString();

                bool cusum_nmfilter = csvParameter.Rows[10]["outlier value"].ToString() == "TRUE" ? true : false;
                double? cusum_nmfilter_coef = Convert.ToDouble(csvParameter.Rows[11]["outlier value"].ToString());
                double? cusum_coef = Convert.ToDouble(csvParameter.Rows[12]["outlier value"].ToString());
                rtd_cusum_nmfilter_cb.Checked = cusum_nmfilter;
                rtd_cusum_nmfilter_coef_tb.Text = cusum_nmfilter_coef.ToString();
                rtd_cusum_coef_tb.Text = cusum_coef.ToString();

                int select_method = Convert.ToInt32(csvParameter.Rows[2]["Median"].ToString());
                rtd_method_tab.SelectedIndex = select_method;
                rtd_method_tab.Enabled = false;
            }
        }
        /*
        private void rtd_identify_bt_Click(object sender, EventArgs e)
        {
            DataTable csvPar = new DataTable();
            csvPar = BasicFunction.read_csvfile(csvPar, rtd_parameterpath_tb.Text);
            double sample_mean = Convert.ToDouble(csvPar.Rows[0]["Value"].ToString());
            double sample_std = Convert.ToDouble(csvPar.Rows[1]["Value"].ToString());

            foreach (var series in rtd_original_chart.Series)
            {
                series.Points.Clear();
            }
            foreach (var series in rtd_remainder_chart.Series)
            {
                series.Points.Clear();
            }
            try
            {
                DataTable csvData = new DataTable();
                csvData = BasicFunction.read_csvfile(csvData, rtd_datapath_tb.Text);
                //unit
                foreach (DataColumn column in csvData.Columns)
                {
                    if (column.ColumnName.Contains("."))  //Value.unit., read unit, R cant read "()" in column names and will change them into "."
                    {
                        string unit = column.ColumnName.Split('.', '.')[1];
                        current_unit = '(' + unit + ')';
                        //if flow unit
                        if (flow_unit_set.Any(unit.Contains))
                        {
                            flow_unit = unit;
                            column.ColumnName = column.ColumnName.Split('.', '.')[0];
                            //Console.WriteLine("flow unit: {0}", unit);
                            break;
                        }
                        //if pressure unit
                        else if (pressure_unit_set.Any(unit.Contains))
                        {
                            pressure_unit = unit;
                            column.ColumnName = column.ColumnName.Split('.', '.')[0];
                            //Console.WriteLine("pressure unit: {0}", unit);
                            break;
                        }
                    }
                    else if (column.ColumnName.Contains("("))  //Value(unit), read unit
                    {
                        string unit = column.ColumnName.Split('(', ')')[1];
                        current_unit = '(' + unit + ')';
                        //if flow unit
                        if (flow_unit_set.Any(unit.Contains))
                        {
                            flow_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("flow unit: {0}", unit);
                            break;
                        }
                        //if pressure unit
                        else if (pressure_unit_set.Any(unit.Contains))
                        {
                            pressure_unit = unit;
                            column.ColumnName = column.ColumnName.Split('(', ')')[0];
                            //Console.WriteLine("pressure unit: {0}", unit);
                            break;
                        }
                    }
                }
                //
                rtd_original_chart.Show();
                rtd_remainder_chart.Show();
                rtd_result_dgv.Show();
                TimeSpan ts = TimeSpan.Parse(rtd_xbar_timestep_tb.Text);
                float time_step = float.Parse(ts.TotalMinutes.ToString());
                int selected_index = rtd_method_tab.SelectedIndex;
                bool nmfilter;
                float nmfilter_coef;
                switch (selected_index)
                {
                    //Xbar
                    case 0:
                        rtd_methodname_lb.Text = "Xbar";
                        float xbar_coef = float.Parse(rtd_xbar_coef_tb.Text);
                        if (rtd_xbar_nmfilter_cb.Checked == true)
                        {
                            nmfilter = true;
                            nmfilter_coef = float.Parse(rtd_xbar_nmfilter_coef_tb.Text);
                        }
                        else
                        {
                            nmfilter = false;
                            nmfilter_coef = 0;
                        }
                        csvData = RealTimeData.Xbar_ol(csvData, sample_mean, sample_std, xbar_coef, time_step);
                        break;
                    //EWMA
                    case 1:
                        rtd_methodname_lb.Text = "EWMA";
                        float EWMA_coef = float.Parse(rtd_EWMA_coef_tb.Text);
                        if (rtd_EWMA_nmfilter_cb.Checked == true)
                        {
                            nmfilter = true;
                            nmfilter_coef = float.Parse(rtd_EWMA_nmfilter_coef_tb.Text);
                        }
                        else
                        {
                            nmfilter = false;
                            nmfilter_coef = 0;
                        }
                        float lamda = float.Parse(rtd_EWMA_lmd_tb.Text);
                        csvData = RealTimeData.EWMA_ol(csvData, sample_mean, sample_std, EWMA_coef, lamda, time_step);
                        break;
                    //Cusum
                    case 2:
                        rtd_methodname_lb.Text = "Cusum";
                        float csm_coef = float.Parse(rtd_cusum_coef_tb.Text);
                        if (rtd_cusum_nmfilter_cb.Checked == true)
                        {
                            nmfilter = true;
                            nmfilter_coef = float.Parse(rtd_cusum_nmfilter_coef_tb.Text);
                        }
                        else
                        {
                            nmfilter = false;
                            nmfilter_coef = 0;
                        }
                        csvData = RealTimeData.Cusum_ol(csvData, sample_mean, sample_std, csm_coef, time_step);
                        break;
                    //SH-ESD
                    case 3:
                        ol_methodname_lb.Text = "SH-ESD";
                        float long_term = float.Parse(ol_SHESD_longterm_tb.Text);
                        CharacterVector ReadPath = engine.CreateCharacterVector(new[] { ol_path_tb.Text });
                        engine.SetSymbol("read_filepath", ReadPath);
                        engine.Evaluate("p1 <- read.csv(read_filepath, header = TRUE)");
                        engine.Evaluate("library(AnomalyDetection)");
                        string period = (1440 / time_step).ToString();
                        string max_ts = ol_SHESD_longterm_tb.Text;
                        string shesd_anom_cmd = @"p1_anoms = AnomalyDetectionVec(p1$Value, max_anoms=0.2, period=" + period + @", direction='both', 
                                                  only_last = FALSE, threshold = 'None', plot = TRUE, longterm_period = " + max_ts + @", 
                                                  e_value = TRUE)";
                        engine.Evaluate(shesd_anom_cmd);
                        string shesd_save_path;
                        SaveFileDialog save_esd = new SaveFileDialog();
                        save_esd.Filter = "CSV|*.csv";
                        DialogResult esd_result = save_esd.ShowDialog();
                        if (esd_result == DialogResult.OK)
                        {
                            shesd_save_path = save_esd.FileName;
                            CharacterVector RSave_path = engine.CreateCharacterVector(new[] { shesd_save_path });
                            engine.SetSymbol("RSave_path", RSave_path);
                            engine.Evaluate("write.csv(p1_anoms$anoms, RSave_path)");
                        }
                        else
                        {
                            return;
                        }
                        csvData = OlDetection.SH_ESD_ol(csvData, shesd_save_path);
                        break;
                    default:
                        break;
                }

                //display result in tablegridview
                //csvData.Columns.Remove("Column 1");
                csvData.Columns[0].ColumnName = "No.";
                rtd_result_dgv.Refresh();


                rtd_result_dgv.DataSource = csvData;
                rtd_result_dgv.Columns["Value"].DefaultCellStyle.Format = "N2";
                rtd_result_dgv.Columns["trend"].DefaultCellStyle.Format = "N2";
                rtd_result_dgv.Columns["seasonal"].DefaultCellStyle.Format = "N2";
                if (rtd_result_dgv.Columns.Contains("new remainder"))
                {//sh-esd does not have new remainder column
                    rtd_result_dgv.Columns["new remainder"].DefaultCellStyle.Format = "N2";
                    rtd_result_dgv.Columns["remainder"].Visible = false;
                }
                else
                {
                    csvData.Columns["remainder"].ColumnName = "new remainder";
                }
                if (rtd_result_dgv.Columns.Contains("out difference"))
                {//ewma and cusum and sh-esd does not have out diff column
                    rtd_result_dgv.Columns["out difference"].DefaultCellStyle.Format = "N2";
                }
                //ol_result_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                foreach (DataGridViewRow row in rtd_result_dgv.Rows)
                {
                    row.HeaderCell.Value = (row.Index + 1).ToString();
                }
                //display result in chart
                string[] file_split = rtd_datapath_tb.Text.Split('\\');
                string sensor_id = file_split[file_split.Length - 1].Split('.')[0];
                rtd_original_chart.Titles[0].Text = "Sensor: " + sensor_id + current_unit + " Original";
                rtd_remainder_chart.Titles[0].Text = "Sensor: " + sensor_id + current_unit + " Residual";

                rtd_original_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                rtd_original_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                rtd_original_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                rtd_original_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                rtd_original_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                rtd_original_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                rtd_original_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

                rtd_remainder_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                rtd_remainder_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                rtd_remainder_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                rtd_remainder_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                rtd_remainder_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                rtd_remainder_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                rtd_remainder_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                for (int i = 0; i < csvData.Rows.Count; i++)
                {
                    if (csvData.Rows[i]["Warning"].ToString() == "")
                    {
                        rtd_original_chart.Series["Time series"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);

                        rtd_remainder_chart.Series["Time series"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["new remainder"]);

                    }
                    else if (csvData.Rows[i]["Warning"].ToString() == "High")
                    {
                        rtd_original_chart.Series["High outliers"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);

                        rtd_remainder_chart.Series["High outliers"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["new remainder"]);
                    }
                    else
                    {
                        rtd_original_chart.Series["Low outliers"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["Value"]);

                        rtd_remainder_chart.Series["Low outliers"].Points.AddXY
                              (csvData.Rows[i]["Timestamp"], csvData.Rows[i]["new remainder"]);
                    }
                }

                string save_path;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV|*.csv";
                DialogResult result = sfd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    save_path = sfd.FileName;
                    foreach (DataColumn column in csvData.Columns)
                    {
                        if (column.ColumnName == "Value" && current_unit != "(null)")
                        {
                            column.ColumnName = column.ColumnName + current_unit;
                        }
                    }
                    BasicFunction.WriteToCsvFile(csvData, save_path);
                }
            }
            catch (FormatException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        */
        private void rtd_xbar_nmfilter_cb_CheckedChanged(object sender, EventArgs e)
        {
            rtd_xbar_nmfilter_coef_tb.Enabled = rtd_xbar_nmfilter_cb.Checked;
        }

        private void rtd_EWMA_nmfilter_cb_CheckedChanged(object sender, EventArgs e)
        {
            rtd_EWMA_nmfilter_coef_tb.Enabled = rtd_EWMA_nmfilter_cb.Checked;
        }

        private void rtd_cusum_nmfilter_cb_CheckedChanged(object sender, EventArgs e)
        {
            rtd_cusum_nmfilter_coef_tb.Enabled = rtd_cusum_nmfilter_cb.Checked;
        }

        private void daily_avg_test_bt_Click(object sender, EventArgs e)
        {
            DataTable csvData = new DataTable();
            string path = "D:\\Work\\NYSR data\\remainder data\\flow remainder.csv";
            csvData = BasicFunction.read_csvfile(csvData, path);
            csvData = BasicFunction.DailyAverageFlow(csvData);
            //csvData = BasicFunction.MiniNightFlow(csvData);
            sse_result_dgv.DataSource = csvData;
        }

        private void ste_mnf_cb_CheckedChanged(object sender, EventArgs e)
        {
            ste_flow_chart.Series[5].Enabled = ste_mnf_cb.Checked;
            ste_flow_chart.ChartAreas[0].RecalculateAxesScale();

            ste_pressure_chart.Series[5].Enabled = ste_mnf_cb.Checked;
            ste_pressure_chart.ChartAreas[0].RecalculateAxesScale();
        }

        private void ste_dailyavg_cb_CheckedChanged(object sender, EventArgs e)
        {
            ste_flow_chart.Series[4].Enabled = ste_dailyavg_cb.Checked;
            ste_flow_chart.ChartAreas[0].RecalculateAxesScale();

            ste_pressure_chart.Series[4].Enabled = ste_dailyavg_cb.Checked;
            ste_pressure_chart.ChartAreas[0].RecalculateAxesScale();
        }

        private void ste_result_dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int dgv_index = ste_result_dgv.CurrentRow.Index;
            string event_start_date = ste_result_dgv.Rows[dgv_index].Cells["Timestamp"].Value.ToString();
            string event_duration = ste_result_dgv.Rows[dgv_index].Cells["duration"].Value.ToString();
            int duration = Convert.ToInt32(event_duration) / 60;
            duration = duration > 72 ? duration : 72;
            DateTime event_s_date = Convert.ToDateTime(event_start_date);
            event_s_date = event_s_date.AddDays(-1);
            ste_starttime_tb.Text = event_s_date.ToString();
            ste_timespan_tb.Text = duration.ToString();
        }

        private void ste_edit_confidencelvl_bt_Click(object sender, EventArgs e)
        {
            EventConfidenceLevel ecfl_form = new EventConfidenceLevel();
            ecfl_form.ShowDialog();
        }

        private void ol_ekf_filepath_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                ol_ekf_filepath_tb.Text = ofd.FileName;
            }
        }

        private void sse_save_model_bt_Click(object sender, EventArgs e)
        {
            SaveModelDetails.missing_time_cb = missing_time_cb.Checked;
            SaveModelDetails.duplicated_time_cb = duplicated_time_cb.Checked;
            SaveModelDetails.irregular_time_cb = irregular_time_cb.Checked;

            SaveModelDetails.dpp_sensorfailure_cb = dpp_sensorfailure_cb.Checked;
            SaveModelDetails.dpp_minstep_tb = dpp_minstep_tb.Text;
            SaveModelDetails.dpp_minvalue_tb = dpp_minvalue_tb.Text;
            SaveModelDetails.dpp_maxvalue_tb = dpp_maxvalue_tb.Text;
            //dcp
            SaveModelDetails.dcp_path_tb = ol_path_tb.Text;
            SaveModelDetails.dcp_period_tb = dcp_period_tb.Text;
            //ol
            SaveModelDetails.ol_xbar_timestep_tb = ol_xbar_timestep_tb.Text;
            SaveModelDetails.ol_xbar_nmfilter_cb = ol_xbar_nmfilter_cb.Checked;
            SaveModelDetails.ol_xbar_nmfilter_coef_tb = ol_xbar_nmfilter_coef_tb.Text;
            SaveModelDetails.ol_xbar_coef_tb = ol_xbar_coef_tb.Text;

            SaveModelDetails.ol_EWMA_timestep_tb = ol_EWMA_timestep_tb.Text;
            SaveModelDetails.ol_EWMA_nmfilter_cb = ol_EWMA_nmfilter_cb.Checked;
            SaveModelDetails.ol_EWMA_nmfilter_coef_tb = ol_EWMA_nmfilter_coef_tb.Text;
            SaveModelDetails.ol_EWMA_coef_tb = ol_EWMA_coef_tb.Text;
            SaveModelDetails.ol_EWMA_lmd_tb = ol_EWMA_lmd_tb.Text;

            SaveModelDetails.ol_Cusum_timestep_tb = ol_Cusum_timestep_tb.Text;
            SaveModelDetails.ol_cusum_nmfilter_cb = ol_cusum_nmfilter_cb.Checked;
            SaveModelDetails.ol_cusum_nmfilter_coef_tb = ol_cusum_nmfilter_coef_tb.Text;
            SaveModelDetails.ol_cusum_coef_tb = ol_cusum_coef_tb.Text;
            //sse
            SaveModelDetails.sse_sensorname_tb = sse_sensorname_tb.Text;
            SaveModelDetails.sse_min_duration_tb = sse_min_duration_tb.Text;
            SaveModelDetails.sse_timegap_tb = sse_timegap_tb.Text;
            SaveModelDetails SMD = new SaveModelDetails();
            SMD.ShowDialog();
        }

        private void rtd_loadmodel_bt_Click(object sender, EventArgs e)
        {
            foreach (var series in rt_event_chart.Series)
            {
                series.Points.Clear();
            }
            /////////read parameter
            DataTable csvParameter = new DataTable();
            csvParameter = BasicFunction.read_csvfile(csvParameter, rtd_parameterpath_tb.Text);

            double median_value = Convert.ToDouble(csvParameter.Rows[0]["Median"].ToString());
            double mean_value = Convert.ToDouble(csvParameter.Rows[4]["Median"].ToString());
            double std_value = Convert.ToDouble(csvParameter.Rows[6]["Median"].ToString());
            double select_method = Convert.ToDouble(csvParameter.Rows[2]["Median"].ToString());
            //dpp
            bool dpp_missing_ts = csvParameter.Rows[0]["dpp value"].ToString() == "true" ? true : false;
            bool dpp_duplicated_ts = csvParameter.Rows[1]["dpp value"].ToString() == "true" ? true : false;
            bool dpp_irregular_ts = csvParameter.Rows[2]["dpp value"].ToString() == "true" ? true : false;
            bool dpp_sensor_failure = csvParameter.Rows[3]["dpp value"].ToString() == "true" ? true : false;
            int dpp_min_step = Convert.ToInt32(csvParameter.Rows[4]["dpp value"].ToString());
            double dpp_min_value = Convert.ToDouble(csvParameter.Rows[5]["dpp value"].ToString());
            double dpp_max_value = Convert.ToDouble(csvParameter.Rows[6]["dpp value"].ToString());
            //outlier
            bool xbar_nmfilter = csvParameter.Rows[1]["outlier value"].ToString() == "true" ? true : false;
            double xbar_nmfilter_coef = Convert.ToDouble(csvParameter.Rows[2]["outlier value"].ToString());
            double xbar_coef = Convert.ToDouble(csvParameter.Rows[3]["outlier value"].ToString());
            rtd_xbar_nmfilter_cb.Checked = xbar_nmfilter;
            rtd_xbar_nmfilter_coef_tb.Text = xbar_nmfilter_coef.ToString();
            rtd_xbar_coef_tb.Text = xbar_coef.ToString();

            bool EWMA_nmfilter = csvParameter.Rows[5]["outlier value"].ToString() == "true" ? true : false;
            double EWMA_nmfilter_coef = Convert.ToDouble(csvParameter.Rows[6]["outlier value"].ToString());
            double EWMA_coef = Convert.ToDouble(csvParameter.Rows[7]["outlier value"].ToString());
            double EWMA_lmd = Convert.ToDouble(csvParameter.Rows[8]["outlier value"].ToString());
            rtd_EWMA_nmfilter_cb.Checked = EWMA_nmfilter;
            rtd_EWMA_nmfilter_coef_tb.Text = EWMA_nmfilter_coef.ToString();
            rtd_EWMA_coef_tb.Text = EWMA_coef.ToString();
            rtd_EWMA_lmd_tb.Text = EWMA_lmd.ToString();

            bool cusum_nmfilter = csvParameter.Rows[10]["outlier value"].ToString() == "true" ? true : false;
            double cusum_nmfilter_coef = Convert.ToDouble(csvParameter.Rows[11]["outlier value"].ToString());
            double cusum_coef = Convert.ToDouble(csvParameter.Rows[12]["outlier value"].ToString());
            rtd_cusum_nmfilter_cb.Checked = cusum_nmfilter;
            rtd_cusum_nmfilter_coef_tb.Text = cusum_nmfilter_coef.ToString();
            rtd_cusum_coef_tb.Text = cusum_coef.ToString();
            //sensor event
            string sensor_name = csvParameter.Rows[0]["sse value"].ToString();
            double min_duration = Convert.ToDouble(csvParameter.Rows[1]["sse value"].ToString());
            double time_gap = Convert.ToDouble(csvParameter.Rows[2]["sse value"].ToString());
            ////////end read parameter

            ////////read data file
            DataTable csvData = new DataTable();
            csvData = BasicFunction.read_csvfile(csvData, rtd_datapath_tb.Text);
            //time step
            DateTime dt1 = Convert.ToDateTime(csvData.Rows[0]["Timestamp"].ToString());
            DateTime dt2 = Convert.ToDateTime(csvData.Rows[1]["Timestamp"].ToString());
            double time_step = (dt2 - dt1).TotalMinutes;
            //
            csvData = BasicFunction.get_unit(csvData);
            //dpp
            csvData = LoadModel.Data_pre_process(csvData,
                                                 dpp_missing_ts,
                                                 dpp_duplicated_ts,
                                                 dpp_irregular_ts,
                                                 dpp_sensor_failure,
                                                 dpp_min_step,
                                                 dpp_min_value,
                                                 dpp_max_value);
            //dcp
            csvData = LoadModel.Decompose(csvData, csvParameter, median_value, time_step);
            DataView dv_dcp = new DataView(csvData);
            csvData = dv_dcp.ToTable("Selected", false, "Timestamp", "Value", "new remainder");
            //outlier
            int selected_index = rtd_method_tab.SelectedIndex;
            switch (selected_index)
            {
                //Xbar
                case 0:
                    rtd_methodname_lb.Text = "Xbar";
                    csvData = LoadModel.XbarOutlier(csvData, xbar_nmfilter, xbar_nmfilter_coef, xbar_coef, time_step, mean_value, std_value);
                    break;
                //EWMA
                case 1:
                    rtd_methodname_lb.Text = "EWMA";
                    csvData = LoadModel.EWMA_Outlier(csvData, EWMA_nmfilter, EWMA_nmfilter_coef, EWMA_coef, EWMA_lmd, time_step, mean_value, std_value);
                    break;
                //Cusum
                case 2:
                    rtd_methodname_lb.Text = "Cusum";
                    csvData = LoadModel.Cusum_Outlier(csvData, cusum_nmfilter, cusum_nmfilter_coef, cusum_coef, time_step, mean_value, std_value);
                    break;
                default:
                    break;
            }
            //sensor event
            csvData = LoadModel.Sensor_Event(csvData, sensor_name, time_gap, min_duration);
            ////////

            ////////visulization, chart and table
            DataTable ol_dt = csvData.Copy();
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                if (csvData.Rows[i]["duration"].ToString() != "")
                {
                    if (float.Parse(csvData.Rows[i]["duration"].ToString()) < min_duration)
                    //if (float.Parse(csvData.Rows[i]["duration"].ToString()) == 15)
                    {
                        csvData.Rows[i]["duration"] = "";
                    }
                }
            }
            //daily average and mnf
            csvData = BasicFunction.DailyAverageFlow(csvData);
            //display result in chart
            rt_event_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            rt_event_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            rt_event_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            rt_event_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            rt_event_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
            rt_event_chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            rt_event_chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

            for (int i = 0; i < ol_dt.Rows.Count; i++)
            {
                //normal value
                rt_event_chart.Series[0].Points.AddXY
                          (ol_dt.Rows[i]["Timestamp"], ol_dt.Rows[i]["Value"]);
            }
            int gap = (int)time_step;
            rt_event_chart.Titles[0].Text = "Sensor:" + sensor_name + current_unit;

            int ol_pointer = 0;
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                //high and low event
                if (csvData.Rows[i]["Warning"].ToString() == "High" && csvData.Rows[i]["duration"].ToString() != "")
                {
                    int event_steps = Convert.ToInt32(csvData.Rows[i]["duration"].ToString()) / gap;
                    string event_start = csvData.Rows[i]["Timestamp"].ToString();
                    while (ol_pointer < ol_dt.Rows.Count)
                    {
                        if (ol_dt.Rows[ol_pointer]["Timestamp"].ToString() == event_start)
                        {
                            break;
                        }
                        else
                        {
                            ol_pointer++;

                        }
                    }
                    for (int j = 0; j < event_steps; j++)
                    {
                        //high events
                        rt_event_chart.Series[1].Points.AddXY
                              (ol_dt.Rows[ol_pointer + j]["Timestamp"], ol_dt.Rows[ol_pointer + j]["Value"]);
                    }
                }
                //low events
                else if (csvData.Rows[i]["Warning"].ToString() == "Low" && csvData.Rows[i]["duration"].ToString() != "")
                {
                    int event_steps = Convert.ToInt32(csvData.Rows[i]["duration"].ToString()) / gap;
                    string event_start = csvData.Rows[i]["Timestamp"].ToString();
                    while (ol_pointer < ol_dt.Rows.Count)
                    {
                        if (ol_dt.Rows[ol_pointer]["Timestamp"].ToString() == event_start)
                        {
                            break;
                        }
                        else
                        {
                            ol_pointer++;

                        }
                    }
                    for (int j = 0; j < event_steps; j++)
                    {
                        rt_event_chart.Series[2].Points.AddXY
                              (ol_dt.Rows[ol_pointer + j]["Timestamp"], ol_dt.Rows[ol_pointer + j]["Value"]);
                    }
                }
            }
            //diaplay result in datagridview
            DataView dv = new DataView(csvData);
            dv.RowFilter = "duration <> ''";
            //string filter_info = "duration > " + sse_min_duration_tb.Text;
            //dv.RowFilter = filter_info;
            csvData = dv.ToTable();

            csvData.Columns.Remove("Name");
            csvData.Columns.Add("Event No.", typeof(string));
            csvData.Columns["Event No."].SetOrdinal(0);
            csvData.Columns["Timestamp"].ColumnName = "Start time";
            csvData.Columns["duration"].SetOrdinal(2);
            csvData.Columns["Warning"].SetOrdinal(3);
            csvData.Columns["duration"].ColumnName = "duration(min)";
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                csvData.Rows[i][0] = i + 1;
            }
            rtd_result_dgv.Refresh();
            rtd_result_dgv.DataSource = csvData;
            rtd_result_dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            //sse_result_dgv.Columns["remainder"].Visible = false;
            rtd_result_dgv.Columns["Value"].DefaultCellStyle.Format = "N2";
            if (rtd_result_dgv.Columns.Contains("Daily Average"))
            {
                rtd_result_dgv.Columns["Daily Average"].DefaultCellStyle.Format = "N2";
                rtd_result_dgv.Columns["Minimal night flow"].DefaultCellStyle.Format = "N2";
            }
            //sse_result_dgv.Columns["new remainder"].DefaultCellStyle.Format = "N2";
            if (rtd_result_dgv.Columns.Contains("out difference"))
            {
                rtd_result_dgv.Columns["out difference"].DefaultCellStyle.Format = "N2";
            }
            if (rtd_result_dgv.Columns.Contains("new remainder"))
            {//sh-esd does not have new remainder column
                rtd_result_dgv.Columns["new remainder"].DefaultCellStyle.Format = "N2";
                if (rtd_result_dgv.Columns.Contains("remainder"))
                {
                    rtd_result_dgv.Columns["remainder"].Visible = false;
                }
            }
            ////////
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV|*.csv";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string save_path = sfd.FileName;
                BasicFunction.WriteToCsvFile(csvData, save_path);
            }
        }

        private void EKF_select_file_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Filter = "CSV|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string file_full_path = ofd.FileName;
                EKF_file_path_tb.Text = Path.GetDirectoryName(file_full_path);
            }
        }

        private void EKF_run_cmd_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "EXE|*.exe";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                var proc1 = new ProcessStartInfo();
                proc1.UseShellExecute = true;

                //proc1.WorkingDirectory = @"D:\\Work\\ArguentTest\\Release";
                //proc1.WorkingDirectory = @"D:\Work\FromQiaoLi9-10-2018\EKF\x64\Debug";

                //proc1.FileName = @"D:\Work\FromQiaoLi9-10-2018\EKF\x64\Debug\EKF.exe";
                proc1.FileName = ofd.FileName;
                proc1.Verb = "runas";
                //proc1.Arguments = "/c " + "this is a test";
                //string EKF_cmd = "train EKF.txt training_weightDBN.txt training_weightEKF.txt s_covariance.txt p_covariance.txt lowerboundM.txt upperBoundM.txt predict_train.txt";
                string EKF_cmd = "train " + EKF_train_cfgfile_name_tb.Text + " " + EKF_DBNtrain_weight_tb.Text + " " + EKF_EKFtrain_weight_tb.Text + " "
                                 + EKF_train_s_cov_tb.Text + " " + EKF_train_p_cov_tb.Text + " " + EKF_train_lowerbound_tb.Text + " " 
                                 + EKF_train_upperbound_tb.Text + " " + EKF_train_prediction_tb.Text;
                string file_location = " " + EKF_file_path_tb.Text + "\\";
                Console.WriteLine(EKF_cmd + file_location);
                proc1.Arguments = EKF_cmd + file_location;
                //proc1.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(proc1);
            }
        }

        private void EKF_prediction_bt_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "EXE|*.exe";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                var proc1 = new ProcessStartInfo();
                proc1.UseShellExecute = true;

                //proc1.WorkingDirectory = @"D:\\Work\\ArguentTest\\Release";
                //proc1.WorkingDirectory = @"D:\Work\FromQiaoLi9-10-2018\EKF\x64\Debug";

                //proc1.FileName = @"D:\Work\FromQiaoLi9-10-2018\EKF\x64\Debug\EKF.exe";
                proc1.FileName = ofd.FileName;
                proc1.Verb = "runas";
                //proc1.Arguments = "/c " + "this is a test";
                //string EKF_cmd = "test EKF.txt training_weightEKF.txt s_covariance.txt p_covariance.txt updated_training_weightEKF.txt s_updated_covariance.txt p_updated_covariance.txt lowerboundMP.txt upperBoundMP.txt predict_test.txt";
                string EKF_cmd = "test " + EKF_predict_cfg_tb.Text + " " + EKF_predict_weight_tb.Text + " " + EKF_predict_s_cov_tb.Text + " " + EKF_predict_p_cov_tb.Text + " "
                                 + EKF_predict_new_weight_tb.Text + " " + EKF_predict_new_s_cov_tb.Text + " " + EKF_predict_new_p_cov_tb.Text + " "
                                 + EKF_predict_lowerbound_tb.Text + " " + EKF_predict_upperbound_tb.Text + " " + EKF_predict_predict_tb.Text;
                //string file_location = " D:\\Work\\FromQiaoLi9-10-2018\\flow1\\";
                string file_location = " " + EKF_file_path_tb.Text + "\\";
                Console.WriteLine(EKF_cmd + file_location);
                proc1.Arguments = EKF_cmd + file_location;
                //proc1.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(proc1);
            }
        }

        private void EKF_train_tocsv_bt_Click(object sender, EventArgs e)
        {
            
        }

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
                string[] str = path.Split('\\');
                string sensor_name = str[str.Length - 1].Split('.')[0];
                temp.Columns.Add("Name", typeof(string));
                for(int i = 0; i < temp.Rows.Count; i++)
                {
                    temp.Rows[i]["Name"] = sensor_name;
                }
                /*
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
                }*/
                csvData.Merge(temp);
            }

            DataTable flow_event = new DataTable();
            flow_event = BasicFunction.read_csvfile(flow_event, flow_path);
            /*
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
            flow_event = dv_flow.ToTable("Selected", false, "Timestamp", "Name", "Value", "Warning", "duration", "out difference");*/
            string[] fstr = flow_path.Split('\\');
            string fsensor_name = fstr[fstr.Length - 1].Split('.')[0];
            flow_event.Columns.Add("Name", typeof(string));
            for (int i = 0; i < flow_event.Rows.Count; i++)
            {
                flow_event.Rows[i]["Name"] = fsensor_name;
            }
            if (flow_event.Rows[0]["Name"].ToString() != "")
            {
                flow_sensor_name = flow_event.Rows[0]["Name"].ToString();
            }
            csvData.Merge(flow_event);

            DataView dv_sort = new DataView(csvData);
            dv_sort.Sort = "Timestamp";
            csvData = dv_sort.ToTable();
            
            double timegap = Convert.ToDouble(ste_pre_only_timegap_tb.Text); 
            // csvData = SystemEvent.pressure_only_event(csvData, timegap);
            csvData = SystemEvent.cluster_evetns(csvData, timegap, 2);

            /**** Show result ****/
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
            /*
            for (int i = 0; i < csvData.Rows.Count; i++)
            {
                if (csvData.Rows[i]["Name"].ToString() == flow_name)
                {
                    event_num++;
                    ste_result_dgv.Rows[i].HeaderCell.Value = event_num.ToString();
                }
            }*/
            //cell alignment
            ste_result_dgv.Columns["Timestamp"].Width = 140;
            //ste_result_dgv.Columns["Name"].Width = 50;
            ste_result_dgv.Columns["Value"].Width = 50;
            //ste_result_dgv.Columns["duration"].Width = 50;
            //ste_result_dgv.Columns["Warning"].Width = 50;
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
        }

        private void button1_Click_1(object sender, EventArgs e)
        {/*
            BasicFunction.reverseVerify2(24);
            BasicFunction.reverseVerify2(48);
            BasicFunction.reverseVerify2(72);
            BasicFunction.reverseVerify2(96);
            BasicFunction.reverseVerify2(120);*/
            BasicFunction.reverseVerify2(144);/*
            BasicFunction.verifyPUBGroundTrue(24);
            BasicFunction.verifyPUBGroundTrue(48);
            BasicFunction.verifyPUBGroundTrue(72);
            BasicFunction.verifyPUBGroundTrue(96);
            BasicFunction.verifyPUBGroundTrue(120);
            BasicFunction.verifyPUBGroundTrue(144);*/

            //BasicFunction.culmative_event_score();
        }
    }
}
