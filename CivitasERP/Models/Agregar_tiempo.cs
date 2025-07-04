using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Channels;
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

            DateTime primerDiaMes = new DateTime(anio, mes, 1);
            DateTime ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            // Encontrar el lunes de la semana donde está el primer día del mes
            int diffLunes = ((int)primerDiaMes.DayOfWeek + 6) % 7;  // lunes=0, domingo=6
            DateTime semanaInicio = primerDiaMes.AddDays(-diffLunes);

            // Encontrar el domingo de la semana donde está el último día del mes
            int diffDomingo = 7 - ((int)ultimoDiaMes.DayOfWeek == 0 ? 7 : (int)ultimoDiaMes.DayOfWeek);
            DateTime ultimoDomingo = ultimoDiaMes.AddDays(diffDomingo);

            // Recorremos desde ese lunes hasta ese domingo, en bloques de semanas completas
            while (semanaInicio <= ultimoDomingo)
            {
                DateTime semanaFin = semanaInicio.AddDays(6);
                semanas.Add($"{semanaInicio:dd/MM/yyyy} - {semanaFin:dd/MM/yyyy}");
                semanaInicio = semanaInicio.AddDays(7);
            }

            return semanas;
        }

    }
}
