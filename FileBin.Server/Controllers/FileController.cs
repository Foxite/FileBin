using System.Net.Mime;
using FileBin.Server.Data;
using FileBin.Server.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace FileBin.Server.Controllers;

[ApiController]
public class FileController : ControllerBase {
	private readonly FileStorage m_Storage;
	private readonly FileDbContext m_DbContext;

	public FileController(FileStorage storage, FileDbContext dbContext) {
		m_Storage = storage;
		m_DbContext = dbContext;
	}

	[HttpGet("{id:guid}")]
	public IActionResult Download(Guid id) {
		FileData? fileData = m_DbContext.Files.Find(id);
		if (fileData != null) {
			return m_Storage.GetFile(fileData);
		} else {
			return NotFound();
		}
	}

	[HttpPost]
	[Authorize("Upload")]
	public async Task<IActionResult> Upload([FromQuery] DateTime? expiration = null) {
		RequestHeaders requestHeaders = HttpContext.Request.GetTypedHeaders();
		
		if (requestHeaders.ContentDisposition == null) {
			return BadRequest("Content disposition is missing");
		}

		if (!requestHeaders.ContentDisposition.FileName.HasValue) {
			return BadRequest("Content disposition is missing filename");
		}
		
		if (!HttpContext.Request.Headers.ContentType.Any()) {
			return BadRequest("Content type is missing");
		}

		var fileData = new FileData() {
			Filename = requestHeaders.ContentDisposition.FileName.Value,
			Expiration = expiration,
			MimeType = HttpContext.Request.Headers.ContentType
		};
		
		string storageId = await m_Storage.StoreFileAsync(fileData, HttpContext.Request.Body);
		fileData.StorageId = storageId;
		m_DbContext.Files.Add(fileData);
		await m_DbContext.SaveChangesAsync();
		return Ok(fileData.Id.ToString());
	}
}
