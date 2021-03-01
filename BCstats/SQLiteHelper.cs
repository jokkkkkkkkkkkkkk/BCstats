using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Windows;
using System.Collections;

class SQLiteHelper {

    /// <summary>
    /// 数据库连接定义
    /// </summary>
    private SQLiteConnection dbConnection;

    /// <summary>
    /// SQL命令定义
    /// </summary>
    private SQLiteCommand dbCommand;

    /// <summary>
    /// 数据读取定义
    /// </summary>
    private SQLiteDataReader dataReader;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString">连接SQLite库字符串</param>
    public SQLiteHelper(string connectionString) {
        try {
            dbConnection = new SQLiteConnection(connectionString);
            dbConnection.Open();
        } catch (Exception e) {
            Log(e.ToString());
        }
    }
    /// <summary>
    /// 执行SQL命令
    /// </summary>
    /// <returns>The query.</returns>
    /// <param name="queryString">SQL命令字符串</param>
    public SQLiteDataReader ExecuteQuery(string queryString) {
        try {
            dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = queryString;
            dataReader = dbCommand.ExecuteReader();
        } catch (Exception e) {
            Log(e.Message);
        }

        return dataReader;
    }


    /// <summary>
    /// 执行一条SQL命令，返回一个内容
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="sql"></param>
    /// <returns></returns>
    public object ExecuteScalar(string connectionString, string sql) {
        try {
            SQLiteCommand cmd = new SQLiteCommand(sql, dbConn(connectionString));
            cmd.CommandText = sql;
            //MessageBox.Show("sql: " + sql);
            return cmd.ExecuteScalar();
        } catch (Exception ex) {
            MessageBox.Show("ExecuteScalar Error: " + ex.ToString());
        } finally {
            closeConn();
        }
        return null;
    }



    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public void CloseConnection() {
        //销毁Commend
        if (dbCommand != null) {
            dbCommand.Cancel();
        }
        dbCommand = null;
        //销毁Reader
        if (dataReader != null) {
            dataReader.Close();
        }
        dataReader = null;
        //销毁Connection
        if (dbConnection != null) {
            dbConnection.Close();
        }
        dbConnection = null;

    }

    /// <summary>
    /// 读取整张数据表
    /// </summary>
    /// <returns>The full table.</returns>
    /// <param name="tableName">数据表名称</param>
    public SQLiteDataReader ReadFullTable(string tableName) {
        string queryString = "SELECT * FROM " + tableName;
        return ExecuteQuery(queryString);
    }




    /// <summary>
    /// 向指定数据表中插入数据
    /// </summary>
    /// <returns>The values.</returns>
    /// <param name="tableName">数据表名称</param>
    /// <param name="values">插入的数值</param>
    public SQLiteDataReader InsertValues(string tableName, ArrayList arr) {
        string queryString = "INSERT INTO " + tableName + " VALUES (" + "'" + arr[0] + "'";
        for (int i = 1; i < arr.Count; i++) {
            queryString += ", " + "'" + arr[i] + "'";
        }
        queryString += " )";
        //MessageBox.Show("insert sql: " + queryString);
        //return null;
        return ExecuteQuery(queryString);
    }





    /// <summary>
    /// 更新指定数据表内的数据
    /// </summary>
    /// <param name="tableName">数据表名称</param>
    /// <param name="colNames">字段名</param>
    /// <param name="colValues">字段名对应的数据</param>
    /// <param name="key">关键字</param>
    /// <param name="value">关键字对应的值</param>
    /// <param name="operation">运算符：=,<,>,...，默认“=”</param>
    public SQLiteDataReader UpdateValues(string tableName,
    ArrayList colNames, ArrayList colValues,
    string key, string value,
    string operation = "=") {
        // 当字段名称和字段数值不对应时引发异常
        if (colNames.Count != colValues.Count) {
            throw new SQLiteException("字段名称的数量和字段数值的数量不对应");
        }
        //Console.WriteLine("colNames.Count:" + colNames.Count);
        //Console.WriteLine("colValues.Count:" + colValues.Count);
        //string queryString = "UPDATE " + tableName + " SET " + colNames[0] + "=" + "'" + colValues[0] + "'";
        string queryString = "UPDATE " + tableName + " SET " + colNames[0] + "=" + colValues[0];

        for (int i = 1; i < colValues.Count; i++) {
            queryString += ", " + colNames[i] + "=" + "'" + colValues[i] + "'";
        }
        //queryString += " WHERE " + key + operation + "'" + value + "'";
        queryString += " WHERE " + key + operation + value;
        //MessageBox.Show("queryString : " + queryString);
        return ExecuteQuery(queryString);
    }



