using System;
using System.Collections.Generic;
using Corpro.Infraestructura.Persistencia.EntidadesBaseDatos;
using Microsoft.EntityFrameworkCore;

namespace Corpro.Infraestructura.Persistencia.Contexto;

public partial class CorproDbContext : DbContext
{
    public CorproDbContext(DbContextOptions<CorproDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CorteProgramado> CorteProgramado { get; set; }

    public virtual DbSet<DatosContactoActualizados> DatosContactoActualizados { get; set; }

    public virtual DbSet<InterrupcionProgramada> InterrupcionProgramada { get; set; }

    public virtual DbSet<SuministroAfectado> SuministroAfectado { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.Entity<CorteProgramado>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("corte_programado");

            entity.Property(e => e.Alimentador)
                .HasMaxLength(20)
                .HasColumnName("alimentador");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.Empresa)
                .HasMaxLength(100)
                .HasColumnName("empresa");
            entity.Property(e => e.FechaCorte).HasColumnName("fecha_corte");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.ImagenAviso)
                .HasMaxLength(300)
                .HasColumnName("imagen_aviso");
            entity.Property(e => e.Motivo)
                .HasMaxLength(500)
                .HasColumnName("motivo");
            entity.Property(e => e.NroInterrupcion).HasColumnName("nro_interrupcion");
            entity.Property(e => e.SectoresAfectados).HasColumnName("sectores_afectados");
            entity.Property(e => e.Suministro)
                .HasMaxLength(20)
                .HasColumnName("suministro");
            entity.Property(e => e.TipoServicio)
                .HasMaxLength(10)
                .HasColumnName("tipo_servicio");
        });

        modelBuilder.Entity<DatosContactoActualizados>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_datos_contacto_actualizados");

            entity.ToTable("datos_contacto_actualizados");

            entity.HasIndex(e => new { e.Suministro, e.FechaRegistro }, "ix_datos_contacto_suministro_fecha").IsDescending(false, true);

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CorreoNuevo)
                .HasMaxLength(150)
                .HasColumnName("correo_nuevo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Suministro)
                .HasMaxLength(20)
                .HasColumnName("suministro");
            entity.Property(e => e.TelefonoNuevo)
                .HasMaxLength(15)
                .HasColumnName("telefono_nuevo");
            entity.Property(e => e.UsuarioCreacion)
                .HasMaxLength(100)
                .HasColumnName("usuario_creacion");
            entity.Property(e => e.UsuarioModificacion)
                .HasMaxLength(100)
                .HasColumnName("usuario_modificacion");
        });

        modelBuilder.Entity<InterrupcionProgramada>(entity =>
        {
            entity.HasKey(e => e.NroInterrupcion).HasName("pk_interrupcion_programada");

            entity.ToTable("interrupcion_programada");

            entity.HasIndex(e => e.FechaCorte, "ix_interrupcion_fecha_activo").HasFilter("activo");

            entity.Property(e => e.NroInterrupcion)
                .ValueGeneratedNever()
                .HasColumnName("nro_interrupcion");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Alimentador)
                .HasMaxLength(20)
                .HasColumnName("alimentador");
            entity.Property(e => e.CodSistemaElectrico)
                .HasMaxLength(10)
                .HasColumnName("cod_sistema_electrico");
            entity.Property(e => e.CodigoDocumento)
                .HasMaxLength(30)
                .HasColumnName("codigo_documento");
            entity.Property(e => e.Empresa)
                .HasMaxLength(100)
                .HasColumnName("empresa");
            entity.Property(e => e.FechaCorte).HasColumnName("fecha_corte");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.ImagenAviso)
                .HasMaxLength(300)
                .HasColumnName("imagen_aviso");
            entity.Property(e => e.Motivo)
                .HasMaxLength(500)
                .HasColumnName("motivo");
            entity.Property(e => e.NombreSistemaElectrico)
                .HasMaxLength(60)
                .HasColumnName("nombre_sistema_electrico");
            entity.Property(e => e.SectoresAfectados).HasColumnName("sectores_afectados");
            entity.Property(e => e.TipoServicio)
                .HasMaxLength(10)
                .HasDefaultValueSql("'ENERGIA'::character varying")
                .HasColumnName("tipo_servicio");
            entity.Property(e => e.UsuarioCreacion)
                .HasMaxLength(100)
                .HasColumnName("usuario_creacion");
            entity.Property(e => e.UsuarioModificacion)
                .HasMaxLength(100)
                .HasColumnName("usuario_modificacion");
        });

        modelBuilder.Entity<SuministroAfectado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_suministro_afectado");

            entity.ToTable("suministro_afectado");

            entity.HasIndex(e => e.Direccion, "ix_suministro_afectado_direccion_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.NroSuministro, "ix_suministro_afectado_nro_suministro");

            entity.HasIndex(e => new { e.NroInterrupcion, e.NroSuministro }, "uq_suministro_afectado").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Cartera)
                .HasMaxLength(5)
                .HasColumnName("cartera");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.Property(e => e.NivelTension)
                .HasMaxLength(5)
                .HasColumnName("nivel_tension");
            entity.Property(e => e.NroInterrupcion).HasColumnName("nro_interrupcion");
            entity.Property(e => e.NroSuministro)
                .HasMaxLength(20)
                .HasColumnName("nro_suministro");
            entity.Property(e => e.SectorTipico)
                .HasMaxLength(5)
                .HasColumnName("sector_tipico");
            entity.Property(e => e.Tarifa)
                .HasMaxLength(10)
                .HasColumnName("tarifa");
            entity.Property(e => e.TipoServicio)
                .HasMaxLength(20)
                .HasColumnName("tipo_servicio");
            entity.Property(e => e.UsuarioCreacion)
                .HasMaxLength(100)
                .HasColumnName("usuario_creacion");
            entity.Property(e => e.UsuarioModificacion)
                .HasMaxLength(100)
                .HasColumnName("usuario_modificacion");

            entity.HasOne(d => d.NroInterrupcionNavigation).WithMany(p => p.SuministroAfectado)
                .HasForeignKey(d => d.NroInterrupcion)
                .HasConstraintName("fk_suministro_afectado_interrupcion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
