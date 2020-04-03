using edu.stanford.nlp.ie.crf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NERWrapper
{
    public class NERHelper
    {
        private string jarPath;
        private CRFClassifier classifier;

        public NERHelper(string stanfordNERJar = "stanford-ner.jar")
        {
            if (!File.Exists(stanfordNERJar))
                throw new FileNotFoundException($"Could not find the path `{stanfordNERJar}`");

            this.jarPath = stanfordNERJar;
        }

        public string[] Tokenize(string sentences)
        {
            string tempInputFile = Path.GetTempFileName();
            string tempOutputFile = Path.GetTempFileName();

            File.WriteAllText(tempInputFile, sentences);

            Tokenize(tempInputFile, tempOutputFile);

            string[] result = File.ReadAllText(tempOutputFile).Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            File.Delete(tempInputFile);
            File.Delete(tempOutputFile);

            return result;
        }

        public void Tokenize(string inputFile, string outputFile)
        {
            RunJava($"edu.stanford.nlp.process.PTBTokenizer {inputFile} > {outputFile}");
        }

        public void LabelTokens(string tokenFile, string outputFile, Func<string, string> tokenLabelFunction = null)
        {
            string[] tokens = File.ReadAllLines(tokenFile);

            if (tokenLabelFunction == null)
                tokenLabelFunction = (token) => "O";

            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = $"{tokens[i]}\t{tokenLabelFunction.Invoke(tokens[i])}";
            }

            File.WriteAllLines(outputFile, tokens);
        }

        public void TrainModel(NERProperties nerProperties)
        {
            RunJava($"edu.stanford.nlp.ie.crf.CRFClassifier {nerProperties.ToArgString()}");
        }

        public void LoadClassifier(string classifierPath)
        {
            if (!File.Exists(classifierPath))
                throw new FileNotFoundException($"Could not find the path `{classifierPath}`");

            this.classifier = CRFClassifier.getClassifier(classifierPath);
        }

        public List<(string, string)> Recognize(string sentences)
        {
            return classifier.ClassifyWordsWithTypes(sentences);
        }

        private void RunJava(string args)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c java -cp {jarPath} {args}",
                    RedirectStandardError = true,
                    UseShellExecute = false
                },
            };

            process.Start();

            string error = process.StandardError.ReadToEnd();
            if (error.Contains("Exception"))
                throw new Exception($"Exception thrown when running command: {error}");

            process.WaitForExit();
        }
    }

    public static class ExtensionMethods
    {
        public static List<(string, string)> ClassifyWordsWithTypes(this CRFClassifier classifier, string sentences)
        {
            List<(string, string)> results = new List<(string, string)>();

            string xmlResults = classifier.classifyWithInlineXML(sentences);
            foreach (Match match in Regex.Matches(xmlResults, @"<(?<tag>[^<>]*)>(?<word>[^<>]*)<\/[^<>]*>"))
            {
                string tag = match.Groups["tag"].Value;
                string word = match.Groups["word"].Value;

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    results.Add((word, tag));
                }
            }

            return results;
        }
    }
}
