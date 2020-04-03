using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NERWrapper
{
    public class NERProperties
    {
        /// <summary>
        /// Location of the training file
        /// </summary>
        [ArgName("trainFile")]
        public string TrainFile { get; set; }
        /// <summary>
        /// Location where you would like to save (serialize) your
        /// classifier; adding .gz at the end automatically gzips the file,
        /// making it smaller, and faster to load
        /// </summary>
        [ArgName("serializeTo")]
        public string SerializeTo { get; set; }
        /// <summary>
        /// Structure of your training file; eg "word=0,answer=1" tells the classifier that
        /// the word is in column 0 and the correct answer is in column 1
        /// </summary>
        [ArgName("map")]
        public string Map { get; set; } = "word=0,answer=1";
        /// <summary>
        /// This specifies the order of the CRF: order 1 means that features
        /// apply at most to a class pair of previous class and current class
        /// or current class and next class
        /// </summary>
        [ArgName("maxLeft")]
        public int MaxLeft { get; set; } = 1;

        //These are the features we'd like to train with. Some are discussed below,
        //the rest can be understood by looking at NERFeatureFactory
        [ArgName("useClassFeature")]
        public bool UseClassFeature { get; set; } = true;
        [ArgName("useWord")]
        public bool UseWord { get; set; } = true;

        //Word character ngrams will be included up to length 6 as prefixes and suffixes only
        [ArgName("useNGrams")]
        public bool UseNGrams { get; set; } = true;
        [ArgName("noMidNGrams")]
        public bool NoMidNGrams { get; set; } = true;
        [ArgName("maxNGramLeng")]
        public int MaxNGramLength { get; set; } = 6;
        [ArgName("usePrev")]
        public bool UsePrev { get; set; } = true;
        [ArgName("useNext")]
        public bool UseNext { get; set; } = true;
        [ArgName("useDisjunctive")]
        public bool UseDisjunctive { get; set; } = true;
        [ArgName("useSequences")]
        public bool UseSequences { get; set; } = true;
        [ArgName("usePrevSequences")]
        public bool UsePrevSequences { get; set; } = true;

        //The last 4 properties deal with word shape features
        [ArgName("useTypeSeqs")]
        public bool UseTypeSeqs { get; set; } = true;
        [ArgName("useTypeSeqs2")]
        public bool UseTypeSeqs2 { get; set; } = true;
        [ArgName("useTypeySequences")]
        public bool UseTypeySequences { get; set; } = true;
        [ArgName("wordShape")]
        public string WordShape { get; set; } = "chris2useLC";

        public string ToArgString()
        {
            string argString = "";

            foreach (KeyValuePair<string, PropertyInfo> prop in GetPropertiesAndAttributes())
            {
                argString += $"-{prop.Key} {prop.Value.GetValue(this)} ";
            }

            return argString.Trim();
        }

        public void ToFile(string filePath)
        {
            List<string> lines = new List<string>();

            foreach (KeyValuePair<string, PropertyInfo> prop in GetPropertiesAndAttributes())
            {
                lines.Add($"{prop.Key}={prop.Value.GetValue(this)}");
            }

            File.WriteAllLines(filePath, lines);
        }

        public static NERProperties FromFile(string filePath)
        {
            NERProperties result = new NERProperties();
            Dictionary<string, PropertyInfo> properties = GetPropertiesAndAttributes();

            foreach (string line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#")) //Skip empty lines & comments
                    continue;

                string[] lineParts = line.Split(new[] { '=' }, 2);
                string key = lineParts[0].Trim();
                string val = lineParts[1].Trim();

                PropertyInfo property = properties[key];
                property.SetValue(result, Convert.ChangeType(val, property.PropertyType));
            }

            return result;
        }

        private static Dictionary<string, PropertyInfo> GetPropertiesAndAttributes()
        {
            Dictionary<string, PropertyInfo> results = new Dictionary<string, PropertyInfo>();

            foreach (PropertyInfo info in typeof(NERProperties).GetProperties())
            {
                string argName = ((ArgNameAttribute)info.GetCustomAttribute(typeof(ArgNameAttribute))).Name;

                results.Add(argName, info);
            }

            return results;
        } 
    }

    public class ArgNameAttribute : Attribute
    {
        public string Name { get; set; }
        public ArgNameAttribute(string name)
        {
            Name = name;
        }
    }
}
