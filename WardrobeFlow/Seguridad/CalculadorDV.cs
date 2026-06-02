namespace Seguridad
{
    /// <summary>
    /// Punto único para obtener el algoritmo de Dígito Verificador en uso.
    /// Para cambiar el algoritmo de todo el sistema (p. ej. a uno basado en hash),
    /// basta con devolver otra implementación de <see cref="ICalculadorDV"/> acá.
    /// </summary>
    public static class CalculadorDV
    {
        public static ICalculadorDV Crear()
        {
            return new DigitoVerificador();
        }
    }
}
