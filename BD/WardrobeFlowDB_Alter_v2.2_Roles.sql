-- ============================================================
-- WardrobeFlow — Parche v2.2: Alineación de Roles con G04
-- ============================================================
-- Ejecutar sobre WardrobeFlowDB en SSMS.
-- Seguro de re-ejecutar (usa IF NOT EXISTS / IF EXISTS).
--
-- Cambios:
--   1. Vendedor recibe permiso mnuPrendas (M03 — Catálogo de Prendas)
--   2. Usuario "operador" pasa de OperadorLogistico → OperadorDeInventario
--      (rol que corresponde a M05 — Despacho en el documento G04)
--
-- Roles resultantes tras el parche:
--   admin      → Administrador      → TODO (M01-M07)
--   supervisor → Supervisor         → Bitácora (M07)
--   vendedor   → Vendedor           → Prendas, Clientes, Planes, PedidosVenta (M02-M04)
--   stock      → ControladorDeStock → Prendas, Stock (M03)
--   operador   → OperadorDeInventario → Despacho (M05)
-- ============================================================

USE WardrobeFlowDB;
GO

-- ── 1. Agregar mnuPrendas a Vendedor (M03) ──────────────────────
-- El Vendedor necesita ver el catálogo para poder crear pedidos.
IF NOT EXISTS (
    SELECT 1 FROM RolPermiso
    WHERE Rol = 'Vendedor' AND IdPermiso = 3
)
BEGIN
    INSERT INTO RolPermiso (Rol, IdPermiso) VALUES ('Vendedor', 3);
    PRINT '✓ Permiso mnuPrendas (3) agregado al rol Vendedor.';
END
ELSE
    PRINT '-- Vendedor ya tenía mnuPrendas. Sin cambios.';
GO

-- ── 2. Corregir usuario "operador": OperadorLogistico → OperadorDeInventario ──
-- OperadorLogistico no existe en el documento G04.
-- OperadorDeInventario tiene permiso 10 (mnuPedidosRealizados = Despacho) → M05.
IF EXISTS (
    SELECT 1 FROM Usuario
    WHERE Username = 'operador' AND Rol = 'OperadorLogistico'
)
BEGIN
    UPDATE Usuario
    SET Rol    = 'OperadorDeInventario',
        Perfil = 'Operador de Inventario'
    WHERE Username = 'operador';
    PRINT '✓ Usuario "operador" actualizado: OperadorLogistico → OperadorDeInventario.';
END
ELSE
    PRINT '-- Usuario "operador" ya fue corregido o no existe. Sin cambios.';
GO

-- ── Verificación final ───────────────────────────────────────────
PRINT '';
PRINT '=== USUARIOS Y SUS PERMISOS ACTUALES ===';

SELECT
    u.Username,
    u.Rol,
    u.Perfil,
    CASE u.Estado WHEN 1 THEN 'Activo' ELSE 'Bloqueado' END AS Estado,
    STRING_AGG(p.NombreMenu, ', ') WITHIN GROUP (ORDER BY p.IdPermiso) AS Permisos
FROM Usuario u
JOIN RolPermiso rp ON rp.Rol = u.Rol
JOIN Permiso    p  ON p.IdPermiso = rp.IdPermiso
GROUP BY u.Username, u.Rol, u.Perfil, u.Estado
ORDER BY u.IdUsuario;
GO
