using System;
using System.Collections.Generic;
using BE;

namespace Seguridad
{
    // PATRON SINGLETON — Gestor de bitacora (auditoria) del sistema.
    // Basado en BitacoraSL del ejemplo de referencia (Nacho/Codigo),
    // adaptado para storage en MEMORIA (lista interna, no SQL).
    // Registra automaticamente quien realizo cada accion y cuando.
    public sealed class BitacoraSL
    {
        // Instancia unica del gestor de bitacora
        private static BitacoraSL _instancia;

        // Objeto de bloqueo para garantizar hilo-seguridad
        private static readonly object _lock = new object();

        // Lista en memoria con todos los eventos registrados
        private readonly List<BitacoraBE> _historial;

        // Constructor privado (Singleton)
        private BitacoraSL()
        {
            _historial = new List<BitacoraBE>();
        }

        // Propiedad estatica para acceder a la instancia unica
        public static BitacoraSL Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    lock (_lock)
                    {
                        if (_instancia == null)
                            _instancia = new BitacoraSL();
                    }
                }
                return _instancia;
            }
        }

        // Registra un evento en la bitacora.
        // Obtiene automaticamente el usuario de la sesion activa.
        public void RegistrarEvento(TipoOperacion tipo, Modulo modulo, string descripcion, bool exitoso = true)
        {
            try
            {
                // Obtener el nombre del usuario de la sesion activa (si la hay)
                string nombreUsuario = "Sistema";
                var usuarioActual = SessionManagerSL.Instancia.ObtenerUsuarioActual();
                if (usuarioActual != null)
                    nombreUsuario = usuarioActual.NombreCompleto;

                var evento = new BitacoraBE
                {
                    FechaHora     = DateTime.Now,
                    NombreUsuario = nombreUsuario,
                    TipoOperacion = tipo,
                    Modulo        = modulo,
                    Descripcion   = descripcion,
                    Exitoso       = exitoso
                };

                // Agregar a la lista (lock para hilo-seguridad)
                lock (_lock)
                {
                    _historial.Add(evento);
                }
            }
            catch (Exception ex)
            {
                // Log silencioso: la bitacora no debe interrumpir el flujo principal
                System.Diagnostics.Debug.WriteLine("Error en BitacoraSL: " + ex.Message);
            }
        }

        // Metodo de conveniencia: registra un inicio de sesion exitoso
        public void RegistrarLogin(string documento)
        {
            RegistrarEvento(
                TipoOperacion.LOGIN,
                Modulo.AUTENTICACION,
                "Inicio de sesion exitoso. Documento: " + documento
            );
        }

        // Metodo de conveniencia: registra un intento de login fallido
        public void RegistrarLoginFallido(string documento)
        {
            RegistrarEvento(
                TipoOperacion.LOGIN_FALLIDO,
                Modulo.AUTENTICACION,
                "Intento de login fallido. Documento: " + documento,
                false
            );
        }

        // Metodo de conveniencia: registra un cierre de sesion
        public void RegistrarLogout(string nombreUsuario)
        {
            RegistrarEvento(
                TipoOperacion.LOGOUT,
                Modulo.AUTENTICACION,
                "Cierre de sesion. Usuario: " + nombreUsuario
            );
        }

        // Devuelve todos los eventos registrados (copia de la lista)
        public IList<BitacoraBE> ObtenerHistorial()
        {
            lock (_lock)
            {
                return new List<BitacoraBE>(_historial);
            }
        }

        // Devuelve los ultimos N eventos registrados
        public IList<BitacoraBE> ObtenerUltimosEventos(int cantidad = 50)
        {
            lock (_lock)
            {
                int inicio = Math.Max(0, _historial.Count - cantidad);
                return _historial.GetRange(inicio, _historial.Count - inicio);
            }
        }
    }
}
