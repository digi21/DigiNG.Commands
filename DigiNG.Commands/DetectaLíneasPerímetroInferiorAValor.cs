using System;
using System.Collections.Generic;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG.Plugin.TaskPanel;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Tasks;
using Digi21.Utilities;

namespace DigiNG.Commands
{
    [LocalizableCommand(typeof(Recursos), "DetectaLíneasPerímetroInferiorAValorName")]
    public class DetectaLíneasPerímetroInferiorAValor : Command
    {
        public DetectaLíneasPerímetroInferiorAValor()
        {
            Initialize += DetectaLíneasPerímetroInferiorAValor_Initialize;
        }

        private void DetectaLíneasPerímetroInferiorAValor_Initialize(object sender, EventArgs e)
        {
            double perímetro = int.Parse(Args[0]);
            var códigos = new List<string>(Args.Skip(1));

            var líneasDetectadas = from línea in Digi21.DigiNG.DigiNG.DrawingFile.NoEliminadas().OfType<ReadOnlyLine>().QueTenganAlgúnCódigoConComodín(códigos)
                                   where línea.Perimeter < perímetro
                                   select línea;
            
            foreach (var línea in líneasDetectadas)
            {
                var mensaje = string.Format(
                    Recursos.LineaXConPerímetroYInferiorAZ, 
                    línea.Codes[0].Name,
                    línea.Perimeter,
                    perímetro);
                Digi3D.Tasks.Add(new TaskZoomEntity(línea, 2, mensaje, TaskSeverity.Error));
            }
        }
    }
}
