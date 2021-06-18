using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;

public sealed class BCstatsHelper {

    #region 连接数据库字符串
    public static string dbFileName = "bcstats.db";
    public static string connectionString = "data source = "
        + Environment.CurrentDirectory
        + "\\bcstats.db";
    #endregion

    #region SQLite数据库相关常量
    SQLiteHelper sqliteHelper = new SQLiteHelper(connectionString);

    public const string tableName = "freqs";

    public const string STR_ID = "id";
    public const string STR_CITY = "city";
    public const string STR_STATION = "station";
    public const string STR_CATEGORY = "category";
    public const string STR_NAME = "name";
    public const string STR_FREQUENCY = "frequency";

    public const string STR_CITY_CH = "地点";
    public const string STR_STATION_CH = "台站";
    public const string STR_STATION_CH_FOR_SHORT = "台";
    public const string STR_FREQUENCY_CH = "Hz";

    public const string STR_OFF_TIME = "off_time";
    public const string STR_ON_TIME = "on_time";
    public const string STR_STOP_3 = "stop_3";
    public const string STR_OFF_TIME_3 = "off_time_3";
    public const string STR_ON_TIME_3 = "on_time_3";
    public const string STR_STOP_2 = "stop_2";
    public const string STR_OFF_TIME_2 = "off_time_2";
    public const string STR_ON_TIME_2 = "on_time_2";
    public const string STR_STOP_LAST_2 = "stop_last_2";

    public const string STR_HOURS = "hours";

    public const string STR_SW = "sw";
    public const string STR_MW = "mw";
    public const string STR_EXP = "exp";
    public const string STR_FM = "fm";
    public const string STR_TV = "tv";
    public const string STR_SW_CN = "短波";
    public const string STR_MW_CN = "中波";
    public const string STR_EXP_CN = "实验";
    public const string STR_FM_CN = "调频";
    public const string STR_TV_CN = "数字电视";
    public const string STR_TOTAL_CN = "总 计";



    #endregion

    #region 其他常量
    public const string STR_MainWindow_NAME = "mWindow";
    public const string STR_StationStatsWindow_TITLE = "台站播出数据";
    public const string STR_StationStatsWindow_NAME = "ssWindow";

    #endregion


    /// <summary> 
    /// 统计一段时间内有多少个星期几 
    /// </summary> 
    /// <param name= "dateFrom "></param>开始日期
    /// <param name= "dateTo "></param> 结束日期 
    /// <param name= "DayOfWeek "></param>星期几
    /// <returns></returns>返回星期几的个数
    public static int DayOfWeekConut(DateTime dateFrom, DateTime dateTo, DayOfWeek DayOfWeek) {
        try {
            TimeSpan vTimeSpan = new TimeSpan(dateTo.Ticks - dateFrom.Ticks);
            int Result = (int)vTimeSpan.TotalDays / 7;
            for (int i = 0; i <= vTimeSpan.TotalDays % 7; i++)
                if (dateFrom.AddDays(i).DayOfWeek == DayOfWeek) {
                    return Result + 1;
                }
            return Result;
        } catch(Exception ex) {
            MessageBox.Show("统计一段时间内有多少个星期几的函数参数错误：" + ex.ToString());
            return 0;
        }


    }


    /// <summary>
    /// 某年某月的天数
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    public static int DaysCountOfTheMonth(int year, int month) {
        try {
            return System.DateTime.DaysInMonth(year, month);
        } catch (Exception ex) {
            MessageBox.Show("获取某年某月天数的函数参数错误：" + ex.ToString());
            return 0;
        }
    }


