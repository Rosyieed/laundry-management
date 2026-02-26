using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaundryManagement.Models.ViewModels;
using System.Threading.Tasks;
using BCrypt.Net;
using LaundryManagement.Helpers; // Added this using directive

namespace LaundryManagement.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Owner")]
    [Route("master/user-management")]
    public class MasterUserController(AppDbContext _context, IFileUploadHelper _fileHelper) : Controller
    {
        // GET: MasterUserController
        [HttpGet("")]
        public async Task<IActionResult> Index() // Changed to async
        {
            var users = await _context.users.ToListAsync(); // Changed to async
            return View(users);
        }

        [HttpGet("tambah")]
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        [HttpPost("tambah")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            try
            {
                // Cek apakah username sudah ada
                var existingUser = await _context.users
                        .FirstOrDefaultAsync(u => u.username == model.Username);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username sudah digunakan.");
                    return View("Create", model);
                }

                // Upload Foto Profil
                string? imagePath = null;
                if (model.ProfilePicture != null)
                {
                    imagePath = await _fileHelper.UploadFileAsync(model.ProfilePicture, "uploads/users");
                }

                // Hash password
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var newUser = new User
                {
                    name = model.Name,
                    username = model.Username,
                    phone_number = model.PhoneNumber,
                    password_hash = passwordHash,
                    role = model.Role, // Changed model.Role to model.role
                    image_path = imagePath // Added image_path
                };

                _context.users.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User berhasil ditambahkan!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan data: " + ex.Message);
            }

            return View("Create", model);
        }

        [HttpGet("ubah/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User tidak ditemukan.";
                return RedirectToAction(nameof(Index));
            }

            var model = new UserEditViewModel
            {
                Id = user.id,
                Name = user.name,
                Username = user.username,
                PhoneNumber = user.phone_number,
                Role = user.role
            };

            // Mengirim path foto lama ke view via ViewBag (Opsional)
            ViewBag.CurrentImagePath = user.image_path;

            return View(model);
        }

        [HttpPost("ubah/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
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
                var user = await _context.users.FindAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User tidak ditemukan.";
                    return RedirectToAction(nameof(Index));
                }

                // Cek apakah username dipakai user lain
                var existingUser = await _context.users
                    .FirstOrDefaultAsync(u => u.username == model.Username && u.id != id);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username sudah digunakan oleh user lain.");
                    return View(model);
                }

                // Proses Update Foto Profil
                if (model.ProfilePicture != null)
                {
                    // Hapus foto lama jika exist
                    if (!string.IsNullOrEmpty(user.image_path))
                    {
                        _fileHelper.DeleteFile(user.image_path);
                    }

                    user.image_path = await _fileHelper.UploadFileAsync(model.ProfilePicture, "uploads/users");
                }

                // Update data
                user.name = model.Name;
                user.username = model.Username;
                user.phone_number = model.PhoneNumber;
                user.role = model.Role;

                // Hash password jika diisi
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.password_hash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Data user berhasil diubah!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(model.Id))
                {
                    TempData["ErrorMessage"] = "User tidak ditemukan.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (System.Exception ex)
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
                var user = await _context.users.FindAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User tidak ditemukan atau sudah dihapus.";
                    return RedirectToAction(nameof(Index));
                }

                // Hapus foto fisik dari server
                if (!string.IsNullOrEmpty(user.image_path))
                {
                    _fileHelper.DeleteFile(user.image_path);
                }

                _context.users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User berhasil dihapus!";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Terjadi kesalahan saat menghapus data: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.users.Any(e => e.id == id);
        }
    }
}
