﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpDependsOnSiblingAttribute : MpDependsOnBase {
        public MpDependsOnSiblingAttribute() { }

        public MpDependsOnSiblingAttribute(params object[] args) : base(args) { }
    }
}