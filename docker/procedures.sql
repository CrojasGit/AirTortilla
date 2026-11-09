DELIMITER //

DROP PROCEDURE IF EXISTS crear_usuario//

CREATE PROCEDURE crear_usuario(
    IN p_actor_documento VARCHAR(20),
    IN p_documento VARCHAR(20),
    IN p_nombre VARCHAR(100),
    IN p_apellido1 VARCHAR(100),
    IN p_apellido2 VARCHAR(100),
    IN p_password_hash VARCHAR(255),
    IN p_tipo_usuario VARCHAR(20)
)
SQL SECURITY DEFINER
BEGIN
    DECLARE v_tipo VARCHAR(20);

    SELECT tipo_usuario
    INTO v_tipo
    FROM Usuario
    WHERE documento = p_actor_documento
      AND activo = TRUE;

    IF v_tipo IS NULL OR v_tipo <> 'administrador' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'No tienes permisos para crear usuarios';
    END IF;

    IF p_tipo_usuario NOT IN ('empleado', 'administrador') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Tipo de usuario no válido';
    END IF;

    INSERT INTO Usuario (
        documento,
        nombre,
        apellido1,
        apellido2,
        password_hash,
        tipo_usuario
    )
    VALUES (
        p_documento,
        p_nombre,
        p_apellido1,
        p_apellido2,
        p_password_hash,
        p_tipo_usuario
    );
END //

DELIMITER ;
