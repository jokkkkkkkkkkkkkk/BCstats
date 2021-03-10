using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Windows;
using System.Collections;
using System.IO;
using System.Linq;

public class SQLiteHelper {

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
    /// 判断数据库文件是否存在
    /// </summary>
    /// <returns></returns>
    public bool CheckDataBase(string dbFileName) {
        try {
            //判断数据文件是否存在
            string dbFilePath = AppDomain.CurrentDomain.BaseDirectory + dbFileName;
            bool dbExist = File.Exists(dbFilePath);
            if (!dbExist) {
                MessageBox.Show("数据库文件 " + dbFilePath +" 不存在。");
            } else {
                //MessageBox.Show("数据库文件 " + dbFilePath + " 存在。");
            }
            return true;
        } catch (Exception ex) {
            return false;
        }
    }
    /// <summary>
    /// 判断数据表书否存在
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public bool CheckDataTable(string connectionString, string tableName) {
        try {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            using (SQLiteCommand cmd = conn.CreateCommand()) {
                conn.Open();
                cmd.CommandText = 
                    string.Format(
                        "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '{0}'", 
                        tableName);
                object ob = cmd.ExecuteScalar();
                long tableCount = Convert.ToInt64(ob);
                if (tableCount != 0) {
                    // 存在
                    //MessageBox.Show("表存在");
                    return true;
                } else if (tableCount == 0) {
                    //MessageBox.Show("表不存在");
                    return false;
                } else {
                    //MessageBox.Show("表存在");
                    return true;
                }
            }
        } catch (Exception ex) {
            MessageBox.Show("读取数据表错误：" + ex.ToString());
            return false;
        }
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
    public SQLiteDataReader InsertValues(string tableName, 
        ArrayList columnNamessArr, ArrayList valuesArr) {
        // ArrayList to string
        string colStr = null;
        string valueStr = null;

        // 去掉ID列，让其自动生成ID
        columnNamessArr.RemoveAt(0);
        colStr = String.Join(", ", columnNamessArr.Cast<string>().ToArray());
        // 转换成功
        //Console.WriteLine("colStr: \n" + colStr);

        valueStr = "'" + valuesArr[0].ToString();
        for(int i = 1; i < valuesArr.Count; i++) {
            if(valuesArr[i].GetType() != typeof(int)) {
                valueStr += "' , '" + valuesArr[i];
            } else {
                valueStr += " , " + valuesArr[i];
            }
        }
        valueStr += "'";
        // 转换成功
        //Console.WriteLine("valueStr: \n" + valueStr);

        string queryString  = null;
        queryString = string.Format("INSERT INTO {0}({1}) VALUES({2})", tableName, colStr, valueStr);
        //Console.WriteLine("INSERT SQL: \n" + queryString);
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
        //Console.WriteLine("queryString : " + queryString);
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
    /// 根据一个条件，选出符合该条件的数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
     public SQLiteDataReader GetDataBy(string tableName, string key, string value) {
        string queryString 
            = string.Format("SELECT * FROM {0} WHERE {1}='{2}'",
            tableName,
            key, value);
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
    /// 判断记录是否存在：where key=value
    /// </summary>
    /// <param name="connectString"></param>
    /// <param name="tableName"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool IfExists(string connectString, string tableName, string key, string value) {
        string sql = string.Format("SELECT ISNULL((SELECT TOP(1) 1 FROM {0} WHERE {1}='{2}'), 0)" ,
            tableName, key, value);
        if(Convert.ToInt16(ExecuteScalar(connectString, sql)) == 1) {
            // 存在
            return true;
        } else {
            return false;
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
                string.Format("SELECT {0} FROM freqs WHERE city='{1}' AND station='{2}' AND frequency='{3}'", 
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
                string.Format("SELECT {0} FROM freqs WHERE city='{1}' AND station='{2}' AND frequency='{3}'",
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
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public List<string> GetColunmValues(string connectionString, string tableName,
                                string columnName,
                                string key, string value) {
        try {
            string sql;
            if (key != null && value != null) {
                sql = string.Format("SELECT {0} FROM {1} WHERE {2}='{3}'",
                    columnName, tableName,
                    key, value);
            } else {
                sql = string.Format("SELECT {0} FROM {1}", columnName, tableName);
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
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public List<string> GetColunmDistinctValues(string connectionString, string tableName,
                                string columnName,
                                string key, string value) {
        try {
            string sql;
            if (key != null && value != null) {
                sql = string.Format("SELECT DISTINCT {0} FROM {1} WHERE {2}='{3}'",
                    columnName, tableName,
                    key, value);
            } else {
                sql = string.Format("SELECT DISTINCT {0} FROM {1}", columnName, tableName);
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
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public int GetDataCounBy(string connectionString, string tableName,
                                string key, string value) {
        try {
            string sql;
            if (key != null && value != null) {
                sql = string.Format("SELECT COUNT(*) FROM {0} WHERE {1}='{2}'",
                    tableName,
                    key, value);
            } else {
                sql = string.Format("SELECT COUNT(*) FROM {1}", tableName);
            }
            return Convert.ToInt16(ExecuteScalar(connectionString, sql));
        } catch {
            return 0;
        }



    }


    /// <summary>
    /// 获取数据表中所有数据的数量
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public int GetDataCounts(string connectionString, string tableName) {
        try {
            string sql = "SELECT COUNT(*) FROM " + tableName;
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
    public void PrintArrayList(ArrayList arr) {
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


