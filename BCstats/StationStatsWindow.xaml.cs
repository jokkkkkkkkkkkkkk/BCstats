using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace BCstats {
    /// <summary>
    /// StationStatsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StationStatsWindow : Window {

        public StationStatsWindow(Window owner, DataTable dt) {
            this.Owner = owner;
            InitializeComponent();


            StationStats_dataGridStats.ItemsSource = dt.DefaultView;
            StationStats_dataGridStats.CanUserDeleteRows = false;
            StationStats_dataGridStats.CanUserAddRows = false;
            StationStats_dataGridStats.CanUserSortColumns = false;
        }

        private void StationStatsWindow_Loaded(object sender, RoutedEventArgs e) {


        }

        private void btn_ss_quit_Click(object sender, RoutedEventArgs e) {
            this.ssWindow.Close();
        }
    }

}
