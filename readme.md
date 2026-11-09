# Air Tortilla




---

## Descripción

Este proyecto trata de generar un **software de gestión de vuelos**, centrando el foco en el acceso a los datos y facilitando una experiencia de usuario sencilla y eficiente.

## Tecnologías

En este proyecto se utilizará **C# (.NET 10)**. He tomado esta decisión con la esperanza de poder ampliar en un futuro la conectividad del proyecto y añadir una posible **interfaz gráfica**.

En cuanto a la base de datos, estaremos usando MariaDB puesto que es facil de levantar y sencilla en su uso

Ademas, con el objetivo de hacer mas facil la ejecucion del programa, en AirTortilla empleo docker compose para levantar tanto la base de datos como el .NET

**Por lo tanto, es obligatorio tener docker y compose instalados para poder arrancarlo todo**

---

## Guía de uso

### Arrancarlo todo:

Para las contraseñas de MariaDB empleo .env por motivos de seguridad, he decidido parametrizarlas. Para que pueda arrancar, el sistema necesita las 
contraseñas tanto de root como de admin, he dejado a modo de ejemplo el archivo **.env.example** podeis renombrarlo a **.env** para que funcione.   


```bash
docker compose run --rm --build airtortilla
```

Una vez dentro, hay que escribir el documento (que identifica al usuario)

Podemos acceder mediante cualquier usuario de la base de datos, a modo de 
ayuda he dejado preparados 2 usuarios: 

**Usuario Administrador:**

- Documento -> `00000000T`
- Contraseña -> `Admin123!`

**Usuario Empleado:**

- Documento -> `11111111H`
- Contraseña -> `Empleado123!`

Una vez dentro del sistema, podemos navegar y utilizar las opciones que se brindan. En caso de ser admin, podremos crear usuarios, en caso de no serlo, no.

---

## Estructura de los registros

Los registros utilizan la siguiente estructura para su version en raw (un csv que se puede leer en el motor):

```text
AirTortilla_ES_2026_10_7_FR_999_(nombre0_apellidos0_documento0_22_4)_(persona1_apellidos1_fg204_18_4)
```

### Estructura de la base de datos

En cuanto a la base de datos encontramos la siguiente estructura:

```text
┌──────────────────────────────────────────────┐
│                    USUARIO                   │
├──────────────────────────────────────────────┤
│ PK  documento          VARCHAR(20)            │
│     nombre             VARCHAR(100)           │
│     apellido1          VARCHAR(100)           │
│     apellido2          VARCHAR(100)           │
│     password_hash      VARCHAR(255)           │
│     tipo_usuario       ENUM(empleado, admin.) │
│     activo             BOOLEAN                │
│     fecha_creacion     DATETIME               │
│     fecha_modificacion DATETIME               │
└───────────────────────┬──────────────────────┘
                        │
                        │ 1
                        │
                        │ N
┌───────────────────────▼──────────────────────┐
│                   AUDITORIA                  │
├──────────────────────────────────────────────┤
│ PK  id_auditoria       BIGINT UNSIGNED        │
│ FK  documento_usuario VARCHAR(20)             │
│     accion             VARCHAR(50)            │
│     tabla_afectada     VARCHAR(50)            │
│     id_registro        VARCHAR(100)           │
│     fecha              DATETIME               │
│     detalles           JSON                   │
└──────────────────────────────────────────────┘


┌──────────────────────────────────────────────┐
│                    RESERVA                   │
├──────────────────────────────────────────────┤
│ PK  id_reserva         INT UNSIGNED           │
│     destino            CHAR(2)                │
│     origen             CHAR(2)                │
│     flight_number      VARCHAR(20)            │
│     fecha              DATETIME               │
│     num_personas       INT UNSIGNED           │
└───────────────────────┬──────────────────────┘
                        │
                        │ 1
                        │
                        │ N
┌───────────────────────▼──────────────────────┐
│                 RESERVAPERSONA               │
├──────────────────────────────────────────────┤
│ PK, FK  id_reserva     INT UNSIGNED           │
│ PK, FK  id_persona     VARCHAR(20)             │
│         Asiento        INT                    │
└───────────────────────┬──────────────────────┘
                        │
                        │ N
                        │
                        │ 1
┌───────────────────────▼──────────────────────┐
│                    PERSONA                   │
├──────────────────────────────────────────────┤
│ PK  Documento          VARCHAR(20)            │
│     Nombre             VARCHAR(100)           │
│     Apellido1          VARCHAR(100)           │
│     Apellido2          VARCHAR(100)           │
│     FechaNacimiento    DATE                   │
└──────────────────────────────────────────────┘
```

### Cabecera de la reserva (raw.csv)

| Campo | Descripción |
|---|---|
| `AirTortilla` | Prefijo permanente |
| `ES` | Código IATA del país de origen |
| `2026` | Año |
| `10` | Mes |
| `7` | Día |
| `FR` | Código IATA del país de destino |
| `999` | Número de vuelo |

### Datos de cada persona

Cada persona asignada a la reserva se representa mediante un bloque entre paréntesis (dentro del csv):

```text
(nombre0_apellidos0_documento0_22_4)
```

| Campo | Descripción |
|---|---|
| `nombre0` | Nombre de la persona |
| `apellidos0` | Apellidos de la persona |
| `documento0` | DNI o documento identificativo |
| `22` | Edad |
| `4` | Número de asiento |

Una misma reserva puede contener **múltiples personas**. Cada bloque entre paréntesis representa a una persona diferente asignada a esa reserva.

Por ejemplo:

```text
AirTortilla_ES_2026_10_7_FR_999_(nombre0_apellidos0_documento0_22_4)_(persona1_apellidos1_fg204_18_4)
```

En este caso, la reserva contiene **dos personas**:

1. `nombre0_apellidos0_documento0_22_4`
2. `persona1_apellidos1_fg204_18_4`

La cantidad de personas por reserva es variable, por lo que pueden existir registros con una sola persona o con múltiples pasajeros.
