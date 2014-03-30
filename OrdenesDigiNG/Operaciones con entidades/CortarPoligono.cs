using System;
using System.Linq;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Shell;

namespace Ordenes.OperacionesConEntidades
{
    [Command(Name = "cortar_poligono")]
    [CommandInMenu("Cortar polígono", MenuItemGroup.EditGroup9Group15)]
    public class CortarPoligono : Command
    {
        private Entity entidadADividir = null;

        public CortarPoligono()
        {
            this.Initialize += new EventHandler(CortarPoligono_Initialize);
            this.SetFocus += new EventHandler(CortarPoligono_SetFocus);
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(CortarPoligono_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(CortarPoligono_EntitySelected);
        }

        void CortarPoligono_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void CortarPoligono_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void CortarPoligono_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (entidadADividir == null)
            {
                entidadADividir = e.Entity;
                SolicitaSeleccionaEntidad();
                return;
            }

            ReadOnlyLine límite = e.Entity as ReadOnlyLine;

            try
            {
                var polígonos = (entidadADividir as IClippable).Clip(límite);
                DigiNG.DrawingFile.Add(polígonos);
                DigiNG.DrawingFile.Delete(entidadADividir);

                if (this.Args.Length != 0 && DigiNG.DrawingFile.Contains(límite))
                    DigiNG.DrawingFile.Delete(límite);
            }
            catch (Exception ex)
            {
                Digi3D.Music(MusicType.Error);
                Digi3D.ShowBallon("Partir polígono",
                    ex.Message,
                    2);
            }
            finally
            {
                Dispose();
            }
        }

        void CortarPoligono_DataUp(object sender, Digi21.Math.Point3DEventArgs e)
        {
            if (entidadADividir == null)
                DigiNG.SelectEntity(e.Coordinates, entidad => DigiNG.DrawingFile.Contains(entidad) && (entidad is ReadOnlyPolygon || entidad is ReadOnlyLine && ((ReadOnlyLine)entidad).Closed));
            else
                DigiNG.SelectEntity(e.Coordinates, entidad => entidad is ReadOnlyLine);
        }

        void SolicitaSeleccionaEntidad()
        {
            if (entidadADividir == null)
                Digi3D.StatusBar.Text = "Selecciona el polígono a Cortar";
            else
                Digi3D.StatusBar.Text = "Selecciona la línea de corte";
        }

    }
}
