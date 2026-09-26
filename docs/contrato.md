# Contrato API — Tracking Logístico

> Fuente de verdad compartida entre el backend y la app Android. Cualquier cambio aquí se acuerda con ambos lados.

## Base URLs

| Entorno | URL |
|---|---|
| Local (navegador/curl) | `http://localhost:8080/` |
| Local (emulador Android) | `http://10.0.2.2:8080/` |
| Local (celular físico) | `http://<IP-de-la-PC>:8080/` |
| Producción (Railway) | `https://<servicio>.up.railway.app/` |

## Serialización

- JSON en camelCase.
- Enums como STRING, nunca número.
- Fechas ISO-8601 en UTC: `"2026-09-26T16:00:00Z"`.
- Los campos opcionales pueden venir `null`.

**Estados:** `PENDIENTE` | `EN_RUTA` | `ENTREGADO` | `NO_ENTREGADO`

**Formato de guía:** `GUA-\d{5}`

## Endpoints

```
GET  /health                              → 200 { "status": "Healthy" }
GET  /api/pilotos/{pilotoId}/paquetes     → 200 [PaqueteResumen] | 404
GET  /api/tracking/{guia}                 → 200 PaqueteDetalle | 400 | 404
PUT  /api/tracking/{guia}/estado          → 200 PaqueteDetalle | 400 | 404
     body: { "estado": "ENTREGADO", "fechaCambio": "2026-09-26T16:00:00Z" }
```

### PaqueteResumen

```json
{ "guia": "GUA-10001", "cliente": "Ferretería El Constructor",
  "zona": "Zona 10", "estado": "PENDIENTE" }
```

### PaqueteDetalle

```json
{ "guia": "GUA-10001", "cliente": "Ferretería El Constructor",
  "telefono": "5555-1234", "direccion": "12 Calle 4-50 Zona 10",
  "zona": "Zona 10", "estado": "PENDIENTE", "observaciones": null,
  "pilotoId": 1, "fechaAsignacion": "2026-09-26T14:00:00Z",
  "ultimaActualizacion": "2026-09-26T14:00:00Z" }
```

## Errores

ProblemDetails (RFC 7807), `Content-Type: application/problem+json`.

**404**

```json
{ "title": "Guía no encontrada", "status": 404,
  "detail": "La guía GUA-99999 no existe" }
```

**400** (`errors` es opcional)

```json
{ "title": "Solicitud inválida", "status": 400,
  "detail": "El formato de guía debe ser GUA-00000",
  "errors": { "estado": ["Valor no permitido"] } }
```

## Datos de prueba

- Pilotos: `id=1` (PIL-001), `id=2` (PIL-002).
- Guías: `GUA-10001` … `GUA-10008` (mezcla de estados, repartidas entre pilotos).
- Guía para probar 404: `GUA-99999` (no existe en el seed).

## Precisiones de la implementación

Comportamientos que el contrato deja abiertos y que el backend resuelve así:

| Caso | Comportamiento |
|---|---|
| Guía en minúsculas o con espacios (`gua-10001`) | Se normaliza a `GUA-10001`. |
| Guía sin prefijo (`10001`) | 400. La app debe normalizar antes de llamar. |
| Piloto que existe sin paquetes | 200 `[]`. |
| Piloto inexistente | 404, `title` = `"Piloto no encontrado"`. |
| `fechaCambio` omitida | Se usa la hora UTC del servidor. |
| `fechaCambio` con zona horaria (`-06:00`) | Se convierte a UTC. |
| `fechaCambio` anterior a `ultimaActualizacion` | 200 con el estado vigente, sin cambios (last-write-wins). |
| `fechaCambio` más de 5 min en el futuro | 400 `errors.fechaCambio`. |
| Mismo estado y observaciones que los actuales | 200, sin cambios ni historial (reenvío idempotente). |
| `observaciones` en el body del PUT | Opcional: `null` o ausente = no modificar, `""` = borrar. Máximo 500 caracteres. |
| JSON mal formado o body vacío | 400 `"Solicitud inválida"`. |
| Error inesperado | 500 `"Error interno"`, sin stack trace. |
