using FileBin.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace FileBin.Server.Storage; 

public abstract class FileStorage {
	public abstract IActionResult GetFile(FileData fileData);
	public abstract Task StoreFileAsync(FileData fileData, Stream fileStream);
	public abstract void Delete(FileData fileData);
}
