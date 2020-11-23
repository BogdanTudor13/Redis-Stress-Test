using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTest.App.Data
{
    public class RedisCacheProvider
    {
        public static string RedisConnString { get; set; }
        public static string NoValueFound = "No value found for specific key";
        public static string Success = "Success!";
        public static string ErrorSet = "Error! The object could not be set";
        public static string ErrorDelete = "Error! The object could not be deleted";
        private readonly int _expireSeconds;
        public AppLogger logger;
        public RedisCacheProvider(AppLogger logger)
        {
            this.logger = logger;
            _expireSeconds = 600;
        }
        public sealed class Database
        {
            private static readonly Lazy<IDatabase>
                lazy =
                new Lazy<IDatabase>
                    (() =>
                    {
                        ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect(RedisConnString);
                        return muxer.GetDatabase();
                    });

            public static IDatabase Instance { get { return lazy.Value; } }

            private Database()
            {
            }
        }

        public void GetItem(string key)
        {
            try
            {
                var redisValue = Database.Instance.StringGet(key);
                var message = redisValue.IsNullOrEmpty ? NoValueFound : Success;
                logger.Info($"Geting item {key} : {Success}");
            }
            catch (Exception e)
            {
                logger.Error(e,e.Message);
            }
        }

        public void SetItem(string key, string item)
        {
            try
            {
                var wasSet = Database.Instance.StringSet(key, item, expiry: TimeSpan.FromSeconds(_expireSeconds));
                var message = wasSet ? Success : ErrorSet;
                logger.Info($"Setting item {key} : {message}");
            }
            catch (Exception e )
            {
                logger.Error(e,e.Message);
            }
        }

        public void DeleteItem(string key)
        {
            try
            {
                var wasDeleted = Database.Instance.KeyDelete(key);
                var message = wasDeleted ? Success : ErrorDelete;
                logger.Info($"Deleting item {key} : {message}");
            }
            catch (Exception e)
            {
                logger.Error(e,e.Message);
            }            
        }


    }
}
