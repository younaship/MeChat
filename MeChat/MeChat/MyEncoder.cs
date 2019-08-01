using System;
using System.Collections.Generic;
using System.Text;

namespace MeChat
{
    public class MyEncoder
    {
        public DateTime Get(string s)
        {
            return DateTime.Parse(s);
        }
    }
}
