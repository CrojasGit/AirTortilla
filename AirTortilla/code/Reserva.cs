using System.Collections;

namespace AirTortilla.code;

public class Reserva
{
    public string destiny;
    public string Origin;
    private string flightNumber;
    private Dictionary<Person, int> personaXasiento =new Dictionary<Person, int>();
    private DateOnly date;

    public Reserva(string destiny, string origin, string flightNumber, Dictionary<Person, int> personaXasiento, DateOnly date)
    {
        this.destiny = destiny;
        this.Origin = origin;
        this.flightNumber = flightNumber;
        this.date = date;
        this.personaXasiento = personaXasiento;
    }

    public static Reserva RecReserva(string linea)
    {
        //AirTortilla_ES_2026_10_7_FR_999(nombre0_apellidos0_documento0_22_4)_(persona1_apellidos1_fg204_18_4)_
        linea = linea.Replace("AirTortilla_", "");
        linea = linea.Replace(")","");
        List<string> partesGrande = linea.Split('(').ToList();
        List<string> datosReserva = partesGrande[0].Split('_').ToList();
        string destino;
        string origin;
        string flightnumber;
        int dia;
        int mes;
        int año;
        DateOnly date;
        Dictionary<Person, int> personaXasiento = new Dictionary<Person, int>();
        
        destino = datosReserva[0];
        origin = datosReserva[4];
        flightnumber = datosReserva[5];
        dia = int.Parse(datosReserva[3]);
        mes = int.Parse(datosReserva[2]);
        año = int.Parse(datosReserva[1]);
        date = new DateOnly(año, mes, dia);
        
        for (int i = 1; i < partesGrande.Count; i++)
        {
            Person personaBase;
            string nombre;
            string apellidos;
            string documento;
            int edad;
            int asiento;
            List<string> datosPersona = partesGrande[i].Split('_').ToList();
            
            nombre =  datosPersona[0];
            apellidos = datosPersona[1];
            documento = datosPersona[2];
            edad = int.Parse(datosPersona[3]);
            asiento = int.Parse(datosPersona[4]);
            personaBase = new Person(nombre, apellidos, edad, documento);
            personaXasiento.Add(personaBase, asiento);
        }
        Reserva devolver = new  Reserva(destino, origin, flightnumber, personaXasiento, date);
        return devolver;
    }

    public DateOnly getDate()
    {
        return this.date;
    }
    public String reservaToRegistro()
    {
        String registro = "";
        registro += "AirTortilla_"
                    + this.destiny
                    + "_" + this.date.Year
                    + "_" + this.date.Month +
                    "_" + this.date.Day +
                    "_" + this.Origin+
                    "_"+this.flightNumber;
        
        String mapaTexto = "";
        foreach (var (clave, valor) in this.personaXasiento)
        {
            mapaTexto += "("+clave.Nombre+
                         "_"+clave.Apellidos+
                         "_"+clave.Documento+
                         "_"+clave.Edad+
                         "_"+valor+")_";
        }
        registro+=mapaTexto;
        return registro;
    }
}