    /// <summary>
    /// 根据ID，删除数据表里对应的数据条目
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public SQLiteDataReader DeleteValuesByRowId(string tableName, int id) {
        string queryString = "DELETE FROM " + tableName + " WHERE id=" + id;
        //MessageBox.Show("del sql: " + queryString);
        return ExecuteQuery(queryString);
    }


    /// <summary>
    /// 删除指定数据表内的数据
    /// </summary>
    /// <returns>The values.</returns>
    /// <param name="tableName">数据表名称</param>
    /// <param name="colNames">字段名</param>
    /// <param name="colValues">字段名对应的数据</param>
    public SQLiteDataReader DeleteValuesOR(string tableName, string[] colNames, string[] colValues, string[] operations) {
        //当字段名称和字段数值不对应时引发异常
        if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length) {
            throw new SQLiteException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
        }

        string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + "'" + colValues[0] + "'";
        for (int i = 1; i < colValues.Length; i++) {
            queryString += "OR " + colNames[i] + operations[0] + "'" + colValues[i] + "'";
        }
        return ExecuteQuery(queryString);
    }

    /// <summary>
    /// 删除指定数据表内的数据
    /// </summary>
    /// <returns>The values.</returns>
    /// <param name="tableName">数据表名称</param>
    /// <param name="colNames">字段名</param>
    /// <param name="colValues">字段名对应的数据</param>
    public SQLiteDataReader DeleteValuesAND(string tableName, string[] colNames, string[] colValues, string[] operations) {
        //当字段名称和字段数值不对应时引发异常
        if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length) {
            throw new SQLiteException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
        }

        string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + "'" + colValues[0] + "'";
        for (int i = 1; i < colValues.Length; i++) {
            queryString += " AND " + colNames[i] + operations[i] + "'" + colValues[i] + "'";
        }
        return ExecuteQuery(queryString);
    }


    /// <summary>
    /// 创建数据表
    /// </summary> +
    /// <returns>The table.</returns>
    /// <param name="tableName">数据表名</param>
    /// <param name="colNames">字段名</param>
    /// <param name="colTypes">字段名类型</param>
    public SQLiteDataReader CreateTable(string tableName, string[] colNames, string[] colTypes) {
        string queryString = "CREATE TABLE IF NOT EXISTS " + tableName + "( " + colNames[0] + " " + colTypes[0];
        for (int i = 1; i < colNames.Length; i++) {
            queryString += ", " + colNames[i] + " " + colTypes[i];
        }
        queryString += "  ) ";
        return ExecuteQuery(queryString);
    }

    /// <summary>
    /// Reads the table.
    /// </summary>
    /// <returns>The table.</returns>
    /// <param name="tableName">Table name.</param>
    /// <param name="items">Items.</param>
    /// <param name="colNames">Col names.</param>
    /// <param name="operations">Operations.</param>
    /// <param name="colValues">Col values.</param>
    public SQLiteDataReader ReadTable(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues) {
        string queryString = "SELECT " + items[0];
        for (int i = 1; i < items.Length; i++) {
            queryString += ", " + items[i];
        }
        queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
        for (int i = 0; i < colNames.Length; i++) {
            queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
        }
        return ExecuteQuery(queryString);
    }



    /// <summary>
    /// 查询一列数据
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="sql">单列查询</param>
    /// <returns></returns>
    public List<string> GetColmn(string connectionString, string sql) {
        try {
            List<string> Column = new List<string>();
            SQLiteCommand sqlcmd = new SQLiteCommand(sql, dbConn(connectionString));//sql语句
            SQLiteDataReader reader = sqlcmd.ExecuteReader();
            while (reader.Read()) {
                Column.Add(reader[0].ToString());
            }
            reader.Close();
            return Column;
        } catch {
            return null;
        } finally {
            closeConn();
        }
    }



    /// <summary>
    /// 获取指定频率的某个时间的字符串
    /// </summary>
    /// <param name="frequencyText">字符串：频率</param>
    /// <param name="fieldName">字符串：某个时间</param>
    /// <returns>string</returns>
    public  string GetTimeValue(string city, string station, string frequency, string fieldName) {
        try {
            string sql = 
                string.Format("SELECT {0} FROM freqs where city='{1}' and station='{2}' and frequency='{3}'", 
                fieldName, city, station, frequency);
            //MessageBox.Show("GetTimeValue sql: " + sql);
            SQLiteDataReader reader = ExecuteQuery(sql);
            string value = null;
            if (reader.HasRows) {
                while (reader.Read()) {
                    value = reader.GetString(0);
                }
            } else {
                return null;
            }
            return value;
        } catch (Exception ex) {
            return ex.ToString();
        } finally {
            closeConn();
        }
    }

    /// <summary>
    /// 获取指定频率的某个时间的int
    /// </summary>
    /// <param name="city"></param>
    /// <param name="station"></param>
    /// <param name="frequency"></param>
    /// <param name="fieldName"></param>
    /// <returns>int</returns>
    public int GetIntValue(string city, string station, string frequency, string fieldName) {
        try {
            string sql =
                string.Format("SELECT {0} FROM freqs where city='{1}' and station='{2}' and frequency='{3}'",
                fieldName, city, station, frequency);
            //MessageBox.Show("GetIntValue sql: " + sql);
            SQLiteDataReader reader = ExecuteQuery(sql);
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
            closeConn();
        }
    }


    /// <summary>
    /// 获取数据表中某一列所有的值
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="columnName"></param>
    /// <param name="whereName"></param>
    /// <param name="whereValue"></param>
    /// <returns></returns>
    public List<string> GetColunmValues(string connectionString,
                                string columnName,
                                string whereName, string whereValue) {
        try {
            string sql;
            if (whereName != null && whereValue != null) {
                sql = string.Format("SELECT {0} FROM {1} WHERE {2}='{3}'",
                    columnName, BCstatsHelper.tableName,
                    whereName, whereValue);
            } else {
                sql = string.Format("SELECT {0} FROM {1}", columnName, BCstatsHelper.tableName);
            }
            return GetColmn(connectionString, sql);
        } catch {
            return null;
        }
    }

    /// <summary>
    /// 获取数据表中某一列所有不重复的值
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="columnName"></param>
    /// <param name="whereName"></param>
    /// <param name="whereValue"></param>
    /// <returns></returns>
    public List<string> GetColunmDistinctValues(string connectionString,
                                string columnName,
                                string whereName, string whereValue) {
        try {
            string sql;
            if (whereName != null && whereValue != null) {
                sql = string.Format("SELECT DISTINCT {0} FROM {1} WHERE {2}='{3}'",
                    columnName, BCstatsHelper.tableName,
                    whereName, whereValue);
            } else {
                sql = string.Format("SELECT DISTINCT {0} FROM {1}", columnName, BCstatsHelper.tableName);
            }
            return GetColmn(connectionString, sql);
        } catch {
            return null;
        }
    }



    /// <summary>
    /// 获取数据表中某一种数据的数量
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="whereName"></param>
    /// <param name="whereValue"></param>
    /// <returns></returns>
    public int GetDataCount(string connectionString,
                                string whereName, string whereValue) {
        try {
            string sql;
            if (whereName != null && whereValue != null) {
                sql = string.Format("SELECT COUNT(*) FROM {0} WHERE {1}='{2}'",
                    BCstatsHelper.tableName,
                    whereName, whereValue);
            } else {
                sql = string.Format("SELECT COUNT(*) FROM {1}", BCstatsHelper.tableName);
            }
            return Convert.ToInt16(ExecuteScalar(connectionString, sql));
        } catch {
            return 0;
        }



    }


    private static SQLiteConnection m_dbConnection;
    /// <summary>
    /// 连接到数据库
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static SQLiteConnection dbConn(string connectionString) {
        m_dbConnection = new SQLiteConnection(connectionString);

        m_dbConnection.Open();

        return m_dbConnection;
    }

    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public static void closeConn() {
        try {
            if (m_dbConnection.State == ConnectionState.Open)
                m_dbConnection.Close();
            else if (m_dbConnection.State == ConnectionState.Broken) {
                m_dbConnection.Close();
            }
        } catch {
            ;
        }
    }



    /// <summary>
    /// 打印数组ArrayList
    /// </summary>
    /// <param name="arr"></param>
    public void PrintArray(ArrayList arr) {
        string str = "[";
        for (int i = 0; i < arr.Count; i++) {
            str += arr[i].ToString();
            if (i != arr.Count - 1) {
                str += ',';
            }
        }
        str += "]";
        Console.WriteLine(str);
    }


    /// <summary>
    /// 本类log
    /// </summary>
    /// <param name="s"></param>
    static void Log(string s) {
        Console.WriteLine("class SQLiteHelper:::" + s);
    }

}


