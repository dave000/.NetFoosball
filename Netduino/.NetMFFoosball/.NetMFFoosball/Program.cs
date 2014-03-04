using System;
using System.IO.Ports;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;
using Xively.NetMF.Interface;
using Toolbox.NETMF.NET;
using Xively.NetMF;
using Xively.NetMF.Entities;
using NetMFFoosball.Helpers;
using Xively.NetMF.Clients;

namespace NetMFFoosball
{
    enum InputType
    {
        Goal = 1,
        Temp = 2
    }

    enum Team
    {
        Blue = 1,
        Red = 2
    }

    public class Program
    {
        private const double refmVolt = 750;
        private const int refCels = 25;
        private const int deltamVolt = 30;

        private static object lockObj = new object();

        private static Toolbox.NETMF.Hardware.WiFlyGSX wifi;

        private static float[] lastTemps = new float[10];

        private static void refreshTime()
        {
            //while (true)
            {
                double milliseconds;
                lock (wifi)
                {
                    var ntpIp = wifi.DnsLookup("hu.pool.ntp.org");
                    milliseconds = wifi.NtpLookup(ntpIp);
                    Debug.Print("NTP:" + milliseconds);
                }

                var timeSpan = TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerSecond);
                var dateTime = new DateTime(1900, 1, 1);
                dateTime += timeSpan;

                var offsetAmount = new TimeSpan(1, 0, 0);
                var networkDateTime = (dateTime + offsetAmount);

                Microsoft.SPOT.Hardware.Utility.SetLocalTime(networkDateTime);
                Debug.Print("New time: " + networkDateTime.ToString("R"));
                //Thread.Sleep(60 * 1000);
            }
        }

        private static Team goalTeam;
        private static DateTime lastGoal = DateTime.MinValue;
        private static XivelyClient xc;
        private static Thread ithread = new Thread(Goal);
        private static TimeSpan deltaGoal = TimeSpan.FromTicks(TimeSpan.TicksPerSecond*2);

        private static void _goalHandler(uint port, uint data, DateTime dateTime)
        {
            if ((dateTime - lastGoal) < deltaGoal)
            {
                return;
            }

            lastGoal = dateTime;
            goalTeam = port == (int)Pins.GPIO_PIN_D8 ? Team.Blue : Team.Red;

            if (ithread.ThreadState == ThreadState.Unstarted)
            {
                ithread.Start();
            }
            else
            {
                ithread.Resume();
            }
        }

        public static void Main()
        {
            Thread.Sleep(1000);
            
            InterruptPort redPort = new InterruptPort(Pins.GPIO_PIN_D10, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            InterruptPort bluePort = new InterruptPort(Pins.GPIO_PIN_D8, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            
            #if EMULATOR
                ISocketHelper helper = new NetduinoSocketHelper();
            #else
                _connectWifi();
                refreshTime();
                ISocketHelper helper = new WiflyHelper(wifi);
            #endif
            
            //Thread ntpThread = new Thread();
            //ntpThread.Start();

            redPort.OnInterrupt += _goalHandler;
            bluePort.OnInterrupt += _goalHandler;

            xc = new XivelyTcpClient(new Uri("http://api.xively.com:8081"), helper);
            xc.CurrentFeed = new Feed(1996686508);
            xc.OnResponse += new XivelyResponse(xc_OnSuccess);
            xc.OnLostConnection += new ConnectionClosed(xc_OnLostConnection);
                       

            Thread.Sleep(5000);

            #if !EMULATOR
                Thread thread = new Thread(() =>
                {
                    MeasureTemp(xc);
                });
                thread.Start();
            #endif
            Thread.Sleep(Timeout.Infinite);
        }

        static void xc_OnLostConnection()
        {
            Debug.Print("Lost connection");
        }

        static bool xc_OnSuccess(long statusCode, string token, dynamic response)
        {
            Debug.Print("Response for:" + token);
            return true;
        }

        private static void _connectWifi()
        {
            lock (wifi ?? new object())
            {
                if (wifi != null)
                {
                    wifi.Dispose();
                }

                wifi = new Toolbox.NETMF.Hardware.WiFlyGSX(SerialPorts.COM1, (int)BaudRate.Baudrate115200, "$", true);
                wifi.JoinNetwork("dd-wrt", 0, WiFlyGSX.AuthMode.WPA2_PSK, "aabbccddff");
                Thread.Sleep(10);
                Debug.Print("IP:" + wifi.LocalIP);
            }
        }

        private static float getAvgTemp(float newTemp)
        {
            int tempDenom = 0;
            float tempNom = 0F;
            for (int i = 1; i < lastTemps.Length; i++)
            {
                tempDenom++;
                var oldTemp = lastTemps[i];
                oldTemp = oldTemp > 0 ? oldTemp : newTemp;
                lastTemps[i - 1] = oldTemp;
                tempNom += oldTemp;
            }

            tempDenom++;
            tempNom += newTemp;
            lastTemps[lastTemps.Length - 1] = newTemp;

            return tempNom / tempDenom;
        }

        private static void MeasureTemp(XivelyClient xc)
        {
            AnalogInput tempAnalog = new AnalogInput(Pins.GPIO_PIN_A0);
            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
            
            var messsureCounter = 0;
            var dataStreamValue = new DataStreamValue("Szoba");
            
            while (true)
            {
                messsureCounter++;
                Debug.Print("Begin sample");
                led.Write(true);
                try
                {
                    float sumCels = 0;
                    var now = DateTime.Now;
                    int i = 0;
                    while (now.AddMilliseconds(500) > DateTime.Now)
                    {
                        i++;
                        var temp = tempAnalog.Read();

                        // Converts the value to a voltage
                        float CurrentVoltage = (float)(temp * 3.3 / 1023);
                        // Converts the value to celsius
                        float aTemperature = (float)(CurrentVoltage - .5) * 100;
                        
                        sumCels += aTemperature;
                        Thread.Sleep(10);
                    }

                    float newTemp = (sumCels / i);
                    float cels = getAvgTemp(newTemp);
                    string currentTemp = cels.ToString("N1");
                    Debug.Print("Current temp:" + currentTemp);
                    if (messsureCounter % 6 == 0)
                    {
                        dataStreamValue.Value = currentTemp;
                        xc.Put(dataStreamValue);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("Error:" + ex.Message);
                }

                led.Write(false);
                Debug.Print("Starting to wait");
                Thread.Sleep(10 * 1000);
                Debug.Print("Waited 15 sec");
            }
        }

        private static void Goal()
        {
            OutputPort led = new OutputPort(Pins.GPIO_PIN_D9, false);
            
            var dataStreamValue = new DataStreamValue("Goal");
            while (true)
            {
                led.Write(true);
                
                dataStreamValue.Value = goalTeam.ToString();
                xc.Put( dataStreamValue);
                led.Write(false);
                Thread.CurrentThread.Suspend();
                //Thread.Sleep(1500);
            }
        }

    }
}
