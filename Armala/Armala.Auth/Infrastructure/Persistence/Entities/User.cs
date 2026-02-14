using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("users")]
[Index("PhoneNumber", Name = "UQ__users__A1936A6B7AA09FD8", IsUnique = true)]
[Index("Email", Name = "UQ__users__AB6E61641EE209A4", IsUnique = true)]
[Index("Dni", Name = "UQ__users__D87608A78088AAC9", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("full_name")]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("phone_number")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [Column("dni")]
    [StringLength(8)]
    public string Dni { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("profile_picture_url")]
    public string? ProfilePictureUrl { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("is_verifired")]
    public bool? IsVerifired { get; set; }

    [InverseProperty("Organizer")]
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [InverseProperty("User")]
    public virtual ICollection<UserFinancialProfile> UserFinancialProfiles { get; set; } = new List<UserFinancialProfile>();
}
