-- EN TU SQL SERVER, CREA UNA BD LLAMADA: BD_RRHH
CREATE DATABASE BD_RRHH;

USE BD_RRHH;
GO

-- 1. MAESTROS (Catalogos)
CREATE TABLE Grado (
    IdGrado INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL, -- Ej: SO2 PNP, CAPITAN
    Jerarquia INT NOT NULL -- 1, 2, 3... para ordenar
);

CREATE TABLE Unidad (
    IdUnidad INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL, -- Ej: COMISARIA SJL
    Siglas VARCHAR(20) NULL,
    IdUnidadPadre INT NULL -- Para jerarquía (Esta comisaría depende de la DIVPOL Este)
);

CREATE TABLE Situacion (
    IdSituacion INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL -- ACTIVO, RETIRO, DISPONIBILIDAD
);

-- 2. LA TABLA MADRE: PERSONAL
CREATE TABLE Personal (
    IdPersonal INT IDENTITY(1,1) PRIMARY KEY,
    
    -- IDENTIFICACIÓN
    CIP CHAR(8) NOT NULL UNIQUE, -- Tu número de 8 dígitos
    DNI CHAR(8) NOT NULL UNIQUE,
    
    -- DATOS BIOGRÁFICOS
    Nombres VARCHAR(100) NOT NULL,
    Apellidos VARCHAR(100) NOT NULL,
    FechaNacimiento DATE NULL,
    Sexo CHAR(1) NULL, -- M / F
    
    -- SITUACIÓN ACTUAL (Lo que más consultas)
    IdGrado INT NOT NULL,
    IdUnidadActual INT NOT NULL,
    IdSituacion INT NOT NULL,
    
    -- AUDITORÍA
    FechaIngreso DATE NULL,
    Estado BIT DEFAULT 1, -- Para borrado lógico
    
    -- RELACIONES INTERNAS
    FOREIGN KEY (IdGrado) REFERENCES Grado(IdGrado),
    FOREIGN KEY (IdUnidadActual) REFERENCES Unidad(IdUnidad),
    FOREIGN KEY (IdSituacion) REFERENCES Situacion(IdSituacion)
);

-- 3. HISTORIAL (Trayectoria básica)
-- Aquí guardas cada vez que cambia de unidad o asciende
CREATE TABLE HistorialMovimiento (
    IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
    IdPersonal INT NOT NULL,
    IdUnidadOrigen INT NULL,
    IdUnidadDestino INT NOT NULL,
    FechaMovimiento DATE NOT NULL,
    Motivo VARCHAR(200) NULL, -- Ej: "POR NECESIDAD DEL SERVICIO"
    Documento VARCHAR(50) NULL, -- Ej: "RD-2026-001"
    FOREIGN KEY (IdPersonal) REFERENCES Personal(IdPersonal)
);


--------------------------------AGREGAMOS DATOS DE PRUEBA---------------------------------------
-- Insertamos los catálogos primero (porque son obligatorios)
INSERT INTO Grado (Nombre, Jerarquia) VALUES ('S2 PNP', 2);
INSERT INTO Unidad (Nombre, Siglas) VALUES ('COMISARIA SANTA ELIZABETH', 'C.ST.ELIZ');
INSERT INTO Situacion (Nombre) VALUES ('ACTIVO');




-- Insertamos al Policía
INSERT INTO Personal (CIP, DNI, Nombres, Apellidos, Sexo, IdGrado, IdUnidadActual, IdSituacion, Estado)
VALUES ('31586707', '12345678', 'JOSUE GABRIEL', 'VILLAGARAY RIVAS', 'M', 1, 1, 1, 1);