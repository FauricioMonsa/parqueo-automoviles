using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ParqueoAutomoviles.Model;

public partial class ParqueoAutomovilesContext : DbContext
{
    public ParqueoAutomovilesContext(DbContextOptions<ParqueoAutomovilesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Prqautomovile> Prqautomoviles { get; set; }

    public virtual DbSet<PrqingresoAutomovile> PrqingresoAutomoviles { get; set; }

    public virtual DbSet<Prqparqueo> Prqparqueos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<PRQTipoAutomovil>("user_08", "PRQTipoAutomovil")
            .HasPostgresEnum("user_08", "match_kind", new[] { "competition", "friendly" })
            .HasPostgresEnum("user_08", "match_status", new[] { "scheduled", "live", "finished" })
            .HasPostgresEnum("user_08", "sport_type", new[] { "soccer", "basketball", "baseball", "volleyball", "rugby", "hockey" })
            .HasPostgresEnum("user_25", "match_kind", new[] { "friendly", "official" })
            .HasPostgresEnum("user_25", "match_status", new[] { "scheduled", "live", "finished" })
            .HasPostgresEnum("user_25", "sport", new[] { "soccer", "basketball", "baseball", "volleyball", "rugby", "hockey" });

        modelBuilder.Entity<Prqautomovile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prq_automoviles_pk");

            entity.ToTable("PRQAutomoviles", "user_08");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"PRQAutomoviles_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(40)
                .HasColumnName("color");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<PrqingresoAutomovile>(entity =>
        {
            entity.HasKey(e => e.Consecutive).HasName("prq_ingreso_automoviles_pk");

            entity.ToTable("PRQIngresoAutomoviles", "user_08");

            entity.Property(e => e.Consecutive)
                .HasDefaultValueSql("nextval('\"PRQIngresoAutomoviles_consecutive_seq\"'::regclass)")
                .HasColumnName("consecutive");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.EntryDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("entry_datetime");
            entity.Property(e => e.ExitDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("exit_datetime");
            entity.Property(e => e.ParkingId).HasColumnName("parking_id");

            entity.HasOne(d => d.Car).WithMany(p => p.PrqingresoAutomoviles)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("prq_ingreso_car_fk");

            entity.HasOne(d => d.Parking).WithMany(p => p.PrqingresoAutomoviles)
                .HasForeignKey(d => d.ParkingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("prq_ingreso_parking_fk");
        });

        modelBuilder.Entity<Prqparqueo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prq_parqueo_pk");

            entity.ToTable("PRQParqueo", "user_08");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"PRQParqueo_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.PricePerHour)
                .HasPrecision(10, 2)
                .HasColumnName("price_per_hour");
            entity.Property(e => e.ProvinceName)
                .HasMaxLength(100)
                .HasColumnName("province_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
