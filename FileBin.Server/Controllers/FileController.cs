using System.Net;
using System.Security.Claims;
using System.Text;
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
	public async Task<IActionResult> Download([FromRoute] Guid id) {
		FileData? fileData = await m_DbContext.Files.FindAsync(id);
		if (fileData != null) {
			return m_Storage.GetFile(fileData);
		} else {
			return NotFound();
		}
	}

	[HttpPost("/")]
	public async Task<IActionResult> Upload([FromQuery] string? filename, [FromQuery] DateTime? expiration = null) {
		var authResult = Authorize();
		if (authResult != null) {
			return authResult;
		}
		
		// TODO: optionally filter mime types such as text/html and text/javascript
		string? mimeType = HttpContext.Request.Headers.ContentType.FirstOrDefault();

		if (mimeType == null) {
			return BadRequest("Content-Type is missing");
		}
		
		var fileData = new FileData() {
			Filename = filename,
			Expiration = expiration,
			MimeType = mimeType,
			CreatedAt = DateTime.UtcNow
		};
		
		m_DbContext.Files.Add(fileData);
		await m_DbContext.SaveChangesAsync();
		await m_Storage.StoreFileAsync(fileData, HttpContext.Request.Body);
		return Ok(fileData.Id.ToString());
	}
	
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete([FromRoute] Guid id) {
		var authResult = Authorize();
		if (authResult != null) {
			return authResult;
		}
		
		FileData? fileData = await m_DbContext.Files.FindAsync(id);

		if (fileData == null) {
			return NotFound();
		}
		
		m_Storage.Delete(fileData);
		m_DbContext.Files.Remove(fileData);
		await m_DbContext.SaveChangesAsync();
		
		return NoContent();
	}

	private IActionResult? Authorize() {
		string? authValue = HttpContext.Request.Headers.Authorization.FirstOrDefault();
		if (authValue == null || !authValue.StartsWith("Basic ")) {
			return Unauthorized();
		}

		byte[] authValueDecoded;
		try {
			authValueDecoded = Convert.FromBase64String(authValue[("Basic ".Length)..]);
		} catch (FormatException) {
			return BadRequest();
		}

		string[] authParsed = Encoding.UTF8.GetString(authValueDecoded).Split(":");
		if (authParsed[0] != m_AuthConfig.Value.Username || authParsed[1] != m_AuthConfig.Value.Password) {
			return StatusCode((int) HttpStatusCode.Forbidden);
		}

		return null;
	}
}
