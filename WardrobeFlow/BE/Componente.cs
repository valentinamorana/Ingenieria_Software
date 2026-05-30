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

        public override string ToString()
        {
            return Nombre;
        }
    }
}
