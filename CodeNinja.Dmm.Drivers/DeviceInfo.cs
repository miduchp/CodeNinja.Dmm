namespace CodeNinja.Dmm.Drivers
{
    public class DeviceInfo
    {
        public DeviceInfo(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }
    }
}
