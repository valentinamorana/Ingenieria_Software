using System;
using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Patrón Composite — T04 Gestión de Perfiles de Usuario.
    /// Nodo compuesto: representa un grupo de permisos (ej: "Inventario", "Ventas").
    /// Puede contener tanto Familia como Patente como hijos.
    /// </summary>
    public class Familia : Componente
    {
        private readonly List<Componente> _hijos = new List<Componente>();

        public override IList<Componente> Hijos => _hijos.ToArray();

        public override void AgregarHijo(Componente c)
        {
            if (c == null) return;

            if (c == this)
                throw new InvalidOperationException(
                    $"Referencia circular directa: '{Nombre}' no puede ser hijo de sí mismo.");

            // Verifica que 'this' no sea ya un descendiente de 'c'.
            // Si lo fuera, agregar 'c' como hijo de 'this' crearía un ciclo.
            if (ContieneDescendiente(c, this))
                throw new InvalidOperationException(
                    $"Referencia circular: agregar '{c.Nombre}' como hijo de '{Nombre}' " +
                    $"crearía un ciclo porque '{Nombre}' ya es descendiente de '{c.Nombre}'.");

            if (!_hijos.Contains(c))
                _hijos.Add(c);
        }

        // Retorna true si 'buscado' aparece en el subárbol de 'nodo'.
        // maxDepth evita recursión infinita si los datos en BD están corrompidos.
        private static bool ContieneDescendiente(Componente nodo, Componente buscado, int depth = 0)
        {
            if (depth > 50)
                throw new InvalidOperationException(
                    "Se alcanzó la profundidad máxima del árbol de permisos. " +
                    "Posible referencia circular en los datos.");
            foreach (var hijo in nodo.Hijos)
            {
                if (hijo == buscado) return true;
                if (ContieneDescendiente(hijo, buscado, depth + 1)) return true;
            }
            return false;
        }

        // Elimina un hijo específico del nodo compuesto.
        // No lanza si el hijo no existe — comportamiento silencioso como en Stach.
        public override void QuitarHijo(Componente c)
        {
            if (c != null)
                _hijos.Remove(c);
        }

        public override void VaciarHijos()
        {
            _hijos.Clear();
        }

        // Paso RECURSIVO: un compuesto agrega las hojas de TODOS sus hijos. Si un hijo es a su
        // vez otra Familia/Rol, éste recorre los suyos — la recursión baja hasta las Patentes.
        // Es el mismo mecanismo que CalcularPrecioBase() de LoteArticulos, que suma recursivamente
        // (y soporta lotes-dentro-de-lotes igual que acá rol-dentro-de-rol).
        // 'visitados' evita revisitar nodos compartidos o cortar ante datos cíclicos corruptos.
        internal override void RecolectarPatentes(IDictionary<int, Patente> acumulador, HashSet<int> visitados)
        {
            if (Id != 0 && !visitados.Add(Id)) return;
            foreach (var hijo in _hijos)
                hijo.RecolectarPatentes(acumulador, visitados);
        }

        public override string ObtenerDescripcion(int nivel = 0)
        {
            if (nivel > 50) return new string(' ', nivel * 2) + "- ...";   // tope defensivo de profundidad
            var sb      = new System.Text.StringBuilder();
            string sangria = new string(' ', nivel * 2);
            string tipo    = this is Rol ? "Rol" : "Familia";
            sb.Append($"{sangria}+ [{tipo}] {Nombre}");
            foreach (var hijo in _hijos)
            {
                sb.AppendLine();
                sb.Append(hijo.ObtenerDescripcion(nivel + 1));
            }
            return sb.ToString();
        }
    }
}
