-- Script para agregar la columna Cip a la tabla Usuario
-- Base de datos: BD_Seguridad

USE BD_Seguridad;
GO

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Usuario]') 
    AND name = 'Cip'
)
BEGIN
    -- Agregar la columna Cip
    ALTER TABLE [dbo].[Usuario]
    ADD [Cip] VARCHAR(50) NULL;
    
    PRINT 'Columna Cip agregada correctamente.';
END
ELSE
BEGIN
    PRINT 'La columna Cip ya existe.';
END
GO

-- Crear el índice único para Cip (si no existe)
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE object_id = OBJECT_ID(N'[dbo].[Usuario]') 
    AND name = 'UQ__Usuario__536C85E4BADCAE5F'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ__Usuario__536C85E4BADCAE5F] 
    ON [dbo].[Usuario]([Cip] ASC)
    WHERE [Cip] IS NOT NULL;
    
    PRINT 'Índice único creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El índice único ya existe.';
END
GO

-- Actualizar registros existentes con valores únicos temporales para Cip
-- IMPORTANTE: Modifica esto según tus necesidades de negocio
-- Este script asigna valores temporales basados en IdUsuario
-- Deberás actualizar estos valores con los CIP reales de cada usuario

UPDATE [dbo].[Usuario]
SET [Cip] = 'CIP' + RIGHT('00000000' + CAST(IdUsuario AS VARCHAR(8)), 8)
WHERE [Cip] IS NULL;
GO

-- Ahora hacer la columna NOT NULL
ALTER TABLE [dbo].[Usuario]
ALTER COLUMN [Cip] VARCHAR(50) NOT NULL;
GO

PRINT 'Script completado. Columna Cip configurada correctamente.';
GO
