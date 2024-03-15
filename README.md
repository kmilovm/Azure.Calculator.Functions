# Azure Calculator

This project was created as part of a technical test for Deloitte

## Attention

Do not forget to add the following keys to your own userSecrets file in order to run the application:

- "AzureSignalRConnectionString": "Endpoint=[YourAzureSignalRUrl];AccessKey=[YourAccessKey];Version=1.0;" 
- "AzureSignalRNegociate": "http://localhost:7249/api/Negotiate?Code=[YourAccessKey]"  
- "AzureSignalRSendMessage": "http://localhost:7249/api/SendMessage?Code=[YourAccessKey]"
  
the url for your local implementation can be changed when you clone the code, please pay attention to definitely one and replace it properly
