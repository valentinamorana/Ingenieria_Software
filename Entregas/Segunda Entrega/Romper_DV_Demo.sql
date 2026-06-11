/* ============================================================================
   WardrobeFlow - DEMO Digitos Verificadores (T07)
   Objetivo: ROMPER la integridad a mano por SQL y comprobar que la app la DETECTA.
   Base: WardrobeFlowDB   (servidor: .\SQLEXPRESS)

   Tablas protegidas: Usuario, Cliente, Empleado, Pedido
     - DVH (horizontal): un digito por FILA -> detecta MODIFICAR un campo.
     - DVV (vertical):   un digito por TABLA (tabla DVVertical) -> detecta
                         AGREGAR / BORRAR / REORDENAR filas.

   Usuario elegido para la demo:  IdUsuario = 4  ('vendedor')
   Valores ORIGINALES (para revertir):  Perfil = 'Vendedor' , Rol = 'Vendedor'
   ----------------------------------------------------------------------------
   COMO USARLO: ejecutar cada PASO por separado (seleccionar el bloque y F5),
   no todo de una. Leer los comentarios de cada paso.
   ============================================================================ */

USE WardrobeFlowDB;
GO

/* ----------------------------------------------------------------------------
   PASO 0 - FOTO ANTES (mirar el estado actual del usuario elegido)
   ---------------------------------------------------------------------------- */
SELECT IdUsuario, Username, Perfil, Rol, Estado, IntentosFallidos, DVH
FROM   Usuario
WHERE  IdUsuario = 4;
-- Anotar: Perfil='Vendedor', Rol='Vendedor', DVH=583914 (este DVH NO se va a tocar).
GO


/* ============================================================================
   OPCION A  ->  ROMPER EL DVH  (manipulacion horizontal de una fila)
   Caso clasico: "escalar privilegios por la ventana" sin pasar por la app.
   Se cambia Perfil y Rol a Administrador, PERO la columna DVH queda con el
   valor viejo -> al recalcular, el DVH no coincide y salta la alarma.
   ============================================================================ */

-- PASO 1A: romper
UPDATE Usuario
SET    Perfil = 'Administrador',
       Rol    = 'Administrador'
WHERE  IdUsuario = 4;
GO

-- PASO 2A: COMO DETECTARLO
--   Cerrar la app y volver a abrirla (o iniciar sesion). La verificacion de
--   integridad de Usuario corre al arranque -> debe avisar "ALERTA DE INTEGRIDAD".
--   Tambien salta antes de cualquier backup y antes de altas/reset de usuarios.

-- PASO 3A: REVERTIR (deja la base sana SIN recalcular, porque el DVH viejo
--   vuelve a coincidir al restaurar los campos originales)
UPDATE Usuario
SET    Perfil = 'Vendedor',
       Rol    = 'Vendedor'
WHERE  IdUsuario = 4;
GO
-- Verificar que quedo como al principio:
SELECT IdUsuario, Username, Perfil, Rol, DVH FROM Usuario WHERE IdUsuario = 4;
GO


/* ============================================================================
   OPCION B  ->  ROMPER EL DVV  (manipulacion vertical: borrar una fila)
   La suma de los DVH de la tabla ya no da el DVV guardado en DVVertical.
   OJO: revertir un DELETE es mas dificil (hay que re-insertar la fila exacta
   y recalcular). Usar solo si tenes backup, o preferir la Opcion A.
   ============================================================================ */

-- Ver el DVV actual de Usuario antes de romper:
-- SELECT NombreTabla, DVV, FechaCalculo FROM DVVertical WHERE NombreTabla = 'Usuario';

-- PASO 1B: romper (borrar un usuario que NO sea admin; aca uso el id 5 'stock')
-- DELETE FROM Usuario WHERE IdUsuario = 5;
-- GO

-- PASO 2B: detectar -> arrancar la app / login -> alerta de integridad de Usuario.

-- PASO 3B: para dejar sana la base despues de un DELETE:
--   Administrar -> Usuarios -> "Recalcular DV"   (recalcula DVH/DVV de las 4 tablas)
--   o restaurar un backup.


/* ============================================================================
   EXTRA  ->  romper las OTRAS tablas protegidas (se detectan desde
   Administrar -> Diagnostico de Integridad, que revisa las 4 tablas).
   Cada UPDATE toca un campo CUBIERTO por el DVH; descomentar el que quieras.
   ============================================================================ */

-- CLIENTE  (campos DVH: Nombre, Apellido, DNI, Email, MetodoPago)
-- SELECT IdCliente, Nombre, Apellido, DNI, Email, MetodoPago, DVH FROM Cliente;
-- UPDATE Cliente SET Email = 'cambiado@hack.com' WHERE IdCliente = 1;   -- romper
-- (revertir: poner el Email original; el DVH viejo vuelve a coincidir)

-- EMPLEADO (campos DVH: Nombre, Apellido, DNI, Email, Puesto, Legajo)
-- SELECT IdEmpleado, Nombre, Apellido, Puesto, Legajo, DVH FROM Empleado;
-- UPDATE Empleado SET Puesto = 'Gerente' WHERE IdEmpleado = 1;          -- romper

-- PEDIDO   (campos DVH: IdCliente, IdEmpleado, Estado + huella de las lineas)
-- SELECT IdPedido, IdCliente, IdEmpleado, Estado, DVH FROM Pedido;
-- UPDATE Pedido SET Estado = 'Entregado' WHERE IdPedido = 1;            -- cabecera
-- DELETE FROM PedidoPrenda WHERE IdPedido = 1 AND IdPrenda = 7;         -- lineas
-- (revertir cabecera: restaurar el Estado original; el DELETE de lineas
--  requiere re-insertar la linea y luego "Recalcular DV" o restaurar backup)
GO
