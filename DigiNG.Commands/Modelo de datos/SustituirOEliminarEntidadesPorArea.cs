using System;
using Digi21.DigiNG.Plugin.Command;

namespace DigiNG.Commands.Modelo_de_datos
{
    // Pongo la clase internal porque esta funcionalidad ha sido implementada en Digi.tab con la funcionalidad de desencadenadores
    [Command(Name="sustituir_o_eliminar_entidades_por_area")]
    internal class SustituirOEliminarEntidadesPorArea : Command
    {
        public static bool Sustituir = true;

        public SustituirOEliminarEntidadesPorArea()
        {
            Initialize += SustituirOEliminarEntidadesPorArea_Initialize;
        }

        private void SustituirOEliminarEntidadesPorArea_Initialize(object sender, EventArgs e)
        {
            if (0 == Args.Length)
                Sustituir = !Sustituir;
            else
            {
                if (int.TryParse(Args[0], out var valor))
                    Sustituir = 0 != valor;
            }

            Dispose();
        }
    }
}
