-- EN TU SQL SERVER, CREA UNA BD LLAMADA: BD_Logistica
CREATE DATABASE BD_Logistica;

USE BD_Logistica;
GO

-- 1. TIPOS DE ARMA
CREATE TABLE TipoArma (
    IdTipo INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL -- PISTOLA, REVOLVER, FUSIL
);

-- 2. INVENTARIO DE ARMAS (El objeto físico)
CREATE TABLE Arma (
    IdArma INT IDENTITY(1,1) PRIMARY KEY,
    IdTipo INT NOT NULL,
    Marca VARCHAR(50) NOT NULL, -- PIETRO BERETTA
    Modelo VARCHAR(50) NULL,
    Serie VARCHAR(50) NOT NULL UNIQUE, -- La serie es única en el mundo
    Estado VARCHAR(20) DEFAULT 'OPERATIVO', -- OPERATIVO, MANTENIMIENTO, PERDIDA
    FOREIGN KEY (IdTipo) REFERENCES TipoArma(IdTipo)
);

-- 3. ASIGNACIÓN (¿Quién la tiene?)
CREATE TABLE AsignacionArma (
    IdAsignacion INT IDENTITY(1,1) PRIMARY KEY,
    IdArma INT NOT NULL,
    IdPersonal INT NOT NULL, -- OJO: Este ID viene de BD_RRHH, aquí es solo un número.
    FechaEntrega DATE NOT NULL,
    FechaDevolucion DATE NULL, -- Si es NULL, todavía la tiene.
    Observacion VARCHAR(200) NULL,
    
    FOREIGN KEY (IdArma) REFERENCES Arma(IdArma)
);

---------------DATOS DE PRUEBAS ----------
-- 1. Tipos de Arma
INSERT INTO TipoArma (Nombre) VALUES ('PISTOLA 9mm PB');
INSERT INTO TipoArma (Nombre) VALUES ('FUSIL AKM');
INSERT INTO TipoArma (Nombre) VALUES ('REVOLVER SW');

-- 2. Inventario de Armas
-- Una Beretta operativa
INSERT INTO Arma (IdTipo, Marca, Modelo, Serie, Estado) 
VALUES (1, 'PIETRO BERETTA', '92FS', 'G-56789', 'OPERATIVO');

-- Un Fusil para servicio
INSERT INTO Arma (IdTipo, Marca, Modelo, Serie, Estado) 
VALUES (2, 'N.KOREA', 'TYPE-68', 'AK-102030', 'MANTENIMIENTO');

-- 3. Asignación (Le damos la pistola al S2 Josue - ID 1)
-- OJO: Aquí ponemos IdPersonal = 1, asumiendo que ese es tu ID en la otra base de datos.
INSERT INTO AsignacionArma (IdArma, IdPersonal, FechaEntrega, Observacion)
VALUES (1, 1, GETDATE(), 'SERVICIO INDIVIDUAL');