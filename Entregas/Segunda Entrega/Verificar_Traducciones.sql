/* ============================================================================
   WardrobeFlow - Verificacion de Traducciones (T05 Multiidioma)
   Base: WardrobeFlowDB

   Tablas que intervienen:
     Idioma      -> los idiomas         (IdIdioma, Codigo, Nombre, Activo, EsDefault)
     Control     -> textos traducibles  (IdControl, Clave 'btn.ingresar', Formulario)
     Traduccion  -> el texto por idioma (IdControl, IdIdioma, Texto)  <-- lo que se
                    escribe al traducir a mano en Gestion de Idiomas (UPSERT).

   Ejecutar cada consulta por separado (seleccionar el bloque y F5).
   ============================================================================ */

USE WardrobeFlowDB;
GO

/* ----------------------------------------------------------------------------
   1) RESUMEN: cuantas claves le faltan a cada idioma
      - Traducidas  : filas que tiene en Traduccion
      - Faltantes   : claves de Control sin fila en ese idioma (usan fallback ES)
      - CopiadasDelES: traducciones cuyo texto es IGUAL al espanol (placeholder,
                       no traducido de verdad). NOTA: para ES da el total (se
                       compara consigo mismo) -> ignorar esa fila.
   ---------------------------------------------------------------------------- */
SELECT
    i.Codigo,
    i.Nombre,
    tot.Total                              AS TotalClaves,
    COUNT(t.IdControl)                     AS Traducidas,
    tot.Total - COUNT(t.IdControl)         AS Faltantes,
    SUM(CASE WHEN es.Texto IS NOT NULL AND t.Texto = es.Texto THEN 1 ELSE 0 END) AS CopiadasDelES
FROM Idioma i
CROSS JOIN (SELECT COUNT(*) AS Total FROM Control) tot
LEFT JOIN Traduccion t  ON t.IdIdioma = i.IdIdioma
LEFT JOIN Idioma idEs   ON idEs.EsDefault = 1
LEFT JOIN Traduccion es ON es.IdControl = t.IdControl AND es.IdIdioma = idEs.IdIdioma
GROUP BY i.Codigo, i.Nombre, tot.Total
ORDER BY Faltantes DESC, i.Codigo;
GO


/* ----------------------------------------------------------------------------
   2) Lista de las CLAVES QUE FALTAN para un idioma (cambia @codigo).
      Muestra al lado el texto en espanol como referencia para traducir.
   ---------------------------------------------------------------------------- */
DECLARE @codigo VARCHAR(5) = 'HU';   -- <--- idioma a revisar (HU, FR, etc.)

SELECT
    c.IdControl,
    c.Clave,
    c.Formulario,
    es.Texto AS TextoES_referencia
FROM Control c
LEFT JOIN Idioma idEs   ON idEs.EsDefault = 1
LEFT JOIN Traduccion es ON es.IdControl = c.IdControl AND es.IdIdioma = idEs.IdIdioma
WHERE NOT EXISTS (
    SELECT 1
    FROM Traduccion t
    JOIN Idioma i ON i.IdIdioma = t.IdIdioma
    WHERE t.IdControl = c.IdControl AND i.Codigo = @codigo
)
ORDER BY c.Formulario, c.Clave;
GO


/* ----------------------------------------------------------------------------
   3) VERIFICAR que una traduccion que hiciste en el Forms LEVANTO en la BD.
      Cambia @clave y @idioma por lo que tradujiste y corre.
        - Si devuelve la fila con tu texto -> se guardo OK en Traduccion.
        - Si NO devuelve nada              -> no se guardo (la app usa fallback ES).
   ---------------------------------------------------------------------------- */
DECLARE @clave  VARCHAR(200) = 'btn.ingresar';   -- <--- la clave del control (ej. boton Ingresar)
DECLARE @idioma VARCHAR(5)   = 'HU';             -- <--- el idioma que tradujiste

SELECT
    c.Clave,
    c.Formulario,
    i.Codigo  AS Idioma,
    t.Texto   AS TextoGuardado
FROM Control c
JOIN Traduccion t ON t.IdControl = c.IdControl
JOIN Idioma i     ON i.IdIdioma  = t.IdIdioma
WHERE c.Clave = @clave
  AND i.Codigo = @idioma;
GO


/* ----------------------------------------------------------------------------
   4) BONUS: ver una clave en TODOS los idiomas, lado a lado (para comparar).
      Cambia @clave. Las celdas vacias = ese idioma no tiene esa traduccion.
   ---------------------------------------------------------------------------- */
DECLARE @claveCmp VARCHAR(200) = 'btn.ingresar';

SELECT
    c.Clave,
    c.Formulario,
    MAX(CASE WHEN i.Codigo = 'ES' THEN t.Texto END) AS ES,
    MAX(CASE WHEN i.Codigo = 'EN' THEN t.Texto END) AS EN,
    MAX(CASE WHEN i.Codigo = 'PT' THEN t.Texto END) AS PT,
    MAX(CASE WHEN i.Codigo = 'RU' THEN t.Texto END) AS RU,
    MAX(CASE WHEN i.Codigo = 'FR' THEN t.Texto END) AS FR,
    MAX(CASE WHEN i.Codigo = 'HU' THEN t.Texto END) AS HU
FROM Control c
LEFT JOIN Traduccion t ON t.IdControl = c.IdControl
LEFT JOIN Idioma i     ON i.IdIdioma  = t.IdIdioma
WHERE c.Clave = @claveCmp
GROUP BY c.Clave, c.Formulario;
GO
