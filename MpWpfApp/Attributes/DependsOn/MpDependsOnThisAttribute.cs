﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpDependsOnThisAttribute : Attribute {
        public MpDependsOnThisAttribute() { }
    }
}