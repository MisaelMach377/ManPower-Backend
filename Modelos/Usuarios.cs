namespace ManPower.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; } 

        public string TipoDocumento { get; set; } 
        public string NumeroDocumento { get; set; } 

        public string Correo { get; set; } 

        public string? Celular { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaActualizacion { get; set; }
    }
}