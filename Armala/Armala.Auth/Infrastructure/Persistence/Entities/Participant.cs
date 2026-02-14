using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("participants")]
[Index("TicketHash", Name = "UQ__particip__6E0DFBA6E4F211DF", IsUnique = true)]
public partial class Participant
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("match_id")]
    public Guid MatchId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("is_new_user_eligible")]
    public bool? IsNewUserEligible { get; set; }

    [Column("ticket_hash")]
    [StringLength(255)]
    public string? TicketHash { get; set; }

    [Column("reserved_at")]
    public DateTime? ReservedAt { get; set; }

    [Column("confirmed_at")]
    public DateTime? ConfirmedAt { get; set; }

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [ForeignKey("MatchId")]
    [InverseProperty("Participants")]
    public virtual Match Match { get; set; } = null!;

    [InverseProperty("Participant")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("UserId")]
    [InverseProperty("Participants")]
    public virtual User User { get; set; } = null!;
}
