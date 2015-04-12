using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Utilities;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.Digi3D;

namespace OrdenesDigiNG
{
    [LocalizableCommand(typeof(Recursos), "DetectaLíneasPerímetroInferiorAValorName")]
    public class DetectaLíneasPerímetroInferiorAValor : Command
    {
        public DetectaLíneasPerímetroInferiorAValor()
        {
            this.Initialize += new EventHandler(DetectaLíneasPerímetroInferiorAValor_Initialize);
        }

        void DetectaLíneasPerímetroInferiorAValor_Initialize(object sender, EventArgs e)
        {
            double perímetro = int.Parse(this.Args[0]);
            List<string> códigos = new List<string>(this.Args.Skip(1));

            var líneasDetectadas = from línea in DigiNG.DrawingFile.NoEliminadas().OfType<ReadOnlyLine>().QueTenganAlgúnCódigoConComodín(códigos)
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
