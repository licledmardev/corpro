# Notas PostgreSQL

## Tipos comunes
- `BIT` en SQL Server suele mapear mejor a `BOOLEAN`
- `DATETIME` suele adaptarse a `TIMESTAMP`
- Revisar diferencias de precisión y zona horaria según el caso

## Consideraciones
- Revisar nombres sensibles a mayúsculas/minúsculas
- Revisar índices y restricciones según patrones de búsqueda
- Revisar compatibilidad de funciones SQL específicas del motor

## Recomendación
Mantener toda diferencia del proveedor aislada en `Infraestructura`.