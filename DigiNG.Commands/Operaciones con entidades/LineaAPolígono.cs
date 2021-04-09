using System;
using System.Collections.Generic;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Utilities;

namespace DigiNG.Commands.Operaciones_con_entidades
{
    [LocalizableCommand(typeof(Recursos), "LineaAPolígonoName", "LineaAPolígonoDescription")]
    public class LineaAPolígono : Command
    {
        public LineaAPolígono()
        {
            Initialize += LineaAPolígono_Initialize;
        }

        private void LineaAPolígono_Initialize(object sender, EventArgs e)
        {
            try
            {
                if (0 == Args.Length)
                {
                    Digi3D.Music(MusicType.Error);
                    Digi3D.ShowBallon(
                        Recursos.LineaAPolígonoName,
                        Recursos.NoHaIndicadoLosCódigosDeLasLíneasAConvertirEnPolígono,
                        2);
                    return;
                }

                var líneasATransformar = (from entidad in Digi21.DigiNG.DigiNG.DrawingFile.OfType<ReadOnlyLine>()
                                          where !entidad.Deleted
                                          where entidad.TieneAlgúnCódigoConComodín(Args)
                                          where entidad.Closed
                                          select entidad).ToList();

                var polígonos = new List<Polygon>();
                foreach (var línea in líneasATransformar)
                {
                    var polígono = new Polygon(línea.Codes);
                    polígono.Points.AddRange(línea.Points);
                    polígonos.Add(polígono);
                }

                Digi21.DigiNG.DigiNG.DrawingFile.Add(polígonos);
                Digi21.DigiNG.DigiNG.DrawingFile.Delete(líneasATransformar);
            }
            finally
            {
                Dispose();
            }
                
        }
    }
}
