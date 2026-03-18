namespace SupplyChainApi.DTOs;

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? BlockchainAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class UserUpdateRequest
{
    public string? Name { get; set; }
    public string? BlockchainAddress { get; set; }
    public bool? IsActive { get; set; }
}
