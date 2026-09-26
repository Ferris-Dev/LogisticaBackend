# Tracking Logístico

Módulo de seguimiento de paquetes: los pilotos consultan sus entregas y actualizan el estado desde una app Android. Este repositorio contiene el backend. El contrato de la API, que es la fuente de verdad compartida con la app, está en [docs/contrato.md](docs/contrato.md).

| Documento | Contenido |
|---|---|
| [docs/contrato.md](docs/contrato.md) | Contrato de la API y cómo se resuelven los casos que deja abiertos |
| [docs/api.md](docs/api.md) | Tabla de endpoints y ejemplos curl de éxito y de error |
| [docs/modelo-datos.md](docs/modelo-datos.md) | Diagrama ER y datos de prueba |

## Backend

.NET 10 (LTS) · ASP.NET Core Web API · EF Core 10 · PostgreSQL · xUnit

```
backend/
├── src/
│   ├── Tracking.Api             controllers delgados, middleware de errores, Swagger, Program.cs
│   ├── Tracking.Application     servicios (casos de uso), DTOs, validaciones, interfaces de repositorio
│   ├── Tracking.Domain          entidades, enum EstadoPaquete, regla last-write-wins, excepciones
│   └── Tracking.Infrastructure  EF Core + Npgsql, repositorios, migraciones, DatabaseConfig
├── tests/Tracking.Tests         xUnit (servicios, dominio, contrato HTTP)
├── railpack.json                cómo compila y arranca Railway la API
├── railway.json                 healthcheck y política de reinicio en Railway
└── .env.example                 referencia de variables de entorno
```

### Requisitos

- **.NET SDK 10**
- **PostgreSQL** 14 o superior, instalado en la máquina.

### Cómo correr en local

1. Guarda la contraseña de tu PostgreSQL con user-secrets, que queda fuera del repositorio:

   ```bash
   cd backend/src/Tracking.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=tracking_logistico;Username=postgres;Password=TU_CLAVE"
   ```

2. Arranca la API con `dotnet run --project backend/src/Tracking.Api` desde la raíz, o con ▶ en Visual Studio.
3. Abre http://localhost:8080/swagger.

En el primer arranque la API crea la base de datos `tracking_logistico`, aplica las migraciones y carga los datos de prueba.

**Verificación mínima**

```bash
curl http://localhost:8080/health
curl http://localhost:8080/api/pilotos/1/paquetes
curl http://localhost:8080/api/tracking/GUA-10001
curl http://localhost:8080/api/tracking/GUA-99999   # 404
curl http://localhost:8080/api/tracking/ABC         # 400
curl -X PUT http://localhost:8080/api/tracking/GUA-10001/estado \
     -H "Content-Type: application/json" \
     -d '{"estado":"ENTREGADO","fechaCambio":"2026-09-26T16:00:00Z"}'
```

**Probar desde Android.** La API escucha en `0.0.0.0:8080`:

- Emulador: `http://10.0.2.2:8080/`.
- Celular físico en la misma red Wi-Fi: `http://<IP-de-la-PC>:8080/`. En Windows hay que permitir el puerto 8080 en el firewall.

En local la API es HTTP, así que la app necesita `android:usesCleartextTraffic="true"` (o una network security config) en el build de desarrollo.

### Cómo correr los tests

```bash
dotnet test
```

Los tests no necesitan PostgreSQL: usan SQLite en memoria con el mismo modelo y los mismos datos de prueba. Cubren:

- Servicios: guía inexistente, formato inválido, cambio válido con historial, last-write-wins, reenvío idempotente, piloto inexistente y piloto sin paquetes.
- Dominio: reglas de `Paquete.CambiarEstado`.
- Contrato HTTP: la API completa en memoria, verificando camelCase, enums como string, fechas `Z` y ProblemDetails con `application/problem+json`.
- Conversión de `DATABASE_URL`.

### Variables de entorno

| Variable | Por defecto | Descripción |
|---|---|---|
| `DATABASE_URL` | — | URI `postgresql://usuario:clave@host:puerto/bd`. Si no existe, se usa `ConnectionStrings:DefaultConnection`. |
| `PORT` | `8080` | Puerto HTTP. Railway lo inyecta. |
| `RUN_MIGRATIONS` | `true` | Aplica migraciones y datos de prueba al arrancar, con 5 reintentos. |
| `ENABLE_SWAGGER` | `true` | Publica Swagger UI en `/swagger`, también en Production. |
| `ASPNETCORE_ENVIRONMENT` | `Production` | `Development` carga `appsettings.Development.json` y user-secrets. |

