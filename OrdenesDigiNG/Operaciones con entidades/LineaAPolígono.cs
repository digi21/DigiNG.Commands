using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.Utilities;
using Digi21.DigiNG;

namespace OrdenesDigiNG.OperacionesConEntidades
{
    [LocalizableCommand(typeof(Recursos), "LineaAPolígonoName", "LineaAPolígonoDescription")]
    public class LineaAPolígono : Command
    {
        public LineaAPolígono()
        {
            this.Initialize += new EventHandler(LineaAPolígono_Initialize);
        }

        void LineaAPolígono_Initialize(object sender, EventArgs e)
        {
            try
            {
                if (0 == this.Args.Length)
                {
                    Digi3D.Music(MusicType.Error);
                    Digi3D.ShowBallon(
                        Recursos.LineaAPolígonoName,
                        Recursos.NoHaIndicadoLosCódigosDeLasLíneasAConvertirEnPolígono,
                        2);
                    return;
                }


                var líneasATransformar = (from entidad in DigiNG.DrawingFile.OfType<ReadOnlyLine>()
                                          where !entidad.Deleted
                                          where entidad.TieneAlgúnCódigoConComodín(this.Args)
                                          where entidad.Closed
                                          select entidad).ToList();

                List<Polygon> polígonos = new List<Polygon>();
                foreach (var línea in líneasATransformar)
                {
                    var polígono = new Polygon(línea.Codes);
                    polígono.Points.Add(línea.Points);
                    polígonos.Add(polígono);
                }

                DigiNG.DrawingFile.Add(polígonos);
                DigiNG.DrawingFile.Delete(líneasATransformar);
            }
            finally
            {
                Dispose();
            }
                
        }
    }
}
