using System;
using System.IO;
using System.Xml.Serialization;

namespace MusicRandomizer
{
    [Serializable]
    public class Configuration
    {
        public string currentVersion;
        public SplatoonRegion region;
        public string currentPlaylist;

        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
        public static Configuration currentConfig;

        public static void Load()
        {
            if (!File.Exists("Configuration.xml"))
            {
                RegionForm requestForm = new RegionForm();
                requestForm.ShowDialog();

                currentConfig = new Configuration
                {
                    currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    region = requestForm.chosenRegion,
                    currentPlaylist = "Default"
                };

                Save();
            }
            else
            {
                using (FileStream stream = File.OpenRead("Configuration.xml"))
                {
                    currentConfig = (Configuration)serializer.Deserialize(stream);
                }
            }
        }

        public static void Save()
        {
            File.Delete("Configuration.xml");
            using (FileStream writer = File.OpenWrite("Configuration.xml"))
            {
                serializer.Serialize(writer, currentConfig);
            }
        }
    }
}
