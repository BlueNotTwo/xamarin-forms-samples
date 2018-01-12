﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaHelpers
{ 
    public interface IVideoPlayerController
    {
        bool CanPause { set; get; }

        bool CanSeek { set; get; }


            TimeSpan Duration { set; get; }
    

        event EventHandler PlayRequested;       // ???? See IWebViewController

    }
}
