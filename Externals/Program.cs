using System;
using System.IO.Ports;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

namespace TempSensor
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
        static Thread ithread = new Thread(Goal);

        private static void _goalHandler(uint port, uint data, DateTime dateTime)
        {
            goalTeam = port == (int)Pins.ONBOARD_SW1 ? Team.Blue : Team.Red;
            
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
            // write your code here

            //for (int i = 0; i < lastTemps.Length; i++)
            //{
            //    lastTemps[i] = 20F;
            //}

            InterruptPort onboard = new InterruptPort(Pins.ONBOARD_SW1, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            InterruptPort breadboard = new InterruptPort(Pins.GPIO_PIN_D8, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            _connectWifi();

            refreshTime();
            //Thread ntpThread = new Thread();
            //ntpThread.Start();

                        onboard.OnInterrupt += _goalHandler;
            breadboard.OnInterrupt += _goalHandler;


            //wifi.
            //wifi.EnableDHCP();




            Thread thread = new Thread(() =>
            {
                MeasureTemp();
            });
            thread.Start();

            Thread.Sleep(Timeout.Infinite);
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
			    var oldTemp =  lastTemps[i];
                oldTemp = oldTemp > 0 ? oldTemp : newTemp;
                lastTemps[i -1] = oldTemp;
                tempNom += oldTemp;
			}

            tempDenom++;
            tempNom += newTemp;
            lastTemps[lastTemps.Length - 1] = newTemp;

            return tempNom / tempDenom;
        }

        private static void MeasureTemp()
        {
            AnalogInput tempAnalog = new AnalogInput(Pins.GPIO_PIN_A0);
            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
            //Toolbox.NETMF.NET.WiFlySocket socket = new Toolbox.NETMF.NET.WiFlySocket("host", 8081, wifi);
            //Toolbox.NETMF.NET.HTTP_Client c = new Toolbox.NETMF.NET.HTTP_Client(socket);
            //c.
            var messsureCounter = 0;
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
                        //Debug.Print("Sample:" + aTemperature.ToString("N1") + " C°");
                        sumCels += aTemperature;
                        Thread.Sleep(10);
                    }

                    float newTemp = (sumCels / i);
                    float cels = getAvgTemp(newTemp);
                    Debug.Print(cels.ToString("N1") + " C°");
                    string json = "{\"method\" : \"put\", \"resource\" : \"/feeds/1996686508\", \"params\" : {}, \"headers\" : {\"X-ApiKey\":\"zrT2OYBhFdHhi0MsFxjlcwXhuoMMmekkkgok3MHcFNGkktF3\"},  \"body\" : {\"version\" : \"1.0.0\", \"datastreams\" : [{ \"id\" : \"Szoba\",\"current_value\" : \"" + cels.ToString("N1") + "\"} ] }, \"token\" : \"0x12345\"}";
                    if (messsureCounter % 6 == 0)
                    {
                        SendData(json, InputType.Temp);
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
            Random rnd = new Random();
            OutputPort led = new OutputPort(Pins.GPIO_PIN_D9, false);
            while (true)
            {
                led.Write(true);
                int team = (int)goalTeam;
                string json = "{\"method\" : \"put\", \"resource\" : \"/feeds/1996686508\", \"params\" : {}, \"headers\" : {\"X-ApiKey\":\"zrT2OYBhFdHhi0MsFxjlcwXhuoMMmekkkgok3MHcFNGkktF3\"},  \"body\" : {\"version\" : \"1.0.0\", \"datastreams\" : [{ \"id\" : \"Goal\",\"current_value\" : \"" + team + "\"} ] }, \"token\" : \"0x12345\"}";
                SendData(json, InputType.Goal);
                led.Write(false);
                Thread.CurrentThread.Suspend();
                //Thread.Sleep(1500);
            }
        }

        static void SendData(string json, InputType type)
        {
            Debug.Print("Starting to acquire lock:" + type);
            
            lock (lockObj)
            {
                Debug.Print("Acquired lock:" + type);
                try
                {
                    if (!wifi.SocketConnected)
                    {
                        Debug.Print("Opening socket");
                        wifi.OpenSocket("api.xively.com", 8081);
                        Debug.Print("Socket opened");
                    }

                    wifi.SocketWrite(json);
                    Debug.Print("Request Sent");
                    var result = Receive(true, wifi);
                    Debug.Print("Response:" + result);
                }
                catch (Exception ex)
                {
                    Debug.Print("Error:" + ex.Message);
                    _connectWifi();
                }
            }
            Debug.Print("Lock released:" + type);
        }

        static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            getResponse(sender as SerialPort);
        }

        private static void sendData(SerialPort serial, string data, bool addLineFeed = true)
        {

            byte[] buffer = new byte[data.Length + (addLineFeed ? 1 : 0)];
            for (int i = 0; i < data.Length; i++)
            {
                buffer[i] = (byte)data[i];
            }

            if (addLineFeed)
            {
                buffer[data.Length] = 0x0A;
            }

            serial.Write(buffer, 0, data.Length);
            serial.Flush();

        }

        public static string Receive(bool Block, WiFlyGSX wifi)
        {
            string RetValue = "";
            do
            {
                //Debug.Print("Reading message");
                RetValue = wifi.SocketRead(-1, "\r\n");
                //if (RetValue == "")
                //{
                //    Thread.Sleep(250);
                //}
            } while (Block && RetValue == "");

            return RetValue;
        }

        private static void getResponse(SerialPort serial)
        {
            String response = "";
            while (serial.BytesToRead > 0)
            {
                byte[] buf = new byte[1];
                serial.Read(buf, 0, 1);
                // Line feed - 0x0A - marks end of data.
                // Append each byte read until end of data.
                if (buf[0] != 0x0A)
                {
                    response += (char)buf[0];
                }
                else
                {
                    Debug.Print(response);
                    break;
                }
            }
        }
    }
}
