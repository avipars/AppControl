using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tizen.Network.Bluetooth;
using Tizen.Network.Connection;
using Tizen.Network.WiFi;
using Tizen.System;
using Xamarin.Forms;
using ContentPage = Xamarin.Forms.ContentPage;

namespace AppControl
{
    /// <summary>
    /// AppControl main class.
    /// 
    /// </summary>
    public class App : Application
    {
        string toggleBT = "Toggle BT", savedIP, grayscale = "Accessibility", visibility = "Visibility", toggleWifi = "Wi-Fi Settings";
        string infoCollected;
        Button bt, gray, vis, wifi;
        Label status, mac, ipaddress;
        static int fontSize = 8, buttonFont = 8;
        bool isBTEnabled = false;

        public App()
        {
            try
            {
                InitializeComponents();

                MainPage = new ContentPage
                {
                    Content = new StackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Orientation = StackOrientation.Vertical,
                        HeightRequest = 500, // Set a fixed height of 500
                        WidthRequest = 300,
                        Spacing = 2,
                        Padding = 10,
                        Children =
                        {
                            bt, gray, vis, ipaddress, mac, status
                        }
                    }
                };

                WifiCheck();
                setAllDetails();
            }
            catch (Exception ex)
            {
                //print to user
                Console.WriteLine("Error : " + ex.Message + ", " + ex.Source + ", " + ex.StackTrace);
                status.Text = "Error " + ex.ToString();
                MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }


        void InitializeComponents()
        {
            ipaddress = CreateLabel("\n IP: ");
            mac = CreateLabel("MAC: ");
            status = CreateLabel("\n Status: ");

            bt = CreateButton(toggleBT, async () => await launchBT());
            gray = CreateButton(grayscale, async () => await launchGrayscale());
            vis = CreateButton(visibility, async () => await launchVisibility());
        }


        

        void setAllDetails()
        {
            infoCollected = "Battery: " + getBattery();
            mac.Text = "MAC: " + getWIFIMac();
            // + " btMAC: " + getBTMac();
            status.Text = infoCollected;
            //https://docs.tizen.org/application/dotnet/guides/system/attached-devices/#ir
        }


        string getBattery()
        {
            return Battery.Percent + "% ";
            //+Battery.Level.ToString() 
            //https://docs.tizen.org/application/dotnet/api/TizenFX/latest/api/Tizen.System.BatteryLevelStatus.html
        }

        string getBTMac()
        {
            return BluetoothAdapter.Address;
        }
        string getWIFIMac()
        {
            // get Wi-Fi MAC address
            return WiFiManager.MacAddress;
        }

        void SetIP(string IP = null)
        {
            string toPrint;
            if (string.IsNullOrEmpty(IP))
            {
                toPrint = "";
                savedIP = "";
                ipaddress.IsVisible = false;
            }
            else
            {
                toPrint = "IP: " + IP;
                savedIP = IP;
                ipaddress.IsVisible = true;
            }
            ipaddress.Text = toPrint;
        }

        bool isConnectedWifi()
        {
            return (ConnectionManager.CurrentConnection.Type == ConnectionType.WiFi)
                      && (ConnectionManager.CurrentConnection.State == ConnectionState.Connected);
        }

        string getIpAddress()
        {
            System.Net.IPAddress ipAddress = ConnectionManager.GetIPAddress(AddressFamily.IPv4);
            savedIP = ipAddress.MapToIPv4().ToString();
            return savedIP;
        }


        bool IsBTEnabled()
        {
            try
            {
                return BluetoothAdapter.IsBluetoothEnabled;
            }
            catch (Exception e)
            {
                status.Text = "Error " + e.ToString();
                return false;
            }
        }

        void WifiCheck()
        {
            if (isConnectedWifi())
            {
                SetIP(getIpAddress());
            }
            else
            {
                SetIP();
            }
        }

        void BTCheck()
        {
           toggleButtons(IsBTEnabled());
        }

        void toggleButtons(bool status)
        {
           if (status)
           {
               toggleBT = "Disable BT";
           }
           else
           {
               toggleBT = "Enable BT";
           }
           bt.Text = toggleBT;
        }

        void UpdateButtonState(Button button, bool isEnabled)
        {
           Device.BeginInvokeOnMainThread(() =>
           {
               button.Text = isEnabled ? "Disable " + button.Text.Substring(7) : "Enable " + button.Text.Substring(7);
           });
        }

        Label CreateLabel(string text)
        {
            Label label = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                Text = text,
                FontSize = fontSize
            };

            return label;
        }

        Button CreateButton(string text, Func<Task> onClick)
        {
            Button button = new Button
            {
                Text = text,
                FontSize = buttonFont,
                Margin = 1,
                Padding = 1,
                Command = new Command(async () =>
                {
                    try
                    {
                        await onClick();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error : " + e.Message + ", " + e.Source + ", " + e.StackTrace);
                        status.Text = "Error " + e.ToString();
                        await MainPage.DisplayAlert("Error", e.Message, "OK");
                    }
                })
            };

            return button;
        }


        async Task openApp(string packageName)
        {
            Tizen.Applications.AppControl myAppControl = new Tizen.Applications.AppControl();
            myAppControl.ApplicationId = packageName;
            try
            {
                await Task.Run(() => Tizen.Applications.AppControl.SendLaunchRequest(myAppControl));
            }
            catch (Exception e)
            {
                status.Text = "Failed to launch: " + e.ToString();
            }
        }

        async Task launchVisibility()
        {
            await openApp("org.tizen.accessibility-setting");
        }

        async Task launchGrayscale()
        {
            //acces settings
            await openApp("com.samsung.clocksetting.accessibility");
        }

        async Task launchDebug()
        {
            await openApp("com.samsung.unit-test-sensor");
        }

        //Opens BT settings
        async Task launchBT()
        {
            Tizen.Applications.AppControl myAppControl = new Tizen.Applications.AppControl();
            if (!IsBTEnabled())
            {
                myAppControl.Operation = Tizen.Applications.AppControlOperations.SettingBluetoothEnable;
            }
            else
            {
                myAppControl.ApplicationId = "com.samsung.clocksetting.connections";
            }

            await Task.Run(() =>
            {
                Tizen.Applications.AppControl.SendLaunchRequest(myAppControl);
            });
        }

        //Opens WIFI settings not working
        async Task launchWIFI()
        {
            Tizen.Applications.AppControl myAppControl = new Tizen.Applications.AppControl();
            if (isConnectedWifi())
            {
                await launchDebug();
            }
            else
            {
                myAppControl.Operation = Tizen.Applications.AppControlOperations.SettingWifi;
            }

            await Task.Run(() => Tizen.Applications.AppControl.SendLaunchRequest(myAppControl));
        }


        /// <summary>
        /// Called when the application resumes from a sleeping state.
        /// </summary>
        protected override void OnResume()
        {
            // Handle when your app resumes
            setAllDetails();
            WifiCheck();
        }
    }
}