using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace BCstats {
    /// <summary>
    /// StationStatsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StationStatsWindow : Window {

        #region SQLite 常量定义
        private SQLiteHelper sqliteHelper = new SQLiteHelper(BCstatsHelper.connectionString);
        #endregion

        public StationStatsWindow(Window owner) {
            // 子窗口跟随主窗口移动
            this.Owner = MainWindow.instance;;
            InitializeComponent();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StationStatsWindow_Loaded(object sender, RoutedEventArgs e) {
            this.GetStationBCStats();
        }

        /// <summary>
        /// 获取台站当月的各种播出时长数据
        /// </summary>
        public void GetStationBCStats() {
            // 获取主窗口的控件状态信息
            MainWindow mw = Application.Current.Windows[0] as MainWindow;
            DataTable dt = new DataTable();
            if (mw.cboxCity.SelectedIndex != -1
                && mw.cboxStation.SelectedIndex != -1) {

                int year = Convert.ToInt32(mw.scbYear.Value);
                int month = Convert.ToInt32(mw.scbMonth.Value);

                this.Title = mw.cboxStation.Text + " "
                        + mw.scbYear.Value.ToString() + "年"
                        + mw.scbMonth.Value.ToString() + "月"
                        + "播出统计";

                int nostop2 = Convert.ToInt16(mw.scbNoStop2.Value);
                int nostop3 = Convert.ToInt16(mw.scbNoStop3.Value);

                string city = mw.cboxCity.Text;
                string station = mw.cboxStation.Text;

                // 台站的所有节目数量，各种类型的节目数量
                int total = sqliteHelper.GetDataCount(BCstatsHelper.connectionString,
                    BCstatsHelper.STR_STATION, station);

                // 列表：节目类型
                List<string> categoryList = sqliteHelper.GetColunmValues(BCstatsHelper.connectionString,
                    BCstatsHelper.STR_CATEGORY,
                    BCstatsHelper.STR_STATION, station);
                // 列表：节目名称
                List<string> nameList = sqliteHelper.GetColunmValues(BCstatsHelper.connectionString,
                    BCstatsHelper.STR_NAME,
                    BCstatsHelper.STR_STATION, station);
                // 列表：节目频率
                List<string> frequencyList = sqliteHelper.GetColunmValues(BCstatsHelper.connectionString,
                    BCstatsHelper.STR_FREQUENCY,
                    BCstatsHelper.STR_STATION, station);
                // DataTable 定义列
                dt.Columns.Add(BCstatsHelper.STR_STATION);
                dt.Columns.Add(BCstatsHelper.STR_CATEGORY);
                dt.Columns.Add(BCstatsHelper.STR_NAME);
                dt.Columns.Add(BCstatsHelper.STR_FREQUENCY);
                dt.Columns.Add(BCstatsHelper.STR_HOURS);

                // DataTable 填充数据
                for (int i = 0; i < total; i++) {
                    DataRow dr = dt.NewRow();
                    dr[BCstatsHelper.STR_CATEGORY] = categoryList[i];
                    dr[BCstatsHelper.STR_NAME] = nameList[i];
                    dr[BCstatsHelper.STR_FREQUENCY] = frequencyList[i];
                    // 各节目的播出时长
                    dr[BCstatsHelper.STR_HOURS] = GetFrequencyHours(year, month,
                        mw.rbtnLastTuesday, nostop2, nostop3,
                        city, station, frequencyList[i]);
                    dt.Rows.Add(dr);
                }

                // 绑定数据
                StationStats_DataGrid.ItemsSource = dt.DefaultView;
                // 数据排序:根据节目类型
                (StationStats_DataGrid.ItemsSource as DataView).Sort = BCstatsHelper.STR_CATEGORY;
                StationStats_DataGrid.CanUserDeleteRows = false;
                StationStats_DataGrid.CanUserAddRows = false;
                StationStats_DataGrid.CanUserSortColumns = true;
                // 各个类型的播出时长之和
                double swHours = GetHoursByCategory(dt, BCstatsHelper.STR_SW);
                double mwHours = GetHoursByCategory(dt, BCstatsHelper.STR_MW);
                double fmHours = GetHoursByCategory(dt, BCstatsHelper.STR_FM);
                double tvHours = GetHoursByCategory(dt, BCstatsHelper.STR_TV);

                // 台站当月所有节目的总播出时长
                //ArrayList hoursList = new ArrayList();
                //for (int i = 0; i < qty; i++) {
                //    hoursList.Add(Convert.ToDouble(dt.Rows[i][BCstatsHelper.STR_HOURS]));
                //}
                //double sumHours = hoursList.Cast<double>().Sum();
                double sumHours = swHours + mwHours + fmHours + tvHours;

                int swQuantity = GetQuantityByCategory(dt, BCstatsHelper.STR_SW);
                int mwQuantity = GetQuantityByCategory(dt, BCstatsHelper.STR_MW);
                int fmQuantity = GetQuantityByCategory(dt, BCstatsHelper.STR_FM);
                int tvQuantity = GetQuantityByCategory(dt, BCstatsHelper.STR_TV);
                lblSW.Content = BCstatsHelper.STR_SW_CN + "(" + swQuantity + ")";
                lblMW.Content = BCstatsHelper.STR_MW_CN + "(" + mwQuantity + ")";
                lblFM.Content = BCstatsHelper.STR_FM_CN + "(" + fmQuantity + ")";
                tbDTV.Text = BCstatsHelper.STR_TV_CN + "(" + tvQuantity + ")";

                tbxSW.Text = swHours.ToString();
                tbxMW.Text = mwHours.ToString();
                tbxFM.Text = fmHours.ToString();
                tbxDTV.Text = tvHours.ToString();
                tbxAllHours.Text = sumHours.ToString();
            }
        }


        /// <summary>
        /// 某个市的某个台站的某个节目的某年某月的播出时长
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="rbtnLastTuesday"></param>
        /// <param name="nostop2"></param>
        /// <param name="nostop3"></param>
        /// <param name="city"></param>
        /// <param name="station"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        private string GetFrequencyHours(int year, int month, 
            RadioButton rbtnLastTuesday, int nostop2, int nostop3,
            string city, string station, string frequency) {
            try {
                double off_time_hour, off_time_min, on_time_hour, on_time_min;
                bool stop3, stop2, stopLast2;
                double off_time_2_hour, off_time_2_min,on_time_2_hour, on_time_2_min;
                double off_time_3_hour, off_time_3_min,on_time_3_hour, on_time_3_min;

                // 停止播出
                string off_time = sqliteHelper.GetTimeValue(city, station,
                    frequency, BCstatsHelper.STR_OFF_TIME);
                off_time_hour = Convert.ToDouble(off_time.Substring(0, 2));
                off_time_min = Convert.ToDouble(off_time.Substring(2, 2));

                // 开始播出
                string on_time = sqliteHelper.GetTimeValue(city, station,
                    frequency, BCstatsHelper.STR_ON_TIME);
                on_time_hour = Convert.ToDouble(on_time.Substring(0, 2));
                on_time_min = Convert.ToDouble(on_time.Substring(2, 2));

                // 周三是否停机检修
                int stop_3 = sqliteHelper.GetIntValue(city, station,
                    frequency, BCstatsHelper.STR_STOP_3);
                if (stop_3 == 1)
                    stop3 = true;
                else stop3 = false;
                // 周三停播
                string off_time_3 = sqliteHelper.GetTimeValue(city, station,
                    frequency, BCstatsHelper.STR_OFF_TIME_3);
                off_time_3_hour = Convert.ToDouble(off_time_3.Substring(0, 2));
                off_time_3_min = Convert.ToDouble(off_time_3.Substring(2, 2));
                // 周三播出
                string on_time_3 = sqliteHelper.GetTimeValue(city, station,
                    frequency, BCstatsHelper.STR_ON_TIME_3);
                on_time_3_hour = Convert.ToDouble(on_time_3.Substring(0, 2));
                on_time_3_min = Convert.ToDouble(on_time_3.Substring(2, 2));

                // 周二是否停机检修
                int stop_2 = sqliteHelper.GetIntValue(city, station,
                    frequency, BCstatsHelper.STR_STOP_2);
                if (stop_2 == 1)
                    stop2 = true;
                else stop2 = false;
                // 周二停播
                string off_time_2 = sqliteHelper.GetTimeValue(city, station,
                    frequency, BCstatsHelper.STR_OFF_TIME_2);
                off_time_2_hour = Convert.ToDouble(off_time_2.Substring(0, 2));
                off_time_2_min = Convert.ToDouble(off_time_2.Substring(2, 2));
                // 周二播出
                string on_time_2 = sqliteHelper.GetTimeValue(city, station,
                    frequency, BCstatsHelper.STR_ON_TIME_2);
                on_time_2_hour = Convert.ToDouble(on_time_2.Substring(0, 2));
                on_time_2_min = Convert.ToDouble(on_time_2.Substring(2, 2));

                // 最后一个周二是否停机检修
                int stop_last_2 = sqliteHelper.GetIntValue(city, station,
                    frequency, BCstatsHelper.STR_STOP_LAST_2);
                if (stop_last_2 == 1) {
                    stopLast2 = true;
                    rbtnLastTuesday.IsChecked = true;
                    // 最后一个周二停机检修
                } else {
                    stopLast2 = false;
                    rbtnLastTuesday.IsChecked = false;
                    // 最后一个周二不停机检修
                }

                // 当月播出总时长
                return BCstatsHelper.TotalHoursOfTheMonth(
                                                year, month,
                                                nostop2, nostop3,
                                                off_time_hour, off_time_min,
                                                on_time_hour, on_time_min,
                                                stop3,
                                                off_time_3_hour, off_time_3_min,
                                                on_time_3_hour, on_time_3_min,
                                                stop2,
                                                off_time_2_hour, off_time_2_min,
                                                on_time_2_hour, on_time_2_min,
                                                stopLast2).ToString();
            } catch {
                return "0";
            }
        }


        /// <summary>
        /// 获取DataTable中某个类型节目的播出时长之和
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        private double GetHoursByCategory(DataTable dt, string category) {
            try {
                ArrayList list = new ArrayList();
                string expression = BCstatsHelper.STR_CATEGORY + "='" + category + "'";
                string sortOrder = BCstatsHelper.STR_CATEGORY + " DESC";
                DataRow[] dr = dt.Select(expression, sortOrder);
                for (int i = 0; i < dr.Length; i++) {
                    list.Add(Convert.ToDouble((dr[i][4])));
                }
                // 用linq对列表元素求和
                return list.Cast<double>().Sum();
            } catch {
                return 0;
            }
        }


        /// <summary>
        /// 根据节目类型，获取相应类型节目的数量
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        private int GetQuantityByCategory(DataTable dt, string category) {
            try {
                ArrayList list = new ArrayList();
                string expression = BCstatsHelper.STR_CATEGORY + "='" + category + "'";
                DataRow[] dr = dt.Select(expression);
                return dr.Length;
            } catch {
                return 0;
            }
        }



        /// <summary>
        /// 按钮：关闭子窗口，主窗口位置回归屏幕中央
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ss_quit_Click(object sender, RoutedEventArgs e) {
            this.Close();
            // 主窗口位置恢复到屏幕中央
            MainWindow mw = Application.Current.Windows[0] as MainWindow;
            mw.Left = MainWindow.mwLeft;
            mw.Top = MainWindow.mwTop;

        }
        /// <summary>
        /// 按钮：刷新统计数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btn_ss_refresh_Click(object sender, RoutedEventArgs e) {
            StationStatsWindow_Loaded(sender, e);
        }


    }

}
