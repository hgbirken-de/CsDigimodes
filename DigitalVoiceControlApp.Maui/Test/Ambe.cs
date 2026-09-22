using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if ANDROID
using Android.Content;
using Android.Hardware.Usb;
#endif

namespace MauiApp2;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        //InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
#if ANDROID
        var act = Platform.CurrentActivity;
        UsbManager manager = (UsbManager)act.GetSystemService(Context.UsbService);
        IDictionary<string, UsbDevice> devicesDictionary = manager.DeviceList;

        if (devicesDictionary != null && devicesDictionary.Count > 0)
        {
            string devicesInfo = "Available USB Devices:\n";
            foreach (var device in devicesDictionary.Values)
            {
                devicesInfo += $"Device Name: {device.DeviceName}, Vendor ID: {device.VendorId}, Product ID: {device.ProductId}\n";
            }
            DisplayAlert("USB Devices", devicesInfo, "OK");
        }
        else
        {
            DisplayAlert("USB Devices", "No USB devices available.", "OK");
        }
#endif
    }
}