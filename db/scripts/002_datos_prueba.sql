-- =============================================================================
-- Script 002: datos de prueba FICTICIOS (solo desarrollo)
-- Cabecera tomada del aviso público PDC15-R02 del 25/10/2026.
-- Los suministros y direcciones son inventados. Los datos reales del Excel se
-- cargan con tools/Corpro.Importador y NUNCA se versionan en el repositorio.
-- =============================================================================

INSERT INTO interrupcion_programada
    (nro_interrupcion, tipo_servicio, empresa, alimentador, cod_sistema_electrico, nombre_sistema_electrico,
     fecha_corte, hora_inicio, hora_fin, motivo, sectores_afectados, codigo_documento, usuario_creacion)
VALUES
    (9320261712, 'ENERGIA', 'Electrocentro S.A.', 'A4906', 'SE0063', 'PASCO',
     DATE '2026-10-25', TIME '06:00', TIME '15:00',
     'Mantenimiento preventivo y correctivo en la red de media tensión.',
     'Distrito Yanacancha: Vista Alegre, 27 de Noviembre, Yanacancha Antigua, San Juan. '
     || 'Distrito de Chaupimarca: Miguel Bravo Quipe, Centro, Real de Minas, Relleno de Patarcocha, Túpac Amaru, '
     || 'Uliachín, La Esperanza, Noruega Alta y Baja, Misti, Santa Rosa. '
     || 'Distrito Simón Bolívar: Buenos Aires, Champamarca.',
     'PDC15-R02', 'script-002')
ON CONFLICT (nro_interrupcion) DO NOTHING;

INSERT INTO suministro_afectado
    (nro_interrupcion, nro_suministro, direccion, tarifa, tipo_servicio, nivel_tension, cartera, sector_tipico, usuario_creacion)
VALUES
    (9320261712, '10000001', 'Av. Los Próceres 101 Urb. San Juan',          'BT5BR', 'Urbano', 'BT', 'C', 'ST2', 'script-002'),
    (9320261712, '10000002', 'Av. Los Próceres 115 Int. B Urb. San Juan',   'BT5BR', 'Urbano', 'BT', 'C', 'ST2', 'script-002'),
    (9320261712, '10000003', 'Jr. Las Retamas 220 Urb. Vista Alegre',       'BT5B',  'Urbano', 'BT', 'C', 'ST2', 'script-002'),
    (9320261712, '10000004', 'Jr. Las Retamas 236 Urb. Vista Alegre',       'BT5BR', 'Urbano', 'BT', 'C', 'ST2', 'script-002'),
    (9320261712, '10000005', 'Calle Real de Minas Mz. C Lote 4',            'BT6',   'Urbano', 'BT', 'C', 'ST2', 'script-002')
ON CONFLICT (nro_interrupcion, nro_suministro) DO NOTHING;
