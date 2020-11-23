using StressTest.App.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTest.App
{
    public class RedisExecutor
    {
        public RedisCacheProvider CacheProvider { get; set; }

        private Stack<string> keys;
        private AppLogger logger;
        public RedisExecutor(AppLogger logger)
        {
            this.logger = logger;
            RedisCacheProvider.RedisConnString = ConfigurationManager.AppSettings["connectionString"];
            CacheProvider = new RedisCacheProvider(this.logger);
        }


        public ConcurrentStack<string> InitializeKeys(int threads, int operations)
        {
            var conStack = new ConcurrentStack<string>();

            for (int i = 0; i < threads * operations; i++)
            {
                conStack.Push($"Key_{i}");
            }

            return conStack;
        }

        public void ExecuteInserts(ConcurrentStack<string> keys, string value, int numberOfTimes)
        {

            try
            {
                for (int index = 0; index < numberOfTimes; index += 1)
                {
                    keys.TryPop(out string key);
                    CacheProvider.SetItem(key, value);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        public void ExecuteReads(ConcurrentStack<string> keys, int numberOfTimes)
        {
            try
            {
                for (int index = 0; index < numberOfTimes; index += 1)
                {
                    keys.TryPop(out string key);
                    CacheProvider.GetItem(key);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        public void ExecuteDeletes(ConcurrentStack<string> keys, int numberOfTimes)
        {
            try
            {
                for (int index = 0; index < numberOfTimes; index += 1)
                {
                    keys.TryPop(out string key);
                    CacheProvider.DeleteItem(key);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

    }
}
