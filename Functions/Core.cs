using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Functions
{
    public abstract class Core 
    {
        public static bool Debug = false; 
      	public abstract void DisplayDebug();
      	public abstract void DisplayDebugWithBuffer();
    }
}