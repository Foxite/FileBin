using FileBin.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace FileBin.Server.Storage;

public abstract class RedirectFileStorage : FileStorage {
	public override IActionResult GetFile(FileData fileData) {
		return new RedirectResult(GetUrl(fileData), true);
	}

	protected abstract string GetUrl(FileData fileData);
}
