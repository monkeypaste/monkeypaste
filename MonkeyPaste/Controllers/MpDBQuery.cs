using MonkeyPaste.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace MonkeyPaste {
    public enum MpUserState {
        PrePending=1,
        Pending,
        Active,
        Reset,
        Inactive,
        Deactivated,
        Offline
    };
    public sealed class MpDBQuery {
        private string _dbPath;

        public MpDBQuery(string dbPath) {
            _dbPath = dbPath;
        }
        
        public bool Register(string email,string password) {
            DataTable dt = MpSDbConnection.Instance.Execute("select * from MpUser where Email='" + email + "'");

            if(dt.Rows.Count > 0) {
                Console.WriteLine("registration error, user already exists.");
                return false;
            }

            dt = MpSDbConnection.Instance.Execute("insert into MpUser(MpUserStateId,Email,Pword) values (" + (int)MpUserState.Pending + ",'" + email + "','" + password + "');");
            // TODO update here to encrypt password

            return true;      
        }
        public bool Login(string email,string password) {
            if(MpSDbConnection.Instance.GetUser() != null) {
                Console.WriteLine("User already exixts, ignoring login");
                return true;
            }
            DataTable dt = MpSDbConnection.Instance.Execute("select * from MpUser where Email='" + email + "' and Pword='" + password + "';");
            if(dt.Rows.Count > 0) {
                MpSDbConnection.Instance.SetUser(new MpUser() {
                    MpUserId = Convert.ToInt32(dt.Rows[0][0].ToString()),
                    MpClientId = Convert.ToInt32(dt.Rows[0][1].ToString()),
                    UserState = (MpUserState)Convert.ToInt32(dt.Rows[0][2].ToString()),
                    Email = dt.Rows[0][3].ToString()
                });
                return true;
            }
            return false;
        }
        public void Logout() {
            if(MpSDbConnection.Instance.GetUser() == null || MpCDbConnection.Instance.GetClient() == null) {
                return;
            }
            MpCDbConnection.Instance.ExecuteNonQuery("update MpClient set Logout DateTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where pk_MpClientId=" + MpCDbConnection.Instance.GetClient().MpClientId);
            MpSDbConnection.Instance.SetUser(null);
            MpCDbConnection.Instance.SetClient(null);
        }
        public void ResetUser(string email) {
            //string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            //password = BCrypt.Net.BCrypt.HashPassword(password,salt);
            DataTable dt = MpSDbConnection.Instance.Execute("select * from MpUser where Email='" + email + "';");
            if(dt.Rows.Count > 0) {
                MpUser user = new MpUser() {
                    MpUserId = Convert.ToInt32(dt.Rows[0][0].ToString()),
                    UserState = (MpUserState)Convert.ToInt32(dt.Rows[0][1].ToString()),
                    Email = dt.Rows[0][2].ToString(),
                    //UserHandle = dt.Rows[0][2].ToString().Replace("@","_at_").Replace(".","_dot_")
                };
                MpSDbConnection.Instance.ExecuteNonQuery("update MpUser set UserState=" + (int)MpUserState.Reset + ", Pword='" + GeneratePassword() + "' where Email='" + email + "';");
                MessageBox.Show("Password reset successfully, check email for further instructions");
            }
            else {
                MessageBox.Show("Email could not be found, try registering a new account");
            }
        }                
         
    }
}
