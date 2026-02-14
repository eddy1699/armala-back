using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Entities;

[Table("match_locations")]
[Index("MatchId", Name = "UQ__match_lo__9D7FCBA2A98EEB28", IsUnique = true)]
public partial class MatchLocation
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("match_id")]
    public Guid MatchId { get; set; }

    [Column("venue_name")]
    [StringLength(100)]
    public string? VenueName { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }

    [Column("district")]
    [StringLength(100)]
    public string? District { get; set; }

    [Column("google_maps_url")]
    public string? GoogleMapsUrl { get; set; }

    [Column("latitude", TypeName = "decimal(9, 6)")]
    public decimal? Latitude { get; set; }

    [Column("longitude", TypeName = "decimal(9, 6)")]
    public decimal? Longitude { get; set; }

    [ForeignKey("MatchId")]
    [InverseProperty("MatchLocation")]
    public virtual Match Match { get; set; } = null!;
}
