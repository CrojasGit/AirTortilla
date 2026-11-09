namespace AirTortilla.code;

public class Usuario
{
    public string Documento { get; }
    public string Nombre { get; }
    public string Apellido1 { get; }
    public string? Apellido2 { get; }
    public string TipoUsuario { get; }

    public Usuario(
        string documento,
        string nombre,
        string apellido1,
        string? apellido2,
        string tipoUsuario)
    {
        Documento = documento;
        Nombre = nombre;
        Apellido1 = apellido1;
        Apellido2 = apellido2;
        TipoUsuario = tipoUsuario;
    }
}
