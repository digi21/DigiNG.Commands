using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digi21.DigiNG.Plugin.Command;

namespace Ordenes.ModeloDeDatos
{
    // Pongo la clase internal porque esta funcionalidad ha sido implementada en Digi.tab con la funcionalidad de desencadenadores
    [Command(Name="sustituir_o_eliminar_entidades_por_area")]
    internal class SustituirOEliminarEntidadesPorArea : Command
    {
        public static bool Sustituir = true;

        public SustituirOEliminarEntidadesPorArea()
        {
            this.Initialize += new EventHandler(SustituirOEliminarEntidadesPorArea_Initialize);
        }

        void SustituirOEliminarEntidadesPorArea_Initialize(object sender, EventArgs e)
        {
            if (0 == this.Args.Length)
                Sustituir = !Sustituir;
            else
            {
                int valor;
                if (int.TryParse(this.Args[0], out valor))
                {
                    Sustituir = 0 != valor;
                } 
            }

            Dispose();
        }
    }
}
