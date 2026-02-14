using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("matches")]
[Index("UrlSlug", Name = "UQ__matches__586CCF1FDEB6655E", IsUnique = true)]
public partial class Match
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("organizer_id")]
    public Guid OrganizerId { get; set; }

    [Column("title")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("target_amount", TypeName = "decimal(10, 2)")]
    public decimal TargetAmount { get; set; }

    [Column("total_slots")]
    public int TotalSlots { get; set; }

    [Column("cost_per_person", TypeName = "decimal(10, 2)")]
    public decimal? CostPerPerson { get; set; }

    [Column("match_date")]
    public DateTime MatchDate { get; set; }

    [Column("payment_deadline")]
    public DateTime? PaymentDeadline { get; set; }

    [Column("url_slug")]
    [StringLength(8)]
    public string? UrlSlug { get; set; }

    [Column("allow_pay_later")]
    public bool? AllowPayLater { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Match")]
    public virtual MatchLocation? MatchLocation { get; set; }

    [InverseProperty("Match")]
    public virtual MatchSettlement? MatchSettlement { get; set; }

    [ForeignKey("OrganizerId")]
    [InverseProperty("Matches")]
    public virtual User Organizer { get; set; } = null!;

    [InverseProperty("Match")]
    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
