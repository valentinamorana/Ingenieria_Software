/* ============================================================================
   03_Permisos_Granulares.sql
   Tier 2 — Permisos de ACCIÓN granular: separar "Ver" de "Configurar".

   Crea una patente de EDICIÓN por cada módulo de negocio (alta/modificación/baja)
   y la propaga ("grandfather") a los roles que hoy pueden editar, de modo que el
   comportamiento NO cambie al aplicar el script. Luego, desde el Gestor de Perfiles,
   el Administrador puede QUITAR la patente de edición a los roles que deban quedar
   de solo-lectura (verán el módulo pero no podrán modificarlo).

   Resolución en runtime: BLL.PermisosAccion. Si estas patentes NO existen, la BLL
   cae al permiso de VER (retrocompatibilidad), así que aplicar el código sin correr
   este script tampoco rompe nada.

   IDEMPOTENTE: se puede ejecutar varias veces sin duplicar filas.
   Reiniciá la aplicación luego de correrlo (el catálogo de patentes se cachea al inicio).
   ============================================================================ */

SET NOCOUNT ON;

DECLARE @map TABLE (VerMenu NVARCHAR(100), EditarMenu NVARCHAR(100), EditarNombre NVARCHAR(200));
INSERT INTO @map (VerMenu, EditarMenu, EditarNombre) VALUES
 ('mnuStock',             'mnuStockEditar',             'Configurar Prendas / Stock'),
 ('mnuClientes',          'mnuClientesEditar',          'Configurar Clientes'),
 ('mnuPlanSuscripciones', 'mnuPlanSuscripcionesEditar', 'Configurar Planes de Suscripción'),
 ('mnuPedidosVenta',      'mnuPedidosVentaEditar',      'Configurar Pedidos de Venta'),
 ('mnuPedidosRealizados', 'mnuPedidosRealizadosEditar', 'Configurar Pedidos Realizados');

-- 1) Crear las patentes de EDICIÓN que falten (EsFamilia=0, EsRol=0 → hojas / patentes).
INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado, EsFamilia, EsRol)
SELECT m.EditarNombre, m.EditarMenu, 'Acción', 1, 0, 0
FROM   @map m
WHERE  EXISTS     (SELECT 1 FROM Permiso v WHERE v.NombreMenu = m.VerMenu)
  AND  NOT EXISTS (SELECT 1 FROM Permiso e WHERE e.NombreMenu = m.EditarMenu);

-- 2) "Grandfather": donde una patente de VER esté asignada DIRECTAMENTE a un rol/familia,
--    asignar también su patente de EDITAR (preserva el comportamiento actual).
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT r.IdPadre, e.IdPermiso
FROM   PermisoRelacion r
JOIN   Permiso v ON v.IdPermiso  = r.IdHijo
JOIN   @map    m ON m.VerMenu    = v.NombreMenu
JOIN   Permiso e ON e.NombreMenu = m.EditarMenu
WHERE  NOT EXISTS (SELECT 1 FROM PermisoRelacion r2
                   WHERE r2.IdPadre = r.IdPadre AND r2.IdHijo = e.IdPermiso);

PRINT 'OK: patentes de acción creadas y propagadas (idempotente).';

/* ----------------------------------------------------------------------------
   OPCIONAL — ocultar también los botones de edición en la GUI para los usuarios
   sin la patente de edición. WardrobeFlow ya tiene el mecanismo data-driven
   (ControlMapeado + ManejadorSeguridad): basta mapear los botones Guardar/Eliminar
   de cada formulario a su patente "…Editar" desde la pantalla "Mapeo de Controles".
   La re-validación de backend (BLL.PermisosAccion) ya protege la operación aunque
   el botón no se oculte.
   ---------------------------------------------------------------------------- */
