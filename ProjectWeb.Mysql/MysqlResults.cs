using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectWeb.Mysql
{
    public class MysqlResults
    {
        public long NumberRows;
        public Dictionary<string, int> NameIndex = new Dictionary<string, int>();

        public static MysqlResults LoadFromDataReader(MySqlDataReader reader)
        {
            var mr = new MysqlResults();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                mr.NameIndex.Add(reader.GetName(i), i);                
            }

            if (reader.HasRows)
            {
                int colCount = reader.FieldCount;

                while(reader.Read())
                {
                    var mro = new MysqlRow();
                    object[] data = new object[colCount];
                    reader.GetValues(data);

                    mr.mysqlRows.AddLast(new MysqlRow() { Data = data, parent = mr });
                }
                mr.NumberRows = mr.mysqlRows.Count;
                mr.currentRow = mr.mysqlRows.First;
            }

            return mr;
        }
        protected LinkedList<MysqlRow> mysqlRows = new LinkedList<MysqlRow>();
        private LinkedListNode<MysqlRow> currentRow = null;

        public MysqlRow FetchAssoc()
        {
            return (currentRow = currentRow.Next)?.Value;
        }

        public static implicit operator bool(MysqlResults mysql)
        {
            return mysql != null;
        }
    }

    public class MysqlRow
    {
        public object[] Data;
        public MysqlResults parent;

        public static implicit operator bool(MysqlRow mysql)
        {
            return mysql != null;
        }

        public object this[int index] 
        {
            get {
                var result = Data[index];
                if (result == DBNull.Value)
                    return null;
                return result;                
            }
        }

        public object this[string name]
        {
            get
            {
                var index = parent.NameIndex[name];
                var result = Data[index];
                if (result == DBNull.Value)
                    return null;
                return result;                
            }
        }
    }
}
