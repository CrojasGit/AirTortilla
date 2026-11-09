USE AirTortilla;

CREATE TABLE IF NOT EXISTS Usuario (
    documento VARCHAR(20) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellido1 VARCHAR(100) NOT NULL,
    apellido2 VARCHAR(100),
    password_hash VARCHAR(255) NOT NULL,
    tipo_usuario ENUM('empleado', 'administrador')
        NOT NULL DEFAULT 'empleado',
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Reserva (
    id_reserva INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    destino CHAR(2) NOT NULL,
    origen CHAR(2) NOT NULL,
    flight_number VARCHAR(20) NOT NULL,
    fecha DATETIME NOT NULL,
    num_personas INT UNSIGNED NOT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Persona (
    Documento VARCHAR(20) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Apellido1 VARCHAR(100) NOT NULL,
    Apellido2 VARCHAR(100),
    FechaNacimiento DATE NOT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ReservaPersona (
    id_reserva INT UNSIGNED NOT NULL,
    id_persona VARCHAR(20) NOT NULL,
    Asiento INT NOT NULL,

    PRIMARY KEY (id_reserva, id_persona),

    FOREIGN KEY (id_reserva)
        REFERENCES Reserva(id_reserva)
        ON DELETE CASCADE,

    FOREIGN KEY (id_persona)
        REFERENCES Persona(Documento)
        ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Auditoria (
    id_auditoria BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    documento_usuario VARCHAR(20),
    accion VARCHAR(50) NOT NULL,
    tabla_afectada VARCHAR(50) NOT NULL,
    id_registro VARCHAR(100),
    fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    detalles JSON,

    FOREIGN KEY (documento_usuario)
        REFERENCES Usuario(documento)
        ON DELETE SET NULL
) ENGINE=InnoDB;

-- Usuarios de prueba
INSERT INTO Usuario (
    documento,
    nombre,
    apellido1,
    apellido2,
    password_hash,
    tipo_usuario,
    activo
) VALUES
(
    '00000000T',
    'Administrador',
    'Prueba',
    NULL,
    '$2a$11$YGxWbA721s6sohEHVqHcAOQTam4aMI8pSNJ12qFwdbevsLTQJaiiS',
    'administrador',
    1
),
(
    '11111111H',
    'Empleado',
    'Prueba',
    NULL,
    '$2a$11$fxvAmstMI/i2OY397XakNOMS0evl34DmQbEacCDTbGy6KyW.mnl62',
    'empleado',
    1
);
