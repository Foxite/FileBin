namespace FileBin.Server.Config; 

public class AuthConfig {
	public string Authority { get; set; }
	public string? DiscoveryDocument { get; set; }
	public string? UserRole { get; set; }
	public string AdminRole { get; set; }
	public string[] Audiences { get; set; }
}
