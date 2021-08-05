using Logger.Contracts;
using System;

namespace Logger
{
    public class Logger : ILogger
    {
        public void Log(string data)
        {
            Console.WriteLine(data);
        }
    }
}
