import urllib3
import json
import datetime
import ssl
import certifi
import random 
import time
  

# Azure Iot Hub Name
AzureIoTHub_Name = "IoTHubTikets"

# IoT Device_ID
device_ID = "device_01"

# Http url

iotHub_call = "https://" + AzureIoTHub_Name + ".azure-devices.net/devices/" + device_ID + "/messages/events?api-version=2020-03-13"

SAS_Token = "..."
  
# Headers
Headers = {
    'Content-type' : 'application/json',
    'Authorization' : SAS_Token
}

id_tiket=2000
while True:
    
    datetime_now =  datetime.datetime.now()
    message = {}
    message['Id'] = id_tiket
    id_tiket+=1
    message['Date_ticket'] = str(datetime_now)
    # Dans les deux instruction suivantes, la valeur de 10 indique le nombre de clients possibles. 
    # et la valeur 5 le nombre de magasins. Nous avons utilisé ces deux valeurs afin d'avoir des données suffisantes dans la base de données 
    # Cosmos DB.  
    message['ID_Client'] = random.randint(1, 10) 
    message['ID_Magasin'] = random.randint(1, 5)
    message['CAT_1'] = round(random.uniform(0, 50), 2)
    message['CAT_2'] = round(random.uniform(0, 80), 2)
    message['CAT_3'] = round(random.uniform(0, 20), 2)
    message['CAT_4'] = round(random.uniform(0, 30), 2)
    message['CAT_5'] = round(random.uniform(0, 30), 2)
    message['CAT_6'] = round(random.uniform(0, 40), 2)
    message['Total'] =round(message['CAT_1'] + message['CAT_1'] + message['CAT_3'] +message['CAT_4'] + message['CAT_5']  + message['CAT_6'],2)
    
    json_message = json.dumps(message)

    http = urllib3.PoolManager(ssl_version=ssl.PROTOCOL_TLSv1_2)
    
    print("Sending of :\n ", json_message)
    httpResponse =  http.request('POST', iotHub_call, headers=Headers, body=json_message)

    print ("Response status: " + str(httpResponse.status))
    time.sleep(5)

 