namespace FileBin.Server; 

public class AuthConfig {
	public string Authority { get; set; }
	public string? DiscoveryDocument { get; set; }
	public string Role { get; set; }
	public string Scope { get; set; }
	public string[] Audiences { get; set; }
}
