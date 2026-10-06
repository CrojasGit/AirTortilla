// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Security.AccessControl;
using AirTortilla.code;

Console.WriteLine("---CREAR RESERVA---");
Reserva r1=crearReserva();
Console.WriteLine(r1.reservaToRegistro());
Console.WriteLine("---LEER RESERVA---");

string[] contenido = leerArchivo("registros/"+r1.destiny+".csv", true, true);
Console.WriteLine("Ingrese el contenido a buscar dentro de la ultima reserva");
List<string> filtrado = busqueda(Console.ReadLine(), contenido);
foreach (string linea in filtrado)
{
    Console.WriteLine(linea);
}


List<string> busqueda(string texto, string[] lineas)
{
    List<string> devolver = new List<string>();
    foreach (string linea in lineas)
    {
        if (linea.Contains(texto))
        {
            devolver.Add(linea);
        }
    }

    return devolver;
}

string[] leerArchivo(String ruta, Boolean lineaAlinea, Boolean imprimir)
{
    string[] contenidoL;
    try
    {
        contenidoL = File.ReadAllLines(ruta);
        foreach (string linea in contenidoL)
        {
            if(imprimir){
                if (lineaAlinea)
                {
                    Console.WriteLine("Pulsa enter para mostrar la siguiente linea: ");
                    Console.ReadLine();
                }

                Console.WriteLine(linea);
            }
        }
        
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
    return contenidoL;
}

void crearArchivo(string ruta)
{
    string carpeta = Path.GetDirectoryName(ruta);
    Directory.CreateDirectory(carpeta);
    try
    {
        if (!File.Exists(ruta))
        {
            using(File.Create(ruta));
            Console.WriteLine("Archivo creado!");
        }
        else
        {
            Console.WriteLine("El archvo ya existia!");
        }
    }
    catch (Exception e)
    {
        Console.WriteLine("Error al crear archivo: " + e.Message);
    }
}

void escribirEnArchivo(String texto, String ruta)
{
    try
    {
        crearArchivo(ruta);
        File.AppendAllText(ruta, texto+"\n");
    }
    catch (Exception e)
    {
        Console.WriteLine("Ha ocurrido un error escribiendo en el archvo: "+ruta + "error:\n"+e);
        throw;
    }
}

Reserva crearReserva()
{
    string terminado = "N";
    string nombre;
    string apellidosPersona = "";
    int edad;
    string dni = "";
    Person persona;
    int nPersonas = 0;
    int asiento;

    string destiny;
    string origin;
    string flightNumber;
    Dictionary<Person, int> personaXasiento =new Dictionary<Person, int>();
    Console.WriteLine("Introduzca el destina de la reserva (en codigo IATA): ");
    destiny = Console.ReadLine();
    Console.WriteLine("Introduzca el origen (en codigo IATA): ");
    origin = Console.ReadLine();
    Console.WriteLine("Inserte el codigo del vuelo: ");
    flightNumber = Console.ReadLine();
    do
    {

        Console.WriteLine("Ingrese el nombre de la persona n"+nPersonas);
        nombre = Console.ReadLine();
        Console.WriteLine("Ingrese los apellidos: ");
        apellidosPersona = Console.ReadLine();
        Console.WriteLine("Ingrese el documento de la persona: ");
        dni = Console.ReadLine();
        Console.WriteLine("Infrese la edad de la persona: ");
        edad = int.Parse(Console.ReadLine());
        Console.WriteLine("Ingrese el numero de asiento: ");
        asiento = int.Parse(Console.ReadLine());
        
        persona = new Person(nombre, apellidosPersona, edad, dni);
        personaXasiento.Add(persona, asiento);

        Console.WriteLine("Hay personas pendientes de entrar en la reserva? (S/N)");
        terminado = Console.ReadLine();
        nPersonas++;


    } while (terminado.Equals("S"));

    Reserva r1 = new Reserva(destiny, origin, flightNumber, personaXasiento);
    escribirEnArchivo(r1.reservaToRegistro()+"\n", "registros/"+r1.destiny+".csv");
    return r1;
}