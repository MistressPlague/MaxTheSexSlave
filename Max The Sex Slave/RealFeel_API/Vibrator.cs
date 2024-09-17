using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using Newtonsoft.Json;

namespace Max_The_Sex_Slave.RealFeel_API
{
    internal class Vibrator
    {
        private HttpClient client = new HttpClient();
        internal bool Busy;

        internal void Vibrate(int Strength, int SensorID = 0)
        {
            if (Busy)
            {
                MelonLogger.Msg("Busy..");
                return;
            }

            //Task.Run(() =>
            {
                Busy = true;

                MelonLogger.Msg($"Sending Strength: {Strength}");

                try
                {
                    var _ = client.PostAsync("http://127.0.0.1:9020/vibrate", new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("Strength", Strength.ToString()),
                        new KeyValuePair<string, string>("SensorID", SensorID.ToString()),
                        new KeyValuePair<string, string>("IsPulse", (Strength != 0 && Strength != 20 && Slave.RealFeelPulseMode).ToString()),
                    }), new CancellationTokenSource(900).Token).Result;
                }
                catch (Exception e)
                {
                    MelonLogger.Error(e);
                }

                MelonLogger.Msg($"Sent Strength: {Strength}");

                Busy = false;
            }//);
        }

        public class RealFeelRequest
        {
            public int Strength;
            public int SensorID; // Default Is 4 Max. User Can Adjust To More.
            public bool IsPulse;
        }
    }
}