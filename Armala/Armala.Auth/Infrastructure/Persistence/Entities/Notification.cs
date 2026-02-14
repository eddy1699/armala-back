using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("notifications")]
public partial class Notification
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string? Title { get; set; }

    [Column("body")]
    public string? Body { get; set; }

    [Column("channel")]
    [StringLength(20)]
    public string? Channel { get; set; }

    [Column("is_read")]
    public bool? IsRead { get; set; }

    [Column("sent_at")]
    public DateTime? SentAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User User { get; set; } = null!;
}
