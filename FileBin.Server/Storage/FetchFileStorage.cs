using FileBin.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace FileBin.Server.Storage;

public abstract class FetchFileStorage : FileStorage {
	public override IActionResult GetFile(FileData fileData) {
		return new FileStreamResult(GetStream(fileData), fileData.MimeType) {
			FileDownloadName = fileData.ServeInline ? null : fileData.Filename,
			EntityTag = EntityTagHeaderValue.Parse('"' + fileData.Id.ToString() + '"'),
			LastModified = fileData.CreatedAt
		};
	}

	protected abstract Stream GetStream(FileData fileData);
}
