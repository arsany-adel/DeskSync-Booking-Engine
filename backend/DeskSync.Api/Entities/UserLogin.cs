namespace DeskSync.Api.Entities;

public enum ProviderType
{
    Google,
    Microsoft
}

public class UserLogin
{
    public ProviderType Provider { get; private set; }
    public string ProviderKey { get; private set; }
    public Guid UserId { get; private set; }

    public UserLogin(ProviderType provider, string providerKey, Guid userId)
    {
        Provider = provider;
        ProviderKey = providerKey;
        UserId = userId;
    }

#pragma warning disable CS8618
    private UserLogin() { }
#pragma warning restore CS8618
}
