using System.Net.Mime;

namespace FileBin.Server.Data;

public class FileData {
	public Guid Id { get; set; }
	public string Filename { get; set; }
	
	// TODO implement expiration. maybe a cronjob
	public DateTime? Expiration { get; set; }
	public string? OwnerId { get; set; }
	public string StorageId { get; set; }
	public string MimeType { get; set; }
	public bool ServeInline { get; set; }
	public DateTime CreatedAt { get; set; }
}
