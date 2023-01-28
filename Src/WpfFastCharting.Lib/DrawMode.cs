using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfFastCharting.Lib
{
    public enum DrawMode
    {
        AllOfData=0,//all of data
        TailOfData,//just end piece of data
        TailOfDataEkg//elektro cardio like 
    }
    
    public enum ScaleMode
    {
        Auto,
        Manual
    }
}
