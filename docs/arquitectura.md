# 🏗️ Arquitectura del Sistema Policial

## Vista general

Cada microservicio es **independiente** con su propia base de datos. Si uno se cae, los demás siguen funcionando.

| Microservicio | Puerto | Base de Datos | Responsabilidad |
|--------------|--------|--------------|-----------------|
| Identity.API | 7117 | BD_Seguridad | Login, usuarios, roles |
| RRHH.API | 7120 | BD_RRHH | Personal, grados, unidades |
| Logistica.API | 7206 | BD_Logistica | Armamento |
| Gateway | 7059 | — | Enruta y valida JWT |
| Blazor Web | — | — | Frontend (corre en el navegador) |

## 🔐 Flujo de Login

U->>B: Escribe CIP + Password
B->>G: POST /seguridad/Login
G->>I: POST /api/Auth/Login
I->>DB: SELECT * FROM Usuario WHERE Cip = ?
DB-->>I: Usuario encontrado
I->>I: Valida Estado + Password
I->>I: Genera JWT Token (2h)
I-->>G: 200 OK + Token
G-->>B: 200 OK + Token
B->>B: Guarda token en LocalStorage
B-->>U: Redirige a Home

**Claims del JWT Token:**

| Claim | Valor | Ejemplo |
|-------|-------|---------|
| Sub | CIP del usuario | "12345678" |
| idPersonal | ID en BD_RRHH | 5 |
| Role | ID del rol | 1 |

## 👤 Flujo de creación de usuario

Note over A: Paso 1 - Buscar policía
A->>G: GET /rrhh/Personal/buscar-cip/12345678
G->>G: Valida JWT
G->>R: Busca en BD_RRHH
R-->>A: Datos del policía (nombre, grado, unidad)

Note over A: Paso 2 - Crear cuenta
A->>G: POST /seguridad/CrearUsuario
G->>I: Valida CIP único + Rol existe
I->>I: INSERT con auditoría (CreadoPor, Fecha)
I-->>A: 201 Created

## 📋 Flujo de listar, desactivar y reactivar

Note over A: Cargar tabla
A->>G: GET /seguridad/Listar
G->>I: SELECT Usuarios + Rol
I-->>A: Lista de usuarios en tabla

Note over A: Desactivar
A->>G: PUT /seguridad/Desactivar/5
G->>I: UPDATE Estado=false
I->>DB: ModificadoPor=CIP_ADMIN, Fecha=ahora
I-->>A: Usuario desactivado

Note over A: Reactivar
A->>G: PUT /seguridad/Reactivar/5
G->>I: UPDATE Estado=true
I->>DB: ModificadoPor=CIP_ADMIN, Fecha=ahora
I-->>A: Usuario reactivado

## 🔑 Cómo funciona JWT en el sistema

graph LR A["1. Login exitoso"] --> B["2. Se genera JWT Token"] B --> C["3. Blazor guarda en LocalStorage"] C --> D["4. Cada petición lleva el Token"] D --> E["5. Gateway valida firma + expiración"] E -->|"Válido"| F["✅ Reenvía a la API"] E -->|"Inválido"| G["❌ 401 Unauthorized"]

## 🛡️ Políticas de seguridad

- **NO se eliminan registros** — Se usa campo `Estado` (true/false) para desactivar
- **Auditoría obligatoria** — Cada acción registra quién la hizo y cuándo
- **Gateway como único punto de entrada** — Las APIs no se acceden directamente
- **Token expira en 2 horas** — Después hay que volver a loguearse