Todas están documentadas en [backend/.env.example](backend/.env.example). El repositorio no contiene secretos.

### Decisiones de diseño

**Endpoint de listado por piloto.** `GET /api/tracking/{guia}` solo sirve si el piloto ya conoce la guía. Al iniciar su ruta, el piloto necesita ver qué tiene asignado, y la app necesita esa lista para trabajar sin conexión. `GET /api/pilotos/{pilotoId}/paquetes` devuelve un resumen liviano (guía, cliente, zona, estado); el detalle se pide por guía solo al abrir un paquete. Un piloto inexistente da 404, y uno sin paquetes da `200 []`. Así la app distingue "configuración errónea" de "no hay entregas hoy".

**Last-write-wins para la sincronización offline.** La app puede encolar cambios sin señal y enviarlos después, en otro orden o repetidos. Cada cambio lleva `fechaCambio`, el momento en que el piloto lo hizo, y el servidor aplica esta regla:

- Si `fechaCambio` es anterior a `ultimaActualizacion`, el cambio es obsoleto: no se aplica y se responde **200 con el estado vigente**, no con un error, para que la app descarte ese elemento de su cola y muestre la verdad del servidor.
- Si el cambio no modifica nada (reenvío idéntico), tampoco se registra. Reintentar es seguro.
- Cada cambio efectivo se guarda en `historial_estados`, con `fecha_cambio` (dispositivo) y `fecha_registro` (servidor).
- Se rechaza un `fechaCambio` más de 5 minutos en el futuro. Con last-write-wins, un celular con el reloj adelantado bloquearía el paquete para siempre.

**Otras decisiones:**

- El estado del PUT se recibe como string y se valida contra los nombres del enum. Así un valor inválido devuelve `errors.estado` y no un error genérico de deserialización. También se rechaza `"1"`, porque `Enum.TryParse` sí lo aceptaría.
- Las excepciones de dominio (`GuiaNoEncontradaException`, `PilotoNoEncontradoException`, `FormatoGuiaInvalidoException`) se traducen a ProblemDetails en un único `IExceptionHandler`. Los controllers no manejan errores, y un error 500 nunca expone el stack trace.
- La cadena de conexión se resuelve una vez al arrancar: primero `DATABASE_URL` y, si no existe, `DefaultConnection`. Npgsql no acepta el formato URI de Railway, así que `DatabaseConfig` lo convierte.

## Despliegue en Railway

1. **New Project → Deploy from GitHub repo** y elegir este repositorio.
2. En el servicio de la API, **Settings**:
   - **Root Directory** = `/backend`
   - **Config-as-code → Railway Config File** = `/backend/railway.json`. Railway no busca este archivo dentro del Root Directory, así que hay que indicar la ruta.
3. En el proyecto: **Add → Database → PostgreSQL**.
4. En el servicio de la API, en **Variables**, agregar `DATABASE_URL = ${{Postgres.DATABASE_URL}}`. `PORT` lo inyecta Railway. `RUN_MIGRATIONS` y `ENABLE_SWAGGER` valen `true` por defecto.
5. **Settings → Networking → Generate Domain**.
6. Verificar `https://<dominio>/health` y `https://<dominio>/swagger`.

**Cómo se compila.** Railway usa Railpack, su sistema de compilación automático, sin Docker. Railpack detecta .NET cuando hay un `.csproj` en la carpeta raíz, pero aquí los proyectos están en `src/`. Por eso `backend/railpack.json` le indica tres cosas: usar .NET 10, compilar con `dotnet publish src/Tracking.Api/Tracking.Api.csproj -c Release -o out` y arrancar con `./out/Tracking.Api`.

En el primer arranque se aplican las migraciones y se cargan los datos de prueba. El healthcheck de Railway (`/health`, 120 s) espera a que la API y la base de datos respondan. Railway termina TLS en su proxy: la API recibe HTTP y respeta las cabeceras `X-Forwarded-*`. Por eso no se usa `UseHttpsRedirection`.
