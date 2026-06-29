namespace BE
{
    /// <summary>
    /// Nombres de las patentes (permisos simples) del sistema, tal como figuran en
    /// la columna [Permiso].NombreMenu. Centraliza los literales usados por los guards
    /// de la BLL y por el menú, para evitar errores de tipeo.
    /// </summary>
    public static class Patentes
    {
        public const string Usuarios          = "mnuUsuarios";
        public const string Auditoria         = "mnuAuditoria";
        public const string Prendas           = "mnuPrendas";
        public const string Stock             = "mnuStock";
        public const string Clientes          = "mnuClientes";
        public const string PlanSuscripciones = "mnuPlanSuscripciones";
        public const string PedidosVenta      = "mnuPedidosVenta";
        public const string PedidosRealizados = "mnuPedidosRealizados";

        // ── Patentes de ACCIÓN granular ("Configurar") — separan VER de EDITAR ───────
        // Cada una gobierna las operaciones de escritura (alta/modificación/baja) del módulo.
        // Convención: <patente de ver> + "Editar". Si la patente no existe en el catálogo,
        // BLL.PermisosAccion cae al permiso de VER (retrocompatibilidad — ver PermisosAccion).
        public const string StockEditar             = "mnuStockEditar";
        public const string ClientesEditar          = "mnuClientesEditar";
        public const string PlanSuscripcionesEditar = "mnuPlanSuscripcionesEditar";
        public const string PedidosVentaEditar      = "mnuPedidosVentaEditar";
        public const string PedidosRealizadosEditar = "mnuPedidosRealizadosEditar";
    }
}
