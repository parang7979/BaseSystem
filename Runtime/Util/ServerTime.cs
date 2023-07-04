using Cysharp.Threading.Tasks;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace Parang.Util
{
    static public class ServerTime
    {
        private static readonly string[] urls =
            { "https://google.com", "https://naver.com", "https://bing.com" };

        public static DateTime UtcNow => DateTime.UtcNow.AddTicks(diffTick);
        public static DateTime Now => DateTime.Now.AddTicks(diffTick);

        private static long diffTick = 0L;
        private static DateTime serverTime = DateTime.UtcNow;

        private static void UpdateTime(string dateStr)
        {
            if (DateTime.TryParse(dateStr, null, DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
            {
                serverTime = dateTime;
                diffTick = (serverTime - DateTime.UtcNow).Ticks;
            }
        }

        public static async UniTask Sync()
        {
            var url = urls[UnityEngine.Random.Range(0, urls.Length)];
            using (var request = UnityWebRequest.Get(url))
            {
                try
                {
                    var res = await request.SendWebRequest();
                    var headers = res.GetResponseHeaders();
                    UpdateTime(headers["Date"]);
                    Debug.Log($"[LastSync From ({url}) : {serverTime}] Device Time : {DateTime.UtcNow} (diff : {diffTick})");
                }
                catch
                {

                }
            }
        }
    }
}
