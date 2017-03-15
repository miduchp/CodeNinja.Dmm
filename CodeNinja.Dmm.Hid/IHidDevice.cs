using System.Threading;
using System.Threading.Tasks;

namespace CodeNinja.Dmm.Hid
{
    public interface IHidDevice
    {
        void Connect(string devicePath);
        T Read<T>() where T : struct;
        Task<T> ReadAsync<T>() where T : struct;
        Task<T> ReadAsync<T>(CancellationToken cancellationToken) where T : struct;
        int ReadRaw(byte[] buffer);
        Task<int> ReadRawAsync(byte[] buffer);
        Task<int> ReadRawAsync(byte[] buffer, CancellationToken cancellationToken);
        bool SetFeatureReport(byte[] buffer);
        bool SetFeatureReport<T>(T input) where T : struct;
        void Close();
    }
}