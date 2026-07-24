namespace DeskSync.Api.Entities;

public enum ProviderType
{
    Google,
    Microsoft
}

public class UserLogin
{
    public ProviderType Provider { get; set; }
    public string ProviderKey { get; set; } = null!;
    public Guid UserId { get; set; }
}
