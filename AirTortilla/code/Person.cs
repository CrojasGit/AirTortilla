namespace AirTortilla.code;

public class Person
{
    private string nombre;
    private string apellidos;
    private string documento;
    private DateOnly fechaNacimiento;

    public Person(string nombre, string apellidos, string documento, DateOnly fechaNacimiento)
    {
        this.nombre = nombre;
        this.apellidos = apellidos;
        this.documento = documento;
        this.fechaNacimiento = fechaNacimiento;

    }
    public string Nombre => this.nombre;
    public string Apellidos => this.apellidos;
    public string Documento => this.documento;
    public DateOnly FechaNacimiento => this.fechaNacimiento;

}
