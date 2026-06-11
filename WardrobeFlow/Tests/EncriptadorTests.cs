using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>T03 — Pruebas de hash de contraseñas (PBKDF2) y cifrado simétrico (AES).</summary>
    [TestClass]
    public class EncriptadorTests
    {
        [TestMethod]
        public void Hash_VerificaContrasenaCorrecta()
        {
            string h = Seguridad.Encriptador.Hash("Secreta1!");
            Assert.IsTrue(Seguridad.Encriptador.VerificarContrasena("Secreta1!", h));
        }

        [TestMethod]
        public void Hash_RechazaContrasenaIncorrecta()
        {
            string h = Seguridad.Encriptador.Hash("Secreta1!");
            Assert.IsFalse(Seguridad.Encriptador.VerificarContrasena("otra", h));
        }

        [TestMethod]
        public void Hash_UsaSaltDistintoPorLlamada()
        {
            // Mismo texto, hashes distintos → hay salt aleatorio.
            Assert.AreNotEqual(Seguridad.Encriptador.Hash("x"), Seguridad.Encriptador.Hash("x"));
        }

        [TestMethod]
        public void Aes_RoundTrip()
        {
            string cifrado = Seguridad.Encriptador.Encriptar("12345678");
            Assert.AreNotEqual("12345678", cifrado, "El texto cifrado no debe coincidir con el plano.");
            Assert.AreEqual("12345678", Seguridad.Encriptador.Desencriptar(cifrado));
        }

        [TestMethod]
        public void TryDesencriptar_ToleraTextoPlano()
        {
            // Un valor que no es un cifrado válido se devuelve tal cual (compat. con datos legacy).
            Assert.AreEqual("texto-plano", Seguridad.Encriptador.TryDesencriptar("texto-plano"));
        }
    }
}
