using System.Security.Claims;
using FileBin.Server.Config;
using FileBin.Server.Data;
using FileBin.Server.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileBin.Server.Controllers;

[ApiController]
public class FileController : ControllerBase {
	private readonly FileStorage m_Storage;
	private readonly FileDbContext m_DbContext;
	private readonly ILogger<FileController> m_Logger;
	private readonly IOptions<AuthConfig> m_AuthConfig;

	public FileController(FileStorage storage, FileDbContext dbContext, ILogger<FileController> logger, IOptions<AuthConfig> authConfig) {
		m_Storage = storage;
		m_DbContext = dbContext;
		m_Logger = logger;
		m_AuthConfig = authConfig;
	}

	[HttpGet("{id:guid}")]
	public async Task<IActionResult> Download(Guid id) {
		FileData? fileData = await m_DbContext.Files.FindAsync(id);
		if (fileData != null) {
			return m_Storage.GetFile(fileData);
		} else {
			return NotFound();
		}
	}

	[HttpPost("/")]
	[Authorize("Upload")]
	public async Task<IActionResult> Upload([FromQuery] string filename, [FromQuery] bool serveInline, [FromQuery] DateTime? expiration = null) {
		// TODO: optionally filter mime types such as text/html and text/javascript
		string? mimeType = HttpContext.Request.Headers.ContentType.FirstOrDefault();

		if (mimeType == null) {
			return BadRequest("Content-Type is missing");
		}
		
		var fileData = new FileData() {
			Filename = filename,
			Expiration = expiration,
			MimeType = mimeType,
			ServeInline = serveInline,
			CreatedAt = DateTime.UtcNow,
			OwnerId = HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value
		};
		
		string storageId;
		try {
			storageId = await m_Storage.StoreFileAsync(fileData, HttpContext.Request.Body);
		} catch (IOException) {
			return Conflict("A file with that hash already exists.");
		}
		fileData.StorageId = storageId;
		m_DbContext.Files.Add(fileData);
		await m_DbContext.SaveChangesAsync();
		return Ok(fileData.Id.ToString());
	}
	
	[HttpDelete("{id:guid}")]
	[Authorize("Delete")]
	public async Task<IActionResult> Delete([FromRoute] Guid id) {
		FileData? fileData = await m_DbContext.Files.FindAsync(id);

		if (fileData == null) {
			return NotFound();
		}
		
		m_Logger.LogInformation("User identity name: {UserIdentityName}", HttpContext.User.Identity?.Name ?? "null");
		if (!((fileData.OwnerId != null && fileData.OwnerId == HttpContext.User.Identity?.Name) || HttpContext.User.IsInRole(m_AuthConfig.Value.AdminRole))) {
			return Forbid();
		}

		m_Storage.Delete(fileData);
		m_DbContext.Files.Remove(fileData);
		await m_DbContext.SaveChangesAsync();
		
		return NoContent();
	}
}
