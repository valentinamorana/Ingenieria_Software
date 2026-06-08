using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Etapa 4 (permisos a nivel de control).
    /// Expone los mapeos control↔patente a la GUI (respetando el orden de capas: GUI → BLL → DAL).
    /// </summary>
    public class ControlMapeado
    {
        private readonly DAL.Interfaces.IControlMapeadoDAL _dal;
        private Servicios.Bitacora _bitacoraLazy;
        private Servicios.Bitacora _bitacora => _bitacoraLazy ?? (_bitacoraLazy = new Servicios.Bitacora());

        public ControlMapeado() : this(new DAL.ControlMapeado()) { }
        public ControlMapeado(DAL.Interfaces.IControlMapeadoDAL dal) { _dal = dal; }

        public List<BE.ControlMapeado> ObtenerTodos() => _dal.ObtenerTodos();

        public List<BE.ControlMapeado> ObtenerPorPermiso(int idPermiso) => _dal.ObtenerPorPermiso(idPermiso);

        // Reemplaza el conjunto de controles asociados a una patente. Solo Administrador.
        public void GuardarAsociados(int idPermiso, List<BE.ControlMapeado> controles)
        {
            VerificarAdmin();
            _dal.GuardarAsociados(idPermiso, controles);
            _bitacora.Registrar("Gestión de Perfiles",
                $"Mapeo de controles actualizado para la patente {idPermiso}: {controles?.Count ?? 0} control(es).",
                BE.Criticidad.Alta);
        }

        private static void VerificarAdmin()
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada", "La sesión expiró. Volvé a iniciar sesión.");
            string perfil = Seguridad.SessionManager.GetInstance().Usuario.Perfil ?? "";
            if (!perfil.Equals(BE.Roles.Administrador, StringComparison.OrdinalIgnoreCase))
                throw new BE.AppException("err.bll.familia.sin_permiso",
                    "Solo un Administrador puede modificar el mapeo de controles.");
        }
    }
}
