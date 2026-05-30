-- ============================================================
-- Migracion_JerarquiaRoles.sql
-- T04 — Jerarquía de Roles (Composite de Roles + Áreas)
--
-- Ejecutar UNA VEZ sobre WardrobeFlowDB.
-- Idempotente: puede ejecutarse varias veces sin errores.
--
-- Agrega:
--   1. Tabla [RolJerarquia] — árbol de roles por área de negocio
--   2. Nuevos roles de negocio en [RolPermiso]
--
-- Nueva estructura de roles:
--
--   Administración
--     └── Administrador        (todos los permisos)
--
--   Auditoría
--     └── Auditor              (ver bitácora — reemplaza Supervisor)
--
--   Comercial
--     └── Gerente Comercial    (visión completa de ventas e inventario)
--         └── Vendedor         (ABM clientes, planes y pedidos de venta)
--
--   Inventario
--     └── Gerente de Inventario (visión completa de stock y logística)
--         ├── Encargado de Stock  (ABM prendas y stock)
--         └── Operador Logístico  (prendas y despacho/entrega)
-- ============================================================

USE WardrobeFlowDB;
GO

-- ============================================================
-- 1. TABLA RolJerarquia
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RolJerarquia')
BEGIN
    CREATE TABLE RolJerarquia (
        RolHijo   NVARCHAR(100) NOT NULL,
        RolPadre  NVARCHAR(100) NULL,
        Nivel     INT           NOT NULL DEFAULT 0,
        Area      NVARCHAR(100) NOT NULL DEFAULT 'General',
        CONSTRAINT PK_RolJerarquia PRIMARY KEY (RolHijo)
    );
    PRINT 'Tabla RolJerarquia creada.';
END
ELSE
    PRINT 'Tabla RolJerarquia ya existe — sin cambios en schema.';
GO

-- ============================================================
-- 2. INSERTAR JERARQUÍA DE ROLES (idempotente)
-- ============================================================

MERGE RolJerarquia AS target
USING (VALUES
    -- Administración
    ('Administrador',       NULL,                   0, N'Administración'),
    -- Auditoría
    ('Auditor',             NULL,                   0, N'Auditoría'),
    ('Supervisor',          NULL,                   0, N'Auditoría'),       -- legacy
    -- Comercial
    ('GerenteComercial',    NULL,                   1, N'Comercial'),
    ('Vendedor',            'GerenteComercial',     2, N'Comercial'),
    -- Inventario
    ('GerenteInventario',   NULL,                   1, N'Inventario'),
    ('EncargadoDeStock',    'GerenteInventario',    2, N'Inventario'),
    ('ControladorDeStock',  'GerenteInventario',    2, N'Inventario'),      -- legacy
    ('OperadorLogistico',   'GerenteInventario',    2, N'Inventario'),
    ('OperadorDeInventario','GerenteInventario',    2, N'Inventario')       -- legacy
) AS source (RolHijo, RolPadre, Nivel, Area)
ON target.RolHijo = source.RolHijo
WHEN NOT MATCHED THEN
    INSERT (RolHijo, RolPadre, Nivel, Area)
    VALUES (source.RolHijo, source.RolPadre, source.Nivel, source.Area);
PRINT 'Jerarquía de roles sincronizada.';
GO

-- ============================================================
-- 3. PERMISOS PARA NUEVOS ROLES
-- ============================================================

-- Auditor: mismos permisos que Supervisor (mnuAuditoria)
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'Auditor', rp.IdPermiso
FROM RolPermiso rp
WHERE rp.Rol = 'Supervisor'
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'Auditor' AND x.IdPermiso = rp.IdPermiso
  );
PRINT 'Permisos Auditor insertados.';
GO

-- GerenteComercial: prendas + clientes + planes + pedidos venta + pedidos realizados
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'GerenteComercial', p.IdPermiso
FROM Permiso p
WHERE p.NombreMenu IN ('mnuPrendas','mnuClientes','mnuPlanSuscripciones','mnuPedidosVenta','mnuPedidosRealizados')
  AND ISNULL(p.EsFamilia, 0) = 0
  AND p.Estado = 1
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'GerenteComercial' AND x.IdPermiso = p.IdPermiso
  );
PRINT 'Permisos GerenteComercial insertados.';
GO

-- GerenteInventario: prendas + stock + pedidos realizados
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'GerenteInventario', p.IdPermiso
FROM Permiso p
WHERE p.NombreMenu IN ('mnuPrendas','mnuStock','mnuPedidosRealizados')
  AND ISNULL(p.EsFamilia, 0) = 0
  AND p.Estado = 1
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'GerenteInventario' AND x.IdPermiso = p.IdPermiso
  );
PRINT 'Permisos GerenteInventario insertados.';
GO

-- EncargadoDeStock: mismos permisos que ControladorDeStock (prendas + stock)
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'EncargadoDeStock', rp.IdPermiso
FROM RolPermiso rp
WHERE rp.Rol = 'ControladorDeStock'
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'EncargadoDeStock' AND x.IdPermiso = rp.IdPermiso
  );
PRINT 'Permisos EncargadoDeStock insertados.';
GO

-- OperadorLogistico: prendas + pedidos realizados (gestión física y despacho)
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'OperadorLogistico', p.IdPermiso
FROM Permiso p
WHERE p.NombreMenu IN ('mnuPrendas','mnuPedidosRealizados')
  AND ISNULL(p.EsFamilia, 0) = 0
  AND p.Estado = 1
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'OperadorLogistico' AND x.IdPermiso = p.IdPermiso
  );
PRINT 'Permisos OperadorLogistico insertados (si no existían).';
GO

PRINT '';
PRINT '=== Migración de Jerarquía de Roles completada. ===';
PRINT 'Nueva estructura:';
PRINT '  Administración: Administrador';
PRINT '  Auditoría:      Auditor (nuevo), Supervisor (legacy)';
PRINT '  Comercial:      GerenteComercial > Vendedor';
PRINT '  Inventario:     GerenteInventario > EncargadoDeStock / OperadorLogistico';
PRINT '';
PRINT 'Los roles legacy (Supervisor, ControladorDeStock, OperadorDeInventario)';
PRINT 'siguen funcionando sin cambios.';
GO
