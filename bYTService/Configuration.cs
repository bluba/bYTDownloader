using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace bYTService
{
    public class Configuration
    {
        public static Configuration Instance { get; set; }

        public int ReadNewCycleMs { get; set; } = 1000 * 60;
        public int UpdateCycleMs { get; set; } = 1000 * 60 * 60 * 12;
        public int MaxDegreeOfParallelism { get; set; } = 4;

        public string LogDir { get; set; } = @"D:\YoutubeService\Logs\";
        public string VideoDL { get; set; } = @"D:\YoutubeService\VideoDL\";
        public string PlaylistDL { get; set; } = @"D:\YoutubeService\PlaylistDL\";

        public string ApiKey { get; set; }

        public List<PlaylistCache> UpdateHistory = new List<PlaylistCache>();

        private const string DefaultConfigurationFilePath = @"D:\YoutubeService\YoutubeService.cfg";

        static Configuration()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (!File.Exists(DefaultConfigurationFilePath))
            {
                Instance = new Configuration();
            }
            else
            {
                var serializer = new XmlSerializer(typeof(Configuration));
                var reader = new StreamReader(DefaultConfigurationFilePath);
                Instance = serializer.Deserialize(reader) as Configuration;
                reader.Close();
            }
        }

        public static void Save()
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            var writer = new StreamWriter(DefaultConfigurationFilePath);
            serializer.Serialize(writer, Instance);
            writer.Close();
        }
    }
}
