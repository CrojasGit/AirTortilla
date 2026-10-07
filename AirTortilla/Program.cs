// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.AccessControl;
using AirTortilla.code;

//Console.WriteLine("---CREAR RESERVA---");
//Reserva r1=crearReserva();
//Console.WriteLine(r1.reservaToRegistro());
//Console.WriteLine("---LEER RESERVA---");

//string[] contenido = leerArchivo("registros/"+r1.destiny+".csv", true, true);
//Console.WriteLine("Ingrese el contenido a buscar dentro de la ultima reserva");
//List<string> filtrado = busqueda(Console.ReadLine(), contenido);
//foreach (string linea in filtrado)
//{
  //  Console.WriteLine(linea);
//}
  menu();

List<string> seleccionarRegistro()
{
    Console.WriteLine("Ingrese el numero del registro a seleccionar (-1 para todos)");
    List<string> devolver = new List<string>();
    string[] registros= Directory.GetFiles("registros", "*.csv");
    for (int i = 0; i < registros.Length; i++)
    {
        Console.WriteLine(i+" - "+registros[i]);
    }

    int opt = int.Parse(Console.ReadLine());
    if (opt !=-1)
    {
        devolver.Add(registros[opt]);
        return devolver;
    }

    return registros.ToList();
}



void menu()
{
    List<String> opciones = ["1 : CREAR RESERVA","2 : LEER REGISTRO","3 : BUSCAR EN REGISTRO","4 : SALIR"];
    
    
    
    while(true){
        foreach (string opcion in opciones)
        {
            Console.WriteLine(opcion);
        }
        Console.WriteLine("\nInserte la opcion deseada: ");
        
            
        
        
        switch (int.Parse(Console.ReadLine()))
        {
            case 1:
                Console.WriteLine("\n---CREAR RESERVA---\n");
                Reserva r1 = crearReserva();
                Console.WriteLine(r1.reservaToRegistro());
                break;
            case 2:
                Console.WriteLine("\n---LEER REGISTRO---\n");
                List<string> archivos = seleccionarRegistro();
                Console.WriteLine("Desea leer linea a linea? (S/N)");
                Boolean siono = Console.ReadLine().ToUpper() == "S";
                foreach (string registro in archivos)
                {
                    leerArchivo(new List<string> { registro }, siono, true);
                }
                break;
            case 3:
                archivos = seleccionarRegistro();
                List<string[]> reg = leerArchivo(archivos, false, false).ToList();
                Console.WriteLine("Ingrese el texto a buscar");
                string texto = Console.ReadLine();
                foreach (string[] cont in reg){
                    foreach (string registro in cont)
                    {
                        List<string> filtrado = busqueda(texto, cont.ToArray()).Distinct().ToList();
                        foreach (string linea in filtrado)
                        {
                            Console.WriteLine("Encontrado: ");
                            Console.WriteLine(linea);
                        }
                    }}
                break;
            case 4:
                Console.WriteLine("Adios!");
                return;
                break;
            default:
                Console.WriteLine("Opcion incorrecta, vuelvalo a intentar...");
                break;
                
        }
    }
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

List<string[]> leerArchivo(List<string> rutas, Boolean lineaAlinea, Boolean imprimir)
{
    List<string[]> devolver = new List<string[]>();
    
    List<string> contenidoL= new List<string>();
    
    foreach (string ruta in rutas)
    {
        try
        {  
            contenidoL = File.ReadAllLines(ruta).ToList();
            devolver.Add(contenidoL.ToArray());
            foreach (string linea in contenidoL)
            {
                if(imprimir){
                    if (lineaAlinea)
                    {
                        Console.WriteLine("Pulsa enter para continuar: \n");
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
    }
    return devolver;
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