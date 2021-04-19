## Serverless-FoodDeliverySystem

This project is a backend for a simple food delivery system which is built using Azure's serverless tech. For technical details, please refer to this blog: https://rohannevrikar.wordpress.com/2021/04/04/developing-a-food-delivery-system-using-azure-functions/

### Getting started


To deploy this serverless solution to your Azure subscritpion, please follow the following steps:

1. Clone this repository.
2. Open terminal and cd to Installation folder.
3. Run install.ps1 script like this: `.\install.ps1 -ResourceGroupName <resourceGroupName> -TemplateFile arm-template.json`

That's it! Running the powershell script will initiate automated deployment of resouces to your Azure subscription. After the script has ran successfully, you should be able to see a new resource group created, and all the required resourced deployed under that RG. 

To see the whole flow in action, make sure the following things are in place:

1. Restaurant details have been seeded. I used [Mockaroo](mockaroo.com) to generate dummy restaurant data. After downloading mock data as json, it can be uploaded to cosmos db container directly as shown in the image below:

![image](https://user-images.githubusercontent.com/26645175/115204207-dc756b80-a115-11eb-9432-ef3dc8d00b51.png)

2. Go to mock orders function app, and start the app. This will start placing orders to orders API function app. It'd be a good practice to monitor the system for a while from Application Insights.
3. To verify if everything is working properly or not, go to CosmosDB account, click on Data Explorer, and then open Orders container. All the orders should have an order status of 4, which means the order was delivered, which means all the orchestration steps got executed successfully. 

