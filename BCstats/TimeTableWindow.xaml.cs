using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Text.RegularExpressions;   //使用ObservableCollection
using System.Data.SQLite;
using System.Collections;

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
        private SQLiteHelper helper = new SQLiteHelper(BCstatsHelper.connectionString);
        #endregion


        /// <summary>
        /// 弹出窗口加载时，读取数据表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeTableWindow_Loaded(object sender, RoutedEventArgs e) {
            try {
                DataTable dt = new DataTable();
                SQLiteDataReader reader = helper.ReadFullTable(BCstatsHelper.tableName);
                dt.Load(reader);

                // TimeTableWindow_dataGrid1.Items.Clear();
                // 将表对象作为DataGrid的数据源
                TimeTableWindow_dataGrid1.ItemsSource = dt.DefaultView;
                // 禁止用户排序
                TimeTableWindow_dataGrid1.CanUserSortColumns = false;
                // 阻止最后一行的空行
                TimeTableWindow_dataGrid1.CanUserAddRows = false;
            } catch (Exception ex) {
                MessageBox.Show("读取数据库错误：" + ex.ToString());
            }
        }

        /// <summary>
        /// 按钮：刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            try {
                DataTable dt = new DataTable();
                SQLiteDataReader reader = helper.ReadFullTable(BCstatsHelper.tableName);
                dt.Load(reader);

                TimeTableWindow_dataGrid1.ItemsSource = dt.DefaultView;
                // 禁止用户排序
                TimeTableWindow_dataGrid1.CanUserSortColumns = false;
                // 阻止最后一行的空行
                TimeTableWindow_dataGrid1.CanUserAddRows = false;
            } catch (Exception ex) {
                MessageBox.Show("读取数据库错误：" + ex.ToString());
            }
        }

        /// <summary>
        /// 按钮：添加新行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            DataTable dt = new DataTable();
            SQLiteDataReader reader = helper.ReadFullTable(BCstatsHelper.tableName);
            dt.Load(reader);
            TimeTableWindow_dataGrid1.ItemsSource = dt.DefaultView;
            ArrayList arr = BCstatsHelper.getColumnNames(dt);
            // 设置新创建的行的单元格的默认值：非时间单元格
            dt.Columns[BCstatsHelper.STR_CITY].DefaultValue = BCstatsHelper.STR_CITY_CH;
            dt.Columns[BCstatsHelper.STR_STATION].DefaultValue = BCstatsHelper.STR_STATION_CH;
            dt.Columns[BCstatsHelper.STR_FREQUENCY].DefaultValue = BCstatsHelper.STR_FREQUENCY_CH;
            dt.Columns[BCstatsHelper.STR_STOP_2].DefaultValue = "0";
            dt.Columns[BCstatsHelper.STR_STOP_LAST_2].DefaultValue = "0";
            // 时间单元格，默认为"0000"
            arr.Remove(BCstatsHelper.STR_ID);
            arr.Remove(BCstatsHelper.STR_CITY);
            arr.Remove(BCstatsHelper.STR_STATION);
            arr.Remove(BCstatsHelper.STR_FREQUENCY);
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
            try {
                DataTable dt = new DataTable();
                // 用DataTable读取datagrid内容
                dt = ((DataView)TimeTableWindow_dataGrid1.ItemsSource).ToTable();
                // 获取DataTable所有的列名称
                ArrayList columnNames = BCstatsHelper.getColumnNames(dt);
                //PrintArray(columnNamess);

                //MessageBox.Show("Grid DataTable 总条数/总行数: " + dt.Rows.Count);

                string sql = "SELECT COUNT(*) FROM " + BCstatsHelper.tableName;
                int tableRowsCount = Convert.ToInt16(helper.ExecuteScalar(BCstatsHelper.connectionString, sql));
                //Console.WriteLine("数据库中表的总条数：" + tableRowsCount);

                // 在datatable新增了数据条目
                if (dt.Rows.Count > tableRowsCount) {
                    // 执行插入语句的次数，即比较后多出的行数
                    for(int i = 1; i <= dt.Rows.Count - tableRowsCount; i++) {
                        //MessageBox.Show("预备插入的数据：");
                        helper.InsertValues(BCstatsHelper.tableName, BCstatsHelper.getTheRowValues(dt, dt.Rows.Count - i));
                    }
                } else if(dt.Rows.Count == tableRowsCount) {
                    //MessageBox.Show("Grid DataTable 总条数/总行数: " + dt.Rows.Count);
                    //MessageBox.Show("数据库中表的总条数：" + tableRowsCount);
                    //MessageBox.Show("多出行数：" + (dt.Rows.Count - tableRowsCount).ToString());
                    // 更新数据库表：第 id 行的值，所有的列名
                    for (int i = 0; i < dt.Rows.Count; i++) {
                        helper.UpdateValues(BCstatsHelper.tableName,
                            columnNames, BCstatsHelper.getTheRowValues(dt, i),
                            BCstatsHelper.STR_ID, BCstatsHelper.getTheRowValues(dt, i)[0].ToString());
                        // 打印输出，验证结果
                        //PrintArray(BCstatsHelper.getTheRowValuess(dt, i));
                    }
                } else {
                    ;
                }
            } catch (Exception ex) {
                MessageBox.Show("更新数据Error：" + ex.ToString());
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
                    helper.DeleteValuesByRowId(BCstatsHelper.tableName, id);
                } catch(Exception ex) {
                    MessageBox.Show("删除数据Error：" + ex.ToString());
                } finally {
                    try {
                        DataTable dt = new DataTable();
                        SQLiteDataReader reader = helper.ReadFullTable(BCstatsHelper.tableName);
                        dt.Load(reader);
                        TimeTableWindow_dataGrid1.ItemsSource = dt.DefaultView;
                        TimeTableWindow_dataGrid1.CanUserSortColumns = false;
                        TimeTableWindow_dataGrid1.CanUserAddRows = false;
                    } catch (Exception ex) {
                        MessageBox.Show("读取数据库错误：" + ex.ToString());
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
            mw.Hours.Text = string.Empty;
            mw.rbtnLastTuesday.IsChecked = false;
            mw.chkBox_Tuesday.IsChecked = false;
            // 关闭台站播出统计子窗口
            if (Application.Current.Windows.Count == 4) {
                StationStatsWindow sw = Application.Current.Windows[2] as StationStatsWindow;
                sw.Close();
            }
            // 主窗口位置恢复到屏幕中央
            mw.Left = MainWindow.mwLeft;
            mw.Top = MainWindow.mwTop;
        }


        /// <summary>
        /// 开始编辑单元格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeTableWindow_dataGrid1_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) 
        {           
            //try
            //{
            //    //读取选中的单元格的值，传到MessageBox

            //    //假设这里就是操作文本列
           /* if (TimeTableWindow_dataGrid1.CurrentColumn.Header.ToString() != "周二检修否")    //选中其他
            {
                var preValue = (e.Column.GetCellContent(e.Row) as TextBlock).Text;

                MessageBox.Show(preValue);
            }
            else  ////选中"周二检修否"
            {
                var preValue = (e.Column.GetCellContent(e.Row) as CheckBox).IsChecked.Value.ToString();

                MessageBox.Show(preValue);
            }
            //}
            //catch (Exception caught)
            //{
            //    MessageBox.Show(caught.Message);              
            //}
            */
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
                    MessageBox.Show("分中心/台站/时间都不能为空！");
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


    }


}
