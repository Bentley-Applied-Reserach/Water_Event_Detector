using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace new_anom
{
    public partial class SystemEventChart : Form
    {
        public static DataTable sys_event_dt;
        public static int flow_index;
        public static string flow_path;
        public static string[] pressure_path;
        public SystemEventChart()
        {
            InitializeComponent();
        }
        
        
        private void SystemEventChart_Load(object sender, EventArgs e)
        {
            //prepare data
            DataTable dt_flow = new DataTable();
            dt_flow = BasicFunction.read_csvfile(dt_flow, flow_path);
            double flow_timestep;
            DateTime dt1 = Convert.ToDateTime(dt_flow.Rows[0]["Timestamp"].ToString());
            DateTime dt2 = Convert.ToDateTime(dt_flow.Rows[1]["Timestamp"].ToString());
            TimeSpan ts = dt2 - dt1;
            flow_timestep = ts.TotalMinutes;

            DataSet ds_pressure = new DataSet();
            for(int i = 0; i < pressure_path.Length; i++)
            {
                DataTable temp = new DataTable();
                temp = BasicFunction.read_csvfile(temp, pressure_path[i]);
                ds_pressure.Tables.Add(temp);
                string sensor_name;
                for(int j = 0; j < temp.Rows.Count; j++)
                {
                    if(temp.Rows[j]["Name"].ToString() != "")
                    {
                        sensor_name = temp.Rows[j]["Name"].ToString();
                        ds_pressure.Tables[i].TableName = sensor_name;
                        break;
                    }
                }
            }
            double pressure_timestep;
            dt1 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[0]["Timestamp"].ToString());
            dt2 = Convert.ToDateTime(ds_pressure.Tables[0].Rows[1]["Timestamp"].ToString());
            ts = dt2 - dt1;
            pressure_timestep = ts.TotalMinutes;
            //find flow index
            string flow_event_start = sys_event_dt.Rows[flow_index]["Timestamp"].ToString();
            int flow_event_step = (int)(Convert.ToDouble(sys_event_dt.Rows[flow_index]["duration"].ToString()) / flow_timestep);
            int flow_event_start_index = 0;
            int flow_start_index = 0;
            int flow_end_index = dt_flow.Rows.Count;
            for(int i = 0; i < dt_flow.Rows.Count; i++)
            {
                if(dt_flow.Rows[i]["Timestamp"].ToString() == flow_event_start)
                {
                    flow_event_start_index = i;
                    flow_start_index = i - flow_event_step / 2 >= 0? i - flow_event_step / 2 : 0;
                    flow_end_index = flow_start_index + 2 * flow_event_step < dt_flow.Rows.Count ?
                                     flow_start_index + 2 * flow_event_step : dt_flow.Rows.Count;
                    break;
                }
            }
            //display flow chart
            flow_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            flow_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            flow_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            flow_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            flow_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";
            for (int i = flow_start_index; i < flow_end_index; i++)
            {
                flow_chart.Series[0].Points.AddXY
                                  (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Value"]);
                if(i >= flow_event_start_index && i <= flow_event_start_index + flow_event_step)
                {
                    flow_chart.Series[1].Points.AddXY
                                  (dt_flow.Rows[i]["Timestamp"], dt_flow.Rows[i]["Value"]);
                }
            }
            //display pressure
            string selected_name = sys_event_dt.Rows[flow_index]["Name"].ToString();

            ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            ste_pressure_chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            ste_pressure_chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            ste_pressure_chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            ste_pressure_chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm";

            ste_pressure_chart.ChartAreas[0].CursorX.Interval = 0;
            ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Minutes;
            ste_pressure_chart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 1D;
            //
            for (int i = flow_index + 1; i < sys_event_dt.Rows.Count; i++)
            {
                string current_sensor_name = sys_event_dt.Rows[i]["Name"].ToString();
                if (current_sensor_name == selected_name)
                {
                    break;
                }
                else
                {
                    string series_name = current_sensor_name;
                    try
                    {
                        //creat new line
                        ste_pressure_chart.Series.Add(current_sensor_name);
                        ste_pressure_chart.Series[current_sensor_name].XValueType = ChartValueType.DateTime;
                        ste_pressure_chart.Series[current_sensor_name].ChartType = SeriesChartType.FastLine;
                        ste_pressure_chart.Series[current_sensor_name].BorderWidth = 3;
                    }
                    catch
                    {
                        series_name = current_sensor_name + "(" + i + ")";
                        ste_pressure_chart.Series.Add(series_name);
                        ste_pressure_chart.Series[series_name].XValueType = ChartValueType.DateTime;
                        ste_pressure_chart.Series[series_name].ChartType = SeriesChartType.FastLine;
                        ste_pressure_chart.Series[series_name].BorderWidth = 3;
                        //ste_pressure_chart.Series[series_name].Color = ste_pressure_chart.Series[current_sensor_name].Color;
                    }
                    //declare find index
                    string pre_event_start_date = sys_event_dt.Rows[i]["Timestamp"].ToString();
                    int pre_event_step = (int)(Convert.ToDouble(sys_event_dt.Rows[i]["duration"].ToString()) / pressure_timestep);
                    int pre_start_index = 0;
                    int pre_end_index = ds_pressure.Tables[current_sensor_name].Rows.Count;
                    for (int j = 0; j < ds_pressure.Tables[current_sensor_name].Rows.Count; j++)
                    {
                        if (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"].ToString() == pre_event_start_date)
                        {
                            pre_start_index = j;
                            break;
                        }
                    }
                    for (int j = pre_start_index; j <= pre_start_index + pre_event_step; j++)
                    {
                        ste_pressure_chart.Series[series_name].Points.AddXY
                         /* X */             (ds_pressure.Tables[current_sensor_name].Rows[j]["Timestamp"],
                         /* Y */              ds_pressure.Tables[current_sensor_name].Rows[j]["Value"]);
                    }
                }
            }
        }

        private void flow_chart_Click(object sender, EventArgs e)
        {

        }
    }
}
