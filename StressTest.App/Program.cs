using StressTest.App.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTest.App
{
    class Program
    {
        static void Main(string[] args)
        {

            string data;
            using (var sr = new StreamReader(@"D:\Work Related\Redis Stress Test\StressTest\StressTest.App\Sample.json"))
            {
                data = sr.ReadToEnd();
            }

            var logger = new AppLogger();
           

            var executor = new RedisExecutor(logger);

            int.TryParse(ConfigurationManager.AppSettings["threadsNumber"], out int threadsNumber);
            int.TryParse(ConfigurationManager.AppSettings["operationsNumber"], out int operationsNumber);
            int.TryParse(ConfigurationManager.AppSettings["secondsNumber"], out int secondsToRun);

            Task[] tasks = new Task[threadsNumber];
            executor.InitializeKeys(threadsNumber, operationsNumber);


            logger.Info($"Starting with {threadsNumber} threads and {operationsNumber} operations.");

            var watcher = new Stopwatch();
            watcher.Start();
            do
            {
                //insert data
                for (int index = 0; index < threadsNumber; index += 1)
                {
                    try
                    {
                        tasks[index] =
                                        Task.Factory.StartNew(() =>
                                        executor.ExecuteInserts(executor.InitializeKeys(threadsNumber, operationsNumber), data, operationsNumber));
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                        throw;
                    }
                }
                //get data
                for (int index = 0; index < threadsNumber; index += 1)
                {
                    try
                    {
                        tasks[index] =
                                        Task.Factory.StartNew(() =>
                                        executor.ExecuteReads(executor.InitializeKeys(threadsNumber, operationsNumber), operationsNumber));
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                        throw;
                    }
                }
                //delete data
                for (int index = 0; index < threadsNumber; index += 1)
                {
                    try
                    {
                        tasks[index] =
                                        Task.Factory.StartNew(() =>
                                        executor.ExecuteDeletes(executor.InitializeKeys(threadsNumber, operationsNumber), operationsNumber));
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                        throw;
                    }
                }
            }
            while (watcher.Elapsed < TimeSpan.FromSeconds(secondsToRun));

            

            

            Console.ReadLine();
        }



    }
}
