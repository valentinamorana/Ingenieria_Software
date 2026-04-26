-- ============================================================
-- WardrobeFlowDB — Parche: rellenar columna usuario en Bitácora
-- para registros de login fallido y bloqueo con usuario = NULL.
--
-- FORMATOS reales en la BD:
--   Intento fallido : "Intento fallido #X/3 para 'USERNAME' a las HH:MM:SS."
--   Bloqueo         : "Cuenta 'USERNAME' bloqueada automaticamente tras ..."
-- ============================================================

USE WardrobeFlowDB;
GO

-- ── Vista previa ──────────────────────────────────────────────
-- Ejecutá este SELECT primero para verificar que el JOIN resuelve bien.
SELECT
    b.Id,
    b.actividad,
    b.detalle,
    b.usuario AS usuario_actual,
    u.IdUsuario AS usuario_nuevo,
    u.Username  AS username_encontrado
FROM Bitacora b
JOIN Usuario u ON u.Username =
    CASE
        WHEN b.actividad = 'Intento Fallido Login' THEN
            -- Extrae entre "para '" y "' a las"
            SUBSTRING(
                b.detalle,
                CHARINDEX('para ''', b.detalle) + 6,
                CHARINDEX(''' a las', b.detalle)
                    - CHARINDEX('para ''', b.detalle) - 6
            )
        WHEN b.actividad = 'Bloqueo de Cuenta' THEN
            -- Extrae entre "Cuenta '" y "' bloqueada"
            SUBSTRING(
                b.detalle,
                CHARINDEX('Cuenta ''', b.detalle) + 8,
                CHARINDEX(''' bloqueada', b.detalle)
                    - CHARINDEX('Cuenta ''', b.detalle) - 8
            )
    END
WHERE b.usuario  IS NULL
  AND b.actividad IN ('Intento Fallido Login', 'Bloqueo de Cuenta');
GO

-- ── UPDATE: Intento Fallido Login ─────────────────────────────
UPDATE b
SET    b.usuario = u.IdUsuario
FROM   Bitacora b
JOIN   Usuario  u ON u.Username =
    SUBSTRING(
        b.detalle,
        CHARINDEX('para ''', b.detalle) + 6,
        CHARINDEX(''' a las', b.detalle)
            - CHARINDEX('para ''', b.detalle) - 6
    )
WHERE  b.usuario  IS NULL
  AND  b.actividad = 'Intento Fallido Login';

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
        CHARINDEX(''' bloqueada', b.detalle)
            - CHARINDEX('Cuenta ''', b.detalle) - 8
    )
WHERE  b.usuario  IS NULL
  AND  b.actividad = 'Bloqueo de Cuenta';

PRINT CONCAT(@@ROWCOUNT, ' fila(s) actualizadas — Bloqueo de Cuenta');
GO

-- ── Verificación final ────────────────────────────────────────
SELECT 'Quedan NULL en actividades de login' AS check_resultado,
       COUNT(*) AS total
FROM   Bitacora
WHERE  usuario   IS NULL
  AND  actividad IN ('Intento Fallido Login', 'Bloqueo de Cuenta');
GO
