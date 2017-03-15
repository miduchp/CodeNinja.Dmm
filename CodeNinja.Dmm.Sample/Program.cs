using CodeNinja.Dmm.Drivers.Ut71b;
using CodeNinja.Dmm.Hid;
using NLog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeNinja.Dmm.Sample
{
    class Program
    {
        private static ILogger Logger = LogManager.GetLogger("Logger");

        static void Main(string[] args)
        {
            using (var connector = new Driver(new HidDevice(), new HidDeviceEnumerator(), Logger))
            {
                var availableDevices = connector.Enumerate();

                for (int i = 0; i < availableDevices.Count(); i++)
                {
                    var device = availableDevices[i];
                    Console.WriteLine($"{i + 1}. Name: {device.Name} Path: {device.Path}");
                }

                string selectedDeviceId = string.Empty;
                int deviceId = 0;

                do
                {
                    Console.Write("Please select device: ");
                    selectedDeviceId = Console.ReadLine();


                } while (!Int32.TryParse(selectedDeviceId, out deviceId) || deviceId > availableDevices.Count() || deviceId < 1);


                connector.Open(availableDevices[deviceId - 1]);

                while (true)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var cancellationTokenSource = new CancellationTokenSource();
                            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(2000));

                            var readAsyncTask = connector.ReadAsync(cancellationTokenSource.Token);
                            
                            var measurment = await readAsyncTask;

                            Console.WriteLine($"{measurment.TimeStamp:hh:mm:ss.fff} {measurment.GetValue()}");
                        }
                        catch(Exception ex)
                        {
                            Logger.Log(LogLevel.Error, ex.Message);
                        }
                    }).GetAwaiter().GetResult();
                }
            }
        }
    }
}
