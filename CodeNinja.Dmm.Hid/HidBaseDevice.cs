using System;
using System.Runtime.InteropServices;

namespace CodeNinja.Dmm.Hid
{
    public class HidBaseDevice : IDisposable
    {
        public IntPtr DeviceHandle { get; protected set; }

        public enum DeviceMode
        {
            NonOverlapped = 0,
            Overlapped = 1
        }

        [Flags]
        public enum ShareMode
        {
            Exclusive = 0,
            ShareRead = NativeMethods.FILE_SHARE_READ,
            ShareWrite = NativeMethods.FILE_SHARE_WRITE
        }

        public void OpenDeviceIO(string devicePath, DeviceMode deviceMode, uint deviceAccess, ShareMode shareMode)
        {
            var security = new NativeMethods.SECURITY_ATTRIBUTES();
            var flags = 0;

            if (deviceMode == DeviceMode.Overlapped)
            {
                flags = NativeMethods.FILE_FLAG_OVERLAPPED;
            }

            security.lpSecurityDescriptor = IntPtr.Zero;
            security.bInheritHandle = true;
            security.nLength = Marshal.SizeOf(security);

            DeviceHandle = NativeMethods.CreateFile(devicePath, deviceAccess, (int)shareMode, ref security, NativeMethods.OPEN_EXISTING, flags, 0);

            if (!IsValidDeviceHandle())
            {
                throw new Exception();
            }
        }

        protected bool IsValidDeviceHandle()
        {
            return DeviceHandle.ToInt32() != NativeMethods.INVALID_HANDLE_VALUE;
        }

        public void CloseDeviceIO()
        {
            if (Environment.OSVersion.Version.Major > 5)
            {
                NativeMethods.CancelIoEx(DeviceHandle, IntPtr.Zero);
            }

            if (!NativeMethods.CloseHandle(DeviceHandle))
            {
                throw new Exception();
            }
            else
            {
                DeviceHandle = IntPtr.Zero;
            }
        }

        public HidDeviceCapabilities GetDeviceCapabilities()
        {
            var capabilities = default(NativeMethods.HIDP_CAPS);
            var preparsedDataPointer = default(IntPtr);

            if (NativeMethods.HidD_GetPreparsedData(DeviceHandle, ref preparsedDataPointer))
            {
                NativeMethods.HidP_GetCaps(preparsedDataPointer, ref capabilities);
                NativeMethods.HidD_FreePreparsedData(preparsedDataPointer);
            }

            return new HidDeviceCapabilities(capabilities);
        }

        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (IsValidDeviceHandle())
                    {
                        CloseDeviceIO();
                    }
                }
                catch
                {

                }                

                _disposed = true;
            }
        }

        
        ~HidBaseDevice() {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
