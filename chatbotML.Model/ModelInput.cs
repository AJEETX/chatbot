// This file was auto-generated by ML.NET Model Builder. 

using Microsoft.ML.Data;

namespace ChatbotML.Model
{
    public class ModelInput
    {
        [ColumnName("Sentiment_Text"), LoadColumn(0)]
        public string Sentiment_Text { get; set; }


        [ColumnName("Sentiment"), LoadColumn(1)]
        public string Sentiment { get; set; }


    }
}
