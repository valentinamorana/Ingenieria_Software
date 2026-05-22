namespace BE
{
    public enum TipoPermiso
    {
        Ninguno = 0,           // valor por defecto para Familia — no usar en IsInRole
        VerPrendas,
        VerStock,
        VerClientes,
        VerPlanSuscripciones,
        VerPedidosVenta,
        VerPedidosRealizados,
        VerUsuarios,
        VerBitacora,
    }
}
