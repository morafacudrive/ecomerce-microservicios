# E-Commerce — Arquitectura de Microservicios

**Materia:** Construcción de Aplicaciones Informáticas  
**Tecnología:** .NET 8 · ASP.NET Core · SQLite + Dapper · Serilog · Swagger/OpenAPI

---

## Descripción

Sistema de eCommerce implementado como arquitectura de microservicios independientes. Cada módulo expone una REST API con documentación Swagger, manejo estructurado de errores con códigos propios, logging estructurado con Serilog, health checks y propagación de Correlation ID entre servicios.

---

## Microservicios

| Servicio | Puerto | Responsabilidad | Swagger |
|---|---|---|---|
| Products.API | 7000 | Gestión de productos, stock y categorías | https://localhost:7000/swagger |
| Users.API | 7100 | Registro, login y bloqueo de usuarios | https://localhost:7100/swagger |
| Orders.API | 7224 | Creación y gestión de órdenes | https://localhost:7224/swagger |
| Cart.API | 7267 | Carrito de compras por usuario | https://localhost:7267/swagger |
| Notifications.API | 7179 | Envío y registro de notificaciones | https://localhost:7179/swagger |

---

## Requisitos previos

- .NET 8 SDK
- Visual Studio 2022 o superior
- No requiere instalación de base de datos (SQLite se crea automáticamente al iniciar cada servicio)

---

## Cómo ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd ecommerce-microservicios
```

### 2. Abrir la solución

Abrí el archivo `ECommerce.sln` con Visual Studio 2022.

### 3. Configurar inicio múltiple

1. Clic derecho sobre la **Solución** en el Explorador de soluciones → **Propiedades**
2. Ir a **Propiedades comunes → Proyecto de inicio**
3. Seleccionar **Proyectos de inicio múltiples**
4. Cambiar la acción a **Iniciar** para los 5 proyectos:
   - `Cart.API`
   - `Notifications.API`
   - `Orders.API`
   - `Products.API`
   - `Users.API`
5. Clic en **Aplicar** → **Aceptar**

### 4. Ejecutar

Presioná **F5** o el botón **Iniciar**. Visual Studio abrirá automáticamente las 5 pestañas de Swagger en el navegador.

---

## Endpoints por servicio

### Products.API — `/api/products`

| Método | Endpoint | Descripción | HTTP Status |
|---|---|---|---|
| GET | /api/products | Listar productos (`?categoria=` `?nombre=`) | 200, 500 |
| GET | /api/products/{id} | Obtener producto por ID | 200, 404, 500 |
| POST | /api/products | Crear producto | 201, 400, 409, 500 |
| PUT | /api/products/{id} | Actualizar producto | 200, 400, 404, 500 |
| DELETE | /api/products/{id} | Eliminar producto | 204, 404, 409, 500 |

### Users.API — `/api/users`

| Método | Endpoint | Descripción | HTTP Status |
|---|---|---|---|
| POST | /api/users/register | Registrar usuario | 201, 400, 409, 500 |
| POST | /api/users/login | Autenticar usuario | 200, 400, 401, 403, 500 |

### Orders.API — `/api/orders`

| Método | Endpoint | Descripción | HTTP Status |
|---|---|---|---|
| GET | /api/orders | Listar órdenes (`?usuarioId=`) | 200, 500 |
| GET | /api/orders/{id} | Obtener orden por ID | 200, 404, 500 |
| POST | /api/orders | Crear orden | 201, 400, 404, 422, 500 |
| PUT | /api/orders/{id}/status | Actualizar estado de orden | 200, 400, 404, 409, 500 |

### Cart.API — `/api/cart`

| Método | Endpoint | Descripción | HTTP Status |
|---|---|---|---|
| GET | /api/cart/{userId} | Obtener carrito del usuario | 200, 404, 500 |
| POST | /api/cart/{userId}/items | Agregar producto al carrito | 200, 400, 404, 422, 500 |
| PUT | /api/cart/{userId}/items/{productId} | Actualizar cantidad | 200, 400, 404, 422, 500 |
| DELETE | /api/cart/{userId}/items/{productId} | Quitar producto | 204, 404, 500 |
| DELETE | /api/cart/{userId} | Vaciar carrito | 204, 404, 500 |

### Notifications.API — `/api/notifications`

| Método | Endpoint | Descripción | HTTP Status |
|---|---|---|---|
| POST | /api/notifications/send | Enviar notificación | 201, 400, 500 |
| GET | /api/notifications/{userId} | Listar notificaciones del usuario | 200, 500 |

---

## Manejo de errores

Todos los errores siguen el formato Problem Details con códigos propios:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no fue encontrado.",
  "instance": "/api/products/99",
  "errorCode": "PRD-001",
  "errorMessage": "Producto no encontrado."
}
```

Implementado con `IExceptionHandler` de .NET 8 — sin try/catch en controladores.

---

## Logging

Cada servicio genera logs con Serilog en dos destinos:

- **Consola:** formato legible con timestamp, nivel, servicio y Correlation ID
- **Archivo:** `logs/<servicio>-.log` en formato JSON estructurado con rotación diaria

Cada log incluye: Timestamp · Nivel · Servicio · CorrelationId · Endpoint · Duración

---

## Health Checks

Cada servicio expone tres endpoints:

| Endpoint | Descripción |
|---|---|
| /health | Estado general del servicio |
| /health/ready | Listo para recibir tráfico |
| /health/live | Servicio activo |

Respuesta de ejemplo:
```json
{
  "estado": "Healthy",
  "checks": [
    { "nombre": "sqlite", "estado": "Healthy" }
  ]
}
```

---

## Seguridad y observabilidad

- **Correlation ID:** cada request recibe un `X-Correlation-Id` único propagado en headers y logs
- **Security Headers:** `Content-Security-Policy`, `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`
- **Rate Limiting:** límite de 100 requests/minuto en Products.API

---

## Diagrama de arquitectura

Ver archivo `docs/arquitectura.png`

---

## Capturas de Swagger

Ver carpeta `docs/swagger/`
