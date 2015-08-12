using System;
using System.Data.SQLite;
using System.Data;
using System.Collections.Generic;

namespace Tvmaid
{
    //SQL実行
    public class Sql : IDisposable
    {
        IDbCommand command = null;
        
        public Sql() { }

        public Sql(bool open)
        {
            if (open) { Open(); }
        }

        public void Open()
        {
            command = new SQLiteCommand();
            command.Connection = new SQLiteConnection();
            command.Connection.ConnectionString = "Default Timeout=90; Synchronous=OFF; Page Size=4096; Data Source=" + System.IO.Path.Combine(Util.GetUserPath(), "Tvmaid.db");
            command.Connection.Open();
        }

        public void Dispose()
        {
            command.Connection.Dispose();
            command.Dispose();
        }

        public string Text
        {
            get { return command.CommandText; }
            set { command.CommandText = value; }
        }

        public void Execute()
        {
            command.ExecuteNonQuery();
        }

        public object GetData()
        {
            return command.ExecuteScalar();
        }

        public DataTable GetTable()
        {
            return new DataTable(command.ExecuteReader());
        }

        public void BeginTrans()
        {
            command.Transaction = command.Connection.BeginTransaction();
        }
        /*
        public bool GetTransState()
        {
            return command.Transaction != null;
        }
        */
        public void Rollback()
        {
            command.Transaction.Rollback();
        }

        public void Commit()
        {
            command.Transaction.Commit();
        }

        public static string SqlEncode(string text)
        {
            return text.Replace("'", "''");
        }

        public static string SqlLikeEncode(string text)
        {
            text = SqlEncode(text);

            text = text.Replace(">", "^>");
            text = text.Replace("_", "^_");
            text = text.Replace("%", "^%");

            return text;
        }

        public int GetNextId(string table)
        {
            Text = "select max(id) as maxid from " + table;
            object obj = GetData();

            if (DBNull.Value.Equals(obj))
            {
                return 0;
            }

            return (int)(long)obj + 1;
        }

        public List<object[]> GetList()
        {
            using (var table = GetTable())
            {
                var list = new List<object[]>();
                while (table.Read())
                {
                    object[] values = new object[table.FieldCount];
                    table.GetValues(values);
                    list.Add(values);
                }
                return list;
            }
        }
    }

    //結果テーブル
    public class DataTable : IDisposable
    {
        protected IDataReader reader = null;

        public DataTable(IDataReader dr)
        {
            reader = dr;
        }

        public string GetStr(int i)
        {
            return (string)reader[i];
        }

        public int GetInt(int i)
        {
            return (int)(long)reader[i];
        }

        public long GetLong(int i)
        {
            return (long)reader[i];
        }

        public string GetStr(string name)
        {
            return (string)reader[name];
        }

        public int GetInt(string name)
        {
            return (int)(long)reader[name];
        }

        public long GetLong(string name)
        {
            return (long)reader[name];
        }

        public bool Read()
        {
            return reader.Read();
        }

        public void Dispose()
        {
            reader.Dispose();
        }
        /*
        public bool IsNull(string name)
        {
            return reader.IsDBNull(reader.GetOrdinal(name));
        }
        */
        public bool IsNull(int i)
        {
            return reader.IsDBNull(i);
        }

        public int FieldCount
        {
            get { return reader.FieldCount; }
        }

        internal void GetValues(object[] values)
        {
            reader.GetValues(values);
        }
    }
}
