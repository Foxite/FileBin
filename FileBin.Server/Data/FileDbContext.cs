using Microsoft.EntityFrameworkCore;

namespace FileBin.Server.Data; 

public class FileDbContext : DbContext {
	public DbSet<FileData> Files { get; set; } = null!;
	
	public FileDbContext() : base() { }
	public FileDbContext(DbContextOptions<FileDbContext> dbcob) : base(dbcob) { }
}