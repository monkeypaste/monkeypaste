using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpRegistryController {
        //private static readonly Lazy<MpRegistryController> lazy = new Lazy<MpRegistryController>(() => new MpRegistryController());
        //public static MpRegistryController Instance { get { return lazy.Value; } }

        private RegistryKey _key = null;

        public MpRegistryController() {
            _key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + MpProgram.AppName,true);
            if(_key == null) {
                RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey("SOFTWARE",true);
                _key = softwareKey.CreateSubKey(MpProgram.AppName,RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
        }
        public string GetDBCPath() {
            if(/*_key.GetValue("DBCPath") == null*/true) {
                string dbPath = @"C:\Users\tkefauver\Documents\" + MpProgram.AppName;//Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + MpProgram.AppName;
                //var vm = new MpVirtualMachineDetails();
                //vm.Init();
                //Console.WriteLine("Documents path: " + vm.DocumentsPath);
                //MpRegistryController.Instance.SetDBCPath(vm.DocumentsPath+"\\"+MpProgram.AppName);              
                SetDBCPath(dbPath);
            }
            return (string)_key.GetValue("DBCPath");
        }
        public bool GetAutoLogin() {
            if(_key.GetValue("AutoLogin") == null) {
                SetAutoLogin(false);
            }
            string val = (string)_key.GetValue("AutoLogin");            
            if(val == "1") return true;
            return false;
        }
        public string GetEmail() {
            return (string)_key.GetValue("Email");
        }
        public string GetPassword() {
            return (string)_key.GetValue("Pword");
        }
        public void SetDBCPath(string dbcPath) {
            _key.SetValue("DBCPath",(object)dbcPath);
        }
        public void SetEmail(string email) {
            _key.SetValue("Email",(object)email); 
        }
        
        public void SetAutoLogin(bool isAutoLogin) {
            string val = isAutoLogin ? "1" : "0";
            _key.SetValue("AutoLogin",(object)val);
        }
        public void SetPassword(string pass) {
            _key.SetValue("Pword",(object)pass);
        }
        public void DeleteEmail() {
            if(_key.GetValue("Email") != null)
            _key.DeleteSubKey("Email");
        }
        public void DeletePassword() {
            if(_key.GetValue("Pword") != null)
            _key.DeleteSubKey("Pword");
        }
    }
}
