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
    public partial class EventConfidenceLevel : Form
    {
        public EventConfidenceLevel()
        {
            InitializeComponent();
        }

        private void EventConfidenceLevel_Load(object sender, EventArgs e)
        {
            mnf_vh_tb.Text = (WaterEventDetector.mnf_vh * 100).ToString();
            mnf_h_tb.Text = (WaterEventDetector.mnf_h * 100).ToString();
            mnf_m_tb.Text = (WaterEventDetector.mnf_m * 100).ToString();
            mnf_l_tb.Text = (WaterEventDetector.mnf_l * 100).ToString();
            mnf_vl_tb.Text = (WaterEventDetector.mnf_vl * 100).ToString();

            pre_od_vh_tb.Text = (WaterEventDetector.pre_od_vh * 100).ToString();
            pre_od_h_tb.Text = (WaterEventDetector.pre_od_h * 100).ToString();
            pre_od_m_tb.Text = (WaterEventDetector.pre_od_m * 100).ToString();
            pre_od_l_tb.Text = (WaterEventDetector.pre_od_l * 100).ToString();
            pre_od_vl_tb.Text = (WaterEventDetector.pre_od_vl * 100).ToString();

            od_vh_tb.Text = (WaterEventDetector.flow_od_vh * 100).ToString();
            od_h_tb.Text = (WaterEventDetector.flow_od_vh * 100).ToString();
            od_m_tb.Text = (WaterEventDetector.flow_od_vh * 100).ToString();
            od_l_tb.Text = (WaterEventDetector.flow_od_vh * 100).ToString();
            od_vl_tb.Text = (WaterEventDetector.flow_od_vh * 100).ToString();

            mnf_cfd_level_cb.Checked = WaterEventDetector.mnf_cfd_level;
            flow_od_cfd_level_cb.Checked = WaterEventDetector.flow_od_cfd_level;
            pre_od_cfd_level_cb.Checked = WaterEventDetector.pre_od_cfd_level;
        }

        private void ecl_save_parameter_bt_Click(object sender, EventArgs e)
        {
            WaterEventDetector.mnf_vh = Convert.ToDouble(mnf_vh_tb.Text) / 100;
            WaterEventDetector.mnf_h = Convert.ToDouble(mnf_h_tb.Text) / 100;
            WaterEventDetector.mnf_m = Convert.ToDouble(mnf_m_tb.Text) / 100;
            WaterEventDetector.mnf_l = Convert.ToDouble(mnf_l_tb.Text) / 100;
            WaterEventDetector.mnf_vl = Convert.ToDouble(mnf_vl_tb.Text) / 100;

            WaterEventDetector.pre_od_vh = Convert.ToDouble(pre_od_vh_tb.Text) / 100;
            WaterEventDetector.pre_od_h = Convert.ToDouble(pre_od_h_tb.Text) / 100;
            WaterEventDetector.pre_od_m = Convert.ToDouble(pre_od_m_tb.Text) / 100;
            WaterEventDetector.pre_od_l = Convert.ToDouble(pre_od_l_tb.Text) / 100;
            WaterEventDetector.pre_od_vl = Convert.ToDouble(pre_od_vl_tb.Text) / 100;

            WaterEventDetector.flow_od_vh = Convert.ToDouble(od_vh_tb.Text) / 100;
            WaterEventDetector.flow_od_h = Convert.ToDouble(od_h_tb.Text) / 100;
            WaterEventDetector.flow_od_m = Convert.ToDouble(od_m_tb.Text) / 100;
            WaterEventDetector.flow_od_l = Convert.ToDouble(od_l_tb.Text) / 100;
            WaterEventDetector.flow_od_vl = Convert.ToDouble(od_vl_tb.Text) / 100;
        }

        private void mnf_cfd_level_cb_CheckedChanged(object sender, EventArgs e)
        {
            WaterEventDetector.mnf_cfd_level = mnf_cfd_level_cb.Checked;
        }

        private void flow_od_cfd_level_cb_CheckedChanged(object sender, EventArgs e)
        {
            WaterEventDetector.flow_od_cfd_level = flow_od_cfd_level_cb.Checked;
        }

        private void pre_od_cfd_level_cb_CheckedChanged(object sender, EventArgs e)
        {
            WaterEventDetector.pre_od_cfd_level = pre_od_cfd_level_cb.Checked;
        }
    }
}
