namespace AirTortilla.code;

public static class Sesion
{
    public static Usuario? UsuarioActual { get; private set; }

    public static void Iniciar(Usuario usuario)
    {
        UsuarioActual = usuario;
    }

    public static void Cerrar()
    {
        UsuarioActual = null;
    }

    public static bool EsAdministrador()
    {
        return UsuarioActual?.TipoUsuario == "administrador";
    }
}
