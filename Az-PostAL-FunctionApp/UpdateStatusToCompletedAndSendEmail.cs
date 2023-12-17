using System;
using System.Collections.Generic;
using System.Linq;
using Az_PostAL_FunctionApp.Data;
using Az_PostAL_FunctionApp.Model;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace Az_PostAL_FunctionApp
{
    public class UpdateStatusToCompletedAndSendEmail
    {
        private readonly ApplicationDBContext _db;

        public UpdateStatusToCompletedAndSendEmail(ApplicationDBContext db)
        {
            _db = db;
        }

        [FunctionName("UpdateStatusToCompletedAndSendEmail")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
            [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            IEnumerable<Transaction> transactionsFromDb = _db.Transaction
            .Where(u => u.Status == "TransactionComplete" && u.Status != "Completed");

            if (transactionsFromDb.Any())
            {
                var message = new SendGridMessage();
                message.AddTo("hoangloc1511@gmail.com");
                message.AddContent("text/html", $"Processing completed for {transactionsFromDb.Count()} records\n" +
                    $"Transaction details: {transactionsFromDb.FirstOrDefault().Note} - {transactionsFromDb.FirstOrDefault().FormattedAmount}");
                message.SetFrom(new EmailAddress("hoangloc1511@gmail.com"));
                message.SetSubject("A new Transaction has been successfully uploaded for AZ-PostAL service bus");
                await messageCollector.AddAsync(message);
            }

            foreach (var transactionReq in transactionsFromDb)
            {
                // For each request update status
                transactionReq.Status = "Completed";
            }


            _db.UpdateRange(transactionsFromDb);
            await _db.SaveChangesAsync();
        }
    }
}
