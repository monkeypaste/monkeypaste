﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public interface MpIController {
        void Link(List<MpIView> viewList = null,List<object> modelList = null);
        void UpdateView();
        object Find(string name);
    }   

    public abstract class MpController :  MpIController {
        public static Dictionary<string,Dictionary<string,object>> ControllerDictionary { get; set; } = new Dictionary<string,Dictionary<string,object>>();
        public static int ControllerCount { get; set; } = 0;
        public static int ViewCount { get; set; } = 0;

        public string ControllerType { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public int ControllerId { get; set; }

        public MpController Parent { get; set; }

        public MpController(MpController p,int cid = -1) {
            ++ControllerCount;
            Parent = p;
            ControllerType = GetType().ToString();
            ControllerId = cid;
            ControllerName = ControllerType + (ControllerId == -1 ? ControllerCount : ControllerId);
        }

        public abstract void UpdateView();

        public void Link(List<MpIView> viewList = null,List<object> modelList = null) {
            return;
            //add controller type as sub-dicitonary
            if(!ControllerDictionary.ContainsKey(ControllerType)) {
                ControllerDictionary.Add(ControllerType,new Dictionary<string,object>());                
            }
            //add this controller to its type dictionary
            ControllerDictionary[ControllerType].Add(ControllerName,this);

            //add views
            foreach(MpIView vo in viewList) {
                if(!ControllerDictionary[ControllerType].ContainsKey(vo.ViewName)) {
                    ControllerDictionary[ControllerType].Add(vo.ViewName,vo.ViewData);
                }
                else {
                    Console.WriteLine("Warning overriding view: " + vo.ViewName);
                    ControllerDictionary[ControllerType][vo.ViewName] = vo;
                }
                if(vo.ViewData.GetType().IsSubclassOf(typeof(ContainerControl)) || vo.ViewData.GetType().IsSubclassOf(typeof(Control))) {
                    ((Control)vo.ViewData).KeyPress += View_KeyPress;
                }
                ((Control)vo.ViewData).Click += View_Click;
            }
        }
        
        public object Find(string name) {
            List<object> cl = new List<object>();
            // loop controller types
            foreach(KeyValuePair<string,Dictionary<string,object>> ctkvp in ControllerDictionary) {
                if(ctkvp.Key.Contains(name)) {
                    foreach(KeyValuePair<string,object> ckvp in ControllerDictionary[ctkvp.Key]) {
                        if(ckvp.Key.Contains(name)) {
                            cl.Add(ckvp.Value);
                        }
                    }
                }
                //loop controller sub components
                foreach(KeyValuePair<string,object> ckvp in ControllerDictionary[ctkvp.Key]) {
                    if(ckvp.Key.Contains(name)) {
                        cl.Add(ckvp.Value);
                    }
                }
            }
            if(cl.Count > 1) {
                return cl;
            }
            else if(cl.Count == 1) {
                return cl[0];
            }
            return null;
        }

        protected virtual void View_KeyPress(object sender,KeyPressEventArgs e) {
            if(MpSingletonController.Instance.GetMpData().GetSearchString() == string.Empty) {
                MpSingletonController.Instance.GetMpData().UpdateSearchString(e.KeyChar.ToString());
            }
        }

        protected void View_Click(object sender,EventArgs e) {
            Console.WriteLine("MpController view clicked w/ sender: " + sender.ToString());
        }
    }

    public abstract class MpController2 {
        public static Dictionary<string,object> ViewDictionary { get; set; } = new Dictionary<string,object>();

        public static Dictionary<string,object> Model { get; set; }

        public MpController2 Parent { get; set; }

        public MpController2(MpController2 Parent) {
            Parent = Parent;
        }

        protected void Link(List<object> vl) {
            foreach(object v in vl) {
                if(v.GetType().IsSubclassOf(typeof(Form)) || v.GetType().IsSubclassOf(typeof(Panel)) || v.GetType().IsSubclassOf(typeof(Control)) || v.GetType().IsSubclassOf(typeof(NotifyIcon))) {
                    string vn = v.GetType().ToString();
                    int count = 1;
                    while(ViewDictionary.ContainsKey(vn)) {                        
                        vn = v.GetType().ToString()+count++;
                    }
                    ViewDictionary.Add(vn,v);
                    if(!v.GetType().IsSubclassOf(typeof(NotifyIcon))) {
                        ((Control)v).KeyPress += View_KeyPress;
                    }                    
                    ((Control)v).Click += View_Click;
                } else {
                    Console.WriteLine("Warning, could not link view named: " + nameof(v) + " of type: " + v.GetType());
                }
            }
        }
        //uses Parent and children to define rect
        public abstract void UpdateView();

        protected virtual void View_KeyPress(object sender,KeyPressEventArgs e) {
            if(MpSingletonController.Instance.GetMpData().GetSearchString() == string.Empty) {
                MpSingletonController.Instance.GetMpData().UpdateSearchString(e.KeyChar.ToString());
            }            
        }

        private void View_Click(object sender,EventArgs e) {
            Console.WriteLine("MpController view clicked w/ sender: " + sender.ToString());
        }
    }
}