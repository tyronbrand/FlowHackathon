using System.ComponentModel.DataAnnotations;

namespace Hackathon.Api.Users;

public class User
{
    [Key]
    public long Id { get; set; }
    public string? UnityId { get; set; }
    public string? WalletId { get; set; }
    public DateTimeOffset? CreatedDate { get; set; } = DateTimeOffset.UtcNow;
}