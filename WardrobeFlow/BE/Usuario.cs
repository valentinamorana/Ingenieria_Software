using System;
using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Capa de Entidades — Usuario del sistema (empleado).
    /// Mapea la tabla [Usuario] de WardrobeFlowDB.
    ///
    /// Columnas:
    ///   Id         int          PK (columna IdUsuario en BD, alias "Id")
    ///   Username   varchar      nombre de acceso único
    ///   Contraseña varchar      hash PBKDF2-SHA256 — NUNCA texto plano
    ///   Perfil     varchar      rol del empleado (ej: "Administrador")
    ///
    /// La lista Permisos se carga en memoria tras el login (no persiste en esta tabla).
    /// </summary>
    public class Usuario : Memento.IOriginator
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Contraseña { get; set; }

        public string Perfil { get; set; }

        public string Rol { get; set; }

        public List<Permiso> Permisos { get; set; } = new List<Permiso>();

        public bool Bloqueado { get; set; }

        public int IntentosFallidos { get; set; }

        public string IdIdioma { get; set; } = "ES";

        // ── Patrón MEMENTO — rol Originator ─────────────────────────────────────

        /// <summary>
        /// Captura el estado restaurable del usuario (clave, estado de bloqueo e
        /// intentos) en un Memento concreto (BE.VersionUsuario).
        /// </summary>
        public Memento.IMemento CrearMemento(string actor, string detalle)
        {
            return new VersionUsuario
            {
                IdUsuario        = this.Id,
                Fecha            = DateTime.Now,
                Actor            = actor,
                Detalle          = detalle,
                UsernameSnapshot = this.Username,
                ClaveSnapshot    = this.Contraseña,
                EstadoSnapshot   = !this.Bloqueado,   // true = activo
                IntentosSnapshot = this.IntentosFallidos
            };
        }

        /// <summary>
        /// Restaura el estado del usuario a partir de un Memento previo.
        /// Solo el Originator conoce qué campos del Memento aplicar.
        /// </summary>
        public void RestaurarDesde(Memento.IMemento memento)
        {
            var v = memento as VersionUsuario;
            if (v == null)
                throw new InvalidOperationException(
                    "El memento recibido no corresponde a un Usuario.");

            this.Contraseña       = v.ClaveSnapshot;
            this.Bloqueado        = !v.EstadoSnapshot;
            this.IntentosFallidos = v.IntentosSnapshot;
        }
    }
}
