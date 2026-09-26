# API — referencia y ejemplos

Reemplaza `{{BASE_URL}}` por:

- Local: `http://localhost:8080`
- Railway: `https://<dominio>.up.railway.app`

```bash
BASE_URL=http://localhost:8080
```

Documentación interactiva: `{{BASE_URL}}/swagger`.

## Endpoints

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| GET | `/health` | Estado del servicio y de la base de datos | 200, 503 |
| GET | `/api/pilotos/{pilotoId}/paquetes` | Paquetes asignados a un piloto, ordenados por guía | 200, 404 |
| GET | `/api/tracking/{guia}` | Detalle de un paquete | 200, 400, 404 |
| PUT | `/api/tracking/{guia}/estado` | Cambia el estado (last-write-wins) | 200, 400, 404 |

Todos los errores usan `Content-Type: application/problem+json`. Las respuestas incluyen además `type` y `traceId`, que se omiten en los ejemplos.

---

## GET /health

```bash
curl {{BASE_URL}}/health
```

```json
{ "status": "Healthy" }
```

Si la base de datos no responde: `503 { "status": "Unhealthy" }`.

---

## GET /api/pilotos/{pilotoId}/paquetes

**200**

```bash
curl {{BASE_URL}}/api/pilotos/1/paquetes
```

```json
[
  { "guia": "GUA-10001", "cliente": "Ferretería El Constructor", "zona": "Zona 10", "estado": "PENDIENTE" },
  { "guia": "GUA-10002", "cliente": "Farmacia La Salud", "zona": "Zona 1", "estado": "EN_RUTA" },
  { "guia": "GUA-10003", "cliente": "Librería Cervantes", "zona": "Zona 11", "estado": "ENTREGADO" },
  { "guia": "GUA-10004", "cliente": "Supermercado Don Pedro", "zona": "Zona 10", "estado": "NO_ENTREGADO" }
]
```

**404: piloto inexistente**

```bash
curl {{BASE_URL}}/api/pilotos/99/paquetes
```

```json
{ "title": "Piloto no encontrado", "status": 404, "detail": "El piloto 99 no existe" }
```

---

## GET /api/tracking/{guia}

**200**

```bash
curl {{BASE_URL}}/api/tracking/GUA-10001
```

```json
{
  "guia": "GUA-10001",
  "cliente": "Ferretería El Constructor",
  "telefono": "5555-1234",
  "direccion": "12 Calle 4-50 Zona 10",
  "zona": "Zona 10",
  "estado": "PENDIENTE",
  "observaciones": null,
  "pilotoId": 1,
  "fechaAsignacion": "2026-09-26T14:00:00Z",
  "ultimaActualizacion": "2026-09-26T14:00:00Z"
}
```

**404: guía inexistente**

```bash
curl {{BASE_URL}}/api/tracking/GUA-99999
```

```json
{ "title": "Guía no encontrada", "status": 404, "detail": "La guía GUA-99999 no existe" }
```

**400: formato inválido**

```bash
curl {{BASE_URL}}/api/tracking/ABC
```

```json
{ "title": "Solicitud inválida", "status": 400, "detail": "El formato de guía debe ser GUA-00000" }
```

---

## PUT /api/tracking/{guia}/estado

Body:

| Campo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `estado` | string | sí | `PENDIENTE`, `EN_RUTA`, `ENTREGADO` o `NO_ENTREGADO` |
| `fechaCambio` | string ISO-8601 | no | Momento del cambio en el dispositivo. Si falta, se usa la hora del servidor. |
| `observaciones` | string | no | `null` = no modificar, `""` = borrar. Máximo 500 caracteres. |

**200: cambio aplicado**

```bash
curl -X PUT {{BASE_URL}}/api/tracking/GUA-10001/estado \
     -H "Content-Type: application/json" \
     -d '{"estado":"ENTREGADO","fechaCambio":"2026-09-26T16:00:00Z"}'
```

```json
{
  "guia": "GUA-10001",
  "cliente": "Ferretería El Constructor",
  "telefono": "5555-1234",
  "direccion": "12 Calle 4-50 Zona 10",
  "zona": "Zona 10",
  "estado": "ENTREGADO",
  "observaciones": null,
  "pilotoId": 1,
  "fechaAsignacion": "2026-09-26T14:00:00Z",
  "ultimaActualizacion": "2026-09-26T16:00:00Z"
}
```

**200: cambio obsoleto ignorado (last-write-wins)**

Tras el cambio anterior, la app reenvía uno más viejo:

```bash
curl -X PUT {{BASE_URL}}/api/tracking/GUA-10001/estado \
     -H "Content-Type: application/json" \
     -d '{"estado":"EN_RUTA","fechaCambio":"2026-09-26T15:00:00Z"}'
```

La respuesta sigue mostrando `"estado": "ENTREGADO"` y `"ultimaActualizacion": "2026-09-26T16:00:00Z"`. La app debe mostrar el estado que devuelve el servidor.

**200: no entregado con observaciones**

```bash
curl -X PUT {{BASE_URL}}/api/tracking/GUA-10005/estado \
     -H "Content-Type: application/json" \
     -d '{"estado":"NO_ENTREGADO","fechaCambio":"2026-09-26T17:00:00Z","observaciones":"Local cerrado"}'
```

**400: estado no permitido**

```bash
curl -X PUT {{BASE_URL}}/api/tracking/GUA-10001/estado \
     -H "Content-Type: application/json" \
     -d '{"estado":"PERDIDO"}'
```

```json
{
  "title": "Solicitud inválida",
  "status": 400,
  "detail": "Uno o más campos no son válidos",
  "errors": { "estado": ["Valor no permitido"] }
}
```

**400: fecha inválida o futura**

```bash
curl -X PUT {{BASE_URL}}/api/tracking/GUA-10001/estado \
     -H "Content-Type: application/json" \
     -d '{"estado":"ENTREGADO","fechaCambio":"ayer"}'
```

```json
{
  "title": "Solicitud inválida",
  "status": 400,
  "detail": "Uno o más campos no son válidos",
  "errors": { "fechaCambio": ["Debe ser una fecha ISO-8601, p. ej. 2026-09-26T16:00:00Z"] }
}
```

**400: JSON mal formado**

```bash
curl -X PUT {{BASE_URL}}/api/tracking/GUA-10001/estado \
     -H "Content-Type: application/json" \
     -d '{estado'
```

```json
{ "title": "Solicitud inválida", "status": 400, "detail": "El cuerpo de la solicitud debe ser un JSON válido" }
```

**404: guía inexistente**

```bash
curl -X PUT {{BASE_URL}}/api/tracking/GUA-99999/estado \
     -H "Content-Type: application/json" \
     -d '{"estado":"ENTREGADO"}'
```

```json
{ "title": "Guía no encontrada", "status": 404, "detail": "La guía GUA-99999 no existe" }
```
