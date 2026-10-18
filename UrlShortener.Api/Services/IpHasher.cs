using System.Security.Cryptography;
using System.Text;

namespace UrlShortener.Api.Services;

public class IpHasher
{
    private readonly IConfiguration _config;

    public IpHasher(IConfiguration config)
    {
        _config = config;
    }

    public string? Hash(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return null;

        var key = _config["AppSettings:IpHashKey"];
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("AppSettings:IpHashKey is not configured.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var bytes = Encoding.UTF8.GetBytes(ip);
        var hash = hmac.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}