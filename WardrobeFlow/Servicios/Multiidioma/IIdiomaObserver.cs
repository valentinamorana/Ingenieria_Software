namespace Servicios.Multiidioma
{
    /// <summary>
    /// Interfaz Observer del patrón Observer para multi-idioma.
    ///
    /// Todo formulario que quiera reaccionar automáticamente al cambio de idioma
    /// debe implementar esta interfaz y registrarse en <see cref="GestorIdioma"/>.
    ///
    /// Equivalente a IIdiomaObserver del ejemplo de cátedra.
    /// </summary>
    public interface IIdiomaObserver
    {
        /// <summary>
        /// Llamado por el sujeto (<see cref="GestorIdioma"/>) cuando el idioma cambia.
        /// El formulario debe actualizar todos sus controles con las traducciones del nuevo idioma.
        /// </summary>
        void UpdateLanguage(Idioma idioma);
    }
}
