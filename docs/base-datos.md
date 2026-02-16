# 🗄️ Base de Datos

## Conexión

| Parámetro | Valor |
|-----------|-------|
| Server | `localhost:1433` (Docker) |
| Usuario | `sa` |
| Password | `sql` |

---

## BD_Seguridad
erDiagram Usuario ||--o{ Rol : "pertenece a" Usuario { int IdUsuario PK string Cip UK int IdPersonal FK int IdRol FK string PasswordHash bool Estado string CreadoPor datetime FechaCreacion string ModificadoPor datetime FechaModificacion } Rol { int IdRol PK string Nombre string Descripcion }

### Tabla: Usuario

| Columna | Tipo | Restricción | Descripción |
|---------|------|-------------|------------|
| IdUsuario | int | PK, Identity | ID auto-generado |
| Cip | nvarchar | Unique | Carnet de Identidad Policial |
| IdPersonal | int | FK → BD_RRHH.Personal | Referencia al policía |
| IdRol | int | FK → Rol | Rol del usuario |
| PasswordHash | nvarchar | Not null | Contraseña |
| Estado | bit | Default true | true = Activo, false = Baja |
| CreadoPor | nvarchar(20) | Nullable | CIP del admin que lo creó |
| FechaCreacion | datetime | Default GETDATE() | Fecha de creación |
| ModificadoPor | nvarchar(20) | Nullable | CIP del último admin que lo modificó |
| FechaModificacion | datetime | Nullable | Fecha de última modificación |

### Tabla: Rol

| Columna | Tipo | Restricción | Descripción |
|---------|------|-------------|------------|
| IdRol | int | PK | ID del rol |
| Nombre | nvarchar | Not null | "Administrador", "Operador", etc. |
| Descripcion | nvarchar | Nullable | Descripción del rol |

---

## BD_RRHH

erDiagram Personal ||--o{ Grado : "tiene" Personal ||--o{ Unidad : "pertenece a" Personal ||--o{ Situacion : "tiene" Personal ||--o{ HistorialMovimiento : "registra" Personal { int IdPersonal PK string Cip UK string Dni string Nombres string Apellidos int IdGrado FK int IdUnidadActual FK int IdSituacion FK } Grado { int IdGrado PK string Nombre } Unidad { int IdUnidad PK string Nombre string Siglas int IdUnidadPadre FK } Situacion { int IdSituacion PK string Nombre } HistorialMovimiento { int IdMovimiento PK int IdPersonal FK int IdUnidadOrigen FK int IdUnidadDestino FK datetime FechaMovimiento string Motivo string Documento }

### Tabla: Personal

| Columna | Tipo | Descripción |
|---------|------|------------|
| IdPersonal | int (PK) | ID del policía |
| Cip | nvarchar (Unique) | Carnet de Identidad Policial |
| Dni | nvarchar | DNI |
| Nombres | nvarchar | Nombres completos |
| Apellidos | nvarchar | Apellidos completos |
| IdGrado | int (FK) | Grado policial (Suboficial, Mayor, etc.) |
| IdUnidadActual | int (FK) | Unidad donde trabaja |
| IdSituacion | int (FK) | Actividad, Retiro, etc. |

### Tabla: Grado

| Columna | Tipo | Descripción |
|---------|------|------------|
| IdGrado | int (PK) | ID del grado |
| Nombre | nvarchar | SUBOFICIAL 2DA, MAYOR, CORONEL, etc. |

### Tabla: Unidad

| Columna | Tipo | Descripción |
|---------|------|------------|
| IdUnidad | int (PK) | ID de la unidad |
| Nombre | nvarchar | DIVINCRI LIMA, DIRANDRO, etc. |
| Siglas | nvarchar | Abreviación |
| IdUnidadPadre | int (FK) | Unidad jerárquica superior |

### Tabla: Situacion

| Columna | Tipo | Descripción |
|---------|------|------------|
| IdSituacion | int (PK) | ID de la situación |
| Nombre | nvarchar | ACTIVIDAD, RETIRO, DISPONIBILIDAD |

### Tabla: HistorialMovimiento

| Columna | Tipo | Descripción |
|---------|------|------------|
| IdMovimiento | int (PK) | ID del movimiento |
| IdPersonal | int (FK) | Policía que se movió |
| IdUnidadOrigen | int (FK) | De dónde salió |
| IdUnidadDestino | int (FK) | A dónde llegó |
| FechaMovimiento | datetime | Cuándo se movió |
| Motivo | nvarchar | Razón del movimiento |
| Documento | nvarchar | Resolución o documento oficial |

---

## Scripts de migración

| Script | Descripción |
|--------|------------|
| `Scripts/AgregarColumnaCip.sql` | Agrega columna CIP a tabla Usuario |
| `Scripts/AgregarAuditoriaUsuario.sql` | Agrega columnas de auditoría |