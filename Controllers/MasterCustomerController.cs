using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaundryManagement.Models.Entities;
using LaundryManagement.Models.ViewModels;
using LaundryManagement.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LaundryManagement.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Owner,Admin,Kasir")]
    [Route("master/customer-management")]
    public class MasterCustomerController(AppDbContext _context) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(customers);
        }

        [HttpGet("tambah")]
        public IActionResult Create()
        {
            return View(new CustomerViewModel());
        }

        [HttpPost("tambah")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var fullName = User.FindFirst("FullName")?.Value ?? "System";

                var newCustomer = new Customer
                {
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    IsActive = model.IsActive,
                    CreatedBy = fullName,
                    UpdatedBy = null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Data pelanggan berhasil ditambahkan!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan data: " + ex.Message);
            }

            return View(model);
        }

        [HttpGet("ubah/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Pelanggan tidak ditemukan.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                IsActive = customer.IsActive
            };

            return View(model);
        }

        [HttpPost("ubah/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Pelanggan tidak ditemukan.";
                    return RedirectToAction(nameof(Index));
                }

                var fullName = User.FindFirst("FullName")?.Value ?? "System";

                customer.Name = model.Name;
                customer.PhoneNumber = model.PhoneNumber;
                customer.Address = model.Address;
                customer.IsActive = model.IsActive;
                customer.UpdatedBy = fullName;
                customer.UpdatedAt = DateTime.Now;

                _context.Update(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Data pelanggan berhasil diubah!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(model.Id))
                {
                    TempData["ErrorMessage"] = "Pelanggan tidak ditemukan.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan data: " + ex.Message);
            }

            return View(model);
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
