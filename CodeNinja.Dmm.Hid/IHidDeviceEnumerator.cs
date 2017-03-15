using System.Collections.Generic;

namespace CodeNinja.Dmm.Hid
{
    public interface IHidDeviceEnumerator
    {
        IList<HidDeviceInfo> Enumerate();
        HidDeviceInfo GetDeviceInfo(string path);
    }
}