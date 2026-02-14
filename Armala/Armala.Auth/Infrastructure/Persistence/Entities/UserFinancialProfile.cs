using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("user_financial_profiles")]
public partial class UserFinancialProfile
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("bank_name")]
    [StringLength(100)]
    public string BankName { get; set; } = null!;

    [Column("account_number")]
    [StringLength(50)]
    public string AccountNumber { get; set; } = null!;

    [Column("cci")]
    [StringLength(20)]
    public string Cci { get; set; } = null!;

    [Column("is_primary")]
    public bool? IsPrimary { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserFinancialProfiles")]
    public virtual User User { get; set; } = null!;
}
