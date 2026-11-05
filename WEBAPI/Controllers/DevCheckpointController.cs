using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Data;
using WEBAPI.Data.Seeders;
using WEBAPI.Entities;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/dev/checkpoint")]
public class DevCheckpointController(
    RmsDbContext db,
    IPasswordHasher<Account> passwordHasher,
    IWebHostEnvironment env) : ControllerBase
{
    /// <summary>
    /// Checkpoint Seeder: Membersihkan seluruh data testing hasil development (sesi baru, penilaian, berkas tugas, dsb)
    /// dan mengembalikan database ke kondisi awal checkpoint seeder secara instan. Hanya aktif di Development.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetToCheckpoint()
    {
        if (!env.IsDevelopment())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Endpoint checkpoint seeder hanya tersedia pada environment Development." });
        }

        await DataSeeder.ResetToCheckpointAsync(db, passwordHasher);

        return Ok(new
        {
            success = true,
            message = "Database berhasil di-reset kembali ke status checkpoint awal seeder.",
            timestamp = DateTime.UtcNow
        });
    }
}

