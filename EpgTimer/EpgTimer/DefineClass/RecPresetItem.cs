﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    public class RecPresetItem
    {
        public RecPresetItem()
        {
        }
        public String DisplayName
        {
            get;
            set;
        }
        public UInt32 ID
        {
            get;
            set;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
