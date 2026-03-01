using System;
using System.Collections.Generic;
using Armala.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Armala.Auth.Infrastructure.Persistence.Context;

public partial class ArmalaDbContext : DbContext
{
    public ArmalaDbContext(DbContextOptions<ArmalaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<MatchLocation> MatchLocations { get; set; }

    public virtual DbSet<MatchSettlement> MatchSettlements { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserFinancialProfile> UserFinancialProfiles { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__matches__3213E83F299779E8");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AllowPayLater).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("OPEN");

            entity.HasOne(d => d.Organizer).WithMany(p => p.Matches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__matches__organiz__06CD04F7");
        });

        modelBuilder.Entity<MatchLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__match_lo__3213E83F20B99265");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Match).WithOne(p => p.MatchLocation).HasConstraintName("FK__match_loc__match__0F624AF8");
        });

        modelBuilder.Entity<MatchSettlement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__match_se__3213E83F7F9E8902");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ProcessedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Match).WithOne(p => p.MatchSettlement)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__match_set__match__2645B050");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83FE69238A5");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasConstraintName("FK__notificat__user___2B0A656D");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__particip__3213E83F6D9D04BF");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsNewUserEligible).HasDefaultValue(false);
            entity.Property(e => e.ReservedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("RESERVED");

            entity.HasOne(d => d.Match).WithMany(p => p.Participants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__participa__match__14270015");

            entity.HasOne(d => d.User).WithMany(p => p.Participants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__participa__user___151B244E");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83F6A31C736");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Currency).HasDefaultValue("PEN");
            entity.Property(e => e.Status).HasDefaultValue("PENDING");

            entity.HasOne(d => d.Participant).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payments__partic__1CBC4616");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__refresh___3213E83F8F5E1F02");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens).HasConstraintName("FK__refresh_t__user___7A672E12");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F4995EA68");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("NEW_USER");
        });

        modelBuilder.Entity<UserFinancialProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_fin__3213E83F5CAC1897");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsPrimary).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany(p => p.UserFinancialProfiles).HasConstraintName("FK__user_fina__user___00200768");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.Property(e => e.Attempts).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__otps__user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
