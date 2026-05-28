namespace ManPower.Modelos
{
    public class Asignaciones
    {
        public int Id { get; set; }

        // Usuario
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // Tipo de herramienta
        public string TipoHerramienta { get; set; } // CELULAR / LAPTOP

        // Celular
        public int? CelularId { get; set; }
        public Celulares? Celular { get; set; }

        // Laptop
        public int? LaptopId { get; set; }
        public Laptops? Laptop { get; set; }

        // Datos adicionales
        public string? NumeroGuia { get; set; }
        public string? Zona { get; set; }

        public string Estado { get; set; } = "ACTIVO";

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public string? Observaciones { get; set; }
    }
}