using Microsoft.AspNetCore.Mvc;
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
    [Route("master/jasa-management")]
    public class MasterJasaController(AppDbContext _context) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var jasas = await _context.Jasas.ToListAsync();
            return View(jasas);
        }

        [HttpGet("tambah")]
        public IActionResult Create()
        {
            return View(new JasaViewModel());
        }

        [HttpPost("tambah")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JasaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var newJasa = new Jasa
                {
                    NamaJasa = model.NamaJasa,
                    Satuan = model.Satuan,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Jasas.Add(newJasa);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Jasa berhasil ditambahkan!";
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
            var jasa = await _context.Jasas.FindAsync(id);
            if (jasa == null)
            {
                TempData["ErrorMessage"] = "Jasa tidak ditemukan.";
                return RedirectToAction(nameof(Index));
            }

            var model = new JasaViewModel
            {
                Id = jasa.Id,
                NamaJasa = jasa.NamaJasa,
                Satuan = jasa.Satuan
            };

            return View(model);
        }

        [HttpPost("ubah/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JasaViewModel model)
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
                var jasa = await _context.Jasas.FindAsync(id);
                if (jasa == null)
                {
                    TempData["ErrorMessage"] = "Jasa tidak ditemukan.";
                    return RedirectToAction(nameof(Index));
                }

                jasa.NamaJasa = model.NamaJasa;
                jasa.Satuan = model.Satuan;
                jasa.UpdatedAt = DateTime.Now;

                _context.Update(jasa);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Data jasa berhasil diubah!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JasaExists(model.Id))
                {
                    TempData["ErrorMessage"] = "Jasa tidak ditemukan.";
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

        [HttpPost("hapus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var jasa = await _context.Jasas.FindAsync(id);
                if (jasa == null)
                {
                    TempData["ErrorMessage"] = "Jasa tidak ditemukan atau sudah dihapus.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Jasas.Remove(jasa);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Jasa berhasil dihapus!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Terjadi kesalahan saat menghapus data: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool JasaExists(int id)
        {
            return _context.Jasas.Any(e => e.Id == id);
        }
    }
}
