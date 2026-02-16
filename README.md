# 🏛️ Sistema Policial Nacional del Perú

Sistema de gestión policial desarrollado con arquitectura de **microservicios** en .NET 10.

## 📋 Descripción

Plataforma interna para la Policía Nacional del Perú que permite:
- 🔐 Autenticación por CIP (Carnet de Identidad Policial)
- 👥 Gestión de usuarios (crear, listar, desactivar, reactivar)
- 🔫 Consulta de armamento asignado
- 📋 Gestión de personal (RRHH)
- 📦 Control logístico

## 🏗️ Arquitectura

El sistema usa **5 proyectos** que se comunican así:

WEB -->|"HTTPS"| GW
GW -->|"/seguridad/*"| ID
GW -->|"/rrhh/*"| RH
GW -->|"/logistica/*"| LG
ID --> SQL
RH --> SQL
LG --> SQL

> **Nota:** Los diagramas se visualizan correctamente en GitHub.

## 🛠️ Tecnologías

| Componente | Tecnología |
|-----------|-----------|
| Frontend | Blazor WebAssembly (.NET 10) |
| Backend APIs | ASP.NET Core Web API (.NET 10) |
| Gateway | Ocelot API Gateway |
| Base de Datos | SQL Server 2022 (Docker) |
| Autenticación | JWT Bearer Token (HMAC-SHA256) |
| ORM | Entity Framework Core |
| Almacenamiento local | Blazored.LocalStorage |

## 🚀 Instalación y ejecución

### Prerrequisitos
- .NET 10 SDK
- Docker Desktop
- Visual Studio 2026

### Paso 1: Clonar el repositorio

git clone https://github.com/Josue1202/git-commit--m-Sistema-Policial-v1.0---Microservicios-Operativos-.git cd SistemaPolicial

### Paso 2: Levantar SQL Server en Docker

````````
docker run -d -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=TuPasswordSa!2023' -p 1433:1433 --name sqlserver --restart always mcr.microsoft.com/mssql/server:2022-latest
````````

### Paso 3: Crear las bases de datos
Ejecutar los scripts SQL en la carpeta `Scripts/`:
1. `AgregarColumnaCip.sql`
2. `AgregarAuditoriaUsuario.sql`

### Paso 4: Ejecutar la solución
Abrir `SistemaPolicial.sln` en Visual Studio → Configurar inicio múltiple → Play ▶️

## 📁 Estructura del proyecto

| Proyecto | Responsabilidad |
|---------|----------------|
| `Policia.Web` | Frontend Blazor WebAssembly |
| `Policia.Gateway` | API Gateway — enruta peticiones a los microservicios |
| `Policia.Identity.API` | Autenticación, usuarios y roles (BD_Seguridad) |
| `Policia.RRHH.API` | Gestión de personal (BD_RRHH) |
| `Policia.Logistica.API` | Control de armamento y logística (BD_Logistica) |

## 📖 Documentación detallada

- [🏗️ Arquitectura y flujos](docs/arquitectura.md)
- [📡 Endpoints del API](docs/endpoints.md)
- [🗄️ Base de datos](docs/base-datos.md)

## 👥 Equipo de desarrollo

| Developer | Rama |
|----------|------|
| Josue | `DEVELOPER-JOSUE` |
| Gabriel | `DEVELOPER-GABRIEL` |

## 📝 Convenciones de commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/):
- `feat:` → Nueva funcionalidad
- `fix:` → Corrección de bug
- `docs:` → Documentación
- `refactor:` → Reestructuración de código

