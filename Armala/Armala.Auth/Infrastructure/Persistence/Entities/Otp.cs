using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("otps")]
public class Otp
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("code")]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Column("purpose")]
    [StringLength(50)]
    public string Purpose { get; set; } = null!;

    [Column("is_used")]
    public bool IsUsed { get; set; }

    [Column("attempts")]
    public int Attempts { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
