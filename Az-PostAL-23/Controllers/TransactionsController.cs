using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Az_PostAL_23.Data;
using Az_PostAL_23.Models;
using Az_PostAL_23.Services;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Az_PostAL_23.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IBlobService _blobService;
        private readonly ServiceBusClient _serviceBusClient;

        public TransactionsController(ApplicationDBContext context, IBlobService blobService, ServiceBusClient serviceBusClient)
        {
            _context = context;
            _blobService = blobService;
            _serviceBusClient = serviceBusClient;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var applicationDBContext = await _blobService.GetAllBlobsWithUri();
            return View(applicationDBContext);
        }


        // GET: Transactions/Create
        public IActionResult AddOrEdit(int id = 0)
        {
            PopulateCategories();
            if (id == 0)
                return View(new Transaction());
            else
                return View(_context.Transaction.Find(id));
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,Amount,Note,Date,Uri,CategoryId")] Transaction transaction, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (transaction.TransactionId == 0)
                    {
                        //Get servicebus namespace primary connection string
                        //Connect to servicebus and post data
                        var sender = _serviceBusClient.CreateSender("blobuploads");

                        transaction.Status = "TransactionProccessing";
                        _context.Add(transaction);
                        await _context.SaveChangesAsync();

                        if (file != null || file.Length > 0)
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" +
                                Guid.NewGuid() + Path.GetExtension(file.FileName);

                            transaction.UriName = fileName;

                            string categoryTitle = _context.Categories
                                .Where(c => c.CategoryId == transaction.CategoryId)
                                .FirstOrDefault().Title.ToLower();

                            var result = await _blobService.UploadBlob(fileName, file,
                                categoryTitle, transaction);
                            await _context.SaveChangesAsync();
                        }

                        //Set up servicebus body and send the message as json format
                        var body = JsonSerializer.Serialize(transaction);
                        var message = new ServiceBusMessage(body);
                        await sender.SendMessageAsync(message);
                    }
                    else
                    {
                        _context.Update(transaction);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            PopulateCategories();
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transaction == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Transaction'  is null.");
            }
            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction != null)
            {
                string categoryTitle = _context.Categories
                    .Where(c => c.CategoryId == transaction.CategoryId)
                    .FirstOrDefault().Title.ToLower();

                _context.Transaction.Remove(transaction);
                await _blobService.DeleteBlob(transaction.UriName, categoryTitle);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        public void PopulateCategories()
        {
            var CategoryCollection = _context.Categories.ToList();
            Category DefaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
            CategoryCollection.Insert(0, DefaultCategory);
            ViewBag.Categories = CategoryCollection;
        }

        private bool TransactionExists(int id)
        {
          return _context.Transaction.Any(e => e.TransactionId == id);
        }
    }
}
