using System.IO.Compression;
using FileBin.Server.Data;
using Microsoft.Extensions.Options;
using SpookilySharp;

namespace FileBin.Server.Storage; 

public class LocalFileStorage : FetchFileStorage {
	private readonly IOptions<Options> m_Options;

	public LocalFileStorage(IOptions<Options> options) {
		m_Options = options;
	}
	
	public async override Task<string> StoreFileAsync(FileData fileData, Stream download) {
		string tempPath = Path.GetTempFileName();
		bool tempFileCreated = false;
		try {
			string hash;
			await using (var hashDownload = new HashedStream(download)) {
				await using (FileStream file = File.OpenWrite(tempPath)) {
					tempFileCreated = true;
					await hashDownload.CopyToAsync(file);
				}

				hash = hashDownload.ReadHash128.ToString();
			}

			File.Move(tempPath, GetPath(hash));
			tempFileCreated = false;
			return hash;
		} finally {
			if (tempFileCreated) {
				File.Delete(tempPath);
			}
		}
	}

	public override void Delete(FileData fileData) {
		File.Delete(GetPath(fileData.StorageId));
	}

	protected override Stream GetStream(FileData fileData) {
		return File.OpenRead(GetPath(fileData.StorageId));
	}

	private string GetPath(string hash) {
		string directory = Path.Combine(m_Options.Value.StoragePath, hash[..2]);

		Directory.CreateDirectory(directory);
		
		return Path.Combine(directory, hash);
	}
	
	public class Options {
		public string StoragePath { get; set; }
	}
}
