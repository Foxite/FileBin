namespace FileBin.Server.Config; 

public class AuthConfig {
	public string AuthEndpoint { get; set; }
	public string TokenEndpoint { get; set; }
	public string UserEndpoint { get; set; }
	public IList<string> Scopes { get; set; }
	public string ClientId { get; set; }
	public string ClientSecret { get; set; }
	public string? UserRole { get; set; }
	public string AdminRole { get; set; }
}
