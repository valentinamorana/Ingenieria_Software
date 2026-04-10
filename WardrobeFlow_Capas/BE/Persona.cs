using System;

namespace BE
{
    public class Persona
    {
        #region Atributos
        private int idPersona;
        private string nombreCompleto;
        private string correo;
        private string documento;
        #endregion

        #region Propiedades
        public int IdPersona { get { return idPersona; } set { idPersona = value; } }
        public string NombreCompleto { get { return nombreCompleto; } set { nombreCompleto = value; } }
        public string Correo { get { return correo; } set { correo = value; } }
        public string Documento { get { return documento; } set { documento = value; } }
        #endregion
    }
}
