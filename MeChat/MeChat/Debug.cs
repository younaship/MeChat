using System;
using System.Collections.Generic;
using System.Text;

namespace MeChat
{
    public class Debug
    {
        public static void Log(string message) {
            Android.Util.Log.Info("MyInfo", message);
        }

        public static void Log(string tag,string message)
        {
            Android.Util.Log.Info("MyInfo", "["+tag+"] "+message);
        }

        public static void Error(string message)
        {
            Android.Util.Log.Info("MyError", message);
        }

        public static void Error(string tag,string message)
        {
            Android.Util.Log.Info("MyError","["+tag+"]"+message);
        }
    }
}
