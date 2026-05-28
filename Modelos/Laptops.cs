using System.ComponentModel.DataAnnotations;

namespace ManPower.Modelos
{
    public class Laptops
    {
        public int Id { get; set; }

        public string Marca { get; set; }

        public string Modelo { get; set; }

 
        public string Serie { get; set; }


        public string? Proveedor { get; set; }

        public string? Observaciones { get; set; }

        public string Estado { get; set; } = "Activo";
    }
}