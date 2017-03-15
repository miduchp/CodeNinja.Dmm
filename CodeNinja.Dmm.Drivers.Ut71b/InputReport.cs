using System.Runtime.InteropServices;

namespace CodeNinja.Dmm.Drivers.Ut71b
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InputReport
    {
        public byte ReportID;
        public byte Bytes;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 7)]
        public byte[] Data;
    }
}
