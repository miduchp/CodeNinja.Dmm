namespace CodeNinja.Dmm.Hid
{
    public class HidDeviceInfo
    {
        public string DevicePath { get; private set; }
        public ushort Vid { get; private set; }
        public ushort Pid { get; private set; }
        public string Product { get; private set; }
        public string Manufacturer { get; private set; }
        public string SerialNumber { get; private set; }

        public HidDeviceInfo(string product, string serial, string manufacturer,
            string devicePath, ushort vid, ushort pid)
        {
            Product = product;
            SerialNumber = serial;
            Manufacturer = manufacturer;
            DevicePath = devicePath;
            Vid = vid;
            Pid = pid;
        }
    }
}
