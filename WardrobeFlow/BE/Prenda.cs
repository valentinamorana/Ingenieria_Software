using System;

namespace BE
{
    /// <summary>
    /// Entidad — Prenda (unidad de stock del catálogo).
    /// Mapea la tabla [Prenda].
    /// </summary>
    public class Prenda
    {
        public int          IdPrenda        { get; set; }
        public string       Nombre          { get; set; }
        public string       Descripcion     { get; set; }
        public string       Talle           { get; set; }
        public string       Color           { get; set; }
        public string       Categoria       { get; set; }
        public EstadoPrenda Estado          { get; set; } = EstadoPrenda.Disponible;

        /// <summary>FK → Cliente. Solo aplica cuando Estado = EnUso.</summary>
        public int?         IdClienteActual { get; set; }

        /// <summary>Nombre del cliente que la tiene (cargado por JOIN, no persiste).</summary>
        public string       NombreCliente   { get; set; }

        public DateTime     FechaAlta       { get; set; }

        /// <summary>Descripción para mostrar en grillas: "Vestido azul – en uso"</summary>
        public string ResumenEstado =>
            Estado == EstadoPrenda.EnUso && !string.IsNullOrEmpty(NombreCliente)
                ? $"{Nombre} — en uso ({NombreCliente})"
                : $"{Nombre} — {Estado}";
    }
}
