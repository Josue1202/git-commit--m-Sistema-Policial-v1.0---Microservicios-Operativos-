namespace Policia.Web.Models
{
    // 1. LA MALETA DE IDA (Lo que envías para entrar)
    public class LoginRequest
    {
        public string Usuario { get; set; } = "";
        public string Password { get; set; } = "";
    }

    // 2. LA MALETA DE VUELTA (Lo que el servidor te responde)
    public class LoginResponse
    {
        public string Mensaje { get; set; } = "";
        public string Token { get; set; } = ""; // <--- ¡AQUÍ LLEGARÁ TU PLACA!
        public string Usuario { get; set; } = "";
        public int IdRol { get; set; }
    }
}
