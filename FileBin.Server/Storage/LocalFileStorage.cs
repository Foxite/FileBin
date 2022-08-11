using FileBin.Server.Data;
using Microsoft.Extensions.Options;
using SpookilySharp;

namespace FileBin.Server.Storage; 

public class LocalFileStorage : FetchFileStorage {
	private readonly IOptions<Options> m_Options;
	
	public async override Task<string> StoreFileAsync(FileData fileData, Stream download) {
		string tempPath = Path.GetTempFileName();
		string hash;
		await using (var hashDownload = new HashedStream(download)) {
			await using (FileStream file = File.OpenWrite(tempPath)) {
				await hashDownload.CopyToAsync(file);
			}

			hash = hashDownload.ReadHash128.ToString();
		}
		
		File.Move(tempPath, GetPath(hash));
		return hash;
	}

	public override void Delete(FileData fileData) {
		File.Delete(GetPath(fileData.StorageId));
	}

	protected override Stream GetStream(FileData fileData) {
		return File.OpenRead(GetPath(fileData.StorageId));
	}

	private string GetPath(string hash) {
		return Path.Combine(m_Options.Value.StoragePath, hash[..2], hash);
	}
	
	public class Options {
		public string StoragePath { get; set; }
	}
}
