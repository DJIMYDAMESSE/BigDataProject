using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Microsoft.Azure.Cosmos; 
using System.Threading.Tasks;

// Dans ce programme, vous devez remplacer les varaibles et arguments "..." par leurs vraies valeurs.
 
namespace BigDataTraining.Function
{
    public static class Prediction_Next_Total
    {
       private static Lazy<CosmosClient> lazyClient = new Lazy<CosmosClient>(InitializeCosmosClient);
       private static CosmosClient cosmosClient => lazyClient.Value;
       private static CosmosClient InitializeCosmosClient(){
       
        var uri = "...";
        var authKey = "...";
        return new CosmosClient(uri, authKey);

        }



         
        [FunctionName("Prediction_Next_Total")]
        public static void Run([CosmosDBTrigger(
            databaseName: "...",
            collectionName: "...",
            ConnectionStringSetting = "...",
            LeaseCollectionName = "leases", 
            CreateLeaseCollectionIfNotExists =true)]IReadOnlyList<Document> input,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                 PrintNewDocument(input, log );
            }
        }
      public static void PrintNewDocument(IReadOnlyList<Document> input, ILogger log){
            foreach(Document newDocument in input){
                int id_client = int.Parse(newDocument.GetPropertyValue<string>("ID_Client"));
                log.LogInformation("ID_Client: " + id_client);
                string id_magasin = newDocument.GetPropertyValue<string>("ID_Magasin");
                log.LogInformation("ID_Magasin: " + id_magasin);
                float total = float.Parse(newDocument.GetPropertyValue<string>("Total"), CultureInfo.InvariantCulture.NumberFormat);

                log.LogInformation("Total: " + total );
                MakeRequest(id_client, id_magasin, total  , log);

            }
        }

        public static async void MakeRequest(int id_client, string id_magasin, float total, ILogger log ){
         Container container = cosmosClient.GetContainer("...", "...");
         var myquery= "SELECT top 3  t.ID_Client, t.ID_Magasin, t.Total FROM tikets t WHERE  t.ID_Client =" + id_client + " and t.ID_Magasin = '" +  id_magasin+ "' ORDER BY t._ts DESC";   
       

         QueryDefinition queryDefinition = new QueryDefinition(myquery);
         FeedIterator<Tiket> feedIterator =  container.GetItemQueryIterator<Tiket>(myquery);

         float prediction = 0.0f;
         List<Tiket> lstResult = new List<Tiket>();
        
       
        while (feedIterator.HasMoreResults )
        {
            FeedResponse<Tiket> results = await feedIterator.ReadNextAsync();
            //log.LogInformation("Il y a " + results.Count +" résultats trouvés");
            foreach(Tiket item in results)
            {   
               
                log.LogInformation("ID_Client: " + item.ID_Client);
                log.LogInformation("ID_Magasin: " + item.ID_Magasin);
                log.LogInformation("Total: " + item.Total);
               
                lstResult.Add(item);  
            }
           
            prediction = await GetPrediction(lstResult[0].Total, lstResult[1].Total, lstResult[2].Total, total,log);
        }

         log.LogInformation("Prédiction = " + prediction);
         Save_Into_SQL_DB.save_prediction(id_client, id_magasin, prediction, log);

        }

      public static async   Task<float> GetPrediction( float total1, float total2, float total3, float total4, ILogger log ){
            log.LogInformation("Total1 = " + total1 + ", Total2 = " + total2 + ", Total3 = " + total3 + ", Total4 = " + total4); 
            
            float res =   await Prediction.GetPrediction(total1,  total2,  total3,  total4,  log);
            return res;
        }



    }

public class Tiket{
      public int ID_Client {get; set;}
      public string ID_Magasin {get; set;}

      public float Total {get; set;}
    }


}
