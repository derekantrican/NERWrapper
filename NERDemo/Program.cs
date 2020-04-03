using NERWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NERDemo
{
    class Program
    {
        static void Main()
        {
            //Guide from stanford.edu: https://nlp.stanford.edu/software/crf-faq.shtml#a
            //Example article on training on recipe ingredients: https://thoughtbot.com/blog/named-entity-recognition

            NERHelper helper = new NERHelper(@"..\..\NERWrapper\bin\stanford-ner.jar"); //Either include stanford-ner.jar at the same level as NERWrapper.dll or specify the path in the constructor
            helper.Tokenize(@"ExampleFiles\jane-austen-emma-ch1.txt", @"ExampleFiles\jane-austen-emma-ch1.tok");
            helper.LabelTokens(@"ExampleFiles\jane-austen-emma-ch1.tok", @"ExampleFiles\jane-austen-emma-ch1.tsv"); //Set all tokens to "O" (background)

            //At this point, you would manually go through the .tsv file and label your tokens (change "O" labels to your labels).
            //For this demo, we instead use an already-labeled version (jane-austen-emma-ch1-labeled.tsv)

            helper.TrainModel(new NERProperties { TrainFile = @"ExampleFiles\jane-austen-emma-ch1-labeled.tsv", SerializeTo = @"ExampleFiles\ner-model.ser.gz" });

            helper.LoadClassifier(@"ExampleFiles\ner-model.ser.gz");

            //Test the model (NOTE: this ML is quite ineffective with the small amount of training data)
            string sentence = " ";
            while (sentence != "")
            {
                Console.WriteLine("\nEnter a sentence (like \"How are you Taylor\"): ");
                sentence = Console.ReadLine();

                Console.WriteLine("RESULTS:");
                Console.WriteLine(string.Join("\n", helper.Recognize(sentence).Select(p => $"\t'{p.Item1}' ({p.Item2})")));
            }
        }
    }
}
