using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CodeNinja.Dmm.Drivers
{
    public abstract class BaseDriver : IDisposable, IBaseDriver
    {
        protected readonly  ILogger Logger;

        public BaseDriver(ILogger logger)
        {
            Logger = logger;
        }

        public abstract void Open();
        public abstract void OpenAsync();

        public abstract void Open(DeviceInfo deviceInfo);
        public abstract void OpenAsync(DeviceInfo deviceInfo);

        public abstract IList<DeviceInfo> Enumerate();

        public abstract Task<Measurement> ReadAsync();
        public abstract Task<Measurement> ReadAsync(CancellationToken cancellationToken);
        public abstract Measurement Read();

        public virtual void Close()
        {
            Dispose();
        }

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
