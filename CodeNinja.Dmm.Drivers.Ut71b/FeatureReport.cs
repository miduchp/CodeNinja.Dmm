using System;
using System.Runtime.InteropServices;

namespace CodeNinja.Dmm.Drivers.Ut71b
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FeatureReport
    {
        public Byte ReportID;
        public Int32 BaudRate;
        public Byte Unknown;
    }
}
