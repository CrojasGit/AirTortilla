
using System.Globalization;
using AirTortilla.code;
using MySqlConnector;

// Es preferible cargar esta cadena desde variables de entorno o configuración.
// No guardes la contraseña real en el repositorio.
string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "127.0.0.1";
uint dbPort = uint.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "3307");
string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "AirTortilla";
string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "airtortilla_admin";

string dbPassword =
    Environment.GetEnvironmentVariable("DB_PASSWORD")
    ?? Environment.GetEnvironmentVariable("DB_ADMIN_PASSWORD")
    ?? throw new InvalidOperationException("No se ha configurado la contraseña de MariaDB.");

string connectionString =
    $"Server={dbHost};Port={dbPort};Database={dbName};User ID={dbUser};Password={dbPassword};";

await Menu();

// ------------------------------------------------------------
// MENÚ PRINCIPAL
// ------------------------------------------------------------

async Task Menu()
{
    while (Sesion.UsuarioActual == null)
    {
        Console.Clear();
        Console.WriteLine("===== AIR TORTILLA =====");

        Usuario? usuario = await Login(connectionString);

        if (usuario == null)
        {
            Console.WriteLine("\nNo se ha podido iniciar sesión.");
            Console.WriteLine("Pulsa ENTER para volver a intentarlo o escribe S para salir.");

            if ((Console.ReadLine() ?? "").Trim()
                .Equals("S", StringComparison.OrdinalIgnoreCase))
                return;
        }
    }

    while (Sesion.UsuarioActual != null)
    {
        Console.Clear();

        Usuario usuario = Sesion.UsuarioActual!;

        Console.WriteLine("===== AIR TORTILLA =====");
        Console.WriteLine($"Sesión: {usuario.Nombre} {usuario.Apellido1}");
        Console.WriteLine($"Rol: {usuario.TipoUsuario}");
        Console.WriteLine();

        Console.WriteLine("1. Crear reserva");
        Console.WriteLine("2. Consultar reservas");
        Console.WriteLine("3. Buscar reservas en la base de datos");
        Console.WriteLine("4. Importar reservas desde rawData/raw.csv");

        if (Sesion.EsAdministrador())
            Console.WriteLine("5. Crear usuario");

        Console.WriteLine("0. Cerrar sesión");
        Console.Write("\nSelecciona una opción: ");

        string opcion = Console.ReadLine() ?? "";

        try
        {
            switch (opcion)
            {
                case "1":
                    await CrearReserva();
                    break;

                case "2":
                    await LeerReservasBD();
                    break;

                case "3":
                    await BuscarReservasBD();
                    break;

                case "4":
                    await RecibirReservasDesdeRaw();
                    break;

                case "5" when Sesion.EsAdministrador():
                    await CrearUsuario();
                    break;

                case "0":
                    Sesion.Cerrar();
                    Console.WriteLine("Sesión cerrada.");
                    break;

                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nSe ha producido un error:");
            Console.WriteLine(ex.Message);
        }

        if (Sesion.UsuarioActual != null)
        {
            Console.WriteLine("\nPulsa ENTER para continuar...");
            Console.ReadLine();
        }
    }
}

// ------------------------------------------------------------
// AUTENTICACIÓN
// ------------------------------------------------------------

async Task<Usuario?> Login(string connectionString)
{
    Console.Write("Documento: ");
    string documento = Console.ReadLine()?.Trim() ?? "";

    Console.Write("Contraseña: ");
    string contraseña = Console.ReadLine() ?? "";

    Usuario? usuario = await Autenticacion.IniciarSesion(
        connectionString,
        documento,
        contraseña
    );

    if (usuario == null)
        return null;

    Sesion.Iniciar(usuario);
    Console.WriteLine($"\nBienvenido, {usuario.Nombre}.");

    return usuario;
}

// ------------------------------------------------------------
// CREACIÓN DE RESERVAS
// ------------------------------------------------------------

async Task CrearReserva()
{
    Console.WriteLine("\n===== CREAR RESERVA =====");

    Reserva reserva = CrearReservaDesdeConsola();

    int idReserva = await GuardarReserva(connectionString, reserva);

    // Se conserva la exportación CSV como copia de los registros.
    EscribirEnArchivo(
        reserva.reservaToRegistro(),
        Path.Combine("registros", reserva.destiny + ".csv")
    );

    Console.WriteLine($"\nReserva guardada correctamente. ID: {idReserva}");
}

Reserva CrearReservaDesdeConsola()
{
    string destiny = LeerTextoObligatorio("Destino (código IATA): ").ToUpperInvariant();
    string origin = LeerTextoObligatorio("Origen (código IATA): ").ToUpperInvariant();
    string flightNumber = LeerTextoObligatorio("Código del vuelo: ").ToUpperInvariant();

    Dictionary<Person, int> personaXasiento = new();
    bool añadirPersona = true;

    while (añadirPersona)
    {
        Console.WriteLine($"\nPasajero {personaXasiento.Count + 1}");

        string nombre = LeerTextoObligatorio("Nombre: ");
        string apellidos = LeerTextoObligatorio("Apellidos: ");
        string documento = LeerTextoObligatorio("Documento: ");

        DateOnly fechaNacimiento = LeerFecha("Fecha de nacimiento (dd/MM/yyyy): ");
        int asiento = LeerEnteroPositivo("Número de asiento: ");

        Person persona = new Person(
            nombre,
            apellidos,
            documento,
            fechaNacimiento
        );

        if (personaXasiento.Keys.Any(p =>
                p.Documento.Equals(documento, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Ese pasajero ya está incluido en esta reserva.");
            continue;
        }

        if (personaXasiento.ContainsValue(asiento))
        {
            Console.WriteLine("Ese asiento ya está ocupado en esta reserva.");
            continue;
        }

        personaXasiento.Add(persona, asiento);

        Console.Write("¿Quieres añadir otro pasajero? (S/N): ");
        añadirPersona = (Console.ReadLine() ?? "")
            .Trim()
            .Equals("S", StringComparison.OrdinalIgnoreCase);
    }

    return new Reserva(
        destiny,
        origin,
        flightNumber,
        personaXasiento,
        DateOnly.FromDateTime(DateTime.Today)
    );
}

// ------------------------------------------------------------
// GUARDAR RESERVAS EN MARIADB
// ------------------------------------------------------------

async Task<int> GuardarReserva(string connectionString, Reserva reserva)
{
    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    await using var transaction = await connection.BeginTransactionAsync();

    try
    {
        const string sqlReserva = @"
            INSERT INTO Reserva
                (destino, origen, flight_number, fecha, num_personas)
            VALUES
                (@destino, @origen, @flightNumber, @fecha, @numPersonas);";

        await using var commandReserva =
            new MySqlCommand(sqlReserva, connection, transaction);

        commandReserva.Parameters.AddWithValue("@destino", reserva.destiny);
        commandReserva.Parameters.AddWithValue("@origen", reserva.Origin);
        commandReserva.Parameters.AddWithValue("@flightNumber", reserva.fnmber);
        commandReserva.Parameters.AddWithValue(
            "@fecha",
            reserva.dt.ToDateTime(TimeOnly.MinValue)
        );
        commandReserva.Parameters.AddWithValue("@numPersonas", reserva.pXa.Count);

        await commandReserva.ExecuteNonQueryAsync();

        int idReserva = Convert.ToInt32(commandReserva.LastInsertedId);

        foreach (var (persona, asiento) in reserva.pXa)
        {
            string[] apellidos = persona.Apellidos.Trim()
                .Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            string apellido1 = apellidos.Length > 0 ? apellidos[0] : "";
            string? apellido2 = apellidos.Length > 1 ? apellidos[1] : null;

            const string sqlPersona = @"
                INSERT INTO Persona
                    (Documento, Nombre, Apellido1, Apellido2, FechaNacimiento)
                VALUES
                    (@documento, @nombre, @apellido1, @apellido2, @fechaNacimiento)
                ON DUPLICATE KEY UPDATE Documento = Documento;";

            await using var commandPersona =
                new MySqlCommand(sqlPersona, connection, transaction);

            commandPersona.Parameters.AddWithValue("@documento", persona.Documento);
            commandPersona.Parameters.AddWithValue("@nombre", persona.Nombre);
            commandPersona.Parameters.AddWithValue("@apellido1", apellido1);
            commandPersona.Parameters.AddWithValue(
                "@apellido2",
                (object?)apellido2 ?? DBNull.Value
            );
            commandPersona.Parameters.AddWithValue(
                "@fechaNacimiento",
                persona.FechaNacimiento.ToDateTime(TimeOnly.MinValue)
            );

            await commandPersona.ExecuteNonQueryAsync();

            const string sqlRelacion = @"
                INSERT INTO ReservaPersona
                    (id_reserva, id_persona, Asiento)
                VALUES
                    (@idReserva, @idPersona, @asiento);";

            await using var commandRelacion =
                new MySqlCommand(sqlRelacion, connection, transaction);

            commandRelacion.Parameters.AddWithValue("@idReserva", idReserva);
            commandRelacion.Parameters.AddWithValue("@idPersona", persona.Documento);
            commandRelacion.Parameters.AddWithValue("@asiento", asiento);

            await commandRelacion.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();

        return idReserva;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

// ------------------------------------------------------------
// IMPORTACIÓN DE rawData/raw.csv
// ------------------------------------------------------------

async Task RecibirReservasDesdeRaw()
{
    string rutaRaw = Path.Combine(
        AppContext.BaseDirectory,
        "rawData",
        "raw.csv"
    );

    if (!File.Exists(rutaRaw))
    {
        Console.WriteLine($"No se encuentra el archivo: {rutaRaw}");
        return;
    }

    string[] lineas = await File.ReadAllLinesAsync(rutaRaw);

    if (lineas.Length <= 1)
    {
        Console.WriteLine("El CSV no contiene reservas para importar.");
        return;
    }

    int importadas = 0;
    int errores = 0;

    Console.WriteLine($"Se han encontrado {lineas.Length - 1} registros.");
    Console.Write("¿Quieres importarlos a la base de datos? (S/N): ");

    if (!(Console.ReadLine() ?? "").Trim()
        .Equals("S", StringComparison.OrdinalIgnoreCase))
        return;

    for (int i = 1; i < lineas.Length; i++)
    {
        string linea = lineas[i].Trim();

        if (string.IsNullOrWhiteSpace(linea))
            continue;

        try
        {
            Reserva reserva = Reserva.RecReserva(linea);

            int idReserva = await GuardarReserva(connectionString, reserva);

            Console.WriteLine(
                $"Línea {i + 1}: importada. ID de reserva: {idReserva}"
            );

            importadas++;
        }
        catch (Exception ex)
        {
            errores++;
            Console.WriteLine(
                $"Línea {i + 1}: no se ha podido importar. {ex.Message}"
            );
        }
    }

    Console.WriteLine("\n===== RESULTADO DE LA IMPORTACIÓN =====");
    Console.WriteLine($"Importadas: {importadas}");
    Console.WriteLine($"Con errores: {errores}");
}

// ------------------------------------------------------------
// CONSULTAS DE RESERVAS EN MARIADB
// ------------------------------------------------------------

// Permite seleccionar un destino o todos los destinos.
async Task<List<string>> SeleccionarDestinosBD()
{
    List<string> destinos = new();

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    const string sql = @"
        SELECT DISTINCT destino
        FROM Reserva
        ORDER BY destino;";

    await using var command = new MySqlCommand(sql, connection);
    await using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
        destinos.Add(reader.GetString("destino"));

    if (destinos.Count == 0)
    {
        Console.WriteLine("No hay reservas registradas en la base de datos.");
        return new List<string>();
    }

    Console.WriteLine("\nDestinos disponibles:");

    for (int i = 0; i < destinos.Count; i++)
        Console.WriteLine($"{i}. {destinos[i]}");

    Console.WriteLine("-1. Seleccionar todos");
    Console.Write("Selecciona un destino: ");

    if (!int.TryParse(Console.ReadLine(), out int opcion))
    {
        Console.WriteLine("Selección no válida.");
        return new List<string>();
    }

    if (opcion == -1)
        return destinos;

    if (opcion < 0 || opcion >= destinos.Count)
    {
        Console.WriteLine("No existe ese número de destino.");
        return new List<string>();
    }

    return new List<string> { destinos[opcion] };
}

// Construye el filtro de destinos utilizando parámetros SQL.
string CrearFiltroDestinos(
    List<string> destinos,
    MySqlCommand command)
{
    List<string> parametros = new();

    for (int i = 0; i < destinos.Count; i++)
    {
        string nombreParametro = $"@destino{i}";

        parametros.Add(nombreParametro);
        command.Parameters.AddWithValue(nombreParametro, destinos[i]);
    }

    return $"r.destino IN ({string.Join(", ", parametros)})";
}

// Muestra reservas y pasajeros.
// Si se proporciona una búsqueda, muestra todas las personas
// de las reservas que coincidan con el texto.
async Task MostrarReservasBD(
    List<string> destinos,
    bool lineaAlinea,
    string? busqueda = null)
{
    if (destinos.Count == 0)
        return;

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    const string sqlBase = @"
        SELECT
            r.id_reserva,
            r.destino,
            r.origen,
            r.flight_number,
            r.fecha,
            r.num_personas,
            p.Documento,
            p.Nombre,
            p.Apellido1,
            p.Apellido2,
            rp.Asiento
        FROM Reserva r
        INNER JOIN ReservaPersona rp
            ON r.id_reserva = rp.id_reserva
        INNER JOIN Persona p
            ON rp.id_persona = p.Documento
        WHERE ";

    await using var command = new MySqlCommand(sqlBase, connection);

    string filtroDestinos = CrearFiltroDestinos(destinos, command);

    string sql = sqlBase + filtroDestinos;

    if (!string.IsNullOrWhiteSpace(busqueda))
    {
        sql += @"
            AND (
                CONCAT_WS(' ',
                    r.id_reserva,
                    r.destino,
                    r.origen,
                    r.flight_number,
                    DATE_FORMAT(r.fecha, '%d/%m/%Y')
                ) LIKE @busqueda

                OR EXISTS (
                    SELECT 1
                    FROM ReservaPersona rp2
                    INNER JOIN Persona p2
                        ON rp2.id_persona = p2.Documento
                    WHERE rp2.id_reserva = r.id_reserva
                    AND CONCAT_WS(' ',
                        p2.Documento,
                        p2.Nombre,
                        p2.Apellido1,
                        p2.Apellido2,
                        rp2.Asiento
                    ) LIKE @busqueda
                )
            )";

        command.Parameters.AddWithValue("@busqueda", $"%{busqueda}%");
    }

    sql += @"
        ORDER BY r.fecha DESC, r.id_reserva, rp.Asiento;";

    command.CommandText = sql;

    await using var reader = await command.ExecuteReaderAsync();

    bool encontrado = false;
    int ultimaReserva = -1;

    while (await reader.ReadAsync())
    {
        encontrado = true;

        int idReserva = reader.GetInt32("id_reserva");

        if (idReserva != ultimaReserva)
        {
            if (lineaAlinea)
            {
                Console.WriteLine("\nPulsa ENTER para continuar...");
                Console.ReadLine();
            }

            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine($"RESERVA Nº: {idReserva}");
            Console.WriteLine($"Origen: {reader["origen"]}");
            Console.WriteLine($"Destino: {reader["destino"]}");
            Console.WriteLine($"Vuelo: {reader["flight_number"]}");

            Console.WriteLine(
                $"Fecha: {Convert.ToDateTime(reader["fecha"]):dd/MM/yyyy}"
            );

            Console.WriteLine($"Número de pasajeros: {reader["num_personas"]}");
            Console.WriteLine("Pasajeros:");

            ultimaReserva = idReserva;
        }

        string apellido2 = reader.IsDBNull(reader.GetOrdinal("Apellido2"))
            ? ""
            : $" {reader["Apellido2"]}";

        Console.WriteLine(
            $"  - {reader["Nombre"]} {reader["Apellido1"]}{apellido2}" +
            $" | DNI: {reader["Documento"]}" +
            $" | Asiento: {reader["Asiento"]}"
        );
    }

    if (!encontrado)
        Console.WriteLine("\nNo se han encontrado reservas.");
}

// ------------------------------------------------------------
// OPCIÓN 2: CONSULTAR RESERVAS
// ------------------------------------------------------------

async Task LeerReservasBD()
{
    List<string> destinos = await SeleccionarDestinosBD();

    if (destinos.Count == 0)
        return;

    Console.Write("¿Quieres leer línea a línea? (S/N): ");

    bool lineaAlinea = (Console.ReadLine() ?? "")
        .Trim()
        .Equals("S", StringComparison.OrdinalIgnoreCase);

    await MostrarReservasBD(destinos, lineaAlinea);
}

// ------------------------------------------------------------
// OPCIÓN 3: BUSCAR RESERVAS
// ------------------------------------------------------------

async Task BuscarReservasBD()
{
    List<string> destinos = await SeleccionarDestinosBD();

    if (destinos.Count == 0)
        return;

    string texto = LeerTextoObligatorio("Texto que quieres buscar: ");

    await MostrarReservasBD(destinos, false, texto);
}

// ------------------------------------------------------------
// GESTIÓN DE USUARIOS (SOLO ADMINISTRADORES)
// ------------------------------------------------------------

async Task CrearUsuario()
{
    if (!Sesion.EsAdministrador() || Sesion.UsuarioActual == null)
    {
        Console.WriteLine("No tienes permisos para crear usuarios.");
        return;
    }

    Console.WriteLine("\n===== CREAR USUARIO =====");

    string documento = LeerTextoObligatorio("Documento: ");
    string nombre = LeerTextoObligatorio("Nombre: ");
    string apellido1 = LeerTextoObligatorio("Primer apellido: ");

    Console.Write("Segundo apellido (opcional): ");
    string? apellido2 = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(apellido2))
        apellido2 = null;

    Console.Write("Contraseña: ");
    string contraseña = Console.ReadLine() ?? "";

    if (string.IsNullOrWhiteSpace(contraseña))
    {
        Console.WriteLine("La contraseña no puede estar vacía.");
        return;
    }

    Console.WriteLine("Tipo de usuario:");
    Console.WriteLine("1. Empleado");
    Console.WriteLine("2. Administrador");
    Console.Write("Opción: ");

    string tipoUsuario = (Console.ReadLine() ?? "") switch
    {
        "1" => "empleado",
        "2" => "administrador",
        _ => ""
    };

    if (tipoUsuario == "")
    {
        Console.WriteLine("Tipo de usuario no válido.");
        return;
    }

    string passwordHash = BCrypt.Net.BCrypt.HashPassword(contraseña);

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    const string sql = @"
        CALL crear_usuario(
            @actor,
            @documento,
            @nombre,
            @apellido1,
            @apellido2,
            @passwordHash,
            @tipoUsuario
        );";

    await using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue(
        "@actor",
        Sesion.UsuarioActual.Documento
    );
    command.Parameters.AddWithValue("@documento", documento);
    command.Parameters.AddWithValue("@nombre", nombre);
    command.Parameters.AddWithValue("@apellido1", apellido1);
    command.Parameters.AddWithValue(
        "@apellido2",
        (object?)apellido2 ?? DBNull.Value
    );
    command.Parameters.AddWithValue("@passwordHash", passwordHash);
    command.Parameters.AddWithValue("@tipoUsuario", tipoUsuario);

    await command.ExecuteNonQueryAsync();

    Console.WriteLine("Usuario creado correctamente.");
}

// ------------------------------------------------------------
// UTILIDADES DE CONSOLA
// ------------------------------------------------------------

string LeerTextoObligatorio(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);

        string valor = Console.ReadLine()?.Trim() ?? "";

        if (!string.IsNullOrWhiteSpace(valor))
            return valor;

        Console.WriteLine("Este campo es obligatorio.");
    }
}

DateOnly LeerFecha(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);

        string valor = Console.ReadLine() ?? "";

        if (DateOnly.TryParseExact(
                valor,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly fecha))
            return fecha;

        Console.WriteLine("Fecha no válida. Usa el formato dd/MM/yyyy.");
    }
}

int LeerEnteroPositivo(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);

        if (int.TryParse(Console.ReadLine(), out int valor) && valor > 0)
            return valor;

        Console.WriteLine("Introduce un número entero positivo.");
    }
}

// ------------------------------------------------------------
// EXPORTACIÓN CSV ANTIGUA
// Se conserva para generar archivos manualmente al crear reservas.
// ------------------------------------------------------------

void CrearArchivo(string ruta)
{
    string? carpeta = Path.GetDirectoryName(ruta);

    if (!string.IsNullOrEmpty(carpeta))
        Directory.CreateDirectory(carpeta);

    if (!File.Exists(ruta))
    {
        File.WriteAllText(
            ruta,
            "Prefijo_PaisOrigen_Año_Mes_Dia_PaisDestino_CodigoVuelo(Nombre_Apellidos_Documento_FechaNacimiento_Asiento)"
            + Environment.NewLine
        );
    }
}

void EscribirEnArchivo(string texto, string ruta)
{
    CrearArchivo(ruta);
    File.AppendAllText(ruta, texto + Environment.NewLine);
}
