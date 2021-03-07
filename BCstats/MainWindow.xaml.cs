using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;     // System.Windows.Threading.DispatcherTimer
using System.Collections.Generic;
using System.Windows.Controls.Primitives;


namespace BCstats {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        #region SQLite 常量定义
        SQLiteHelper sqliteHelper = new SQLiteHelper(BCstatsHelper.connectionString);
        #endregion

        #region
        public static MainWindow instance;
        public static double mwLeft, mwTop;
        #endregion

        public MainWindow() {
            InitializeComponent();
            // 用于子窗口根秀主窗口移动
            instance = this;
        }


        #region 主窗口 事件
        /// <summary>
        /// 主窗口：初始化控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mWindow_Loaded(object sender, RoutedEventArgs e) {
            // 初始化
            try {
                // 默认打开的TabItem： 3 台站播出统计
                tabControl1.SelectedIndex = 3;

                #region 显示系统当前时间
                // 定义名叫timer的DispatcherTimer
                System.Windows.Threading.DispatcherTimer timer;
                // 实例化
                timer = new System.Windows.Threading.DispatcherTimer();
                // 超过计时器间隔的时候触发
                timer.Tick += new EventHandler(timer_Tick);
                // 启动计时
                timer.Start();
                // 显示当前时间
                DateTime dt = System.DateTime.Now;
                lblNow1.Content = dt.ToString();
                lblNow2.Content = dt.ToString();
                lblNow3.Content = dt.ToString();
                #endregion

                // 数据库和数据表都存在，才进行读取数据库操作
                if (sqliteHelper.CheckDataBase(BCstatsHelper.dbFileName)
                    && sqliteHelper.CheckDataTable(BCstatsHelper.connectionString, BCstatsHelper.tableName)) {
                    // 台站播出统计：读取city字段
                    getComboBoxBinding(cboxCity, sqliteHelper, BCstatsHelper.connectionString,
                    BCstatsHelper.STR_CITY, null, null, "InitializeComponent");
                    // 月播出时间统计：读取city字段
                    getComboBoxBinding(cbx_city, sqliteHelper, BCstatsHelper.connectionString,
                    BCstatsHelper.STR_CITY, null, null, "InitializeComponent");
                }

                #region 台站播出统计 初始化
                // 滚动条 ScrollBar 年、月
                scbYear.Value = System.DateTime.Now.Year;
                scbMonth.Value = System.DateTime.Now.Month;
                scbMonth.Minimum = 1;

                // 周二 / 周三的数量：有几个周二 / 周三
                // 本月1日 - 本月30/31日
                // 滚动条的最大值：几个周二/周三不停机检修
                int intYear = Convert.ToInt32(scbYear.Value);
                int intMonth = Convert.ToInt32(scbMonth.Value);
                scbNoStop2.Maximum =
                    BCstatsHelper.DayOfWeekConutOfTheMonth(intYear, intMonth, DayOfWeek.Tuesday);
                scbNoStop3.Maximum =
                    BCstatsHelper.DayOfWeekConutOfTheMonth(intYear, intMonth, DayOfWeek.Wednesday);
                // 滚动条：初始化，显示的值
                scbNoStop2.Value = 0;
                scbNoStop3.Value = 0;
                #endregion

                #region 月播出时间统计 初始化
                // 月播出时间统计：单选框 周二下午停机检修 
                chkBox_Tuesday.IsChecked = false;
                // 滚动条 ScrollBar 年、月
                scb_Year.Value = System.DateTime.Now.Year;
                scb_Month.Value = System.DateTime.Now.Month;
                scb_Month.Minimum = 1;
                int int_Year = Convert.ToInt32(scbYear.Value);
                int int_Month = Convert.ToInt32(scbMonth.Value);
                // 周二 / 周三的数量：有几个周二 / 周三
                // 本月1日 - 本月30/31日
                // 滚动条的最大值：几个周二/周三不停机检修
                scb_NoStop2.Maximum =
                    BCstatsHelper.DayOfWeekConutOfTheMonth(int_Year, int_Month, DayOfWeek.Tuesday);
                scb_NoStop3.Maximum =
                    BCstatsHelper.DayOfWeekConutOfTheMonth(int_Year, int_Month, DayOfWeek.Wednesday);

                // 滚动条：初始化，显示的值
                scb_NoStop2.Value = 0;
                scb_NoStop3.Value = 0;

                #endregion


            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }

