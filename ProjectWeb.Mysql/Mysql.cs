using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectWeb.Mysql
{
    public class Mysql
    {
        public string Servername { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public int Port { get; set; }

        public MySqlConnection Connection;

        public bool ConnectError => this;

        public void Close()
        {
            if(Connection != null)
            {
                Connection.Close();
            }
        }

        private Exception exception;
        public Exception Error()
        {
            return exception;
        }

        public static Exception Error(Mysql mysql)
        {
            return mysql?.Error();
        }

        private long insertId;
        public long InsertId()
        {
            return insertId;
        }

        public static long InsertId(Mysql mysql)
        {
            if (mysql == null)
                return 0;
            return mysql.insertId;
        }

        public static long NumberRows(MysqlResults results)
        {
            if (results == null)
                return 0;
            return results.NumberRows;
        }        

        public static MysqlRow FetchAssoc(MysqlResults results)
        {
            if (results == null)
                return null;
            return results.FetchAssoc();
        }

        public MysqlResults Query(string query, params MySqlParameter[] mySqlParameters)
        {
            insertId = 0;
            exception = null;
            try
            {                
                using (var command = new MySqlCommand(query, Connection))
                {
                    if (mySqlParameters != null && mySqlParameters.Length > 0)
                        command.Parameters.AddRange(mySqlParameters);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        insertId = command.LastInsertedId;

                        return MysqlResults.LoadFromDataReader(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }            
        }

        public static MysqlResults Query(Mysql mysql, string query, params MySqlParameter[] mySqlParameters)
        {
            if (mysql == null)
                return null;
            return mysql.Query(query, mySqlParameters);
        }

        public static void Close(Mysql mysql)
        {
            mysql?.Close();
        }

        public static implicit operator bool(Mysql mysql)
        {
            return mysql?.Connection != null && mysql.Connection.State == System.Data.ConnectionState.Open;
        }

        public Mysql(string servername, string username, string password, string database = "", int port = 3306) // $servername, $username, $password
        {
            Servername = servername;
            Username = username;
            Password = password;
            Database = database;
            Port = port;

            Connection = new MySqlConnection($"Server={Servername};Port={port};Uid={username};Pwd='{password}';" + (string.IsNullOrWhiteSpace(database) ? "" : $"Database={database}") + ";");
            Connection.Open();
        }

        public static Mysql Connect(string servername, string username, string password, string database = "", int port = 3306)
        {
            return new Mysql(servername, username, password, database, port);
        }
    }
}
