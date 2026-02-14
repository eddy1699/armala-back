using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("match_settlements")]
[Index("MatchId", Name = "UQ__match_se__9D7FCBA2644BB08E", IsUnique = true)]
public partial class MatchSettlement
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("match_id")]
    public Guid MatchId { get; set; }

    [Column("total_collected", TypeName = "decimal(10, 2)")]
    public decimal? TotalCollected { get; set; }

    [Column("bonus_percentage_applied", TypeName = "decimal(5, 2)")]
    public decimal? BonusPercentageApplied { get; set; }

    [Column("bonus_amount", TypeName = "decimal(10, 2)")]
    public decimal? BonusAmount { get; set; }

    [Column("final_payout", TypeName = "decimal(10, 2)")]
    public decimal? FinalPayout { get; set; }

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    [Column("receipt_url")]
    public string? ReceiptUrl { get; set; }

    [ForeignKey("MatchId")]
    [InverseProperty("MatchSettlement")]
    public virtual Match Match { get; set; } = null!;
}
