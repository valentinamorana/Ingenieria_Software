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

        // Identifica al Administrador del sistema en UN SOLO lugar (flag), en vez de repetir la
        // comparación de strings dispersa por SessionManager, BLL y GUI. Considera tanto Perfil
        // como Rol, es case-insensitive y null-safe. Si el modelo migrara a un Id/columna de BD,
        // este es el único punto a cambiar.
        public bool EsAdministrador =>
            Roles.EsAdministrador(Perfil) || Roles.EsAdministrador(Rol);

        public List<Permiso> Permisos { get; set; } = new List<Permiso>();

        public bool Bloqueado { get; set; }

        public int IntentosFallidos { get; set; }

        // ── Bloqueo de login PROGRESIVO ─────────────────────────────────────────
        // CantidadBloqueos: cuántas veces se bloqueó la cuenta (define la duración del bloqueo:
        // 1/5/15/60 min). FechaBloqueo: instante del último bloqueo (para saber cuándo expira);
        // null = no bloqueada por tiempo. No forman parte del DVH (metadata operativa).
        public int CantidadBloqueos { get; set; }

        public DateTime? FechaBloqueo { get; set; }

        public string IdIdioma { get; set; } = "ES";

        // ── RF-10 — Baja lógica (archivado) ─────────────────────────────────────
        // Activo=false → usuario archivado: no puede iniciar sesión ni aparece en la
        // lista de gestión. FechaBaja registra cuándo se archivó (para la purga >1 año).
        public bool Activo { get; set; } = true;

        public DateTime? FechaBaja { get; set; }

        // Cambio de clave OBLIGATORIO: true cuando la cuenta tiene una clave temporal/generada
        // (alta nueva o reset) que el usuario todavía no reemplazó. El login fuerza el cambio
        // antes de abrir el menú. No forma parte del DVH (metadata operativa).
        public bool RequiereCambioClave { get; set; }

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
