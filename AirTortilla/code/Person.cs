namespace AirTortilla.code;

public class Person
{
    private string nombre;
    private string apellidos;
    private int edad;
    private string documento;

    public Person(string nombre, string apellidos, int edad, string documento)
    {
        this.nombre = nombre;
        this.apellidos = apellidos;
        this.edad = edad;
        this.documento = documento;
        
    }
    public string Nombre => this.nombre;
    public string Apellidos => this.apellidos;
    public int Edad => this.edad;
    public string Documento => this.documento;
    
}