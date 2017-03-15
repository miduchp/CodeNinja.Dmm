using System;
using System.Runtime.InteropServices;

namespace CodeNinja.Dmm.Hid
{
    public static class NativeUtils
    {
        public static byte[] StructToBuffer<T>(T input) where T : struct
        {
            var bufferSize = Marshal.SizeOf(input);
            byte[] buffer = new byte[bufferSize];
            IntPtr ptr = IntPtr.Zero;

            try
            {
                ptr = Marshal.AllocHGlobal(bufferSize);
                Marshal.StructureToPtr(input, ptr, false);
                Marshal.Copy(ptr, buffer, 0, bufferSize);
                return buffer;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static T BufferToStruct<T>(byte[] buffer) where T : struct
        {
            GCHandle gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            return (T)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(T));
        }
    }
}
