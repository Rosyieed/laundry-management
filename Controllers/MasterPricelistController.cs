using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LaundryManagement.Models.Entities;
using LaundryManagement.Models.ViewModels;
using LaundryManagement.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LaundryManagement.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Owner,Admin")]
    [Route("master/pricelist-management")]
    public class MasterPricelistController(AppDbContext _context) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var pricelists = await _context.PricelistJasas.Include(p => p.Jasa).ToListAsync();
            return View(pricelists);
        }

        private async Task PrepareViewBagsAsync()
        {
            var jasas = await _context.Jasas.ToListAsync();
            ViewBag.Jasas = new SelectList(jasas, "Id", "NamaJasa");

            ViewBag.TipeLayananOptions = new[]
            {
                new SelectListItem { Value = "1", Text = "Reguler" },
                new SelectListItem { Value = "2", Text = "Express 2 Hari" },
                new SelectListItem { Value = "3", Text = "Express 1 Hari" },
                new SelectListItem { Value = "4", Text = "Express 6 Jam" },
            };
        }

        [HttpGet("tambah")]
        public async Task<IActionResult> Create()
        {
            await PrepareViewBagsAsync();
            return View(new PricelistJasaViewModel());
        }

        [HttpPost("tambah")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PricelistJasaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewBagsAsync();
                return View(model);
            }

            try
            {
                var newPricelist = new PricelistJasa
                {
                    JasaId = model.JasaId,
                    TipeLayanan = model.TipeLayanan,
                    Harga = model.Harga,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.PricelistJasas.Add(newPricelist);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Pricelist jasa berhasil ditambahkan!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan data: " + ex.Message);
            }

            await PrepareViewBagsAsync();
            return View(model);
        }

        [HttpGet("ubah/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var pricelist = await _context.PricelistJasas.FindAsync(id);
            if (pricelist == null)
            {
                TempData["ErrorMessage"] = "Pricelist tidak ditemukan.";
                return RedirectToAction(nameof(Index));
            }

            var model = new PricelistJasaViewModel
            {
                Id = pricelist.Id,
                JasaId = pricelist.JasaId,
                TipeLayanan = pricelist.TipeLayanan,
                Harga = pricelist.Harga
            };

            await PrepareViewBagsAsync();
            return View(model);
        }

        [HttpPost("ubah/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PricelistJasaViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PrepareViewBagsAsync();
                return View(model);
            }

            try
            {
                var pricelist = await _context.PricelistJasas.FindAsync(id);
                if (pricelist == null)
                {
                    TempData["ErrorMessage"] = "Pricelist tidak ditemukan.";
                    return RedirectToAction(nameof(Index));
                }

                pricelist.JasaId = model.JasaId;
                pricelist.TipeLayanan = model.TipeLayanan;
                pricelist.Harga = model.Harga;
                pricelist.UpdatedAt = DateTime.Now;

                _context.Update(pricelist);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Data pricelist berhasil diubah!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PricelistExists(model.Id))
                {
                    TempData["ErrorMessage"] = "Pricelist tidak ditemukan.";
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

            await PrepareViewBagsAsync();
            return View(model);
        }

        [HttpPost("hapus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var pricelist = await _context.PricelistJasas.FindAsync(id);
                if (pricelist == null)
                {
                    TempData["ErrorMessage"] = "Pricelist tidak ditemukan atau sudah dihapus.";
                    return RedirectToAction(nameof(Index));
                }

                _context.PricelistJasas.Remove(pricelist);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Pricelist berhasil dihapus!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Terjadi kesalahan saat menghapus data: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PricelistExists(int id)
        {
            return _context.PricelistJasas.Any(e => e.Id == id);
        }
    }
}
