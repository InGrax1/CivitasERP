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

        public List<string> GetSemanasDelMes(int anio, int mes)
        {
            List<string> semanas = new List<string>();
            DateTime primerDia = new DateTime(anio, mes, 1);
            DateTime ultimoDia = primerDia.AddMonths(1).AddDays(-1);
            DateTime actual = primerDia;

            while (actual <= ultimoDia)
            {
                // Alinear al lunes anterior (o el mismo día si es lunes)
                DateTime inicioSemana = actual.DayOfWeek == DayOfWeek.Monday
                    ? actual
                    : actual.AddDays(-(int)actual.DayOfWeek + (actual.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

                DateTime finSemana = inicioSemana.AddDays(6);
                if (finSemana > ultimoDia)
                    finSemana = ultimoDia;

                if (finSemana >= primerDia)
                {
                    semanas.Add($"{inicioSemana:dd/MM/yyyy} - {finSemana:dd/MM/yyyy}");
                }

                actual = finSemana.AddDays(1);
            }

            return semanas.Distinct().ToList();
        }





    }
}
