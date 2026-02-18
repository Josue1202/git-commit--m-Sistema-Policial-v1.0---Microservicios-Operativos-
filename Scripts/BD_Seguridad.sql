 -- EN TU SQL SERVER, CREA UNA BD LLAMADA: BD_Seguridad
CREATE DATABASE BD_Seguridad;
 
USE BD_Seguridad;
GO

-- 1. ROLES (Admin, Operador, etc.)
CREATE TABLE Rol (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(100) NULL
);

-- 2. USUARIOS (El login)
CREATE TABLE Usuario (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    IdPersonal INT NULL, -- Solo el ID, sin relación fuerte a la otra BD
    IdRol INT NOT NULL,
    Cip VARCHAR(50) NOT NULL UNIQUE, --LOGIN POR CIP
    PasswordHash VARCHAR(255) NOT NULL, -- Aquí guardas la clave encriptada
    Estado BIT DEFAULT 1, -- 1=Activo, 0=Inactivo
    FOREIGN KEY (IdRol) REFERENCES Rol(IdRol)
);



DELETE FROM Usuario WHERE Username = 'admin';




-- DATOS SEMILLA
INSERT INTO Rol (Nombre, Descripcion) VALUES ('ADMIN', 'Acceso Total'), ('USER', 'Acceso Limitado');


INSERT INTO Usuario (IdPersonal, IdRol, Cip , PasswordHash, Estado)
VALUES (1, 1, '31586707', 'clave_encriptada_aqui', 1);

INSERT INTO Usuario (IdPersonal, IdRol, Username, PasswordHash) VALUES (NULL, 2, 'user', 'clave_encriptada_aqui');

---------------------------------------------------------------------------------------------------------------
------------------SCRIPT PARA AGREGAR COLUMNAS EN LA TABLA USUARIOS PARA VER QUIEN CREO DICHO USUARIO -----------

-- ============================================================
-- SCRIPT: Agregar columnas de auditoría a tabla Usuario
-- BASE DE DATOS: BD_Seguridad
-- FECHA: 2026-02-12
-- ============================================================

USE BD_Seguridad;
GO

-- ¿QUÉ VAMOS A AGREGAR?
-- 4 columnas nuevas para saber:
--   1. CreadoPor       → ¿Quién creó este usuario?
--   2. FechaCreacion   → ¿Cuándo se creó?
--   3. ModificadoPor   → ¿Quién lo modificó por última vez?
--   4. FechaModificacion → ¿Cuándo se modificó?

-- PASO 1: Agregar CreadoPor
-- Es VARCHAR(50) porque guardará el CIP del admin que creó la cuenta
-- Es NULL porque los usuarios que YA EXISTEN no tienen este dato
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Usuario' AND COLUMN_NAME = 'CreadoPor')
BEGIN
    ALTER TABLE Usuario ADD CreadoPor VARCHAR(50) NULL;
    PRINT '✅ Columna CreadoPor agregada';
END
ELSE
    PRINT '⚠️ CreadoPor ya existe, se omitió';
GO

-- PASO 2: Agregar FechaCreacion
-- Es DATETIME porque guardará fecha + hora exacta
-- DEFAULT GETDATE() → Si no le pasas valor, pone la fecha actual automáticamente
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Usuario' AND COLUMN_NAME = 'FechaCreacion')
BEGIN
    ALTER TABLE Usuario ADD FechaCreacion DATETIME NULL DEFAULT GETDATE();
    PRINT '✅ Columna FechaCreacion agregada';
END
ELSE
    PRINT '⚠️ FechaCreacion ya existe, se omitió';
GO

-- PASO 3: Agregar ModificadoPor
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Usuario' AND COLUMN_NAME = 'ModificadoPor')
BEGIN
    ALTER TABLE Usuario ADD ModificadoPor VARCHAR(50) NULL;
    PRINT '✅ Columna ModificadoPor agregada';
END
ELSE
    PRINT '⚠️ ModificadoPor ya existe, se omitió';
GO

-- PASO 4: Agregar FechaModificacion
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Usuario' AND COLUMN_NAME = 'FechaModificacion')
BEGIN
    ALTER TABLE Usuario ADD FechaModificacion DATETIME NULL;
    PRINT '✅ Columna FechaModificacion agregada';
END
ELSE
    PRINT '⚠️ FechaModificacion ya existe, se omitió';
GO

-- VERIFICACIÓN FINAL: Mostrar cómo quedó la tabla
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Usuario'
ORDER BY ORDINAL_POSITION;
GO

-- RESULTADO ESPERADO:
-- IdUsuario         | int      | NO  | NULL
-- IdPersonal        | int      | YES | NULL
-- IdRol             | int      | NO  | NULL
-- Cip               | varchar  | NO  | NULL
-- PasswordHash      | varchar  | NO  | NULL
-- Estado            | bit      | YES | (1)
-- CreadoPor         | varchar  | YES | NULL        ← NUEVA
-- FechaCreacion     | datetime | YES | getdate()   ← NUEVA
-- ModificadoPor     | varchar  | YES | NULL        ← NUEVA
-- FechaModificacion | datetime | YES | NULL        ← NUEVA