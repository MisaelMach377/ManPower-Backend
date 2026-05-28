namespace ManPower.Modelos
{
    public class Herramientas
    {
        public int Id { get; set; }

        public string Descripcion { get; set; }

        public string? UnidadMedida { get; set; }

        public string? Familia { get; set; }

        public int Stock { get; set; }

        public decimal Precio { get; set; }

        public string? Categoria { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }
    }
}