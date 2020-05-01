namespace new_anom
{
    partial class SystemEventChart
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.flow_chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.ste_pressure_chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.flow_chart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ste_pressure_chart)).BeginInit();
            this.SuspendLayout();
            // 
            // flow_chart
            // 
            this.flow_chart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.VariableCount;
            chartArea1.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Minutes;
            chartArea1.Name = "ChartArea1";
            this.flow_chart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.flow_chart.Legends.Add(legend1);
            this.flow_chart.Location = new System.Drawing.Point(0, 0);
            this.flow_chart.Name = "flow_chart";
            series1.BorderWidth = 3;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.Legend = "Legend1";
            series1.Name = "Flow";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series2.BorderWidth = 5;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series2.Color = System.Drawing.Color.Red;
            series2.Legend = "Legend1";
            series2.Name = "Event";
            this.flow_chart.Series.Add(series1);
            this.flow_chart.Series.Add(series2);
            this.flow_chart.Size = new System.Drawing.Size(903, 275);
            this.flow_chart.TabIndex = 5;
            this.flow_chart.Text = "chart1";
            this.flow_chart.Click += new System.EventHandler(this.flow_chart_Click);
            // 
            // ste_pressure_chart
            // 
            this.ste_pressure_chart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.AxisX.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.VariableCount;
            chartArea2.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Minutes;
            chartArea2.Name = "ChartArea1";
            this.ste_pressure_chart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.ste_pressure_chart.Legends.Add(legend2);
            this.ste_pressure_chart.Location = new System.Drawing.Point(0, 271);
            this.ste_pressure_chart.Name = "ste_pressure_chart";
            this.ste_pressure_chart.Size = new System.Drawing.Size(903, 275);
            this.ste_pressure_chart.TabIndex = 6;
            this.ste_pressure_chart.Text = "chart1";
            // 
            // SystemEventChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(901, 558);
            this.Controls.Add(this.ste_pressure_chart);
            this.Controls.Add(this.flow_chart);
            this.Name = "SystemEventChart";
            this.Text = "SystemEventChart";
            this.Load += new System.EventHandler(this.SystemEventChart_Load);
            ((System.ComponentModel.ISupportInitialize)(this.flow_chart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ste_pressure_chart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart flow_chart;
        private System.Windows.Forms.DataVisualization.Charting.Chart ste_pressure_chart;
    }
}