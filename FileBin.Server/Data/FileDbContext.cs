using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace FileBin.Server.Data; 

public class FileDbContext : DbContext {
	public DbSet<FileData> Files { get; set; }
}

public class FileData {
	public Guid Id { get; set; }
	public string Filename { get; set; }
	public DateTime? Expiration { get; set; }
	public string StorageId { get; set; }
	public string MimeType { get; set; }
	public ContentDisposition Disposition { get; set; }
}
