using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Policia.RRHH.API.Models;

public partial class BdRrhhContext : DbContext
{
    public BdRrhhContext()
    {
    }

    public BdRrhhContext(DbContextOptions<BdRrhhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Grado> Grados { get; set; }

    public virtual DbSet<HistorialMovimiento> HistorialMovimientos { get; set; }

    public virtual DbSet<Personal> Personals { get; set; }

    public virtual DbSet<Situacion> Situacions { get; set; }

    public virtual DbSet<Unidad> Unidads { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1433; Database=BD_RRHH; User Id=sa; Password=sql; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Grado>(entity =>
        {
            entity.HasKey(e => e.IdGrado).HasName("PK__Grado__393DFCB88346DEF4");

            entity.ToTable("Grado");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HistorialMovimiento>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PK__Historia__881A6AE0E37F65EC");

            entity.ToTable("HistorialMovimiento");

            entity.Property(e => e.Documento)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Motivo)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.HistorialMovimientos)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historial__IdPer__5812160E");
        });

        modelBuilder.Entity<Personal>(entity =>
        {
            entity.HasKey(e => e.IdPersonal).HasName("PK__Personal__05A9201B2E512DED");

            entity.ToTable("Personal");

            entity.HasIndex(e => e.Dni, "UQ__Personal__C035B8DDC66A5DFB").IsUnique();

            entity.HasIndex(e => e.Cip, "UQ__Personal__C1F8DC542C2E5506").IsUnique();

            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Cip)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CIP");
            entity.Property(e => e.Dni)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("DNI");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Sexo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.IdGradoNavigation).WithMany(p => p.Personals)
                .HasForeignKey(d => d.IdGrado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Personal__IdGrad__534D60F1");

            entity.HasOne(d => d.IdSituacionNavigation).WithMany(p => p.Personals)
                .HasForeignKey(d => d.IdSituacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Personal__IdSitu__5535A963");

            entity.HasOne(d => d.IdUnidadActualNavigation).WithMany(p => p.Personals)
                .HasForeignKey(d => d.IdUnidadActual)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Personal__IdUnid__5441852A");
        });

        modelBuilder.Entity<Situacion>(entity =>
        {
            entity.HasKey(e => e.IdSituacion).HasName("PK__Situacio__1B051F6FECCA5C59");

            entity.ToTable("Situacion");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Unidad>(entity =>
        {
            entity.HasKey(e => e.IdUnidad).HasName("PK__Unidad__437725E69A099F79");

            entity.ToTable("Unidad");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Siglas)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
