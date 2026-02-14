using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("payments")]
public partial class Payment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("participant_id")]
    public Guid ParticipantId { get; set; }

    [Column("amount", TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    [Column("currency")]
    [StringLength(3)]
    public string? Currency { get; set; }

    [Column("method")]
    [StringLength(20)]
    public string Method { get; set; } = null!;

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("transaction_external_id")]
    [StringLength(255)]
    public string? TransactionExternalId { get; set; }

    [Column("authorization_code")]
    [StringLength(255)]
    public string? AuthorizationCode { get; set; }

    [Column("proof_image_url")]
    public string? ProofImageUrl { get; set; }

    [Column("metadata")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ParticipantId")]
    [InverseProperty("Payments")]
    public virtual Participant Participant { get; set; } = null!;
}
