using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitasERP.Models
{
    internal class Variables
    {
        private static bool _Jefe;
        private static string _usuario;
        private static int? _idAdmin;
        private static int? _idObra;
        private static string _obraNom;
        private static string _fecha;
        private static string _fechaInicio;
        private static string _fechaFin;

        private static string _indexComboboxAño;
        private static string? _indexComboboxMes;
        private static string? _indexComboboxSemana;

        private static string _AdminSeleccionado;


        public static bool Jefe
        {
            get { return _Jefe; }
            set { _Jefe = value; }
        }

        public static string Usuario
        {
            get { return _usuario; }
            set { _usuario = value; }
        }

        public static int? IdAdmin
        {
            get { return _idAdmin; }
            set { _idAdmin = value; }
        }

        public static int? IdObra
        {
            get { return _idObra; }
            set { _idObra = value; }
        }

        public static string ObraNom
        {
            get { return _obraNom; }
            set { _obraNom = value; }
        }

        public static string Fecha
        {
            get { return _fecha; }
            set { _fecha = value; }
        }

        public static string FechaInicio
        {
            get { return _fechaInicio; }
            set { _fechaInicio = value; }
        }

        public static string FechaFin
        {
            get { return _fechaFin; }
            set { _fechaFin = value; }
        }


        public static string indexComboboxAño
        {
            get { return _indexComboboxAño; }
            set { _indexComboboxAño = value; }
        }

        public static string indexComboboxMes
        {
            get { return _indexComboboxMes; }
            set { _indexComboboxMes = value; }
        }

        public static string indexComboboxSemana
        {
            get { return _indexComboboxSemana; }
            set { _indexComboboxSemana = value; }
        }

        public static string AdminSeleccionado
        {
            get { return _AdminSeleccionado; }
            set { _AdminSeleccionado = value; }
        }

    }
}
