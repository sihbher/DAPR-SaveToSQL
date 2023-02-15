# Dapr Save to SQL

Imagine a sleek and efficient system that effortlessly handles your incoming data with ease. Utilizing the power of Dapr and RabbitMQ, our intelligent component listens to messages as they flow through the queue, seamlessly capturing and transforming them for optimized processing. With precision and accuracy, the component skillfully stores this vital information in SQL Server, ensuring that your data is secure and always within reach. Experience the ultimate in data management with our cutting-edge component, built to enhance your business operations and streamline your workflow.


## SQL
This example consider working with a database called Dispatcher and a table called Orders, use the next script to create these elements:

```sql
--Cretae the database
create DATABASE Dispatcher;

--Create table Orders
use Dispatcher;
CREATE TABLE Orders (
  IDPlant INT,
  IDOrder INT,
  Amount FLOAT,
  Price FLOAT,
  Dispatched BIT,
  CreatedTimeStamp DATETIME,
  LastUpdatedTimeStamp DATETIME,
  PRIMARY KEY (IDPlant, IDOrder)
);
``` 


## Source folder
This folder contains the source code for the component that will receive the data from RabbitMQ (Dapr subscription) and will save it to SQL:

```cs
[HttpPost("/Process")]
    public async Task<IActionResult> Run([FromBody] object body)
    {
        _logger.LogInformation("Received request");
        _logger.LogInformation(body.ToString());

        try
        {
            await InsertOrUpdateSQL(body);
            return Ok();
        }
        catch (Exception ex)
        {
            //in case of error, log it and return 500, this way the message will be retried
            _logger.LogError(ex, $"Error while processing message. Error: {ex.ToString()}");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
    }
```




## Components foder
### a. [app-dapr2sql.yaml](components/app-dapr2sql.yaml). 
This is a YAML file that defines a Kubernetes Deployment for a containerized application named `dapr2sql`. It includes metadata and a specification for the containers that will run in the Pods. The YAML file also includes Dapr-specific annotations that enable the application to interact with the Dapr sidecar, and an environment variable named `SQL_CONNECTION_STRING` used by the application to connect to a SQL database. The value of this environment variable must be set separately. 
### b. [pubsub-component.yaml](components/pubsub-component.yaml).
This is a YAML file that defines a Dapr component for a RabbitMQ topic-based pub/sub system that forwards messages to a SQL database. It includes metadata and a specification of the component's type and version. The YAML file also specifies the RabbitMQ connection string and auto-acknowledgment setting, and the scope for the component.
### c. [subscription-definition.yaml](components/subscription-definition.yaml). 
This is a YAML file that defines a Dapr subscription for a RabbitMQ topic named "sql" that is forwarded to a SQL database. It includes metadata for the subscription, as well as a specification of the pub/sub component to use, the topic to subscribe to, and the route to forward messages to. The YAML file also specifies the scope for the subscription.