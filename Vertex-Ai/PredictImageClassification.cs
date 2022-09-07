using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Google.Cloud.AIPlatform.V1.Schema.Predict.Instance;
using Google.Cloud.AIPlatform.V1.Schema.Predict.Params;
using Google.Cloud.AIPlatform.V1.Schema.Predict.Prediction;
using Value = Google.Protobuf.WellKnownTypes.Value;

namespace Vertex_Ai
{
    internal class PredictImageClassification
    {
        // PROJECT_ID
        string project = "PROJECT_ID";
        // IMAGE_FILE_PATH
        string fileName = @"IMAGE_FILE_PATH";
        // ENDPOINT_ID
        string endpointId = "ENDPOINT_ID";
        // LOCATION
        string location = "LOCATION"; // e.g. us-central1
                                      
        string credentialJson = @"JSON_FILE_PATH";

        private GoogleCredential GetCredential()
        {
            return GoogleCredential.FromFile(credentialJson)
                .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        }

        internal void PredictImage()
        {
            PredictionServiceClient predictionServiceClient = new PredictionServiceClientBuilder()
            {
                Endpoint = $"https://{location}-aiplatform.googleapis.com",
                GoogleCredential = GetCredential()
            }.Build();
            EndpointName endpoint = EndpointName.FromProjectLocationEndpoint(project, location, endpointId);

            string content = Convert.ToBase64String(File.ReadAllBytes(fileName));

            ImageClassificationPredictionInstance predictionInstance = new ImageClassificationPredictionInstance();
            predictionInstance.Content = content;

            List<Value> instances = new List<Value>();
            instances.Add(ValueConverter.ToValue(predictionInstance));

            ImageClassificationPredictionParams predictionParams = new ImageClassificationPredictionParams();
            predictionParams.ConfidenceThreshold = 0.5f;
            predictionParams.MaxPredictions = 5;

            PredictResponse predictResponse = predictionServiceClient.Predict(endpoint, instances, ValueConverter.ToValue(predictionParams));

            Console.WriteLine("Predict Image Classification Response");
            Console.WriteLine("Deployed Model Id: " + predictResponse.DeployedModelId);
            Console.WriteLine("Predictions");

            foreach (Value prediction in predictResponse.Predictions)
            {
                ClassificationPredictionResult result = ValueConverter.ToMessage<ClassificationPredictionResult>(prediction);

                int counter = 0;
                foreach (long id in result.Ids)
                {
                    Console.WriteLine("Label ID: " + id);
                    Console.WriteLine("Label: " + result.DisplayNames[counter]);
                    Console.WriteLine("Confidence: " + result.Confidences[counter]);
                    counter++;
                }
            }

        }
    }
}
