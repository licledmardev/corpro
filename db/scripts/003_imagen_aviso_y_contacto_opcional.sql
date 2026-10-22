-- =============================================================================
-- Script 003 (incremental, idempotente) para bases creadas con la versión anterior de 001.
-- 1. Imagen del aviso por interrupción (se muestra en la página).
-- 2. La actualización de contacto deja de ser "declaración jurada": se elimina
--    acepta_declaracion. Solo se reciben celular y/o correo.
-- El script 001 ya contiene estos cambios para instalaciones nuevas.
-- =============================================================================

ALTER TABLE interrupcion_programada
    ADD COLUMN IF NOT EXISTS imagen_aviso VARCHAR(300) NULL;  -- ruta relativa al sitio (ej. avisos/9320261712.webp) o URL

ALTER TABLE datos_contacto_actualizados DROP CONSTRAINT IF EXISTS ck_datos_contacto_acepta;
ALTER TABLE datos_contacto_actualizados DROP COLUMN IF EXISTS acepta_declaracion;

-- Celular peruano: 9 dígitos iniciando en 9
ALTER TABLE datos_contacto_actualizados DROP CONSTRAINT IF EXISTS ck_datos_contacto_celular;
ALTER TABLE datos_contacto_actualizados
    ADD CONSTRAINT ck_datos_contacto_celular CHECK (telefono_nuevo IS NULL OR telefono_nuevo ~ '^9[0-9]{8}$');

-- La vista agrega la imagen del aviso al final (CREATE OR REPLACE solo permite añadir columnas al final)
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
