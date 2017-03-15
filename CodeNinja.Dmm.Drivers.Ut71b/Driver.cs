using CodeNinja.Dmm.Hid;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace CodeNinja.Dmm.Drivers.Ut71b
{
    public class Driver : BaseDriver
    {
        private IHidDevice _device;
        private IHidDeviceEnumerator _deviceEnumerator;
        private PacketParser _packetParser;

        private byte[] _dataBuffer = new byte[256];
        private int _dataBufferPosition = -1;        
        private bool _disposed = false;

        private const int PacketLength = 11;
        private const ushort DeviceVid = 0x1A86;
        private const ushort DevicePid = 0xE008;


        public Driver(IHidDevice device, IHidDeviceEnumerator deviceEnumerator, ILogger logger) : base(logger)
        {
            _device = device;
            _deviceEnumerator = deviceEnumerator;
            _packetParser = new PacketParser(logger);
        }

        public override void Open()
        {
            var selectedDevice = Enumerate().First();

            Open(selectedDevice);
        }

        public override void Open(DeviceInfo deviceInfo)
        {
            ResetBuffer();

            Init(deviceInfo);
        }

        public override async void OpenAsync()
        {
            throw new NotImplementedException();
        }

        public override async void OpenAsync(DeviceInfo deviceInfo)
        {
            throw new NotImplementedException();
        }

        public override IList<DeviceInfo> Enumerate()
        {
            return _deviceEnumerator.Enumerate()
                                    .Where(device=> device.Vid == DeviceVid && device.Pid == DevicePid)
                                    .Select(device => new DeviceInfo(device.Product, device.DevicePath))
                                    .ToList();
        }
        
        public override async Task<Measurement> ReadAsync()
        {
            Measurement measurment = null;

            do
            {
                var inputReport = await _device.ReadAsync<InputReport>();
                measurment = ParsePacket(inputReport);
            }
            while (measurment == null);

            return measurment;
        }

        public override async Task<Measurement> ReadAsync(CancellationToken cancellationToken)
        {
            Measurement measurment = null;

            do
            {
                var inputReport = await _device.ReadAsync<InputReport>(cancellationToken);
                measurment = ParsePacket(inputReport);
                cancellationToken.ThrowIfCancellationRequested();
            }
            while (measurment == null);

            return measurment;
        }

        public override Measurement Read()
        {
            Measurement measurment = null;

            do
            {
                var inputReport = _device.Read<InputReport>();
                measurment = ParsePacket(inputReport);
            }
            while (measurment == null);

            return measurment;
        }

        private void Init(DeviceInfo deviceInfo)
        {
            var featureReport = new FeatureReport()
            {
                BaudRate = 2400,
                ReportID = 0,
                Unknown = 0x03
            };

            _device.Connect(deviceInfo.Path);
            _device.SetFeatureReport(featureReport);
        }

        private int GetInputReportBytes(InputReport inputReport)
        {
            return inputReport.Bytes & 0x0f;
        }

        private InputReport RemovePartityBits(InputReport inputReport)
        {
            for(int i=0; i < GetInputReportBytes(inputReport); i++)
            {
                inputReport.Data[i] = (byte)(inputReport.Data[i] & ~(1 << 7));
            }

            return inputReport;
        }

        private void TransferToDataBuffer(InputReport inputReport)
        {
            var actualNumberOfBytes = GetInputReportBytes(inputReport);

            if (actualNumberOfBytes > 0 && _dataBufferPosition < _dataBuffer.Length)
            {
                inputReport = RemovePartityBits(inputReport);

                Array.Copy(inputReport.Data, 0, _dataBuffer, _dataBufferPosition + 1, actualNumberOfBytes);

                _dataBufferPosition += actualNumberOfBytes;
            }
        }

        private void ResetBuffer()
        {
            _dataBufferPosition = -1;
        }

        private void RemovePacketFromBuffer(int i)
        {
            if (_dataBufferPosition >= (PacketLength + i))
            {
                var startIndex = PacketLength + i;
                var bytesToCopy = _dataBufferPosition - (PacketLength + i) + 1;

                Array.Copy(_dataBuffer, startIndex, _dataBuffer, 0, bytesToCopy);

                _dataBufferPosition = _dataBufferPosition - (PacketLength + i);
            }
            else
            {
                ResetBuffer();
            }
        }

        private Measurement ParsePacket(InputReport inputReport)
        {            
            TransferToDataBuffer(inputReport);

            if (_dataBufferPosition >= PacketLength)
            {
                for (int offset = 0; offset < _dataBufferPosition - PacketLength; offset++)
                {
                    var packet = _dataBuffer.Skip(offset).Take(PacketLength).ToArray();

                    if (_packetParser.IsValid(packet))
                    {
                        try
                        {
                            return _packetParser.Parse(packet);
                        }
                        finally
                        {
                            RemovePacketFromBuffer(offset);
                        }
                    }
                }
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if(_device!=null)
                    {
                        _device.Close();
                    }
                }
                _disposed = true;
            }
        }
    }
}
