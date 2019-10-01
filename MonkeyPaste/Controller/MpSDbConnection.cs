using MonkeyPaste.Model;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SQLite;
using System.Net;
using System.Net.NetworkInformation;

namespace MonkeyPaste {
    public sealed class MpSDbConnection {
        //private static readonly Lazy<MpSDbConnection> lazy = new Lazy<MpSDbConnection>(() => new MpSDbConnection());
        //public static MpSDbConnection Instance { get { return lazy.Value; } }
        private MySqlConnection _connection;
        private MpUser _user = null;
        //private MpSDbConnection() { }
        private void SetConnection() {
            string server = "localhost";
            string database = "mpsdb";
            string uid = "root";
            string password = "F00dlion";
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            _connection = new MySqlConnection(connectionString);
        }
        public MpUser GetUser() {
            return _user;
        }
        public void SetUser(MpUser newUser) {
            _user = newUser;
        }
        // insert / update / delete
        public void ExecuteNonQuery(string sql) {
            SetConnection();
            _connection.Open();
            var cmd = new MySqlCommand(sql,_connection);
            cmd.ExecuteNonQuery();
            _connection.Close();
        }

        // select
        public DataTable Execute(string sql) {
            SetConnection();
            _connection.Open();
            var cmd = new MySqlCommand(sql,_connection);
            DataTable dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            return dt;
        }
    }
}
