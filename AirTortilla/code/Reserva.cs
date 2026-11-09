using System.Collections;
using System.Globalization;

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
    public Dictionary<Person, int> pXa => this.personaXasiento;
    public DateOnly dt => this.date;
    public string fnmber => this.flightNumber;

    public static Reserva RecReserva(string linea)
    {
        // Formato:
        // AirTortilla_ORIGEN_AÑO_MES_DIA_DESTINO_VUELO
        // (nombre_apellidos_documento_fechaNacimiento_asiento)

        linea = linea.Replace("AirTortilla_", "");

        List<string> partesGrande = linea.Split('(').ToList();
        List<string> datosReserva = partesGrande[0].Split('_').ToList();

        string origin = datosReserva[0];
        string destiny = datosReserva[4];
        string flightnumber = datosReserva[5];

        int año = int.Parse(datosReserva[1]);
        int mes = int.Parse(datosReserva[2]);
        int dia = int.Parse(datosReserva[3]);

        DateOnly date = new DateOnly(año, mes, dia);

        Dictionary<Person, int> personaXasiento = new Dictionary<Person, int>();

        for (int i = 1; i < partesGrande.Count; i++)
        {
            string partePersona = partesGrande[i].TrimEnd(')', '_');

            if (string.IsNullOrWhiteSpace(partePersona))
                continue;

            List<string> datosPersona = partePersona.Split('_').ToList();

            string nombre = datosPersona[0];
            string apellidos = datosPersona[1];
            string documento = datosPersona[2];

            DateOnly fechaNacimiento = DateOnly.ParseExact(
                datosPersona[3],
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            );

            int asiento = int.Parse(datosPersona[4]);

            Person personaBase = new Person(
                nombre,
                apellidos,
                documento,
                fechaNacimiento
            );

            personaXasiento.Add(personaBase, asiento);
        }

        Reserva devolver = new Reserva(
            destiny,
            origin,
            flightnumber,
            personaXasiento,
            date
        );

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
                    + "_" + this.date.Month
                    + "_" + this.date.Day
                    + "_" + this.Origin
                    + "_" + this.flightNumber;

        String mapaTexto = "";

        foreach (var (clave, valor) in this.personaXasiento)
        {
            mapaTexto += "(" + clave.Nombre
                         + "_" + clave.Apellidos
                         + "_" + clave.Documento
                         + "_" + clave.FechaNacimiento.ToString("yyyy-MM-dd")
                         + "_" + valor + ")_";
        }

        registro += mapaTexto;
        return registro;
    }
}
