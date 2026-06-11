using System.Collections.Generic;

namespace BE
{
    public abstract class Componente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public TipoPermiso Permiso { get; set; }

        public abstract IList<Componente> Hijos { get; }
        public abstract void AgregarHijo(Componente c);
        // Elimina un hijo específico del nodo. En hojas (Patente) lanza InvalidOperationException.
        // Equivalente a Quitar() de Stach.
        public abstract void QuitarHijo(Componente c);
        public abstract void VaciarHijos();

        // ── Operaciones RECURSIVAS del Composite (estilo GoF, igual que el ejemplo de
        //    lotes/artículos: cada nodo sabe resolver su propio subárbol) ─────────────

        // Devuelve las HOJAS (Patentes) alcanzables desde este nodo, sin duplicados.
        // Es la operación uniforme del patrón: en una Patente (hoja) se devuelve a sí misma;
        // en una Familia/Rol (compuesto) se agregan recursivamente las de todos sus hijos —
        // exactamente como CalcularPrecioBase() suma recursivamente en LoteArticulos.
        // La recursión termina al llegar a las hojas; un Rol puede contener otros Roles
        // (rol-dentro-de-rol) igual que un Lote puede contener otros Lotes.
        public IList<Patente> ObtenerPatentesEfectivas()
        {
            var acumulador = new Dictionary<int, Patente>();
            RecolectarPatentes(acumulador, new HashSet<int>());
            return new List<Patente>(acumulador.Values);
        }

        // Paso recursivo concreto de cada tipo de nodo (hoja vs compuesto).
        // 'internal' (no 'protected') para poder invocarlo sobre una referencia Componente
        // de un hijo desde dentro de Familia (mismo ensamblado BE).
        internal abstract void RecolectarPatentes(IDictionary<int, Patente> acumulador, HashSet<int> visitados);

        // Descripción textual del subárbol con sangría (recorrido en profundidad),
        // equivalente a ObtenerDescripcion() del ejemplo de lotes anidados.
        public abstract string ObtenerDescripcion(int nivel = 0);

        public override string ToString()
        {
            return Nombre;
        }
    }
}
