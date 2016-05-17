using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Collector.Services
{
    public class Neurons : Service
    {
        private string[] otherSymbols = new string[] { "~", "`", "!", "@", "#", "$", "%", "^", "&", "*", "-", "_", "+", "=", ":", "\"", "'", "<", ">", "?", "/", "\\" };
        private string[] webUIwords = new string[] {
            "menu", "blog", "posts", "entries", "contact", "customer service", "rss", "gmail.com",
            "next post", "more posts", "prev post", "previous post",
            "next entry", "more entires", "prev entry", "previous entry",
            "more..", "view", "submit", "comments", "comment", "sign up", "login", "edit"
        };
        private Neural.Network _NN_1 = null;

        public Neurons (Core CollectorCore, string[] paths) : base(CollectorCore, paths) { }

        #region "Neural Networks"
        public Neural.Network NN_Sentences
        {
            get
            {
                if (S.Util.IsEmpty(_NN_1))
                {
                    if (S.Server.Cache.ContainsKey("NN_Sentences"))
                    {
                        //found object in memory
                        _NN_1 = (Neural.Network)S.Server.Cache["NN_Sentences"];
                    }
                    else
                    {
                        //load new neural network into memory
                        int[] hiddenLayers = new int[1];
                        hiddenLayers[0] = 30; //number of neurons in 1st hidden layer
                        _NN_1 = new Neural.Network(13, hiddenLayers, 1); //# of input neurons, hidden layers, and output neurons

                        //load existing weight data
                        var weights = GetNeuralNetworkState("sentences");
                        if(weights.Length >= 3)
                        {
                            //weight data exists
                            NN_Sentences.Weights = weights;
                        }
                        else
                        {
                            //weight data does not exist, so we should
                            //run training data through neural network so it can learn proper weights / biases
                            var training = GetTrainingData("sentences"); //load valid sentences into training list
                            training.Concat(GetTrainingData("sentences", false)); //add invalid sentences to training list
                            NN_Sentences.Train(training, 2000);

                            //save neural network to disk
                            SaveNeuralNetworkState("sentences", NN_Sentences);
                        }
                    }
                }
                return _NN_1;
            }
            set
            {
                _NN_1 = value;
            }
        }
        #endregion

        #region "Training"
        private double[] TranslateSentenceToInput(string sentence)
        {
            var nums = new double[13];
            var articles = new Articles(S, S.Page.Url.paths);
            var article = articles.Analyze("", sentence);
            
            //count facts about sentence
            int commonWords = 0;
            int suspicious = 0;
            int webUI = webUIwords.Where(c => sentence.IndexOf(c) >= 0).Count();
            var numlist = S.Util.Str.GetNumbersFromText(sentence);
            int numbers = numlist.Length;
            int periods = article.rawHtml.Split('.').Length - 1;
            int commas = article.rawHtml.Split(',').Length - 1;
            int spaces = article.rawHtml.Split(' ').Length - 1;
            int parentheses = (article.rawHtml.Split('(').Length - 1) +
                            (article.rawHtml.Split(')').Length - 1) +
                            (article.rawHtml.Split('[').Length - 1) +
                            (article.rawHtml.Split(']').Length - 1);
            int sourcecode = (article.rawHtml.Split('{').Length - 1) +
                            (article.rawHtml.Split('}').Length - 1) +
                            (article.rawHtml.Split(';').Length - 1);
            int symbols = 0;

            //get symbols
            for (var x = 0; x < otherSymbols.Length; x++)
            {
                symbols += article.rawHtml.Split(new string[] { otherSymbols[x] }, StringSplitOptions.None).Length - 1;
            }

            //get count of common words
            for (var x = 0; x < article.words.Count; x++)
            {
                if (article.words[x].importance == 2) { commonWords++; }
                if (article.words[x].suspicious == true) { suspicious++; }
            }
            nums[0] = article.rawHtml.Length / 100.0;
            nums[1] = article.totalWords / 10.0;
            nums[2] = article.totalImportantWords / 10.0;
            nums[3] = commonWords / 10.0;
            nums[4] = suspicious / 10.0;
            nums[5] = webUI / 10.0;
            nums[6] = numbers / 10.0;
            nums[7] = spaces / 10.0;
            nums[8] = commas / 10.0;
            nums[9] = spaces / 10.0;
            nums[10] = parentheses / 10.0;
            nums[11] = sourcecode / 10.0;
            nums[12] = symbols / 10.0;
            return nums;
        }

        public Inject TranslateSentenceToInputUI(string sentence)
        {
            var inject = new Inject();
            var js = new List<string>();

            //count facts about sentence
            double[] facts = TranslateSentenceToInput(sentence);
            
            //send normalized values to web form
            for(var x = 1; x <= 11; x++)
            {
                js.Add("$('.txt-" + x + "').val('" + facts[x - 1].ToString() + "');");
            }

            S.Page.RegisterJS("results", string.Join("", js.ToArray()));
            inject.element = "";
            inject.inject = enumInjectTypes.replace;
            inject.js = CompileJs();
            return inject;
        }

        public void AddTrainingSentence(string sentence, bool valid)
        {
            SaveTrainingSentences(sentence, valid);
            SaveTrainingData("sentences", TranslateSentenceToInput(sentence), valid);
        }

        public Inject AddTrainingData(string testName, string sentence, bool valid, double totalChars, double totalWords, double subjectWords, double commonWords, double suspicious, double spaces, double parenthesis, double periods, double commas, double sourceCode, double symbols)
        {
            var inject = new Inject();
            SaveTrainingSentences(sentence, valid);
            SaveTrainingData(testName, new double[] {
                S.Util.FormatNumber(totalChars),
                S.Util.FormatNumber(totalWords),
                S.Util.FormatNumber(subjectWords),
                S.Util.FormatNumber(commonWords),
                S.Util.FormatNumber(suspicious),
                S.Util.FormatNumber(spaces),
                S.Util.FormatNumber(parenthesis),
                S.Util.FormatNumber(periods),
                S.Util.FormatNumber(commas),
                S.Util.FormatNumber(sourceCode),
                S.Util.FormatNumber(symbols)
            }, valid);
            //reset form
            var js = "$('.txt-sentence').val('');";
            for (var x = 1; x <= 11; x++)
            {
                js += "$('.txt-" + x + "').val(0);";
            }
            S.Page.RegisterJS("results", js);
            inject.element = ".training-data";
            inject.html = GetTrainingDataUI(testName);
            inject.inject = enumInjectTypes.replace;
            inject.js = CompileJs();
            return inject;
        }

        public string GetTrainingDataUI(string testName)
        {
            var htm = "";
            var valid = GetTrainingData(testName);
            var invalid = GetTrainingData(testName, false);

            htm = 
                "<div class=\"row bottom\">" +
                    "<div class=\"column label\">Valid:</div>" +
                    "<div class=\"column value\">" + valid.Count + " sentence(s)</div>" +
                "</div>" +
                "<div class=\"row bottom\">" +
                    "<div class=\"column label\">Invalid:</div>" +
                    "<div class=\"column value\">" + invalid.Count + " sentence(s)</div>" +
                "</div>";

            return htm;
        }

        private List<Neural.DataSet> GetTrainingData(string testName, bool valid = true)
        {
            var file = "/content/neurons/" + testName + "_" + (valid ? "" : "in") + "valid.json";
            if (File.Exists(S.Server.MapPath(file)) == false)
            {
                //training data is missing, check if raw data exists so we can translate raw data into training input data
                if (File.Exists(S.Server.MapPath(file.Replace("valid.", "valid_raw."))) == true)
                {
                    //raw training data exists, convert to neural network input parameters
                    var training = new List<Neural.DataSet>();
                    switch (testName)
                    {
                        case "sentences":
                            //generate training input data list by analyzing sentences
                            string[] sentences = GetTrainingSentences(valid);
                            double[] input;
                            Neural.DataSet d;
                            foreach (var sentence in sentences)
                            {
                                input = TranslateSentenceToInput(sentence);
                                d = new Neural.DataSet(input, new double[] { valid ? 1 : 0 });
                                training.Add(d);
                            }
                            S.Server.SaveFile(file, S.Util.Serializer.WriteObjectAsString(training.ToArray(), Newtonsoft.Json.Formatting.None).Replace("},", "},\n"));
                            break;
                    }
                    return training;
                }
            }

            var s = S.Server.OpenFile(file);
            if(s != "")
            {
                return (List<Neural.DataSet>)S.Util.Serializer.ReadObject(S.Server.OpenFile(file), typeof(List<Neural.DataSet>));
            }
            return new List<Neural.DataSet>();
        }

        private void SaveTrainingData(string testName, double[] data, bool valid = true)
        {
            var training = GetTrainingData(testName, valid);
            var d = new Neural.DataSet(data, new double[] { valid ? 1 : 0 });
            training.Add(d);
            S.Server.SaveFile("/content/neurons/" + testName + "_" + (valid ? "" : "in") + "valid.json", S.Util.Serializer.WriteObjectAsString(training.ToArray(), Newtonsoft.Json.Formatting.None).Replace("},","},\n"));
        }

        private string[] GetTrainingSentences(bool valid = true)
        {
            var s = S.Server.OpenFile("/content/neurons/sentences_" + (valid ? "" : "in") + "valid_raw.json");
            if (s != "")
            {
                return (string[])S.Util.Serializer.ReadObject(S.Server.OpenFile("/content/neurons/sentences_" + (valid ? "" : "in") + "valid_raw.json"), typeof(string[]));
            }
            return new string[0] { };
        }

        private void SaveTrainingSentences(string sentence, bool valid = true)
        {
            List<string> training = GetTrainingSentences(valid).ToList();
            training.Add(sentence);
            S.Server.SaveFile("/content/neurons/sentences_" + (valid ? "" : "in") + "valid_raw.json", S.Util.Serializer.WriteObjectAsString(training.ToArray(), Newtonsoft.Json.Formatting.None).Replace("\",", "\",\n"));
        }

        private double[][][][] GetNeuralNetworkState(string testName)
        {
            var networkfile = S.Server.OpenFile("/content/neurons/" + testName + "_weights.json");
            if (File.Exists(networkfile))
            {
                return (double[][][][])S.Util.Serializer.ReadObject(networkfile, typeof(double[][][][]));
            }
            return new double[0][][][];
        }

        private void SaveNeuralNetworkState(string testName, Neural.Network network)
        {
            S.Server.SaveFile("/content/neurons/" + testName + "_weights.json", S.Util.Serializer.WriteObjectAsString(network.Weights, Newtonsoft.Json.Formatting.Indented));
            S.Server.Cache.Remove("nn_" + testName);
            S.Server.Cache.Add("nn_" + testName, network);
        }
        #endregion

        #region "Testing"
        public Inject RunTestForSentence(string sentence)
        {
            var inject = new Inject();
            var inputs = TranslateSentenceToInput(sentence);
            var results = NN_Sentences.Query(inputs);
            S.Page.RegisterJS("results",
                "$('.test-results').html('<div class=\"row top\"><div class=\"column title\">Test Results</div>" +
                "</div><div class=\"results\">" + (results[0] < 1 && results[0] > 0 ? "0" : "") + results[0].ToString("#.##") + "</div></div>');"
                );


            inject.element = ".sentence-results";
            inject.inject = enumInjectTypes.replace;
            inject.js = CompileJs();
            return inject;
        }
        #endregion

    }
}
