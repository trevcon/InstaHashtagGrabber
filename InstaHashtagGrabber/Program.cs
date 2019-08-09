using System;
using InstaHashtagGrabber.Model;

namespace InstaHashtagGrabber
{
    public class Program
    {
        static void Main(string[] args)
        {
            var x = new Configuration()
            {
                InstagramUserName = "username",
                InstagramPassword = "password",
                InstagramCsrfToken = "instagramCsrfToken",
                ConnectionString = "mssqlConnectionString",
                MediaMinDate = new DateTime(2016, 1, 1)
            };
            var xJson = Newtonsoft.Json.JsonConvert.SerializeObject(x, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(Utils.GetPath("config.sample.json"), xJson);


            //read config
            var configurationJson = System.IO.File.ReadAllText(Utils.GetPath("config.json"));
            var configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(configurationJson);

            //initialize directory structure
            System.IO.Directory.CreateDirectory(Utils.GetPath("data"));
            System.IO.Directory.CreateDirectory(Utils.GetPath("logs"));

            Console.Write("tag?: ");
            var tag = Console.ReadLine();
            Console.Write("use_savepoint (y/n)?: ");
            var useSavepoint = Console.ReadLine().ToLower() == "y";
            if (useSavepoint)
            {
                //load savepoint + get next data of hashtag
                var savepointJson = System.IO.File.ReadAllText(Utils.GetPath($"data\\{tag}_savepoint.json"));
                var savepoint = Newtonsoft.Json.JsonConvert.DeserializeObject<SavePoint>(savepointJson);
                var processor = new HashtagProcessor();
                processor.Initialize(configuration).Wait();
                processor.GetHashtagMedia(tag, savepoint.NextMaxId, savepoint.PageCount, savepoint.Total).Wait();
            }
            else
            {
                //get data of hashtag
                var processor = new HashtagProcessor();
                processor.Initialize(configuration).Wait();
                processor.GetHashtagMedia(tag).Wait();
            }

            Console.WriteLine("finished");
            Console.ReadKey();
        }
    }
}