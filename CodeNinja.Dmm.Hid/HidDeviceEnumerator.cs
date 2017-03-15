using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeNinja.Dmm.Hid
{
    public class HidDeviceEnumerator : IHidDeviceEnumerator
    {
        public IList<HidDeviceInfo> Enumerate()
        {
            List<HidDeviceInfo> info = new List<HidDeviceInfo>();

            var hidGuid = GetHidGuid();

            var hInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero, NativeMethods.DIGCF_DEVICEINTERFACE | NativeMethods.DIGCF_PRESENT);

            var iface = new NativeMethods.DeviceInterfaceData();

            iface.Size = Marshal.SizeOf(iface);

            uint index = 0;

            while (NativeMethods.SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref hidGuid, index, ref iface))
            {
                var path = GetPath(hInfoSet, ref iface);

                try
                {
                    var deviceInfo = GetDeviceInfo(path);

                    if (deviceInfo != null)
                    {
                        info.Add(deviceInfo);
                    }
                }
                catch
                {

                }                

                index++;
            }

            if (NativeMethods.SetupDiDestroyDeviceInfoList(hInfoSet) == false)
            {
                throw new Win32Exception();
            }

            return info;
        }

        public HidDeviceInfo GetDeviceInfo(string path)
        {
            ushort vid, pid;

            using (var hidDevice = new HidBaseDevice())
            {
                hidDevice.OpenDeviceIO(path, HidBaseDevice.DeviceMode.Overlapped, 0, HidBaseDevice.ShareMode.ShareRead);

                var man = GetManufacturer(hidDevice.DeviceHandle);
                var prod = GetProduct(hidDevice.DeviceHandle);
                var serial = GetSerialNumber(hidDevice.DeviceHandle);
                GetVidPid(hidDevice.DeviceHandle, out vid, out pid);

                return new HidDeviceInfo(prod, serial, man, path, vid, pid);
            }
        }

        private static Guid GetHidGuid()
        {
            Guid hidGuild;
            NativeMethods.HidD_GetHidGuid(out hidGuild);
            return hidGuild;
        }

        private static string GetPath(IntPtr hInfoSet, ref NativeMethods.DeviceInterfaceData iface)
        {
            
            var detIface = new NativeMethods.DeviceInterfaceDetailData();
          
            uint reqSize = (uint)Marshal.SizeOf(detIface);

            detIface.Size = Marshal.SizeOf(typeof(IntPtr)) == 8 ? 8 : 5;

            bool status = NativeMethods.SetupDiGetDeviceInterfaceDetail(hInfoSet,
                ref iface, ref detIface, reqSize, ref reqSize, IntPtr.Zero);

            if (!status)
            {
                throw new Win32Exception();
            }

            return detIface.DevicePath;
        }

        private static string GetManufacturer(IntPtr handle)
        {
            var s = new StringBuilder(256);
            string rc = String.Empty;

            if (NativeMethods.HidD_GetManufacturerString(handle, s, s.Capacity))
            {
                rc = s.ToString();
            }

            return rc;
        }

        private static string GetProduct(IntPtr handle)
        {
            var s = new StringBuilder(256);
            string rc = String.Empty;

            if (NativeMethods.HidD_GetProductString(handle, s, s.Capacity))
            {
                rc = s.ToString();
            }

            return rc;
        }

        private static string GetSerialNumber(IntPtr handle)
        {
            var s = new StringBuilder(256);
            string rc = String.Empty;

            if (NativeMethods.HidD_GetSerialNumberString(handle, s, s.Capacity))
            {
                rc = s.ToString();
            }

            return rc;
        }

        private static void GetVidPid(IntPtr handle, out ushort Vid, out ushort Pid)
        {
            var attr = new NativeMethods.HidAttributtes();
            attr.Size = Marshal.SizeOf(attr);

            if (NativeMethods.HidD_GetAttributes(handle, ref attr) == false)
            {
                throw new Win32Exception();
            }

            Vid = attr.VendorID;
            Pid = attr.ProductID;
        }
    }
}