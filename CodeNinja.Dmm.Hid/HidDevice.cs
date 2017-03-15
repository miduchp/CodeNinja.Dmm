using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CodeNinja.Dmm.Hid
{
    public class HidDevice : HidBaseDevice, IHidDevice
    {
        private DeviceMode _deviceMode;
        private HidDeviceCapabilities _deviceCapabilities;
        private FileStream _fileStream;
        private bool _disposed;

        public HidDevice(DeviceMode deviceMode = DeviceMode.Overlapped)
        {
            _deviceMode = deviceMode;
        }

        public void Connect(string devicePath)
        {           
            OpenDeviceIO(devicePath, _deviceMode, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE, ShareMode.ShareRead | ShareMode.ShareWrite);
            HidDeviceQuery();
            PrepareFileStream();
        }

        public bool SetFeatureReport(byte[] buffer)
        {
            return NativeMethods.HidD_SetFeature(DeviceHandle, buffer, buffer.Length);
        }

        public bool SetFeatureReport<T>(T input) where T : struct
        {
            byte[] buffer = NativeUtils.StructToBuffer<T>(input);

            return SetFeatureReport(buffer);
        }

        public int ReadRaw(byte [] buffer)
        {
            if(buffer.Length < _deviceCapabilities.InputReportByteLength)
            {
                throw new ArgumentException("Buffer too small");
            }

            return _fileStream.Read(buffer, 0, _deviceCapabilities.InputReportByteLength);
        }

        public Task<int> ReadRawAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            if (buffer.Length < _deviceCapabilities.InputReportByteLength)
            {
                throw new ArgumentException("Buffer too small");
            }

            return _fileStream.ReadAsync(buffer, 0, _deviceCapabilities.InputReportByteLength, cancellationToken);
        }

        public Task<int> ReadRawAsync(byte[] buffer)
        {
            if (buffer.Length < _deviceCapabilities.InputReportByteLength)
            {
                throw new ArgumentException("Buffer too small");
            }

            return _fileStream.ReadAsync(buffer, 0, _deviceCapabilities.InputReportByteLength);
        }

        public T Read<T>() where T : struct
        {
            byte[] buffer = GetBuffer();

            int bytesReaded = ReadRaw(buffer);
           
            return NativeUtils.BufferToStruct<T>(buffer);
        }

        public async Task<T> ReadAsync<T>() where T : struct
        {
            byte[] buffer = GetBuffer();

            int bytesReaded = await ReadRawAsync(buffer);

            return NativeUtils.BufferToStruct<T>(buffer);
        }

        public async Task<T> ReadAsync<T>(CancellationToken cancellationToken) where T : struct
        {
            byte[] buffer = GetBuffer();

            int bytesReaded = await ReadRawAsync(buffer, cancellationToken);

            return NativeUtils.BufferToStruct<T>(buffer);
        }

        public void Close()
        {
            Dispose(true);
        }

        private void HidDeviceQuery()
        {
            _deviceCapabilities = GetDeviceCapabilities();
        }

        private void PrepareFileStream()
        {
            SafeFileHandle safeHandle = new SafeFileHandle(DeviceHandle, false);

            if (!safeHandle.IsInvalid)
            {
                _fileStream = new FileStream(safeHandle, FileAccess.ReadWrite, _deviceCapabilities.InputReportByteLength, true);
            }
            else
            {
                throw new Exception("Could not create filestream!");
            }
        }

        private byte [] GetBuffer()
        {
            return new byte[_deviceCapabilities.InputReportByteLength];
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if(disposing)
                {
                    if(_fileStream != null)
                    { 
                        _fileStream.Dispose();
                        _fileStream = null;
                    }
                }

                this._disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
