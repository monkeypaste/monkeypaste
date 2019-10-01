using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;


namespace MonkeyPaste
{
    public class MpVirtualMachineDetails {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public bool IsVirtual {
            get {
                return IsParallels || IsVirtualBox || IsVmWare || IsVirtualPc;
            }
        }
        public bool IsParallels { get; internal set; }
        public bool IsVmWare { get; set; }
        public bool IsVirtualBox { get; set; }
        public bool IsVirtualPc { get; set; }
        public string DocumentsPath { get; set; }
    }

    public static class MpVirtualMachineDetailsExtensions {
        public static void Init(this MpVirtualMachineDetails host) {
            using(var searcher =
                new ManagementObjectSearcher("Select * from Win32_ComputerSystem")) {
                using(var items = searcher.Get()) {
                    foreach(var item in items) {
                        host.Manufacturer = item["Manufacturer"].ToString();
                        host.Model = item["Model"].ToString();

                        string manufacturer = host.Manufacturer.ToLower();
                        string model = host.Model.ToLower();

                        host.IsVirtualPc =
                            manufacturer == "microsoft corporation" &&
                                            model.Contains("virtual");
                        host.IsParallels = manufacturer.Contains("parallels");
                        host.IsVmWare = manufacturer.Contains("vmware");
                        host.IsVirtualBox = model == "virtualbox";
                    }
                }
            }
            host.DocumentsPath = host.FixDocPath();
        }

        private static string FixDocPath(this MpVirtualMachineDetails host) {
            string pathSeperator = @"\";

            string path = Environment.GetFolderPath(
                           Environment.SpecialFolder.MyDocuments);

            string userPath = Environment.GetFolderPath(
                               Environment.SpecialFolder.UserProfile);

            var fixPath = new Func<string,string,string>((p,u) => (p.Contains(u)) ?
                  p :
                  Path.Combine(u,p.Substring(p.LastIndexOf(pathSeperator))));

            if(host.IsVirtual) {
                string parallelsPath = @"\\Mac\Home";
                string vmWarePath = @"\\vmware-host\Shared Folders";

                if(host.IsParallels) {
                    if(path.Contains(parallelsPath))
                        path = path.Replace(parallelsPath,userPath);
                    else
                        fixPath(path,userPath);
                }

                else if(host.IsVmWare) {
                    if(path.Contains(vmWarePath))
                        path = path.Replace(vmWarePath,userPath);
                    else
                        fixPath(path,userPath);
                }

                else
                    fixPath(path,userPath);
            }

            return path;
        }
    }
    
}
