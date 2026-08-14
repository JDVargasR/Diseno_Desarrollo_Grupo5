using System.Globalization;
using System.Text;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Helpers
{
    public static class TextHelper
    {
        /// <summary>
        /// Normaliza un texto para b�squeda: quita tildes/diacr�ticos y lo pasa a min�sculas.
        /// Permite comparar "Cuarzo Blanco Polar" con "cuarzo blanco polar" o "polár".
        /// </summary>
        public static string Normalizar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;

            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalizado.Length);

            foreach (var c in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        public static bool Contiene(string origen, string busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda)) return true;
            return Normalizar(origen).Contains(Normalizar(busqueda));
        }
    }
}
