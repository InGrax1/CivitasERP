using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CivitasERP.Views.LoginPage;

namespace CivitasERP.Models
{
    internal class Agregar_tiempo
    {
        public (bool exito, DateTime fechaInicio, DateTime fechaFin) ObtenerFechasDesdeGlobal()
        {
            string texto = Variables.Fecha;

            if (string.IsNullOrWhiteSpace(texto))
                return (false, DateTime.MinValue, DateTime.MinValue);

            string[] partes = texto.Split(" - ");

            if (partes.Length == 2 &&
                DateTime.TryParseExact(partes[0], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime inicio) &&
                DateTime.TryParseExact(partes[1], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime fin))
            {
                return (true, inicio, fin);
            }

            return (false, DateTime.MinValue, DateTime.MinValue);
        }




        public List<int> GetAnios()
        {
            int anioActual = DateTime.Now.Year;
            List<int> anios = new List<int>();
            for (int i = anioActual - 5; i <= anioActual + 25; i++)
            {
                anios.Add(i);
            }
            return anios;
        }

        public List<string> GetMeses()
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();
        }


        // Obtiene las semanas del mes de lunes a domingo indepindependientemente de que parte de esa semana caiga en el mes anterior o siguiente
        public List<string> GetSemanasDelMes(int anio, int mes)
        {
            var semanas = new List<string>();
            // Primer y último día del mes
            DateTime primerDia = new DateTime(anio, mes, 1);
            DateTime ultimoDia = primerDia.AddMonths(1).AddDays(-1);

            // 1) Hallamos el lunes de la primera “semana” que toque este mes
            //    DayOfWeek.Monday == 1, Sunday == 0, así que hacemos:
            int diasParaLunes = ((int)primerDia.DayOfWeek + 6) % 7;
            DateTime semanaInicio = primerDia.AddDays(-diasParaLunes);

            while (semanaInicio <= ultimoDia)
            {
                DateTime semanaFin = semanaInicio.AddDays(6);

                // 3) Comprobamos que ese bloque toque el mes
                if (semanaFin >= primerDia && semanaInicio <= ultimoDia)
                {
                    // Ajustamos los extremos al mes (opcional)
                    DateTime inicioVisible = semanaInicio < primerDia ? primerDia : semanaInicio;
                    DateTime finVisible = semanaFin > ultimoDia ? ultimoDia : semanaFin;

                    semanas.Add($"{inicioVisible:dd/MM/yyyy} – {finVisible:dd/MM/yyyy}");
                }

                // Avanzamos al siguiente bloque de lunes
                semanaInicio = semanaInicio.AddDays(7);
            }

            return semanas;
        }
    }
}
