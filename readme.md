# Air Tortilla

---

## Descripción

Este proyecto trata de generar un **software de gestión de vuelos**, centrando el foco en el acceso a los datos y facilitando una experiencia de usuario sencilla y eficiente.

## Tecnologías

En este proyecto se utilizará **C# (.NET 10)**. He tomado esta decisión con la esperanza de poder ampliar en un futuro la conectividad del proyecto y añadir una posible **interfaz gráfica**.

## Estructura de los registros

Los registros utilizan la siguiente estructura:

```text
AirTortilla_ES_2026_10_7_FR_999_(nombre0_apellidos0_documento0_22_4)_(persona1_apellidos1_fg204_18_4)
```

### Cabecera de la reserva

| Campo         | Descripción                     |
| ------------- | ------------------------------- |
| `AirTortilla` | Prefijo permanente              |
| `ES`          | Código IATA del país de origen  |
| `2026`        | Año                             |
| `10`          | Mes                             |
| `7`           | Día                             |
| `FR`          | Código IATA del país de destino |
| `999`         | Número de vuelo                 |

### Datos de cada persona

Cada persona asignada a la reserva se representa mediante un bloque entre paréntesis:

```text
(nombre0_apellidos0_documento0_22_4)
```

| Campo        | Descripción                    |
| ------------ | ------------------------------ |
| `nombre0`    | Nombre de la persona           |
| `apellidos0` | Apellidos de la persona        |
| `documento0` | DNI o documento identificativo |
| `22`         | Edad                           |
| `4`          | Número de asiento              |

Una misma reserva puede contener **múltiples personas**. Cada bloque entre paréntesis representa a una persona diferente asignada a esa reserva.

Por ejemplo:

```text
AirTortilla_ES_2026_10_7_FR_999_(nombre0_apellidos0_documento0_22_4)_(persona1_apellidos1_fg204_18_4)
```

En este caso, la reserva contiene **dos personas**:

1. `nombre0_apellidos0_documento0_22_4`
2. `persona1_apellidos1_fg204_18_4`

La cantidad de personas por reserva es variable, por lo que pueden existir registros con una sola persona o con múltiples pasajeros.
