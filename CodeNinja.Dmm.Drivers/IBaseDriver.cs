using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CodeNinja.Dmm.Drivers
{
    public interface IBaseDriver
    {
        void Close();
        IList<DeviceInfo> Enumerate();
        void Open();
        void Open(DeviceInfo deviceInfo);
        void OpenAsync();
        void OpenAsync(DeviceInfo deviceInfo);
        Measurement Read();
        Task<Measurement> ReadAsync();
        Task<Measurement> ReadAsync(CancellationToken cancellationToken);
    }
}