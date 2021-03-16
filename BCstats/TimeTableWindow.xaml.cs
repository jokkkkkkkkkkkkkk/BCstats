using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Text.RegularExpressions;   //使用ObservableCollection
using System.Data.SQLite;
using System.Collections;
using System.Collections.Generic;

namespace BCstats {
    /// <summary>
    /// TimeTableWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TimeTableWindow : Window {

        public TimeTableWindow(Window owner) {
            // 保持弹出窗口在主窗口前，主窗口也可以控制
            this.Owner = owner;
            InitializeComponent();
        }

        #region SQLite 常量定义
        private SQLiteHelper sqlHelper = new SQLiteHelper(BCstatsHelper.connectionString);
        #endregion


        /// <summary>
        /// 弹出窗口加载时，读取数据表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeTableWindow_Loaded(object sender, RoutedEventArgs e) {
            int temp_ttw_cboxStation_SelectedIndex = ttw_cboxStation.SelectedIndex;
            MainWindow mw = Application.Current.Windows[0] as MainWindow;
            try {
                if (mw.cboxCity.SelectedIndex != -1) {
                    if (sqlHelper.CheckDataBase(BCstatsHelper.dbFileName)
                        && sqlHelper.CheckDataTable(BCstatsHelper.connectionString, BCstatsHelper.tableName)) {
                        DataTable dt = new DataTable();
                        SQLiteDataReader reader = sqlHelper.GetDataBy(BCstatsHelper.tableName,
                            BCstatsHelper.STR_STATION, mw.cboxStation.Text);
                        if (ttw_cboxStation.SelectedIndex != -1) {
                            reader = sqlHelper.GetDataBy(BCstatsHelper.tableName,
                            BCstatsHelper.STR_STATION, ttw_cboxStation.Text);
                        }
                        dt.Load(reader);
                        // 将表对象作为DataGrid的数据源
                        TimeTableWindow_dataGrid1.ItemsSource = dt.DefaultView;
                        // 数据排序:根据台站名称
                        (TimeTableWindow_dataGrid1.ItemsSource as DataView).Sort = BCstatsHelper.STR_STATION;
                        // 禁止用户排序
                        TimeTableWindow_dataGrid1.CanUserSortColumns = false;
                        // 阻止最后一行的空行
                        TimeTableWindow_dataGrid1.CanUserAddRows = false;

                        // 读取city字段
                        getComboBoxBinding(ttw_cboxCity, sqlHelper, BCstatsHelper.connectionString,
                            BCstatsHelper.STR_CITY, null, null, "TimeTableWindow_Loaded");
                        // 和主窗口的地点下拉菜单同步
                        ttw_cboxCity.SelectedIndex = mw.cboxCity.SelectedIndex;
                        // 根据地点，读取station字段
                        getComboBoxBinding(ttw_cboxStation, sqlHelper, BCstatsHelper.connectionString,
                            BCstatsHelper.STR_STATION, BCstatsHelper.STR_CITY, ttw_cboxCity.Text, "TimeTableWindow_Loaded");
                        if (mw.cboxCity.SelectedIndex != -1 && mw.cboxStation.SelectedIndex != -1) {
                            ttw_cboxStation.SelectedIndex = mw.cboxStation.SelectedIndex;
                        }
                        if (temp_ttw_cboxStation_SelectedIndex != -1) {
                            ttw_cboxStation.SelectedIndex = temp_ttw_cboxStation_SelectedIndex;
                        }

                        ttWindow.Title = ttw_cboxCity.Text + " " + ttw_cboxStation.Text+ " " 
                            + "播出时间表设置";
                    }
                } else {
                    MessageBox.Show("请先选择地点");
                    //this.Close();
                }
            } catch (Exception ex) {
                MessageBox.Show("数据库读取错误：" + ex.ToString());
            }
        }



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
                // 清除ComboBox下拉选项
                cbox.Items.Clear();
                string tableName = BCstatsHelper.tableName;

                List<string> columnValueList = null;
                //MessageBox.Show(sql);
                // 地点、台站，有重复
                if (columnName == BCstatsHelper.STR_CITY || columnName == BCstatsHelper.STR_STATION) {
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
            } catch (Exception ex) {
                MessageBox.Show(functionName + " 事件错误：" + ex.ToString());
            }
        }



        /// <summary>
        /// 按钮：刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            TimeTableWindow_Loaded(sender, e);
        }


        /// <summary>
        /// 按钮：添加节目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddProgram_Click(object sender, RoutedEventArgs e) {
            DataTable dt = new DataTable();
            SQLiteDataReader reader 
                = sqlHelper.GetDataBy(BCstatsHelper.tableName, 
                BCstatsHelper.STR_STATION ,ttw_cboxStation.Text);
            dt.Load(reader);
            TimeTableWindow_dataGrid1.ItemsSource = dt.DefaultView;

            ArrayList arr = BCstatsHelper.getColumnNames(dt);
            // 设置新创建的行的单元格的默认值：非时间单元格
            dt.Columns[BCstatsHelper.STR_CITY].DefaultValue = "市";
            dt.Columns[BCstatsHelper.STR_STATION].DefaultValue = BCstatsHelper.STR_STATION_CH_FOR_SHORT;
            dt.Columns[BCstatsHelper.STR_CATEGORY].DefaultValue = BCstatsHelper.STR_FM;
            dt.Columns[BCstatsHelper.STR_NAME].DefaultValue = "节目";
            dt.Columns[BCstatsHelper.STR_FREQUENCY].DefaultValue = BCstatsHelper.STR_FREQUENCY_CH;
            dt.Columns[BCstatsHelper.STR_STOP_3].DefaultValue = "0";
            dt.Columns[BCstatsHelper.STR_STOP_2].DefaultValue = "0";
            dt.Columns[BCstatsHelper.STR_STOP_LAST_2].DefaultValue = "0";
            // 时间单元格，默认为"0000"
            arr.Remove(BCstatsHelper.STR_ID);
            arr.Remove(BCstatsHelper.STR_CITY);
            arr.Remove(BCstatsHelper.STR_STATION);
            arr.Remove(BCstatsHelper.STR_CATEGORY);
            arr.Remove(BCstatsHelper.STR_NAME);
            arr.Remove(BCstatsHelper.STR_FREQUENCY);
            arr.Remove(BCstatsHelper.STR_STOP_3);
            arr.Remove(BCstatsHelper.STR_STOP_2);
            arr.Remove(BCstatsHelper.STR_STOP_LAST_2);
            for (int i = 0; i < arr.Count; i++) {
                dt.Columns[arr[i].ToString()].DefaultValue = "0000";
            }
            // 允许创建新的行
            TimeTableWindow_dataGrid1.CanUserAddRows = true;
        }

        /// <summary>
        /// 按钮：保存修改的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e) {
            MainWindow mw = Application.Current.Windows[0] as MainWindow;
            try {
                // 根据台站名称，获取该台站的节目数量
                int programsCount = sqlHelper.GetDataCounBy(BCstatsHelper.connectionString, BCstatsHelper.tableName,
                    BCstatsHelper.STR_STATION, mw.cboxStation.Text);
                //Console.WriteLine("该台站的节目数量：" + programsCount);

                DataTable dt = new DataTable();
                // 用DataTable读取datagrid内容
                dt = ((DataView)TimeTableWindow_dataGrid1.ItemsSource).ToTable();
                // 获取DataTable所有的列名称
                ArrayList columnNames = BCstatsHelper.getColumnNames(dt);
                //PrintArray(columnNamess);
                //Console.WriteLine("Grid DataTable 总条数/总行数: " + dt.Rows.Count);
                if (ttw_cboxStation.SelectedIndex != -1) {
                    // 在datatable新增了数据条目
                    if (dt.Rows.Count > programsCount) {
                        for (int i = 1; i <= dt.Rows.Count - programsCount; i++) {
                            ArrayList arr = BCstatsHelper.getTheRowValues(dt, dt.Rows.Count - i);
                            // 去掉ID列的值，让其自动生成
                            arr.RemoveAt(0);
                            //Console.WriteLine("预备插入的数据：");
                            sqlHelper.InsertValues(BCstatsHelper.tableName, 
                                BCstatsHelper.getColumnNames(dt), arr);
                        }
                    } else if (dt.Rows.Count == programsCount) {
                        //Console.WriteLine("Grid DataTable 总条数/总行数: " + dt.Rows.Count);
                        //Console.WriteLine("数据库中表的总条数：" + programsCount);
                        //Console.WriteLine("多出行数：" + (dt.Rows.Count - programsCount).ToString());
                        // 更新数据库表：第 id 行的值，所有的列名
                        for (int i = 0; i < dt.Rows.Count; i++) {
                            sqlHelper.UpdateValues(BCstatsHelper.tableName,
                                columnNames, BCstatsHelper.getTheRowValues(dt, i),
                                BCstatsHelper.STR_ID, BCstatsHelper.getTheRowValues(dt, i)[0].ToString());
                            // 打印输出，验证结果
                            //PrintArray(BCstatsHelper.getTheRowValuess(dt, i));
                        }
                    } else {
                        ArrayList arr = new ArrayList(sqlHelper.GetColunmValues(
                            BCstatsHelper.connectionString, BCstatsHelper.tableName,
                            BCstatsHelper.STR_ID,
                            BCstatsHelper.STR_STATION, mw.cboxStation.Text));
                        //sqlHelper.PrintArrayList(arr);
                    }
                } else if(ttw_cboxStation.SelectedIndex == -1) {
                    // 没选台站，则添加新的节目
                    //Console.WriteLine("没选台站，则添加新的节目...");
                    for (int i = 1; i <= dt.Rows.Count; i++) {
                        ArrayList arr = BCstatsHelper.getTheRowValues(dt, dt.Rows.Count - i);
                        // 去掉ID列的值，让其自动生成
                        arr.RemoveAt(0);
                        //Console.WriteLine("新节目的值：");
                        //sqlHelper.PrintArrayList(arr);
                        sqlHelper.InsertValues(BCstatsHelper.tableName, BCstatsHelper.getColumnNames(dt), arr);
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("更新数据错误：" + ex.ToString());
            }
        }

        /// <summary>
        /// 按钮：删除某一行的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            // 目前选中的一行
            if (TimeTableWindow_dataGrid1.SelectedIndex >= 0) {
                // 获取DataGrid的某一行的某一个单元格的内容
                DataRowView dataRowView = (DataRowView)TimeTableWindow_dataGrid1.SelectedItem;
                int id = Convert.ToInt16(dataRowView.Row[0].ToString());
                try {
                    sqlHelper.DeleteValuesByRowId(BCstatsHelper.tableName, id);
                } catch(Exception ex) {
                    MessageBox.Show("删除数据Error：" + ex.ToString());
                } finally {
                    try {
                        DataTable dt = new DataTable();
                        SQLiteDataReader reader = sqlHelper.GetDataBy(BCstatsHelper.tableName, 
                            BCstatsHelper.STR_STATION, ttw_cboxStation.Text);
                        dt.Load(reader);
                        TimeTableWindow_dataGrid1.ItemsSource = dt.DefaultView;
                        TimeTableWindow_dataGrid1.CanUserSortColumns = false;
                        TimeTableWindow_dataGrid1.CanUserAddRows = false;
                    } catch (Exception ex) {
                        MessageBox.Show("数据库读取错误：" + ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 按钮：关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClosePopUp_Click(object sender, RoutedEventArgs e) {
            this.Close();
            // 主窗口的下拉菜单变为未选中，计算播出时长结果清零，周二检修选框变为未选中
            MainWindow mw = Application.Current.Windows[0] as MainWindow;
            mw.cboxFrequency.SelectedIndex = -1;
            mw.cbx_frequency.SelectedIndex = -1;
            mw.tbxTotalHours.Text = string.Empty;
            mw.lblDetail.Text = string.Empty;
            mw.Hours.Text = string.Empty;
            mw.rbtnLastTuesday.IsChecked = false;
            mw.chkBox_Tuesday.IsChecked = false;
            // 关闭台站播出统计子窗口
            //if (Application.Current.Windows.Count > 2) {
            //    StationStatsWindow sw = Application.Current.Windows[2] as StationStatsWindow;
            //    sw.Close();
            //}
            foreach (Window window in Application.Current.Windows) {
                if (window.Name == BCstatsHelper.STR_StationStatsWindow_NAME) {
                    window.Close();
                }
            }
            // 主窗口位置恢复到屏幕中央
            mw.Left = MainWindow.mwLeft;
            mw.Top = MainWindow.mwTop;
        }

        /// <summary>
        /// 结束编辑单元格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeTableWindow_dataGrid1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            try {
                DataTable dt = new DataTable();
                // 用DataTable读取datagrid内容
                dt = ((DataView)TimeTableWindow_dataGrid1.ItemsSource).ToTable();

                int id = TimeTableWindow_dataGrid1.SelectedIndex;
                ArrayList values = BCstatsHelper.getTheRowValues(dt, id);

                if (values.Contains(String.Empty)) {
                    MessageBox.Show("地点/台站/时间都不能为空！");
                }
            } catch {
                //MessageBox.Show(caught.Message);
            }

        }


        #region 本窗口 textbox 通用事件
        /// <summary>
        /// 屏蔽空格键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void tb_PreviewKeyDown(object sender, KeyEventArgs e) {
            // 是空格键
            if (e.Key == Key.Space)     
                // 已经处理了事件（即不处理当前键盘事件）    
                e.Handled = true;          
        }
        /// <summary>
        /// 是否是数字键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void tb_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            //      !（输入文本是不是数字? 1: 0 ）
            if (IsNumberic(e.Text)) {
                // 已经处理了事件（即不处理当前键盘事件）
                e.Handled = false;
            } else {
                e.Handled = true; 
            }
        }
        /// <summary>
        /// 双击文本框内部，自动全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void tb_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }
        public void dg_tb_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox tb = (TextBox)sender;
            var reg = new Regex("^[0-9]*$");
            var str = tb.Text.Trim();//去除空格
            var sb = new StringBuilder();
            if (!reg.IsMatch(str)) {
                for (int i = 0; i < str.Length; i++) {
                    if (reg.IsMatch(str[i].ToString())) { sb.Append(str[i].ToString()); }
                }
                tb.Text = sb.ToString();
                // 定义输入焦点在最后一个字符
                tb.SelectionStart = tb.Text.Length;
            } else {
                // tb.Text = preText;
            }
        }

        /// <summary>
        /// 是否是数字
        /// </summary>
        /// <param name="_string"></param>
        /// <returns></returns>
        public bool IsNumberic(string _string) {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string) {
                if (!char.IsDigit(c))
                    //if(c<'0' c="">'9')//最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
                    return false;
            }
            return true;
        }




        #endregion


        #region 所有的下拉菜单 事件 禁止鼠标滚轮
        public void comboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }
        #endregion



        #region 事件 下拉菜单动作
        private void ttw_cboxCity_DropDownClosed(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(ttw_cboxCity.Text) && ttw_cboxCity.Text.Length != 0) {
                getComboBoxBinding(ttw_cboxStation, sqlHelper,
                    BCstatsHelper.connectionString,
                    BCstatsHelper.STR_STATION,
                    BCstatsHelper.STR_CITY, ttw_cboxCity.Text,
                    "ttw_cboxCity_DropDownClosed");
            }
        }

        private void ttw_cboxCity_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!string.IsNullOrEmpty(ttw_cboxCity.Text) && ttw_cboxCity.Text.Length != 0) {
                getComboBoxBinding(ttw_cboxStation, sqlHelper,
                    BCstatsHelper.connectionString,
                    BCstatsHelper.STR_STATION,
                    BCstatsHelper.STR_CITY, ttw_cboxCity.Text,
                    "ttw_cboxCity_SelectionChanged");
            }
        }

        private void ttw_cboxStation_DropDownClosed(object sender, EventArgs e) {
            try {
                if (!string.IsNullOrEmpty(ttw_cboxCity.Text) && ttw_cboxStation.Text.Length != 0) {

                    if (sqlHelper.CheckDataBase(BCstatsHelper.dbFileName)
                            && sqlHelper.CheckDataTable(BCstatsHelper.connectionString, BCstatsHelper.tableName)) {
                        DataTable dt = new DataTable();
                        SQLiteDataReader reader = sqlHelper.GetDataBy(BCstatsHelper.tableName,
                            BCstatsHelper.STR_STATION, ttw_cboxStation.Text);
                        dt.Load(reader);

                        // 将表对象作为DataGrid的数据源
                        TimeTableWindow_dataGrid1.ItemsSource = dt.DefaultView;
                        // 数据排序:根据台站名称
                        (TimeTableWindow_dataGrid1.ItemsSource as DataView).Sort = BCstatsHelper.STR_STATION;
                        // 禁止用户排序
                        TimeTableWindow_dataGrid1.CanUserSortColumns = false;
                        // 阻止最后一行的空行
                        TimeTableWindow_dataGrid1.CanUserAddRows = false;

                        ttWindow.Title = ttw_cboxCity.Text + " " + ttw_cboxStation.Text + " " 
                            + "播出时间表设置";
                    }
                }
            } catch {
                ;
            }
        }

        private void ttw_cboxStation_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }
        #endregion


    }


}
