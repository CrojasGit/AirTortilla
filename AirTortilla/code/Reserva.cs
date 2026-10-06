using System.Collections;

namespace AirTortilla.code;

public class Reserva
{
    public string destiny;
    public string Origin;
    private string flightNumber;
    private Dictionary<Person, int> personaXasiento =new Dictionary<Person, int>();
    private DateOnly date;

    public Reserva(string destiny, string origin, string flightNumber, Dictionary<Person, int> personaXasiento)
    {
        this.destiny = destiny;
        this.Origin = origin;
        this.flightNumber = flightNumber;
        this.date = DateOnly.FromDateTime(DateTime.Now);
        this.personaXasiento = personaXasiento;
    }

    public String reservaToRegistro()
    {
        String registro = "";
        registro += "AirTortilla_"
                    + this.destiny
                    + "_" + this.date.Year
                    + "_" + this.date.Month +
                    "_" + this.date.Day +
                    "_" + this.Origin+"_";
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