    /// <summary>
    /// 某年某月的星期几的天数
    /// </summary>
    /// <param name="year">某年</param>
    /// <param name="month">某月</param>
    /// <param name="dayOfWeek">星期几</param>
    /// <returns></returns>
    public static int DayOfWeekConutOfTheMonth(int year, int month, DayOfWeek dayOfWeek) {
        try {
            return DayOfWeekConut(
                new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1),
                new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), DaysCountOfTheMonth(year, month)),
                dayOfWeek);
        } catch (Exception ex) {
            MessageBox.Show("获取某年某月周几天数的参数错误：" + ex.ToString());
            return 0;
        }
    }


    /// <summary>
    /// 计算单个节目的当月播出时长
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="nonstop2">不停机周二天数</param>
    /// <param name="nonstop3">不停机周三天数</param>
    /// <param name="offHour">每日关机时间：时</param>
    /// <param name="offMin">每日关机时间：分</param>
    /// <param name="onHour">每日开机时间：时</param>
    /// <param name="onMin">每日开机时间：分</param>
    /// <param name="stop2">bool：周二下午是否停机检修</param>
    /// <param name="stopLast2">bool：当月的最后一个周二下午是否停机检修</param>
    /// <param name="offHour3">周三关机时间：时</param>
    /// <param name="offMin3">周三关机时间：分</param>
    /// <param name="onHour3">周三开机时间：时</param>
    /// <param name="onMin3">周三开机时间：分</param>
    /// <returns>返回播出时长、详细时长计算的字符串</returns>
    public static Tuple<double, string> TotalHoursOfTheMonth(int year, int month,
                                            int nonStop2, int nonStop3,
                                            double offHour, double offMin,
                                            double onHour, double onMin,
                                            bool stop3,
                                            double offHour3, double offMin3,
                                            double onHour3, double onMin3,
                                            bool stop2,
                                            double offHour2, double offMin2,
                                            double onHour2, double onMin2,
                                            bool stopLast2) {
        string strDetail = null;
        try {
            #region 当月天数、周二天数、周三天数
            // 当月的天数
            int daysCountTheMonth = System.DateTime.DaysInMonth(year, month);
            // 当月的周二的天数
            int daysTues = DayOfWeekConut(
                new DateTime(year, month, 01), new DateTime(year, month, daysCountTheMonth), DayOfWeek.Tuesday);
            // 当月的周三的天数
            int daysWed = DayOfWeekConut(
                new DateTime(year, month, 01), new DateTime(year, month, daysCountTheMonth), DayOfWeek.Wednesday);
            // 当月的非周二周三天数
            int nonTuesWedDays = daysCountTheMonth - daysTues - daysWed;
            #endregion
            //MessageBox.Show("当月的天数：" + daysCountTheMonth);
            //MessageBox.Show("几个周二不停机检修：" + nonStop2 + "\n"
            //    + "几个周三不停机检修：" + nonStop3);

            #region 当月的非周二周三天数、每天播出时长
            // 0点到凌晨关机  + 早上开播到24点
            double nonTuesWedHours = offHour + offMin / 60 + (24 - (onHour + onMin / 60));
            #endregion
            //MessageBox.Show("当月的非周二周三，每天播出时长：" + nonTuesWedHours + "\n"
            //    + "当月的非周二周三，天数：" + nonTuesWedDays);


            #region 周三每天播出时长
            // 零点到关机，开机到24点
            double wednesdayHours = 0;
            if (stop3) {
                wednesdayHours = offHour + offMin / 60
                    + (offHour3 + offMin3 / 60 - (onHour + onMin / 60))
                    + (24 - (onHour3 + onMin3 / 60));
            } else {
                wednesdayHours = nonTuesWedHours;
            }
            #endregion
            //MessageBox.Show("当月的周三，每天播出时长：" + wednesdayHours);


            #region 周二每天播出时长
            double tuesdayHours = 0;
            if (stop2) {
                // 零点到凌晨停播，早晨开播到中午停播检修，下午开播到24点
                tuesdayHours = offHour + offMin / 60
                    + (offHour2 + offMin2 / 60) - (onHour + onMin / 60)
                    + (24 - (onHour2 + onMin2 / 60));
            } else {
                // 周二下午不检修，则和非周二周三相同时长
                tuesdayHours = nonTuesWedHours;
            }
            #endregion
            //MessageBox.Show("当月的周二，每天播出时长：" + tuesdayHours);


            #region 最后一个周二的播出时长
            double lastTuesHours = 0;
            // 周二下午停机检修时间，是否有设定
            if (onHour2 == 0 || offHour2 == 0 ) {
                offHour2 = 13; offMin2 = 0;
                onHour2 = 17; onMin2 = 30;
            }
            // 若最后周二下午停机，则与周二检修的既定时间相同
            if(stopLast2) {
                lastTuesHours = offHour + offMin / 60
                    + (offHour2 + offMin2 / 60) - (onHour + onMin / 60)
                    + (24 - (onHour2 + onMin2 / 60));
            } else {
                // 若最后周二下午不停机，则和非周二三相同时长
                lastTuesHours = nonTuesWedHours;
            }
            #endregion
            //MessageBox.Show("最后一个周二的播出时长：" + lastTuesHours);



            #region 总播出时长
            // 当月的非周二非周三的总播出时长：非周二/三播出时长 * 非周二/三天数
            double totalNonTuesWedHours = nonTuesWedHours * nonTuesWedDays;
            totalNonTuesWedHours = Math.Round(totalNonTuesWedHours, 1, MidpointRounding.AwayFromZero);


            // 当月的周三的总播出时长：
            double totalWednesdayHours = 0;
            if (stop3) {
                if(nonStop3 > 0) {
                    totalWednesdayHours = wednesdayHours * (daysWed - nonStop3) 
                        + nonTuesWedHours * nonStop3;
                } else {
                    totalWednesdayHours = wednesdayHours * daysWed;
                }
            } else {
                totalWednesdayHours = wednesdayHours * daysWed;
            }
            totalWednesdayHours = Math.Round(totalWednesdayHours, 1, MidpointRounding.AwayFromZero);



            // 当月的周二的总播出时长
            double totalTuesdayHours = 0;
            if (stop2) {
                if (nonStop2 > 0) {
                    totalTuesdayHours = tuesdayHours * (daysTues - nonStop2 - 1)
                        + nonTuesWedHours * nonStop2
                        + lastTuesHours;
                } else {
                    totalTuesdayHours = tuesdayHours * (daysTues - 1) + lastTuesHours;
                }
            } else {
                totalTuesdayHours = tuesdayHours * (daysTues - 1) + lastTuesHours;
            }
            totalTuesdayHours =  Math.Round(totalTuesdayHours, 1, MidpointRounding.AwayFromZero);



            // 播出总时长；周二播出总时长 + 周三播出总时长 + 非周二/三播出总时长
            double totalHours = totalNonTuesWedHours + totalWednesdayHours + totalTuesdayHours;
            #endregion
            strDetail = "播出时长 =" + "\n"
                + "非周二/三播出总时长 " + totalNonTuesWedHours + "\n"
                + "+ 周二播出总时长 " + totalTuesdayHours + "\n"
                + "+ 周三播出总时长 " + totalWednesdayHours + "\n"
                ;

            // 四舍五入，保留一位
            return Tuple.Create(Math.Round(totalHours, 1, MidpointRounding.AwayFromZero), strDetail);
        } catch {
            return Tuple.Create(0.0, string.Empty); 
        }
    }


    /// <summary>
    /// 时间格式转换，返回字符串格式 00 : 00 : 00
    /// </summary>
    /// <param name="stopHours">小时的时长</param>
    /// <param name="stopMininutes">分钟的时长</param>
    /// <param name="stopSeconds">秒的时长</param>
    /// <returns></returns>
    public static string GetStopTimeString(int stopHours, int stopMininutes, int stopSeconds) {
        try {
            // 总停止播出显示：最后显示，加上补零显示
            // 三个都>=10，不用补0
            if (stopHours >= 10 && stopMininutes >= 10 && stopSeconds >= 10) {
                return stopHours.ToString() + " : " +
                                         stopMininutes.ToString() + " : " +
                                         stopSeconds.ToString();
                // 三个都<10，都补0
            } else if (stopHours < 10 && stopMininutes < 10 && stopSeconds < 10) {
                return "0" + stopHours.ToString() + " : "
                                         + "0" + stopMininutes.ToString() + " : "
                                         + "0" + stopSeconds.ToString();
                // 三个分别<10
            } else if (stopHours >= 10 && stopMininutes >= 10 && stopSeconds < 10) {
                return stopHours.ToString() + " : " +
                                         "0" + stopMininutes.ToString() + " : "
                                          + "0" + stopSeconds.ToString();
            } else if (stopHours >= 10 && stopMininutes < 10 && stopSeconds >= 10) {
                return stopHours.ToString() + " : "
                                         + "0" + stopMininutes.ToString() + " : " +
                                         stopSeconds.ToString();
            } else if (stopHours < 10 && stopMininutes >= 10 && stopSeconds >= 10) {
                return "0" + stopHours.ToString() + " : " +
                                          stopMininutes.ToString() + " : " +
                                         stopSeconds.ToString();
                // 两个<10
            } else if (stopHours >= 10 && stopMininutes < 10 && stopSeconds < 10) {
                return stopHours.ToString() + " : "
                                         + "0" + stopMininutes.ToString() + " : "
                                         + "0" + stopSeconds.ToString();
            } else if (stopHours < 10 && stopMininutes < 10 && stopSeconds >= 10) {
                return "0" + stopHours.ToString() + " : " +
                                         "0" + stopMininutes.ToString() + " : " +
                                         stopSeconds.ToString();
                // 00 10 00
            } else if (stopHours < 10 && stopMininutes >= 10 && stopSeconds < 10) {
                return "0" + stopHours.ToString() + " : " +
                                          stopMininutes.ToString() +
                                        "0" + stopSeconds.ToString();
            } else {
                return "0";
            }
        } catch(Exception ex) {
            MessageBox.Show(ex.ToString());
            return "0";
        }
    }


    /// <summary>
    /// 计算停播率：停播总时长 / 播出总时长
    /// </summary>
    /// <param name="totalStopTime">停播总时长</param>
    /// <param name="totalHours">播出总时长</param>
    /// <returns>返回字符串格式 1h 2min 3.4s</returns>
    public static string GetStopRate(double totalStopTime, double totalHours) {
        double stopRate;
        try {
            stopRate =
                Math.Round(totalStopTime / Convert.ToDouble(totalHours), 3, MidpointRounding.AwayFromZero);
            // 得到的秒数除于0.6后，只保留整数位
            int int_stoprate = (int)(stopRate / 0.600);

            // 停播率 < 1 min    eg.  = 0.0389, ---- 3.9s
            if (stopRate < 0.600) {
                return Math.Round((stopRate * 100), 1, MidpointRounding.AwayFromZero).ToString() + "s";
            } // 停播率 > 1 min , < 1 h
            else if (0.600 < stopRate && stopRate < 36.000) {
                return int_stoprate.ToString() + "min " +   // 分
                    Math.Round((stopRate * 100 % 60), 1, MidpointRounding.AwayFromZero).ToString() + "s";  // 秒，对60取模，保留1位小数                      
            } // 停播率 > 1 h
            else if (stopRate > 36.000) {
                return ((int)(stopRate / 36)).ToString() + "h " +
                    (int)((stopRate % 36) / 0.6) + "min " +
                    Math.Round((stopRate * 100 % 60), 1, MidpointRounding.AwayFromZero).ToString() + "s";  // 秒，对60取模，保留1位小数     
            } // 停播率 = 1 min 
            else if (stopRate == 0.600) {
                return "1 min";
            } // 停播率 = 1 h
            else if (stopRate == 36.000) {
                return "1 h";
            } else {
                return "0";
            }
        } catch {
            return "0";
        }
    }


    /// <summary>
    /// 获取指定频率的某个时间的整型值
    /// </summary>
    /// <param name="city"></param>
    /// <param name="station"></param>
    /// <param name="frequency"></param>
    /// <param name="timeName"></param>
    /// <returns>int</returns>
    public int GetIntValue(string city, string station, string frequency, string timeName) {
        try {
            string sql =
                string.Format("SELECT {0} FROM {1} WHERE {2}='{3}' AND {4}='{5}' AND {6}='{7}'",
                timeName, tableName, 
                STR_CITY, city, 
                STR_STATION, station, 
                STR_FREQUENCY, frequency);
            SQLiteDataReader reader = sqliteHelper.ExecuteQuery(sql);
            int value = 0;
            if (reader.HasRows) {
                while (reader.Read()) {
                    value = reader.GetInt16(0);
                }
            } else {
                return 0;
            }
            return value;
        } catch {
            return 0;
        } finally {
            SQLiteHelper.closeConn();
        }
    }

    /// <summary>
    /// 获取DataTable的所有列的列名称
    /// </summary>
    /// <param name="dt">DataTable</param>
    /// <returns>返回字符串数组</returns>
    public static ArrayList getColumnNames(DataTable dt) {
        ArrayList columnNames = new ArrayList();
        for (int i = 0; i < dt.Columns.Count; i++) {
            columnNames.Add(dt.Columns[i].ColumnName);
        }
        return columnNames;
    }
    /// <summary>
    /// 获取DataTable中的某行的所有值
    /// </summary>
    /// <param name="dt">DataTable</param>
    /// <param name="rowId">某一行的行号</param>
    /// <returns>返回字符串数组</returns>
    public static ArrayList getTheRowValues(DataTable dt, int rowId) {
        ArrayList arr = new ArrayList();
        for (int i = 0; i < dt.Rows.Count; i++) {
            for (int j = 0; j < dt.Columns.Count; j++) {
                if (i == rowId)
                    arr.Add(dt.Rows[i][j]);
            }
        }
        return arr;
    }

}

