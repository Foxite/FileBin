using FileBin.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace FileBin.Server.Storage;

public abstract class FetchFileStorage : FileStorage {
	public override IActionResult GetFile(FileData fileData) {
		return new FileStreamResult(GetStream(fileData), fileData.MimeType) {
			FileDownloadName = fileData.Filename
		};
	}

	protected abstract Stream GetStream(FileData fileData);
}
