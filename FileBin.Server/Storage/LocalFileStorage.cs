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
	
	public async override Task StoreFileAsync(FileData fileData, Stream download) {
		string tempPath = Path.GetTempFileName();
		try {
			await using (FileStream file = File.OpenWrite(GetPath(fileData.Id.ToString()))) {
				await download.CopyToAsync(file);
			}
		} finally {
			File.Delete(tempPath);
		}
	}

	public override void Delete(FileData fileData) {
		File.Delete(GetPath(fileData.Id.ToString()));
	}

	protected override Stream GetStream(FileData fileData) {
		return File.OpenRead(GetPath(fileData.Id.ToString()));
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
