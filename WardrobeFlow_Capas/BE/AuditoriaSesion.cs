using System;

namespace BE
{
    // T06a - Bitacora: registra inicio/cierre de sesion de cada usuario
    public class AuditoriaSesion
    {
        #region Atributos
        private int idAuditoria;
        private Usuario oUsuario;
        private string descripcionAuditoria;
        private DateTime fechaHora;
        #endregion

        #region Propiedades
        public int IdAuditoria { get { return idAuditoria; } set { idAuditoria = value; } }
        public Usuario OUsuario { get { return oUsuario; } set { oUsuario = value; } }
        public string DescripcionAuditoria { get { return descripcionAuditoria; } set { descripcionAuditoria = value; } }
        public DateTime FechaHora { get { return fechaHora; } set { fechaHora = value; } }
        #endregion
    }
}
