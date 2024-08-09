using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BigDataTraining.Function
{
    class Save_Into_SQL_DB
    {

        
        public static void save_prediction(int id_client, string id_magasin, float prediction, ILogger log )
        {
             
            

            //define the insert sql command, here I insert data into the student table in azure db.
            string myquery = @"insert into prediction_total
                   (ID_Client, ID_Magasin, Next_Total)
                   values(@ID_Client, @ID_Magasin, @Next_Total)";

            string conString = "Server=tcp:predictionserver.database.windows.net,1433;Initial Catalog=predictiondb;Persist Security Info=False;User ID=mkhichane;Password='Dada!100';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
  
            using(SqlConnection con = new SqlConnection(conString))           
            using (SqlCommand cmd = new SqlCommand(myquery, con))
            {
                con.Open();
                cmd.Parameters.AddWithValue("@ID_Client", id_client);
                cmd.Parameters.AddWithValue("@ID_Magasin", id_magasin);
                cmd.Parameters.AddWithValue("@Next_Total", prediction);
                
                cmd.ExecuteNonQuery();

                con.Close();
            }

             log.LogInformation("Prédiction Sauvegardée! ");
        }
    }
}
