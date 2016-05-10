namespace Collector.PageViews
{
    public class Neurons : PageView
    {
        public Neurons(Core CollectorCore, Scaffold ParentScaffold) : base(CollectorCore, ParentScaffold){}

        public override string Render()
        {
            Scaffold scaffold = null;

            //setup dashboard menu
            string menu = "<div class=\"left\"><ul><li><a href=\"/dashboard/neurons/sentence\" class=\"button blue\">Sentence Validation</a></li></ul></div>";
            parentScaffold.Data["menu"] = menu;

            //determine which section to load for neural network tests
            if (S.Page.Url.paths.Length > 2)
            {
                switch (S.Page.Url.paths[2].ToLower())
                {
                    case "sentence":
                        if(scaffold == null) { scaffold = LoadSentence(); }
                        break;
                }
            }
            else
            {
                //load default PageView
                if (scaffold == null) { scaffold = LoadSentence(); }
            }
            return scaffold.Render();
        }

        private Scaffold LoadSentence()
        {
            //get sentence validation neural network test from web service
            var scaffold = new Scaffold(S, "/app/pageviews/dashboard/neurons/sentence.html", "", new string[] { "training-data" });
            var neurons = new Services.Neurons(S, S.Page.Url.paths);

            //load current training data information
            scaffold.Data["training-data"] = neurons.GetTrainingDataUI("sentences");

            S.Page.RegisterJSFromFile("/app/pageviews/dashboard/neurons/sentence.js");
            return scaffold;
        }

    }
}
