-- ============================================================
-- WardrobeFlowDB — Parche: rellenar columna usuario en Bitácora
-- para registros de login fallido y bloqueo creados antes del fix.
--
-- PROBLEMA: antes del fix en BLL/Usuario.cs, los eventos
--   "Intento Fallido Login" y "Bloqueo de Cuenta" se grababan
--   con usuario = NULL aunque el usuario existía en la BD.
--
-- SOLUCIÓN: extraer el username del campo detalle y
--   hacer JOIN con Usuario para recuperar el IdUsuario correcto.
--
-- FORMATOS conocidos en detalle:
--   Intento fallido: "Intento fallido #X/3 para 'USERNAME' (ID: ?) a las ..."
--   Bloqueo        : "Cuenta 'USERNAME' (ID: ?) bloqueada automáticamente ..."
--
-- CONDICIÓN de seguridad: solo toca filas donde usuario IS NULL
--   y el detalle contiene '(ID: ?)' (marca del bug, no del fix).
-- ============================================================

USE WardrobeFlowDB;
GO

-- ── Vista previa: filas que se van a actualizar ───────────────
SELECT
    b.Id,
    b.fecha,
    b.actividad,
    b.detalle,
    b.usuario AS usuario_actual,
    u.IdUsuario AS usuario_nuevo
FROM Bitacora b
JOIN Usuario u ON u.Username =
    CASE
        WHEN b.actividad = 'Intento Fallido Login' THEN
            SUBSTRING(
                b.detalle,
                CHARINDEX('para ''', b.detalle) + 6,
                CHARINDEX(''' (ID:', b.detalle)
                    - CHARINDEX('para ''', b.detalle) - 6
            )
        WHEN b.actividad = 'Bloqueo de Cuenta' THEN
            SUBSTRING(
                b.detalle,
                CHARINDEX('Cuenta ''', b.detalle) + 8,
                CHARINDEX(''' (ID:', b.detalle)
                    - CHARINDEX('Cuenta ''', b.detalle) - 8
            )
    END
WHERE b.usuario IS NULL
  AND b.actividad IN ('Intento Fallido Login', 'Bloqueo de Cuenta')
  AND b.detalle LIKE '%(ID: ?)%';   -- solo los del bug, no los del fix
GO

-- ── UPDATE: Intento Fallido Login ─────────────────────────────
UPDATE b
SET    b.usuario = u.IdUsuario
FROM   Bitacora b
JOIN   Usuario  u ON u.Username =
    SUBSTRING(
        b.detalle,
        CHARINDEX('para ''', b.detalle) + 6,
        CHARINDEX(''' (ID:', b.detalle)
            - CHARINDEX('para ''', b.detalle) - 6
    )
WHERE  b.usuario   IS NULL
  AND  b.actividad  = 'Intento Fallido Login'
  AND  b.detalle   LIKE '%(ID: ?)%';

PRINT CONCAT(@@ROWCOUNT, ' fila(s) actualizadas — Intento Fallido Login');
GO

-- ── UPDATE: Bloqueo de Cuenta ─────────────────────────────────
UPDATE b
SET    b.usuario = u.IdUsuario
FROM   Bitacora b
JOIN   Usuario  u ON u.Username =
    SUBSTRING(
        b.detalle,
        CHARINDEX('Cuenta ''', b.detalle) + 8,
        CHARINDEX(''' (ID:', b.detalle)
            - CHARINDEX('Cuenta ''', b.detalle) - 8
    )
WHERE  b.usuario   IS NULL
  AND  b.actividad  = 'Bloqueo de Cuenta'
  AND  b.detalle   LIKE '%(ID: ?)%';

PRINT CONCAT(@@ROWCOUNT, ' fila(s) actualizadas — Bloqueo de Cuenta');
GO

-- ── Verificación final ────────────────────────────────────────
SELECT 'Quedan sin usuario' AS descripcion,
       COUNT(*) AS total
FROM   Bitacora
WHERE  usuario   IS NULL
  AND  actividad IN ('Intento Fallido Login', 'Bloqueo de Cuenta');
GO
