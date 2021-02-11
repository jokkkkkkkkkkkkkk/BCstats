using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Windows;

public sealed class BCstatsHelper {

    #region 连接数据库字符串
    public static string connectionString = "data source = "
        + Environment.CurrentDirectory
        + "/bcstats.db";
    #endregion

    #region 数据库字段名称常量
    public const string tableName = "freqs";

    public const string STR_ID = "id";
    public const string STR_CITY = "city";
    public const string STR_STATION = "station";
    public const string STR_FREQUENCY = "frequency";

    public const string STR_CITY_CH = "分中心";
    public const string STR_STATION_CH = "台";
    public const string STR_FREQUENCY_CH = "HZ";

    public const string STR_OFF_TIME = "off_time";
    public const string STR_ON_TIME = "on_time";
    public const string STR_OFF_TIME_3 = "off_time_3";
    public const string STR_ON_TIME_3 = "on_time_3";
    public const string STR_STOP_2 = "stop_2";
    public const string STR_OFF_TIME_2 = "off_time_2";
    public const string STR_ON_TIME_2 = "on_time_2";
    public const string STR_STOP_LAST_2 = "stop_last_2";
    public const string STR_OFF_TIME_LAST_2 = "off_time_last_2";
    public const string STR_ON_TIME_LAST_2 = "on_time_last_2";


    #endregion


    /// <summary> 
    /// TotalWeeks：统计一段时间内有多少个星期几 
    /// </summary> 
    /// <param name= "DateFrom "></param>开始日期
    /// <param name= "DateTo "></param> 结束日期 
    /// <param name= "DayOfWeek "></param>星期几
    /// <returns> 返回个数 </returns> 
    public static int TotalWeeks(DateTime DateFrom, DateTime DateTo, DayOfWeek DayOfWeek) {
        TimeSpan vTimeSpan = new TimeSpan(DateTo.Ticks - DateFrom.Ticks);
        int Result = (int)vTimeSpan.TotalDays / 7;
        for (int i = 0; i <= vTimeSpan.TotalDays % 7; i++)
            if (DateFrom.AddDays(i).DayOfWeek == DayOfWeek)
                return Result + 1;
        return Result;
    }


