using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LaundryManagement.Models.Entities;
using LaundryManagement.Models.ViewModels;
using LaundryManagement.Data;
using System.Security.Claims;

namespace LaundryManagement.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Owner,Admin,Kasir")]
    [Route("transaksi")]
    public class MasterTransactionController(AppDbContext _context) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Customer)
                .OrderByDescending(t => t.OrderDate)
                .ThenByDescending(t => t.Id)
                .ToListAsync();
            return View(transactions);
        }

        [HttpGet("pos")]
        public async Task<IActionResult> Create()
        {
            await PrepareViewBagsAsync();
            return View(new TransactionViewModel());
        }

        [HttpPost("pos")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionViewModel model)
        {
            if (model.Details == null || model.Details.Count == 0)
            {
                ModelState.AddModelError("", "Keranjang belanja kosong! Silakan tambah layanan minimal 1.");
            }

            if (model.IsNewCustomer)
            {
                if (string.IsNullOrWhiteSpace(model.NewCustomerName) || string.IsNullOrWhiteSpace(model.NewCustomerPhone))
                {
                    ModelState.AddModelError("", "Nama dan Nomor Telepon pelanggan baru wajib diisi!");
                }
            }
            else
            {
                if (model.CustomerId == null || model.CustomerId <= 0)
                {
                    ModelState.AddModelError("CustomerId", "Silakan pilih pelanggan yang ada, atau centang Pelanggan Baru.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PrepareViewBagsAsync();
                return View(model);
            }

            // Memulai transaksi DB agar aman (rollback jika ada error sebagian)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var fullName = User.FindFirst("FullName")?.Value ?? "System";
                int targetCustomerId = model.CustomerId ?? 0;

                // 1. Buat Customer Baru jika dicentang
                if (model.IsNewCustomer)
                {
                    var newCustomer = new Customer
                    {
                        Name = model.NewCustomerName!,
                        PhoneNumber = model.NewCustomerPhone!,
                        Address = model.NewCustomerAddress,
                        IsActive = true,
                        CreatedBy = fullName,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();
                    targetCustomerId = newCustomer.Id; // Ambil ID yang baru digenerate
                }

                // 2. Kalkulasi Ulang Harga dari Database (Hindari spoofing dari front-end)
                decimal calculatedTotalPrice = 0;
                decimal calculatedTotalDiscount = 0;
                var transactionDetails = new List<TransactionDetail>();

                var pricelistIds = model.Details.Select(d => d.PricelistJasaId).ToList();
                var validPricelists = await _context.PricelistJasas
                    .Where(p => pricelistIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                foreach (var detailVM in model.Details)
                {
                    if (validPricelists.TryGetValue(detailVM.PricelistJasaId, out var pricelistItem))
                    {
                        decimal itemPriceDb = pricelistItem.Harga;
                        // Subtotal = (Qty * Harga Database) - Discount
                        decimal subTotal = (detailVM.Qty * itemPriceDb) - detailVM.Discount;

                        calculatedTotalPrice += (detailVM.Qty * itemPriceDb);
                        calculatedTotalDiscount += detailVM.Discount;

                        transactionDetails.Add(new TransactionDetail
                        {
                            PricelistJasaId = pricelistItem.Id,
                            Qty = detailVM.Qty,
                            PriceAtTime = itemPriceDb,
                            Discount = detailVM.Discount,
                            SubTotal = subTotal
                        });
                    }
                }

                // Jika ternyata tidak ada barang valid di DB
                if (transactionDetails.Count == 0)
                {
                    ModelState.AddModelError("", "Produk/Jasa yang dikirimkan tidak valid atau tidak ditemukan di database.");
                    await transaction.RollbackAsync();
                    await PrepareViewBagsAsync();
                    return View(model);
                }

                // 3. Buat Entitas Traksaksi Utama dengan Pembangkitan Invoice yang aman dari Race Condition
                // Menggunakan Raw SQL dengan UPDLOCK / ROWLOCK agar baca-tulis id transaksi terakhir dilock per eksekusi
                string invoiceNo = await GenerateInvoiceNoSafeAsync(_context);

                var newTransaction = new Transaction
                {
                    InvoiceNo = invoiceNo,
                    CustomerId = targetCustomerId,
                    OrderDate = model.OrderDate,
                    EstimatedFinishDate = model.EstimatedFinishDate,
                    TotalPrice = calculatedTotalPrice,
                    TotalDiscount = calculatedTotalDiscount,
                    GrandTotal = calculatedTotalPrice - calculatedTotalDiscount,
                    Status = "Pending",
                    PaymentStatus = model.PaymentStatus,
                    PaymentMethod = model.PaymentMethod,
                    Notes = model.Notes,
                    CreatedBy = fullName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TransactionDetails = transactionDetails
                };

                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync();

                // Commit Transaction Database
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Transaksi berhasil disimpan dengan Invoice: {invoiceNo}";
                TempData["PrintInvoiceId"] = newTransaction.Id;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Terjadi kesalahan sistem saat menyimpan transaksi: " + ex.Message);
                await PrepareViewBagsAsync();
                return View(model);
            }
        }

        [HttpGet("print/{id}")]
        public async Task<IActionResult> Print(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.TransactionDetails)
                    .ThenInclude(d => d.PricelistJasa)
                        .ThenInclude(p => p.Jasa)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.TransactionDetails)
                    .ThenInclude(d => d.PricelistJasa)
                        .ThenInclude(p => p.Jasa)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.TransactionDetails)
                    .ThenInclude(d => d.PricelistJasa)
                        .ThenInclude(p => p.Jasa)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null) return NotFound();

            await PrepareViewBagsAsync();

            var vm = new TransactionEditViewModel
            {
                Id = transaction.Id,
                InvoiceNo = transaction.InvoiceNo,
                CurrentStatus = transaction.Status,
                Status = transaction.Status,
                CustomerId = transaction.CustomerId,
                CustomerName = transaction.Customer?.Name ?? "",
                CustomerPhone = transaction.Customer?.PhoneNumber ?? "",
                OrderDate = transaction.OrderDate,
                EstimatedFinishDate = transaction.EstimatedFinishDate,
                PaymentStatus = transaction.PaymentStatus,
                PaymentMethod = transaction.PaymentMethod,
                Notes = transaction.Notes,
                Details = transaction.TransactionDetails.Select(d => new TransactionDetailViewModel
                {
                    PricelistJasaId = d.PricelistJasaId,
                    JasaName = d.PricelistJasa?.Jasa?.NamaJasa,
                    Qty = d.Qty,
                    PriceAtTime = d.PriceAtTime,
                    Discount = d.Discount
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransactionEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            var transaction = await _context.Transactions
                .Include(t => t.TransactionDetails)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null) return NotFound();

            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Owner");

            // Validasi Matrix Rules
            bool canEditItems = transaction.Status == "Pending" || (transaction.Status == "Process" && isAdmin);
            bool canEditStatus = transaction.Status != "Taken";

            if (transaction.Status == "Finished" && model.Status != "Taken" && model.Status != "Finished")
            {
                ModelState.AddModelError("Status", "Transaksi dengan status 'Finished' hanya bisa diubah menjadi 'Taken'.");
            }

            if (canEditItems && (model.Details == null || model.Details.Count == 0))
            {
                ModelState.AddModelError("", "Keranjang belanja kosong! Silakan tambah layanan minimal 1.");
            }

            if (!ModelState.IsValid)
            {
                await PrepareViewBagsAsync();
                model.CurrentStatus = transaction.Status; // Reset for view UI logic
                return View(model);
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Always editable
                transaction.Notes = model.Notes;

                // Partially editable
                if (canEditStatus)
                {
                    transaction.Status = model.Status;
                }

                if (transaction.Status != "Taken")
                {
                    transaction.PaymentStatus = model.PaymentStatus;
                    transaction.PaymentMethod = model.PaymentMethod;
                    transaction.OrderDate = model.OrderDate;
                    transaction.EstimatedFinishDate = model.EstimatedFinishDate;
                }

                if (canEditItems)
                {
                    _context.TransactionDetails.RemoveRange(transaction.TransactionDetails);

                    decimal calculatedTotalPrice = 0;
                    decimal calculatedTotalDiscount = 0;
                    var transactionDetails = new List<TransactionDetail>();

                    var pricelistIds = (model.Details ?? new List<TransactionDetailViewModel>()).Select(d => d.PricelistJasaId).ToList();
                    var validPricelists = await _context.PricelistJasas
                        .Where(p => pricelistIds.Contains(p.Id))
                        .ToDictionaryAsync(p => p.Id);

                    foreach (var detailVM in model.Details)
                    {
                        if (validPricelists.TryGetValue(detailVM.PricelistJasaId, out var pricelistItem))
                        {
                            decimal itemPriceDb = pricelistItem.Harga;
                            decimal subTotal = (detailVM.Qty * itemPriceDb) - detailVM.Discount;

                            calculatedTotalPrice += (detailVM.Qty * itemPriceDb);
                            calculatedTotalDiscount += detailVM.Discount;

                            transactionDetails.Add(new TransactionDetail
                            {
                                TransactionId = transaction.Id,
                                PricelistJasaId = pricelistItem.Id,
                                Qty = detailVM.Qty,
                                PriceAtTime = itemPriceDb,
                                Discount = detailVM.Discount,
                                SubTotal = subTotal
                            });
                        }
                    }

                    if (transactionDetails.Count == 0)
                    {
                        ModelState.AddModelError("", "Produk/Jasa tidak valid atau tidak ditemukan di database.");
                        await dbTransaction.RollbackAsync();
                        await PrepareViewBagsAsync();
                        return View(model);
                    }

                    transaction.TotalPrice = calculatedTotalPrice;
                    transaction.TotalDiscount = calculatedTotalDiscount;
                    transaction.GrandTotal = calculatedTotalPrice - calculatedTotalDiscount;

                    await _context.TransactionDetails.AddRangeAsync(transactionDetails);
                }

                transaction.UpdatedAt = DateTime.Now;
                transaction.UpdatedBy = User.FindFirst("FullName")?.Value ?? "System";

                _context.Transactions.Update(transaction);
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                TempData["SuccessMessage"] = $"Transaksi {transaction.InvoiceNo} berhasil diperbarui.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                ModelState.AddModelError("", "Terjadi kesalahan sistem: " + ex.Message);
                await PrepareViewBagsAsync();
                return View(model);
            }
        }

        private async Task PrepareViewBagsAsync()
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, DisplayText = $"{c.Name} ({c.PhoneNumber})" })
                .ToListAsync();

            ViewBag.Customers = new SelectList(customers, "Id", "DisplayText");

            var pricelists = await _context.PricelistJasas
                .Include(p => p.Jasa)
                .Select(p => new
                {
                    p.Id,
                    DisplayText = $"{(p.Jasa != null ? p.Jasa.NamaJasa : "Unknown")} - {GetTipeLayananName(p.TipeLayanan)} (Rp{p.Harga})",
                    Price = p.Harga,
                    Satuan = p.Jasa != null ? p.Jasa.Satuan : ""
                })
                .ToListAsync();

            ViewBag.Pricelists = pricelists; // Mengirim sebagai list dinamis agar bisa dibaca Front-end JS
        }

        private static string GetTipeLayananName(int typeId)
        {
            return typeId switch
            {
                1 => "Reguler",
                2 => "Express 2 Hari",
                3 => "Express 1 Hari",
                4 => "Express 6 Jam",
                _ => "Lainnya"
            };
        }

        private static async Task<string> GenerateInvoiceNoSafeAsync(AppDbContext context)
        {
            // Format: INV-DDMMYY-XXXX
            string prefixDate = DateTime.Now.ToString("ddMMyy");
            string prefix = $"INV-{prefixDate}-";

            // Gunakan FromSqlRaw dengan UPDLOCK dan HOLDLOCK untuk mencegah Race Condition (T-SQL Specific)
            // Ini akan mengunci baris pembacaan terakhir hingga transaction di-commit/rollbacked.
            var lastTransaction = await context.Transactions
                .FromSqlRaw("SELECT TOP 1 * FROM Transactions WITH (UPDLOCK, HOLDLOCK) WHERE InvoiceNo LIKE {0} ORDER BY InvoiceNo DESC", prefix + "%")
                .FirstOrDefaultAsync();

            if (lastTransaction == null)
            {
                return prefix + "0001";
            }
            else
            {
                string numPartStr = lastTransaction.InvoiceNo.Substring(prefix.Length);
                if (int.TryParse(numPartStr, out int lastNum))
                {
                    return prefix + (lastNum + 1).ToString("D4");
                }
                return prefix + "0001";
            }
        }
    }
}
