using System.Security.Cryptography;
using System.Text;

namespace Visma.Yuki.Blog.Domain.ValueObjects;

public partial class UniqueNameIdentifier
{
    public string Value { get; }

    private UniqueNameIdentifier(string value)
    {
        Value = value;
    }

    public static UniqueNameIdentifier FromString(string uniqueNameIdentifier)
    {
        if (string.IsNullOrWhiteSpace(uniqueNameIdentifier))
        {
            throw new ArgumentException("Stored identifier cannot be null or empty.");
        }
        
        return new(uniqueNameIdentifier);
    }

    public static UniqueNameIdentifier Create(string name, string surname)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname))
        {
            throw new ArgumentException("Name and Surname are required to generate the identifier.");
        }

        string rawInput = $"{name.Trim().ToLower()}_{surname.Trim().ToLower()}";
        
        byte[] inputBytes = Encoding.UTF8.GetBytes(rawInput);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        
        StringBuilder hexBuilder = new(64);
        foreach (byte b in hashBytes)
        {
            hexBuilder.Append(b.ToString("x2"));
        }
        
        string fullHash = hexBuilder.ToString();

        string finalValue = fullHash.Length > 50 
            ? fullHash[..50] 
            : fullHash;

        return new UniqueNameIdentifier(finalValue);
    }

    public override bool Equals(object? obj) => obj is UniqueNameIdentifier other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}