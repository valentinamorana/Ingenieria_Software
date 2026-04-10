using System;
using System.Collections.Generic;
using BE;

namespace Seguridad
{
    public class BLL_AuditoriaSesion
    {
        private DAL_AuditoriaSesion _dal = new DAL_AuditoriaSesion();

        public void RegistrarAuditoriaSesion(AuditoriaSesion a)
        {
            try { _dal.RegistrarAuditoriaSesion(a); }
            catch (Exception ex) { throw new Exception("Error al registrar auditoria: " + ex.Message); }
        }

        public List<AuditoriaSesion> ListarAuditorias()
        {
            try { return _dal.ListarAuditorias(); }
            catch (Exception ex) { throw new Exception("Error al listar auditorias: " + ex.Message); }
        }
    }
}