    /// <summary>
    /// 某年某月的天数
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    public static int DaysCountOfTheMonth(int year, int month) {
        try {
            return System.DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));

        } catch (Exception ex) {
            MessageBox.Show("获取天数的参数错误：" + ex.ToString());
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
            return TotalWeeks(
                new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1),
                new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), DaysCountOfTheMonth(year, month)),
                dayOfWeek);
        } catch (Exception ex) {
            MessageBox.Show("获取周几天数的参数错误：" + ex.ToString());
            return 0;
        }
    }


    /// <summary>
    /// 计算单个频率的月播出时间
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
    /// <returns></returns>
    public static double TotalHoursOfTheMonth(int year, int month,
                                            int nonStop2, int nonStop3,
                                            double offHour, double offMin,
                                            double onHour, double onMin,

                                            bool stop2,
                                            double offHour2, double offMin2,
                                            double onHour2, double onMin2,
                                            
                                            bool stopLast2,
                                            double offHourLast2, double offMinLast2,
                                            double onHourLast2, double onMinLast2,

                                            double offHour3, double offMin3, 
                                            double onHour3, double onMin3) {
        try {
            // 当月的天数
            int daysCountTheMonth = System.DateTime.DaysInMonth(year, month);
            //MessageBox.Show("当月的天数：" + daysCountTheMonth);
            //MessageBox.Show("几个周二不停机检修：" + nonStop2);
            //MessageBox.Show("几个周三不停机检修：" + nonStop3);

            #region 周二天数、每天播出小时
            // 当月的周二的天数
            int daysTues = TotalWeeks(
                new DateTime(year, month, 01), new DateTime(year, month, daysCountTheMonth), DayOfWeek.Tuesday);
            // 当月的周二，每天播出小时
            double tuesdayHours = offHour + offMin / 60      // 0点到停播
                        // 周二早上开机时间到下午关机时间：
                        // 周二下午关机时间 - 开机时间 off_time_2_hour - onhour
                     + (offHour2 + offMin2 / 60 - (onHour + onMin / 60)) 
                       // 周二下午开机时间到24点：24 - on_time_2_hour 
                     + (24 - (onHour2 + onMin2 / 60));
            #endregion
            //MessageBox.Show("当月的周二，每天播出小时：" + tuesdayHours);

            #region 周三天数、每天播出小时
            // 当月的周三的天数
            int daysWed = TotalWeeks(
                new DateTime(year, month, 01), new DateTime(year, month, daysCountTheMonth), DayOfWeek.Wednesday);
            // 当月的周三，每天播出小时  0点到停播  + 开播到24点
            double wednesdayHours = offHour3 + offMin3 / 60 + (24 - (onHour3 + onMin3 / 60));
            #endregion
            //MessageBox.Show("当月的周三，每天播出小时：" + wednesdayHours);

            #region 当月的非周二周三天数、每天播出小时
            // 当月的非周二周三天数
            int nonTuesWedDays = daysCountTheMonth - daysTues - daysWed;
            // 当月的非周二周三，每天播出小时
            double nonTuesWedHours = offHour + offMin / 60        // 0点到停播
                                +
                            (24 - (onHour + onMin / 60));  // + 开播到24点
            #endregion
            //MessageBox.Show("当月的非周二周三，每天播出小时：" + nonTuesWedHours);


            #region 总播出时间
            // 一个月的周二的总播出时间
            double totalTuesdayHours;

            //if(stop2) {
            //    // 周二停机检修，所以包括了最后一个周二停机检修
            //    // 周二播出小时 * 周二天数
            //    totalTuesdayHours = tuesdayHours * (daysTues - nonStop2);
            //} else if(!stop2 & stopLast2) {
            //    // 只有最后一个周二停机检修，其余周二不停机检修
            //    // 周二播出小时 * 周二天数，减去，最后一个周二停机检修的时长
            //    totalTuesdayHours = tuesdayHours * (daysTues - nonStop2)
            //        - (onHourLast2 + onMinLast2 / 60 - (offHourLast2 + offMinLast2 / 60));


            //}

            //MessageBox.Show("最后一个周二停机检修？ " + stopLast2);

            if (!stopLast2) {
                // 最后一个周二不停机检修，周二播出小时 * 周二天数
                totalTuesdayHours = tuesdayHours * (daysTues - nonStop2);
            } else {
                // 最后一个周二停机检修，则周二总播出时间，减去最后一个周二停机的时长
                totalTuesdayHours = tuesdayHours * (daysTues - nonStop2)
                    - (onHourLast2 + onMinLast2 / 60 - (offHourLast2 + offMinLast2 / 60));
            }
            // 一个月的周三的总播出时间：周三播出小时 * 周三天数
            double totalWednesdayHours = wednesdayHours * (daysWed - nonStop3);
            // 一个月的非周二非周三的总播出时间：非周二/三播出小时 * 非周二/三天数
            // 重保期不停机检修：周二/三，按非周二/三的时间播出；
            // 则不停机检修的周二/三，都算入非周二周三的天数
            double totalNonTuesWedHours = nonTuesWedHours * (nonTuesWedDays + nonStop2 + nonStop3);


            // 播出总小时；周二播出总时长 + 周三播出总时长 + 非周二/三播出总时长
            double totalHours = totalTuesdayHours + totalWednesdayHours + totalNonTuesWedHours;
            #endregion
            //MessageBox.Show("播出总小时 = "
            //    + "周二播出总时长 " + totalTuesdayHours + " + "
            //    + "周三播出总时长 " + totalWednesdayHours + " + "
            //    + "非周二/三播出总时长 " + totalNonTuesWedHours);

            // 四舍五入，保留一位
            double temp = Math.Round(totalHours, 1, MidpointRounding.AwayFromZero); 
            return temp;
        } catch {
            return 0; 
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
    /// <param name="stopTotalTime">停播总时长</param>
    /// <param name="totalTime">播出总时长</param>
    /// <returns>返回字符串格式 1h 2min 3.4s</returns>
    public static string GetStopRate(double stopTotalTime, double totalTime) {
        double stopRate;
        try {
            stopRate = 
                Math.Round(stopTotalTime / Convert.ToDouble(totalTime), 3, MidpointRounding.AwayFromZero);
            // 除于0.6后，只保留整数位
            int int_stoprate = (int)(stopRate / 0.600);

            // 停播率 < 1 min    eg.  = 0.0389, ---- 3.9s
            if (stopRate < 0.600) {
                return Math.Round((stopRate * 100), 1, MidpointRounding.AwayFromZero).ToString() + "s";
            } // > 1 min , < 1 h
            else if (0.600 < stopRate && stopRate < 36.000) {
                return int_stoprate.ToString() + "min " +   // 分
                    Math.Round((stopRate * 100 % 60), 1, MidpointRounding.AwayFromZero).ToString() + "s";  // 秒，对60取模，保留1位小数                      
            } // > 1 h
            else if (stopRate > 36.000) {
                return ((int)(stopRate / 36)).ToString() + "h " +
                    (int)((stopRate % 36) / 0.6) + "min " +
                    Math.Round((stopRate * 100 % 60), 1, MidpointRounding.AwayFromZero).ToString() + "s";  // 秒，对60取模，保留1位小数     
            } // 1 min 
            else if (stopRate == 0.600) {
                return "1 min";
            } // 1 h
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

