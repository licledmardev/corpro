-- =============================================================================
-- Corpro - Avisos de cortes programados
-- Script 001: esquema inicial (PostgreSQL 15+ / Neon)
-- Flujo Database-First: este script es la fuente de verdad del modelo.
-- Luego se ejecuta `dotnet ef dbcontext scaffold ... --force` (ver README).
-- =============================================================================

-- Búsqueda parcial por dirección (ILIKE '%texto%') con índice GIN de trigramas.
-- Neon soporta pg_trgm sin configuración adicional.
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- -----------------------------------------------------------------------------
-- 1. Interrupción programada (cabecera del aviso, ej. PDF "PDC15-R02")
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS interrupcion_programada (
    nro_interrupcion          BIGINT        NOT NULL,           -- ID del sistema comercial (ej. 9320261712)
    tipo_servicio             VARCHAR(10)   NOT NULL DEFAULT 'ENERGIA',
    empresa                   VARCHAR(100)  NOT NULL,           -- ej. Electrocentro S.A.
    alimentador               VARCHAR(20)   NULL,               -- ej. A4906
    cod_sistema_electrico     VARCHAR(10)   NULL,               -- ej. SE0063
    nombre_sistema_electrico  VARCHAR(60)   NULL,               -- ej. PASCO
    fecha_corte               DATE          NOT NULL,
    hora_inicio               TIME          NOT NULL,
    hora_fin                  TIME          NOT NULL,
    motivo                    VARCHAR(500)  NULL,
    sectores_afectados        TEXT          NULL,               -- texto del aviso por distrito
    codigo_documento          VARCHAR(30)   NULL,               -- ej. PDC15-R02
    imagen_aviso              VARCHAR(300)  NULL,               -- ruta relativa al sitio (avisos/<nro>.webp) o URL
    -- Auditoría
    fecha_creacion            TIMESTAMPTZ   NOT NULL DEFAULT now(),
    usuario_creacion          VARCHAR(100)  NOT NULL,
    fecha_modificacion        TIMESTAMPTZ   NULL,
    usuario_modificacion      VARCHAR(100)  NULL,
    activo                    BOOLEAN       NOT NULL DEFAULT TRUE,

    CONSTRAINT pk_interrupcion_programada PRIMARY KEY (nro_interrupcion),
    CONSTRAINT ck_interrupcion_tipo_servicio CHECK (tipo_servicio IN ('ENERGIA', 'AGUA')),
    CONSTRAINT ck_interrupcion_horario CHECK (hora_fin > hora_inicio)
);

CREATE INDEX IF NOT EXISTS ix_interrupcion_fecha_activo
    ON interrupcion_programada (fecha_corte)
    WHERE activo;

-- -----------------------------------------------------------------------------
-- 2. Suministros afectados por cada interrupción (ej. Excel "Reportes de Interrupciones")
--    No se almacena el nombre del titular: la consulta es pública y no lo necesita
--    (minimización de datos personales, Ley N.° 29733).
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS suministro_afectado (
    id                        BIGINT        GENERATED ALWAYS AS IDENTITY,
    nro_interrupcion          BIGINT        NOT NULL,
    nro_suministro            VARCHAR(20)   NOT NULL,           -- IDNroServicio
    direccion                 VARCHAR(200)  NOT NULL,           -- normalizada (espacios colapsados)
    tarifa                    VARCHAR(10)   NULL,
    tipo_servicio             VARCHAR(20)   NULL,               -- Urbano / Rural
    nivel_tension             VARCHAR(5)    NULL,               -- BT / MT
    cartera                   VARCHAR(5)    NULL,
    sector_tipico             VARCHAR(5)    NULL,
    -- Auditoría
    fecha_creacion            TIMESTAMPTZ   NOT NULL DEFAULT now(),
    usuario_creacion          VARCHAR(100)  NOT NULL,
    fecha_modificacion        TIMESTAMPTZ   NULL,
    usuario_modificacion      VARCHAR(100)  NULL,
    activo                    BOOLEAN       NOT NULL DEFAULT TRUE,

    CONSTRAINT pk_suministro_afectado PRIMARY KEY (id),
    CONSTRAINT fk_suministro_afectado_interrupcion FOREIGN KEY (nro_interrupcion)
        REFERENCES interrupcion_programada (nro_interrupcion) ON DELETE CASCADE,
    CONSTRAINT uq_suministro_afectado UNIQUE (nro_interrupcion, nro_suministro)
);

-- Coincidencia exacta por suministro
CREATE INDEX IF NOT EXISTS ix_suministro_afectado_nro_suministro
    ON suministro_afectado (nro_suministro);

-- Coincidencia parcial por dirección (ILIKE '%...%')
CREATE INDEX IF NOT EXISTS ix_suministro_afectado_direccion_trgm
    ON suministro_afectado USING GIN (direccion gin_trgm_ops);

-- -----------------------------------------------------------------------------
-- 3. Vista "corte_programado": modelo plano que pide el negocio
--    (Suministro, Direccion, FechaCorte, HoraInicio, HoraFin).
--    El scaffold la genera como entidad sin clave (keyless) de solo lectura.
-- -----------------------------------------------------------------------------
CREATE OR REPLACE VIEW corte_programado AS
SELECT  s.nro_suministro            AS suministro,
        s.direccion                 AS direccion,
        i.fecha_corte               AS fecha_corte,
        i.hora_inicio               AS hora_inicio,
        i.hora_fin                  AS hora_fin,
        i.nro_interrupcion          AS nro_interrupcion,
        i.tipo_servicio             AS tipo_servicio,
        i.empresa                   AS empresa,
        i.alimentador               AS alimentador,
        i.motivo                    AS motivo,
        i.sectores_afectados        AS sectores_afectados,
        i.imagen_aviso              AS imagen_aviso
FROM    suministro_afectado s
JOIN    interrupcion_programada i ON i.nro_interrupcion = s.nro_interrupcion
WHERE   s.activo AND i.activo;

-- -----------------------------------------------------------------------------
-- 4. Actualización (opcional) de datos de contacto: celular y/o correo
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS datos_contacto_actualizados (
    id                        BIGINT        GENERATED ALWAYS AS IDENTITY,
    suministro                VARCHAR(20)   NOT NULL,
    telefono_nuevo            VARCHAR(15)   NULL,               -- celular: 9 dígitos iniciando en 9
    correo_nuevo              VARCHAR(150)  NULL,
    fecha_registro           TIMESTAMPTZ   NOT NULL DEFAULT now(),
    -- Auditoría
    fecha_creacion            TIMESTAMPTZ   NOT NULL DEFAULT now(),
    usuario_creacion          VARCHAR(100)  NOT NULL,
    fecha_modificacion        TIMESTAMPTZ   NULL,
    usuario_modificacion      VARCHAR(100)  NULL,
    activo                    BOOLEAN       NOT NULL DEFAULT TRUE,

    CONSTRAINT pk_datos_contacto_actualizados PRIMARY KEY (id),
    CONSTRAINT ck_datos_contacto_algun_dato CHECK (telefono_nuevo IS NOT NULL OR correo_nuevo IS NOT NULL),
    CONSTRAINT ck_datos_contacto_celular CHECK (telefono_nuevo IS NULL OR telefono_nuevo ~ '^9[0-9]{8}$')
);

CREATE INDEX IF NOT EXISTS ix_datos_contacto_suministro_fecha
    ON datos_contacto_actualizados (suministro, fecha_registro DESC);
