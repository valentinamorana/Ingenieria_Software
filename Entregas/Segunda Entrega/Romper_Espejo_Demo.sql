/* ============================================================================
   WardrobeFlow - DEMO Recuperacion por TABLA ESPEJO (T07)
   Objetivo: romper la integridad de Usuario de distintas formas y ver como la
             CONSOLA DE RECUPERACION (Espejo) las diagnostica y repara.
   Base: WardrobeFlowDB   (servidor: .\SQLEXPRESS)

   Que es el espejo:
     Usuario_Seguridad = copia sombra de los campos del DVH de cada usuario
     (Id, Username, Clave, Rol, Perfil, Estado, IntentosFallidos + DVH).
     La app la mantiene al dia con cada escritura LEGITIMA. Ante manipulacion,
     permite (1) saber QUE campo cambio y (2) RESTAURAR el valor legitimo.

   Donde se ve / repara:
     - App logueada:  Administrar -> Diagnostico de Integridad -> "Recuperacion (Espejo)..."
     - App bloqueada: pantalla de arranque -> "Recuperacion Asistida..."
   Tres botones, habilitados segun el dano:
     - Reparar desde Espejo  : revierte campos modificados y borra inserciones externas.
     - Asumir Perdida        : acepta los datos actuales y recalcula todos los DV.
     - Restaurar Backup      : cuando el espejo no alcanza (vaciado / borrado fisico).

   COMO USARLO: ejecutar cada PASO por separado (seleccionar el bloque y F5).
   ============================================================================ */

USE WardrobeFlowDB;
GO

/* ----------------------------------------------------------------------------
   PASO 0 - FOTO ANTES: comparar la tabla real contra el espejo.
   Si todo esta sano, ambas filas deben coincidir campo a campo.
   ---------------------------------------------------------------------------- */
SELECT 'REAL'   AS Origen, IdUsuario, Username, Perfil, Rol, Estado, IntentosFallidos, DVH
FROM   Usuario WHERE IdUsuario = 4
UNION ALL
SELECT 'ESPEJO' AS Origen, IdUsuario, Username, Perfil, Rol, Estado, IntentosFallidos, DVH
FROM   Usuario_Seguridad WHERE IdUsuario = 4;
GO


/* ============================================================================
   CASO 1  ->  MODIFICACION EXTERNA de un campo (escalar privilegios por SQL)
   Se cambia Perfil/Rol a Administrador SIN pasar por la app. El DVH queda viejo.
   Diagnostico esperado en la consola:  Tipo "Modificada",
     Campo [Perfil] Actual 'Administrador' / Esperado 'Vendedor'  (idem Rol).
   Recuperacion: "Reparar desde Espejo" revierte Perfil/Rol a 'Vendedor'.
   ============================================================================ */

-- PASO 1.1: romper
UPDATE Usuario SET Perfil = 'Administrador', Rol = 'Administrador' WHERE IdUsuario = 4;
GO
-- PASO 1.2: abrir la app -> queda bloqueada -> "Recuperacion Asistida..." -> "Reparar desde Espejo".
--           (o, si preferis dejarla sana por SQL para repetir la demo, correr el PASO 1.3)
-- PASO 1.3 (opcional, revertir a mano): el DVH viejo vuelve a coincidir al restaurar el valor.
-- UPDATE Usuario SET Perfil = 'Vendedor', Rol = 'Vendedor' WHERE IdUsuario = 4;
-- GO


/* ============================================================================
   CASO 2  ->  SOLO el DVH corrompido a mano (datos OK, digito cambiado)
   Diagnostico esperado: Tipo "DVH corrupto" (los datos coinciden con el espejo
   pero el DVH almacenado no recalcula).  Recuperacion: "Reparar desde Espejo".
   ============================================================================ */

-- PASO 2.1: romper (poner un DVH cualquiera, distinto del real)
-- UPDATE Usuario SET DVH = 123456 WHERE IdUsuario = 4;
-- GO


/* ============================================================================
   CASO 3  ->  INSERCION EXTERNA (alta de un usuario directo por SQL)
   La fila no existe en el espejo. Diagnostico esperado: Tipo "Insercion externa".
   Recuperacion: "Reparar desde Espejo" ELIMINA esa fila (no estaba en el espejo).
   ============================================================================ */

-- PASO 3.1: romper (insertar un admin trucho)
-- INSERT INTO Usuario (Username, Clave, Rol, Perfil, Estado, IntentosFallidos, DVH)
-- VALUES ('hacker', 'x', 'Administrador', 'Administrador', 1, 0, 0);
-- GO
-- PASO 3.2: consola -> "Reparar desde Espejo" -> la fila 'hacker' desaparece.


/* ============================================================================
   CASO 4  ->  ELIMINACION FISICA de una fila (borrado por SQL)
   La fila esta en el espejo pero no en la tabla. Diagnostico: Tipo "Eliminada".
   Recuperacion: "Reparar desde Espejo" queda DESHABILITADO (no se reinsertan
   filas por IDENTITY/FK); habilita "Asumir Perdida" o "Restaurar Backup".
   ============================================================================ */

-- PASO 4.1: romper (borrar un usuario NO admin; aca id 5)
-- DELETE FROM Usuario WHERE IdUsuario = 5;
-- GO
-- PASO 4.2: consola -> el espejo lista la fila faltante; usar "Restaurar Backup".


/* ============================================================================
   UTIL  ->  re-sincronizar el espejo a mano (equivale a "Asumir Perdida"/recalcular)
   Solo si queres resetear la demo dejando el espejo == estado actual.
   ============================================================================ */
-- TRUNCATE TABLE Usuario_Seguridad;
-- INSERT INTO Usuario_Seguridad (IdUsuario, Username, Clave, Rol, Perfil, Estado, IntentosFallidos, DVH, FechaActualizacion)
-- SELECT IdUsuario, Username, Clave, Rol, Perfil, Estado, IntentosFallidos, DVH, GETDATE() FROM Usuario;
-- GO
