using System;
using System.Linq;
using System.Threading.Tasks;
using Az_PostAL_FunctionApp.Data;
using Az_PostAL_FunctionApp.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Az_PostAL_FunctionApp
{
    public class TransactionUploadServiceBusTrigger
    {
        private readonly ApplicationDBContext _db;

        public TransactionUploadServiceBusTrigger(ApplicationDBContext db)
        {
            _db = db;
        }

        [FunctionName("TransactionUploadServiceBusTrigger")]
        public async Task Run([ServiceBusTrigger("blobuploads", Connection = "AzureServiceBus")] string myQueueItem, ILogger log)
        {
            // Deserialize the JSON string into a Transaction object
            Transaction queuedTransaction = JsonConvert.DeserializeObject<Transaction>(myQueueItem);

            // Retrieve the transaction from db context based on the Id of the queued object
            Transaction transaction = _db.Transaction.FirstOrDefault(t => t.TransactionId == queuedTransaction.TransactionId);

            if (transaction != null)
            {
                transaction.Status = "TransactionComplete";
                await _db.SaveChangesAsync();

                log.LogInformation($"blobuploads ServiceBus queue trigger function" +
                    $" updated status for transaction #{queuedTransaction.TransactionId}");
            }
            else
            {
                log.LogError($"Transaction with ID {queuedTransaction.TransactionId} not found in the database.");
            }
        }
    }
}
