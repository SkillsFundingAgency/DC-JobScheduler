# DC-JobScheduler
This is a console app used to connect to the JobScheduler database and pick up the jobs by priority. Job details are then pushed into the service bus queue for processing. This component will then mark the status of job as "MovedToProcessing".
# Usage
Setup the appsettings file with relevant settings and run the app.

    {
      "QueueSettings": {
        "ConnectionString": "",// connection string for the queue to send messages to
        "QueueName": ""  // queue name
      },
      "JobQueueManagerSettings": {
        "ConnectionString": "" // Connection string for the database which contains Jobs to be processed e.g. JobScheduler
      } 
    }

# Dependencies 
* JobScheduler Database with jobs data 
* ESFA.DC.Auditing
* ESFA.DC.Logging
* ESFA.DC.Queueing
* ESFA.DC.Queueing.Interface
* ESFA.DC.Serialization.Interfaces
* ESFA.DC.Serialization.Json
