# 📡 Endpoints del API

> Todas las rutas pasan por el **Gateway** en `https://localhost:7059`

## 🔐 Identity — Seguridad (`/seguridad/*`)

| Método | Ruta | Auth | Descripción |
|--------|------|:----:|------------|
| `POST` | `/seguridad/Login` | ❌ | Iniciar sesión con CIP + Password |
| `POST` | `/seguridad/CrearUsuario` | ✅ | Crear cuenta para un policía |
| `GET` | `/seguridad/Listar` | ✅ | Listar todos los usuarios con su rol |
| `PUT` | `/seguridad/Desactivar/{id}` | ✅ | Desactivar usuario (Estado = false) |
| `PUT` | `/seguridad/Reactivar/{id}` | ✅ | Reactivar usuario (Estado = true) |

### POST /seguridad/Login

**Request:**
{ "cip": "12345678", "password": "123" }

### POST /seguridad/CrearUsuario

**Request:**
````````


# Response
````````markdown


````````

### PUT /seguridad/Desactivar/{id}

**Response 200:**
````````


# Response
````````markdown


````````

### GET /seguridad/Listar

**Response 200:**
````````


# Response
````````markdown


````````

### PUT /seguridad/Reactivar/{id}

**Response 200:**
````````


# Response
````````markdown


````````

---

## 👥 RRHH — Personal (`/rrhh/*`)

| Método | Ruta | Auth | Descripción |
|--------|------|:----:|------------|
| `GET` | `/rrhh/Personal` | ✅ | Listar todo el personal |
| `GET` | `/rrhh/Personal/buscar-cip/{cip}` | ✅ | Buscar policía por CIP |

### GET /rrhh/Personal/buscar-cip/{cip}

**Response 200:**
````````


# Response
````````markdown


````````

---

## 🔫 Logística — Armamento (`/logistica/*`)

| Método | Ruta | Auth | Descripción |
|--------|------|:----:|------------|
| `GET` | `/logistica/Armamento/mis-armas/{idPersonal}` | ✅ | Armas asignadas al policía |

---

## 🚪 Rutas del Gateway (ocelot.json)

| Ruta pública | API destino | Puerto | Auth requerida |
|-------------|-------------|--------|:--------------:|
| `/seguridad/*` | Identity.API | 7117 | ❌ |
| `/rrhh/*` | RRHH.API | 7120 | ✅ Bearer |
| `/logistica/*` | Logistica.API | 7206 | ✅ Bearer |

