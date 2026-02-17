using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Policia.Logistica.API.Models;

public partial class BdLogisticaContext : DbContext
{
    public BdLogisticaContext()
    {
    }

    public BdLogisticaContext(DbContextOptions<BdLogisticaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Arma> Armas { get; set; }

    public virtual DbSet<AsignacionArma> AsignacionArmas { get; set; }

    public virtual DbSet<TipoArma> TipoArmas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=ConnectionStrings:CadenaConexion");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Arma>(entity =>
        {
            entity.HasKey(e => e.IdArma).HasName("PK__Arma__2FC1809C3ECA3D28");

            entity.ToTable("Arma");

            entity.HasIndex(e => e.Serie, "UQ__Arma__798299D70B2F546E").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("OPERATIVO");
            entity.Property(e => e.Marca)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Modelo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Serie)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Armas)
                .HasForeignKey(d => d.IdTipo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Arma__IdTipo__4E88ABD4");
        });

        modelBuilder.Entity<AsignacionArma>(entity =>
        {
            entity.HasKey(e => e.IdAsignacion).HasName("PK__Asignaci__A7235DFFB1D9FE40");

            entity.ToTable("AsignacionArma");

            entity.Property(e => e.Observacion)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdArmaNavigation).WithMany(p => p.AsignacionArmas)
                .HasForeignKey(d => d.IdArma)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Asignacio__IdArm__5165187F");
        });

        modelBuilder.Entity<TipoArma>(entity =>
        {
            entity.HasKey(e => e.IdTipo).HasName("PK__TipoArma__9E3A29A53F0C0155");

            entity.ToTable("TipoArma");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