            mwLeft = this.Left;
            mwTop = this.Top;
        }
        /// <summary>
        /// 子窗口位置跟随主窗口位置改变而改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mWindow_LocationChanged(object sender, EventArgs e) {
            foreach (Window win in this.OwnedWindows) {
                win.Top = this.Top + (this.Height - win.ActualHeight) / 2;
                win.Left = this.Left + this.Width;
            }
        }

        #endregion


        /// <summary>
        /// ComboBox 下拉菜单 绑定数据项：一个字段中，不重复的字段值
        /// </summary>
        /// <param name="cbox">ComboBox</param>
        /// <param name="sqliteHelper">SQLiteHelper</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="columnName">字段名</param>
        /// <param name="whereName">where 字段名</param>
        /// <param name="whereValue">where 字段的值</param>
        /// <param name="functionName">控件的事件名称，用于报错</param>
        private void getComboBoxBinding(ComboBox cbox, SQLiteHelper sqliteHelper,
                                        string connectionString, 
                                        string columnName, 
                                        string whereName, string whereValue,
                                        string functionName) {
            try {
                //lblDetail.Text = BCstatsHelper.connectionString;
                // 先清除ComboBox下拉选项
                cbox.Items.Clear();

                string tableName = BCstatsHelper.tableName;

                List<string> columnValueList = null;
                //MessageBox.Show(sql);
                // 城市、台站，有重复
                if(columnName == BCstatsHelper.STR_CITY || columnName == BCstatsHelper.STR_STATION) {
                    // 获取数据表中某一列的所有不重复的值
                    columnValueList = sqliteHelper.GetColunmDistinctValues(connectionString, tableName,
                        columnName,
                        whereName, whereValue);
                } else {
                    // 获取数据表中某一列所有的值
                    columnValueList = sqliteHelper.GetColunmValues(connectionString, tableName,
                        columnName,
                        whereName, whereValue);
                }

                for (int i = 0; i < columnValueList.Count; i++) {
                    cbox.Items.Add(columnValueList[i]);
                }
                cbox.SelectedIndex = -1;
            } catch(Exception ex) {
                MessageBox.Show(functionName + " 事件错误：" + ex.ToString());
            }
        }


        /// <summary>
        /// 系统时间，刷新显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e) {
            // xxxx-x-xx 11:22:33 周几
            string time = string.Concat(DateTime.Now.ToString("G"))
                + " "
                + DateTime.Now.ToString("ddd",new System.Globalization.CultureInfo("zh-cn"));

            lblNow1.Content = time;
            lblNow2.Content = time; 
            lblNow3.Content = time;
        }


        /// <summary>
        /// 台站播出统计：按钮 弹出窗口，播出时间表设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPopup_TimeTable_Click(object sender, RoutedEventArgs e) {
            // 保持弹出窗口在主窗口前
            var popup = new TimeTableWindow(mWindow);
            popup.ShowDialog();
        }

        /// <summary>
        /// 台站播出统计：按钮 弹出窗口，台站播出统计
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPopUp_StationStats_Click(object sender, RoutedEventArgs e) {
            int windowsCount = Application.Current.Windows.Count;
            try {
                if (cboxCity.SelectedIndex != -1 && cboxStation.SelectedIndex != -1) {
                    // 未打开子窗口
                    if (windowsCount <= 2) {
                        this.Left = mwLeft;
                        this.Top = mwTop;
                        // 打开子窗口
                        var ssw = new StationStatsWindow(mWindow);
                        // 子窗口的起始位置：紧贴在主窗口右侧
                        ssw.Height = this.Height;
                        ssw.WindowStartupLocation = WindowStartupLocation.Manual;
                        ssw.Left = mwLeft + this.Width;
                        ssw.Top = mwTop + (this.Height - ssw.Height) / 2;
                        ssw.Title = cboxStation.Text + " "
                            + scbYear.Value.ToString() + "年"
                            + scbMonth.Value.ToString() + "月"
                            + "播出统计";
                        ssw.Show();
                        // 主窗口的位置：往左移，让软件的整个界面位置接近屏幕中央
                        this.Left = this.Left - ssw.Width + 120;
                        this.Top = mwTop;
                    } else {
                        // 已打开子窗口，则刷新子窗口数据
                        foreach (Window window in Application.Current.Windows) {
                            StationStatsWindow sw = Application.Current.Windows[2] as StationStatsWindow;
                            sw.StationStatsWindow_Loaded(sender, e);
                        }
                    }
                }
            } catch (Exception ex) {
                ;
            }
        }


        /// <summary>
        /// 刷新子窗口数据：台站播出时长统计
        /// </summary>
        private void RefreshStationStatsWindow() {
            //MessageBox.Show("num: " + Application.Current.Windows.Count);

            foreach (Window window in Application.Current.Windows) {
                if (window.Name == BCstatsHelper.STR_StationStatsWindow_NAME) {
                    StationStatsWindow sw 
                        = Application.Current.Windows[Application.Current.Windows.Count - 1] as StationStatsWindow;
                    sw.GetStationBCStats();
                }
            }

            //if (Application.Current.Windows.Count > 2) {
            //    StationStatsWindow sw 
            //        = Application.Current.Windows[Application.Current.Windows.Count - 1] as StationStatsWindow;
            //    sw.GetStationBCStats();
            //}
        }



        /// <summary>
        /// 通用 按钮 ，退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }


        #region TabItem3 台站播出统计 comboBox 下拉菜单 绑定数据库
        /// <summary>
        /// 台站播出统计 下拉菜单：地点，点击下拉的时候绑定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboxCity_DropDownClosed(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(cboxCity.Text) && cboxCity.Text.Length != 0) {
                getComboBoxBinding(cboxStation, sqliteHelper,
                    BCstatsHelper.connectionString,
                    BCstatsHelper.STR_STATION,
                    BCstatsHelper.STR_CITY, cboxCity.Text,
                    "cboxCity_DropDownClosed");
            }
        }
        /// <summary>
        /// 台站播出统计 下拉菜单：地点，选项改变的时候绑定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboxCity_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!string.IsNullOrEmpty(cboxCity.Text) && cboxCity.Text.Length != 0) {
                getComboBoxBinding(cboxStation, sqliteHelper,
                    BCstatsHelper.connectionString,
                    BCstatsHelper.STR_STATION,
                    BCstatsHelper.STR_CITY, cboxCity.Text,
                    "cboxCity_SelectionChanged");
            }
        }
        /// <summary>
        /// 台站播出统计 下拉菜单：台站，点击下拉的时候绑定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboxStation_DropDownClosed(object sender, EventArgs e) {
            getComboBoxBinding(cboxFrequency, sqliteHelper, BCstatsHelper.connectionString,
                BCstatsHelper.STR_FREQUENCY,
                BCstatsHelper.STR_STATION, cboxStation.Text,
                "cboxStation_DropDownClosed");
            Hours.Text = string.Empty;
            // 刷新子窗口数据
            this.RefreshStationStatsWindow();
        }

        /// <summary>
        /// 台站播出统计 下拉菜单：频率 点击下拉的时候，计算总播出时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboxFrequency_DropDownClosed(object sender, EventArgs e) {
            try {
                int year, month;
                string city, station;
                double off_time_hour, off_time_min, on_time_hour, on_time_min;
                bool stop3, stop2, stopLast2;
                double off_time_2_hour, off_time_2_min, on_time_2_hour, on_time_2_min;
                double off_time_3_hour, off_time_3_min, on_time_3_hour, on_time_3_min;

                // 3个cbx都选择了确定项
                if (cboxCity.SelectedIndex != -1 
                    && cboxStation.SelectedIndex != -1 
                    && cboxFrequency.SelectedIndex != -1) {
                    // 滚动条：年，月
                    year = Convert.ToInt32(scbYear.Value);
                    month = Convert.ToInt32(scbMonth.Value);

                    city = cboxCity.Text;
                    station = cboxStation.Text;

                    // 停止播出
                    string off_time = sqliteHelper.GetTimeValue(city, station, 
                        cboxFrequency.Text, BCstatsHelper.STR_OFF_TIME);
                    off_time_hour = Convert.ToDouble(off_time.Substring(0, 2));
                    off_time_min = Convert.ToDouble(off_time.Substring(2, 2));

                    // 开始播出
                    string on_time = sqliteHelper.GetTimeValue(city, station, 
                        cboxFrequency.Text, BCstatsHelper.STR_ON_TIME);
                    on_time_hour = Convert.ToDouble(on_time.Substring(0, 2));
                    on_time_min = Convert.ToDouble(on_time.Substring(2, 2));

                    // 周三停播
                    string off_time_3 = sqliteHelper.GetTimeValue(city, station, 
                        cboxFrequency.Text, BCstatsHelper.STR_OFF_TIME_3);
                    off_time_3_hour = Convert.ToDouble(off_time_3.Substring(0, 2));
                    off_time_3_min = Convert.ToDouble(off_time_3.Substring(2, 2));
                    // 周三是否停机检修
                    int stop_3 = sqliteHelper.GetIntValue(city, station,
                        cboxFrequency.Text, BCstatsHelper.STR_STOP_3);
                    if (stop_3 == 1)
                        stop3 = true;
                    else stop3 = false;
                    // 周三播出
                    string on_time_3 = sqliteHelper.GetTimeValue(city, station, 
                        cboxFrequency.Text, BCstatsHelper.STR_ON_TIME_3);
                    on_time_3_hour = Convert.ToDouble(on_time_3.Substring(0, 2));
                    on_time_3_min = Convert.ToDouble(on_time_3.Substring(2, 2));

                    // 周二是否停机检修
                    int stop_2 = sqliteHelper.GetIntValue(city, station, 
                        cboxFrequency.Text, BCstatsHelper.STR_STOP_2);
                    if (stop_2 == 1)
                        stop2 = true;
                    else stop2 = false;
                    // 周二停播
                    string off_time_2 = sqliteHelper.GetTimeValue(city, station, 
                        cboxFrequency.Text, BCstatsHelper.STR_OFF_TIME_2);
                    off_time_2_hour = Convert.ToDouble(off_time_2.Substring(0, 2));
                    off_time_2_min = Convert.ToDouble(off_time_2.Substring(2, 2));
                    // 周二播出
                    string on_time_2 = sqliteHelper.GetTimeValue(city, station, 
                        cboxFrequency.Text, BCstatsHelper.STR_ON_TIME_2);
                    on_time_2_hour = Convert.ToDouble(on_time_2.Substring(0, 2));
                    on_time_2_min = Convert.ToDouble(on_time_2.Substring(2, 2));

                    // 最后一个周二是否停机检修
                    int stop_last_2 = sqliteHelper.GetIntValue(city, station, 
                        cboxFrequency.Text, BCstatsHelper.STR_STOP_LAST_2);
                    if (stop_last_2 == 1) {
                        stopLast2 = true;
                        rbtnLastTuesday.IsChecked = true;
                        // 最后一个周二停机检修
                    } else {
                        stopLast2 = false;
                        rbtnLastTuesday.IsChecked = false;
                        // 最后一个周二不停机检修
                    }

                    Tuple<double, string> t = BCstatsHelper.TotalHoursOfTheMonth(
                                                   year, month,
                                                   Convert.ToInt32(scbNoStop2.Value),
                                                   Convert.ToInt32(scbNoStop3.Value),
                                                   off_time_hour, off_time_min,
                                                   on_time_hour, on_time_min,

                                                   stop3,
                                                   off_time_3_hour, off_time_3_min,
                                                   on_time_3_hour, on_time_3_min,

                                                   stop2,
                                                   off_time_2_hour, off_time_2_min,
                                                   on_time_2_hour, on_time_2_min,
                                                   stopLast2);

                    // 当月播出总时长
                    Hours.Text = t.Item1.ToString();
                    // 详细过程
                    lblDetail.Text = t.Item2;
                } else {
                    Hours.Text = "0";
                }
            } catch {
                Hours.Text = "0";
            }
        }

        #endregion

        #region TabItem3 台站播出统计 滚动条
        /// <summary>
        /// 台站播出统计：滚动条 共几个非周二/有几个周三不停机检修
        /// 当选择发生变化，直接计算播出时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabItem3_ScrollBar_Scroll(object sender, 
            System.Windows.Controls.Primitives.ScrollEventArgs e) {
            // 计算播出时长
            if (cboxCity.SelectedIndex != -1
                && cboxStation.SelectedIndex != -1
                && cboxFrequency.SelectedIndex != -1) {
                cboxFrequency_DropDownClosed(sender, e);
            }
            // 刷新子窗口数据
            if (cboxCity.SelectedIndex != -1 && cboxStation.SelectedIndex != -1) {
                RefreshStationStatsWindow();
            }
        }

        /// <summary>
        /// 台站播出统计：滚动条 年        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scbYear_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e) {
            int theYear = Convert.ToInt32(scbYear.Value);
            int theMonth = Convert.ToInt32(scbMonth.Value);
            // 当月天数
            int daysTheMonth = System.DateTime.DaysInMonth(theYear, theMonth);
            // 当月
            DateTime dateFrom = new DateTime(theYear, theMonth, 01);
            DateTime dateTo = new DateTime(theYear, theMonth, daysTheMonth);
            // 月份变化，周二/三的最大值重新界定，为当月的周二/三天数
            scbNoStop2.Maximum = BCstatsHelper.DayOfWeekConut(dateFrom, dateTo, DayOfWeek.Tuesday);
            scbNoStop3.Maximum = BCstatsHelper.DayOfWeekConut(dateFrom, dateTo, DayOfWeek.Wednesday);

            // 计算播出时长
            if (cboxCity.SelectedIndex != -1
                && cboxStation.SelectedIndex != -1
                && cboxFrequency.SelectedIndex != -1) {
                cboxFrequency_DropDownClosed(sender, e);
            }
            // 刷新子窗口数据
            // 刷新子窗口数据
            if (cboxCity.SelectedIndex != -1 && cboxStation.SelectedIndex != -1) {
                RefreshStationStatsWindow();
            }
        }
        /// <summary>
        /// 台站播出统计：滚动条 月
        /// 当选择发生变化，直接计算播出时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scbMonth_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e) {
            int theYear = Convert.ToInt32(scbYear.Value);
            int theMonth = Convert.ToInt32(scbMonth.Value);
            // 当月天数
            int daysTheMonth = System.DateTime.DaysInMonth(theYear, theMonth);
            // 当月
            DateTime dateFrom = new DateTime(theYear, theMonth, 01);
            DateTime dateTo = new DateTime(theYear, theMonth, daysTheMonth);
            // 月份变化，周二/三的最大值重新界定，为当月的周二/三天数
            scbNoStop2.Maximum = BCstatsHelper.DayOfWeekConut(dateFrom, dateTo, DayOfWeek.Tuesday);
            scbNoStop3.Maximum = BCstatsHelper.DayOfWeekConut(dateFrom, dateTo, DayOfWeek.Wednesday);

            // 计算播出时长
            if (cboxCity.SelectedIndex != -1
                && cboxStation.SelectedIndex != -1
                && cboxFrequency.SelectedIndex != -1) {
                cboxFrequency_DropDownClosed(sender, e);
            }
            // 刷新子窗口数据
            if (cboxCity.SelectedIndex != -1 && cboxStation.SelectedIndex != -1) {
                RefreshStationStatsWindow();
            }
        }

        #endregion


        #region TabItem2 停播率统计 按钮事件
        /// <summary>
        /// 停播率统计：StopAllTime方法，计算tiembox内的时间之和，得到停播总时长 
        /// </summary>
        /// <param name="uiControls"></param>
        private void StopAllTime(UIElementCollection uiControls) {
            // 每个tiembox的时长
            int hour = 0, min = 0, sec = 0;
            // 总时长
            int stopHours = 0, stopMinutes = 0, stopSeconds = 0;
            // 正则表达式：是否为数字，只保留数字
            var reg = new Regex("^[0-9]*$");
            // 转换为的字符串
            string each = "";
            // 遍历此grid2中所有textbox，判断冒号后去掉，取出时分秒分别相加
            foreach (UIElement element in uiControls) {   // 过滤掉指定的textbox 
                if (element is TextBox
                    && (element as TextBox).Name != "tbxStopTotalTime"
                    && (element as TextBox).Name != "tbxTotalTime"
                    && (element as TextBox).Name != "tbxStopRate") {
                    var str = (element as TextBox).Text.Trim();
                    var sb = new StringBuilder();
                    // 判断是否为数字，只保留数字
                    if (!reg.IsMatch(str)) {
                        for (int i = 0; i < str.Length; i++) {
                            if (reg.IsMatch(str[i].ToString())) { sb.Append(str[i].ToString()); }   //在新的sb后追加字符串。找到就存起来
                        }
                        // 去掉冒号和空格，先转为字符串
                        each = sb.ToString();
                        // 从左边开始取2个字符
                        hour = int.Parse(each.Substring(0, 2));
                        // 从左边第三个开始取2个字符
                        min = int.Parse(each.Substring(2, 2));
                        // 从右边开始取2个字符
                        sec = int.Parse(each.Substring(each.Length - 2));
                    }
                    // (element as TextBox).Text = ""; 清空所有textbox，成功
                }
                // 遍历中，每个都累加
                stopHours += hour;
                stopMinutes += min;
                stopSeconds += sec;
            };
            // 总秒数大于60，先除以60加到分，在取模保留秒位
            if (stopSeconds > 60) {
                stopMinutes = stopMinutes + stopSeconds / 60;
                stopSeconds = stopSeconds % 60;
            };
            // 总分钟数大于60，先除以60加到小时，在取模保留分位
            if (stopMinutes > 60) {
                stopHours = stopHours + stopMinutes / 60;
                stopMinutes = stopMinutes % 60;
            };

            // 总停止时长显示
            tbxStopTotalTime.Text = BCstatsHelper.GetStopTimeString(stopHours, stopMinutes, stopSeconds);
        }
        /// <summary>
        /// 停播率统计：按钮 计算停播率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetStopRate_Click(object sender, RoutedEventArgs e) {
            tbxStopRate.Text = "0";
            try {
                int hour = 0, min = 0, sec = 0; ;
                hour = int.Parse(tbxStopTotalTime.Text.Substring(0, 2));
                // 冒号和空格都算位数
                min = int.Parse(tbxStopTotalTime.Text.Substring(5, 2));
                sec = int.Parse(tbxStopTotalTime.Text
                    .Substring(tbxStopTotalTime.Text.Length - 2, 2));

                string str = tbxTotalTime.Text;
                // 提示错误？（因为预设的000 . 0有空格）
                double totaltime = Convert.ToDouble(str);
                // 播出总时长如果为0.0，结果直接为0
                if (totaltime == 0.0)
                    tbxStopRate.Text = "0";
                else {
                    double stopTotalTime;
                    // 停播总时间 = xxx秒
                    stopTotalTime = sec + min * 60 + hour * 3600;

                    tbxStopRate.Text = BCstatsHelper.GetStopRate(stopTotalTime, totaltime);
                }
            } catch (Exception ex) {
                MessageBox.Show("请输入数字：" + ex.ToString());
            }
        }
        /// <summary>
        /// 停播率统计：按钮 重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear2_Click(object sender, RoutedEventArgs e) {
            // 先清空再初始化
            TraverseVisualTree(grid2);
            clrgrid2tb(this.grid2.Children);
        }
        #endregion


        #region TabItem1 月播出时间统计 控件事件
        /// <summary>
        /// 月播出时间统计：按钮，计算播出总时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCalculate_Click(object sender, RoutedEventArgs e) {
            try {
                int year, month;
                double off_time_hour, off_time_min, on_time_hour, on_time_min;
                int nonStop2 = 0, nonStop3 = 0;
                bool stop3, stop2, stopLast2;
                double off_time_2_hour, off_time_2_min, on_time_2_hour, on_time_2_min;
                double off_time_3_hour, off_time_3_min, on_time_3_hour, on_time_3_min;

                // 限制年/月文本框输入范围
                if (Convert.ToInt16(scb_Year.Value) <= 1900) {
                    scb_Year.Value = System.DateTime.Now.Year;
                }
                if (Convert.ToInt16(scb_Month.Value) > 12
                    || Convert.ToInt16(scb_Month.Value) == 0) {
                    scb_Month.Value = System.DateTime.Now.Month;
                }

                // 滚动条：年，月
                year = Convert.ToInt32(scb_Year.Value);
                month = Convert.ToInt32(scb_Month.Value);

                // 停止播出
                off_time_hour = Convert.ToDouble(wtbxOffHour.Text);
                off_time_min = Convert.ToDouble(wtbxOffMin.Text);
                // 开始播出
                on_time_hour = Convert.ToDouble(wtbxOnHour.Text);
                on_time_min = Convert.ToDouble(wtbxOnMin.Text);
                // 周三是否停机检修
                if ((bool)chkBox_Wednesday.IsChecked) {
                    stop3 = true;
                } else {
                    stop3 = false;
                }
                // 周三停播
                off_time_3_hour = Convert.ToDouble(wtbxOffHour3.Text);
                off_time_3_min = Convert.ToDouble(wtbxOffMin3.Text);
                // 周三播出
                on_time_3_hour = Convert.ToDouble(wtbxOnHour3.Text);
                on_time_3_min = Convert.ToDouble(wtbxOnMin3.Text);

                // 周二是否停机检修
                if ((bool)chkBox_Tuesday.IsChecked) {
                    stop2 = true;
                } else {
                    stop2 = false;
                }
                // 周二停播
                off_time_2_hour = Convert.ToDouble(wtbxOffHour2.Text);
                off_time_2_min = Convert.ToDouble(wtbxOffMin2.Text);
                // 周二播出
                on_time_2_hour = Convert.ToDouble(wtbxOnHour2.Text);
                on_time_2_min = Convert.ToDouble(wtbxOnMin2.Text);

                // 最后一个周二是否停机检修
                // 先读取数据库
                if (cbx_frequency.Text != null) {
                    string city = cbx_city.Text;
                    string station = cbx_station.Text;
                    string stop_last_2 = sqliteHelper.GetTimeValue(city, station,
                        cbx_frequency.Text, BCstatsHelper.STR_STOP_LAST_2);
                    //MessageBox.Show("最后一个周二是否停机检修" + Convert.ToBoolean(stop_last_2));
                }

                if ((bool)chkBox_LastTuesday.IsChecked) {
                    stopLast2 = true;
                } else {
                    stopLast2 = false;
                }

                // 有几个周二/周三不停机检修
                if(String.IsNullOrEmpty(scb_NoStop2.Value.ToString())) {
                    scb_NoStop2.Value = 0;
                    nonStop2 = 0;
                } else {
                    nonStop2 = Convert.ToInt16(scb_NoStop2.Value);
                }
                if (String.IsNullOrEmpty(scb_NoStop3.Value.ToString())) {
                    scb_NoStop3.Value = 0;
                    nonStop3 = 0;
                } else {
                    nonStop3 = Convert.ToInt16(scb_NoStop3.Value);
                }

                Tuple<double, string> t = BCstatsHelper.TotalHoursOfTheMonth(
                                                   year, month,
                                                   nonStop2, nonStop3,
                                                   off_time_hour, off_time_min,
                                                   on_time_hour, on_time_min,
                                                   stop3,
                                                   off_time_3_hour, off_time_3_min,
                                                   on_time_3_hour, on_time_3_min,
                                                   stop2,
                                                   off_time_2_hour, off_time_2_min,
                                                   on_time_2_hour, on_time_2_min,
                                                   stopLast2
                                                   );

                // 当月播出总时长
                tbxTotalHours.Text = t.Item1.ToString();
                // 详细过程
                lblDetail.Text = t.Item2;
            } catch (Exception ex) {
                tbxTotalHours.Text = "0";
                MessageBox.Show("输入的内容有误：" + ex.ToString());
            }
        }

        /// <summary>
        /// 月播出时间统计：滚动条 年        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scb_Year_Scroll(object sender, ScrollEventArgs e) {
            int theYear = Convert.ToInt32(scb_Year.Value);
            int theMonth = Convert.ToInt32(scb_Month.Value);
            // 当月天数
            int daysTheMonth = System.DateTime.DaysInMonth(theYear, theMonth);
            // 当月
            DateTime dateFrom = new DateTime(theYear, theMonth, 01);
            DateTime dateTo = new DateTime(theYear, theMonth, daysTheMonth);
            // 月份变化，周二/三的最大值重新界定，为当月的周二/三天数
            scb_NoStop2.Maximum = BCstatsHelper.DayOfWeekConut(dateFrom, dateTo, DayOfWeek.Tuesday);
            scb_NoStop3.Maximum = BCstatsHelper.DayOfWeekConut(dateFrom, dateTo, DayOfWeek.Wednesday);
        }
        /// <summary>
        /// 月播出时间统计：滚动条 月
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scb_Month_Scroll(object sender, ScrollEventArgs e) {
            int theYear = Convert.ToInt32(scb_Year.Value);
            int theMonth = Convert.ToInt32(scb_Month.Value);
            // 当月天数
            int daysTheMonth = System.DateTime.DaysInMonth(theYear, theMonth);
            // 当月
            DateTime dateFrom = new DateTime(theYear, theMonth, 01);
            DateTime dateTo = new DateTime(theYear, theMonth, daysTheMonth);
            // 月份变化，周二/三的最大值重新界定，为当月的周二/三天数
            scb_NoStop2.Maximum = BCstatsHelper.DayOfWeekConut(dateFrom, dateTo, DayOfWeek.Tuesday);
            scb_NoStop3.Maximum = BCstatsHelper.DayOfWeekConut(dateFrom, dateTo, DayOfWeek.Wednesday);
        }

        /// <summary>
        /// 月播出时间统计：按钮 清空重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear1_Click(object sender, RoutedEventArgs e) {
            cbx_frequency.SelectedIndex = -1;

            // 停机检修
            this.chkBox_Tuesday.IsChecked = false;
            this.chkBox_LastTuesday.IsChecked = false;

            clrgrid1tb(this.grid1.Children);
        }

        /// <summary>
        /// 月播出时间统计：下拉菜单 地点 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbx_city_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!string.IsNullOrEmpty(cbx_city.Text) && cbx_city.Text.Length != 0) {
                getComboBoxBinding(cbx_station, sqliteHelper,
                    BCstatsHelper.connectionString,
                    BCstatsHelper.STR_STATION,
                    BCstatsHelper.STR_CITY, cbx_city.Text,
                    "cbx_city_SelectionChanged");
            }
        }
        /// <summary>
        /// 月播出时间统计：下拉菜单 地点 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbx_city_DropDownClosed(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(cbx_city.Text) && cbx_city.Text.Length != 0) {
                getComboBoxBinding(cbx_station, sqliteHelper,
                    BCstatsHelper.connectionString,
                    BCstatsHelper.STR_STATION,
                    BCstatsHelper.STR_CITY, cbx_city.Text,
                    "cbx_city_DropDownClosed");
            }
        }
        /// <summary>
        /// 月播出时间统计：下拉菜单 台站 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbx_station_DropDownClosed(object sender, EventArgs e) {
            getComboBoxBinding(cbx_frequency, sqliteHelper, BCstatsHelper.connectionString,
                BCstatsHelper.STR_FREQUENCY,
                BCstatsHelper.STR_STATION, cbx_station.Text,
                "cbx_station_DropDownClosed");
        }
        /// <summary>
        /// 月播出时间统计：下拉菜单 频率：填充多个输入框的时间值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbx_frequency_DropDownClosed(object sender, EventArgs e) {
            try {
                // 3个cbx都选择了确定项
                if (cbx_city.SelectedIndex != -1
                    && cbx_station.SelectedIndex != -1
                    && cbx_frequency.SelectedIndex != -1) {
                    // 滚动条：年，月
                    int year = Convert.ToInt32(scb_Year.Value);
                    int month = Convert.ToInt32(scb_Month.Value);
                    string city = cbx_city.Text;
                    string station = cbx_station.Text;
                    string frequency = cbx_frequency.Text;

                    // 停止播出
                    string off_time_Text = BCstatsHelper.STR_OFF_TIME;
                    string off_time = sqliteHelper.GetTimeValue(city, station, frequency, off_time_Text);
                    wtbxOffHour.Text = off_time.Substring(0, 2);
                    wtbxOffMin.Text =off_time.Substring(2, 2);
                    // 开始播出
                    string on_time_Text = BCstatsHelper.STR_ON_TIME;
                    string on_time = sqliteHelper.GetTimeValue(city, station, frequency, on_time_Text);
                    wtbxOnHour.Text = on_time.Substring(0, 2);
                    wtbxOnMin.Text = on_time.Substring(2, 2);

                    // 周三是否停机检修
                    int stop_3 = sqliteHelper.GetIntValue(city, station, frequency,
                        BCstatsHelper.STR_STOP_3);
                    if (stop_3 == 1) {
                        chkBox_Wednesday.IsChecked = true;
                    } else {
                        chkBox_Wednesday.IsChecked = false;
                        wtbxOffHour3.Text = "0";
                        wtbxOffMin3.Text = "0";
                        wtbxOnHour3.Text = "0";
                        wtbxOnMin3.Text = "0";
                    }
                    // 周三停播
                    string off_time_3_Text = BCstatsHelper.STR_OFF_TIME_3;
                    string off_time_3 = sqliteHelper.GetTimeValue(city, station, frequency, off_time_3_Text);
                    wtbxOffHour3.Text = off_time_3.Substring(0, 2);
                    wtbxOffMin3.Text = off_time_3.Substring(2, 2);
                    // 周三播出
                    string on_time_3_Text = BCstatsHelper.STR_ON_TIME_3;
                    string on_time_3 = sqliteHelper.GetTimeValue(city, station, frequency, on_time_3_Text);
                    wtbxOnHour3.Text = on_time_3.Substring(0, 2);
                    wtbxOnMin3.Text = on_time_3.Substring(2, 2);

                    // 周二是否停机检修
                    int stop_2 = sqliteHelper.GetIntValue(city, station, frequency, 
                        BCstatsHelper.STR_STOP_2);
                    if (stop_2 == 1) {
                        chkBox_Tuesday.IsChecked = true;
                    } else {
                        chkBox_Tuesday.IsChecked = false;
                        wtbxOffHour2.Text = "0";
                        wtbxOffMin2.Text = "0";
                        wtbxOnHour2.Text = "0";
                        wtbxOnMin2.Text = "0";
                    }

                    // 最后一个周二是否停机检修
                    int stop_last_2 = sqliteHelper.GetIntValue(city, station, frequency, 
                        BCstatsHelper.STR_STOP_LAST_2);
                    if (stop_last_2 == 1) {
                        chkBox_LastTuesday.IsChecked = true;
                    } else {
                        chkBox_LastTuesday.IsChecked = false;
                    }
                } else {
                    ;
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 月播出时间统计：复选框 周二是否检修
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkBox_Tuesday_Checked(object sender, RoutedEventArgs e) {
            wtbxOffHour2.Text = "13";
            wtbxOffMin2.Text = "00";
            wtbxOnHour2.Text = "17";
            wtbxOnMin2.Text = "30";
            // 可以编辑检修时间
            wtbxOffHour2.IsEnabled = true;
            wtbxOffMin2.IsEnabled = true;
            wtbxOnHour2.IsEnabled = true;
            wtbxOnMin2.IsEnabled = true;

            wtbxOffHour2.IsReadOnly = false;
            wtbxOffMin2.IsReadOnly = false;
            wtbxOnHour2.IsReadOnly = false;
            wtbxOnMin2.IsReadOnly = false;

            // 最后一个周二也停机检修
            chkBox_LastTuesday.IsChecked = true;
            chkBox_LastTuesday.IsEnabled = false;

        }
        private void chkBox_Tuesday_UnChecked(object sender, RoutedEventArgs e) {
            wtbxOffHour2.Text = "0";
            wtbxOffMin2.Text = "0";
            wtbxOnHour2.Text = "0";
            wtbxOnMin2.Text = "0";

            // 只读，不可编辑
            wtbxOffHour2.IsReadOnly = true;
            wtbxOffMin2.IsReadOnly = true;
            wtbxOnHour2.IsReadOnly = true;
            wtbxOnMin2.IsReadOnly = true;

            chkBox_LastTuesday.IsEnabled = true;
        }

        /// <summary>
        /// 月播出时间统计：单选框 当月最后一个周二停机检修
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkBox_LastTuesday_Checked(object sender, RoutedEventArgs e) {
            wtbxOffHour2.Text = "13";
            wtbxOffMin2.Text = "00";
            wtbxOnHour2.Text = "17";
            wtbxOnMin2.Text = "30";
            // 可以编辑检修时间
            wtbxOffHour2.IsEnabled = true;
            wtbxOffMin2.IsEnabled = true;
            wtbxOnHour2.IsEnabled = true;
            wtbxOnMin2.IsEnabled = true;

            wtbxOffHour2.IsReadOnly = false;
            wtbxOffMin2.IsReadOnly = false;
            wtbxOnHour2.IsReadOnly = false;
            wtbxOnMin2.IsReadOnly = false;
        }
        private void chkBox_LastTuesday_Unchecked(object sender, RoutedEventArgs e) {

        }

        /// <summary>
        /// 月播出时间统计：单选框 周三凌晨是否停机检修
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkBox_Wednesday_Checked(object sender, RoutedEventArgs e) {
            wtbxOffHour3.Text = "01";
            wtbxOffMin3.Text = "00";
            wtbxOnHour3.Text = "06";
            wtbxOnMin3.Text = "00";
            // 可以编辑检修时间
            wtbxOffHour3.IsEnabled = true;
            wtbxOffMin3.IsEnabled = true;
            wtbxOnHour3.IsEnabled = true;
            wtbxOnMin3.IsEnabled = true;

            wtbxOffHour3.IsReadOnly = false;
            wtbxOffMin3.IsReadOnly = false;
            wtbxOnHour3.IsReadOnly = false;
            wtbxOnMin3.IsReadOnly = false;
        }
        private void chkBox_Wednesday_Unchecked(object sender, RoutedEventArgs e) {
            wtbxOffHour3.Text = "0";
            wtbxOffMin3.Text = "0";
            wtbxOnHour3.Text = "0";
            wtbxOnMin3.Text = "0";

            // 只读，不可编辑
            wtbxOffHour3.IsReadOnly = true;
            wtbxOffMin3.IsReadOnly = true;
            wtbxOnHour3.IsReadOnly = true;
            wtbxOnMin3.IsReadOnly = true;
        }

        #endregion



        #region 所有的下拉菜单 事件 禁止鼠标滚轮
        private void cboxCity_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }
        private void cboxStation_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }
        private void cboxFrequency_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }
        private void cbx_city_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }
        private void cbx_station_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }
        private void cbx_frequency_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }
        #endregion

        #region 文本框 textbox 通用事件
        /// <summary>
        /// 停播率统计 屏蔽空格键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void textbox_PreviewKeyDown(object sender, KeyEventArgs e) {
            // 是空格键
            if (e.Key == Key.Space) {
                // 已经处理了事件（即不处理当前键盘事件）     
                e.Handled = true;
            }
        }
        /// <summary>
        /// 是否是数字键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PreviewTextinput(object sender, TextCompositionEventArgs e) {
            // !（输入文本是不是数字? 1: 0 ）
            if (!isNumberic(e.Text)) {
                // 不是数字：已经处理了事件（即不处理当前键盘事件）
                e.Handled = true;
            } else {
                // 是数字：可以接受该事件
                e.Handled = false;
            }
        }
        /// <summary>
        /// 在文本框内部双击，自动全选文本框内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void textbox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            TextBox tbx = sender as TextBox;
            tbx.SelectAll();
        }
        /// <summary>
        /// 月播出时间统计 文本输入框 textbox 只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void grid1_textbox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox tbx = (TextBox)sender;
            var reg = new Regex("^[0-9]*$");
            var str = tbx.Text.Trim();
            var sb = new StringBuilder();
            if (!reg.IsMatch(str)) {
                for (int i = 0; i < str.Length; i++) {
                    if (reg.IsMatch(str[i].ToString())) { sb.Append(str[i].ToString()); }
                }
                tbx.Text = sb.ToString();
                // 定义输入焦点在最后一个字符
                tbx.SelectionStart = tbx.Text.Length;
            }
        }  


        #region 停播率统计 timebox通用事件
        /// <summary>
        /// timebox 判断输入内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timebox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox tbx = (TextBox)sender;     
            try {
                var reg = new Regex("^[0-9]*$"); 
                var str = tbx.Text.Trim();
                var sb = new StringBuilder();
                // 是否是数字，只保留数字
                if (!reg.IsMatch(str)) {
                    for (int i = 0; i < str.Length; i++) {
                        if (reg.IsMatch(str[i].ToString())) { sb.Append(str[i].ToString()); }
                    }
                    tbx.Text = sb.ToString().Substring(0, 2) + " : " +
                        sb.ToString().Substring(2, 2) + " : " +
                        sb.ToString().Substring(4, 2);
                }
            } catch (Exception ex) {
                tbx.Text = "000000";
                tbx.SelectAll();
                MessageBox.Show("请输入数字. " + ex.Message);
            }
        }
        /// <summary>
        /// 屏蔽空格键，回车键计算停播率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timebox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.btnGetStopRate_Click(sender, e);
            }
            // 如果是空格键：则已经处理了事件（即不处理当前键盘事件）
            if (e.Key == Key.Space) {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 焦点在该textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timebox_GotFocus(object sender, RoutedEventArgs e) {
            if (sender != null) {
                TextBox tbx = sender as TextBox;
                // 自动全选
                tbx.SelectAll();
                tbx.PreviewMouseDown -= new MouseButtonEventHandler(timebox_PreviewMouseDown);
            }
        }
        /// <summary>
        /// 鼠标点击，变为6位数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timebox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            //TextBox _textbox = (TextBox)sender;  //调用sender
            TextBox tbx = sender as TextBox;
            if (tbx != null) {
                tbx.Focus();
                e.Handled = true;
            }

            var reg = new Regex("^[0-9]*$");
            var str = tbx.Text.Trim();
            var sb = new StringBuilder();
            if (!reg.IsMatch(str)) {
                for (int i = 0; i < str.Length; i++) {
                    if (reg.IsMatch(str[i].ToString())) { sb.Append(str[i].ToString()); }
                }
                tbx.Text = sb.ToString();
                // 定义输入焦点在最后一个字符
                tbx.SelectionStart = tbx.Text.Length;
            }
            tbx.SelectAll();
        }
        /// <summary>
        /// 焦点离开，变回00：00：00格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timebox_LostFocus(object sender, RoutedEventArgs e) {
            TextBox tbx = sender as TextBox;
            if (tbx != null) {
                tbx.PreviewMouseDown += new MouseButtonEventHandler(timebox_PreviewMouseDown);
            }
            string str = tbx.Text;
            string sec, min, hour;
            // 判断输入数字长度，仿excel输入显示方式，补零
            switch (str.Length) {
                case 6:
                    sec = str.Substring(4, 2);
                    min = str.Substring(2, 2);
                    hour = str.Substring(0, 2);
                    tbx.Text = hour + " : " + min + " : " + sec;
                    break;
                case 1:
                    sec = "0" + str;
                    min = "00"; hour = "00";
                    tbx.Text = hour + " : " + min + " : " + sec;
                    break;
                case 2:
                    sec = str;
                    min = "00"; hour = "00";
                    tbx.Text = hour + " : " + min + " : " + sec;
                    break;
                case 3:
                    sec = str.Substring(1, 2);
                    min = "0" + str.Substring(0, 1);
                    hour = "00";
                    tbx.Text = hour + " : " + min + " : " + sec;
                    break;
                case 4:
                    sec = str.Substring(2, 2);
                    min = str.Substring(0, 2);
                    hour = "00";
                    tbx.Text = hour + " : " + min + " : " + sec;
                    break;
                case 5:
                    sec = str.Substring(3, 2);
                    min = str.Substring(1, 2);
                    hour = "0" + str.Substring(0, 1);
                    tbx.Text = hour + " : " + min + " : " + sec;
                    break;
                default:
                    break;
            }
            // 自动计算timebox停播总时间
            if (tbx.Name != "tbxStopTotalTime"
                && tbx.Name != "tbxTotalTime"
                && tbx.Name != "tbxStopTotalTime") {
                StopAllTime(this.grid2.Children);
            }
        }

        #endregion

        /// <summary>
        /// 停播率统计：文本框 停播总时长：只能输入数字和小数点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxTotalTime_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            string _string = e.Text;

            if (string.IsNullOrEmpty(_string)) e.Handled = false;
            // 只能输入数字和小数点
            foreach (char c in _string) {
                // 允许内容：数字或小数点
                if (c >= '0' && c <= '9' || c == '.') {   
                    e.Handled = false;
                } else {
                    e.Handled = true;
                }
            }
        }

        #endregion


        #region 通用事件
        /// <summary>
        /// isNumberic：判断是否是数字的子函数
        /// </summary> isDigit：是否是数字
        /// <param name="_string"></param>
        /// <returns></returns>
        public bool isNumberic(string _string) {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string) {
                if (!char.IsDigit(c))
                    // 最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
                    // if(c<'0' c="">'9')
                    return false;
            }
            return true;
        }

        /// <summary>
        /// TraverseVisualTree：清空所有textbox
        /// </summary>
        /// <param name="myMainWindow"></param>
        public void TraverseVisualTree(Visual myMainWindow) {
            // 返回指定对象的子集（控件）个数
            int childrenCount = VisualTreeHelper.GetChildrenCount(myMainWindow);
            // 整个循环一遍
            for (int i = 0; i < childrenCount; i++) {
                var visualChild = (Visual)VisualTreeHelper.GetChild(myMainWindow, i);
                // 为textbox
                if (visualChild is TextBox) {
                    // 每次循环都实例化新的textbox
                    TextBox tb = (TextBox)visualChild;
                    tb.Clear();
                }
                TraverseVisualTree(visualChild);
            }
        }
        
        /// <summary>
        /// clrgrid2tb：停播率统计 所有textbox初始化
        /// </summary>
        /// <param name="uiControls"></param>
        public void clrgrid2tb(UIElementCollection uiControls) {
            foreach (UIElement element in uiControls) {
                if (element is TextBox) {
                    switch ((element as TextBox).Name) {
                        case "tbxStopTotalTime":
                            (element as TextBox).Text = "00 : 00 : 00";
                            break;
                        case "tbxTotalTime":
                            (element as TextBox).Text = "000.0";
                            break;
                        case "tbxStopRate":
                            (element as TextBox).Text = "0";
                            break;
                        default:  
                            // timebox
                            (element as TextBox).Text = "00 : 00 : 00";
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// clrgrid1tb：月播出时间统计 控件初始化
        /// </summary>
        /// <param name="uiControls"></param>
        public void clrgrid1tb(UIElementCollection uiControls) {
            foreach (UIElement element in uiControls) {
                if (element is ScrollBar) {
                    switch ((element as ScrollBar).Name) {
                        case "scb_Year":
                            (element as ScrollBar).Value = System.DateTime.Now.Year;
                            break;
                        case "scb_Month":
                            (element as ScrollBar).Value = System.DateTime.Now.Month;
                            break;
                        case "scb_NoStop2":
                            (element as ScrollBar).Value = 0;
                            break;
                        case "scb_NoStop3":
                            (element as ScrollBar).Value = 0;
                            break;
                        default:
                            break;
                    }
                } else if(element is TextBox) {
                    (element as TextBox).Text = "0";
                }
            }
        }

        #endregion

    }
   
}
