using System;
using System.Linq;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Shell;

namespace Ordenes.OperacionesConEntidades
{
    [Command(Name = "recortar_poligono")]
    [CommandInMenu("Recortar polígono", MenuItemGroup.EditGroup9Group15)]
    public class RecortarPoligono : Command
    {
        private Entity entidadADividir = null;

        public RecortarPoligono()
        {
            this.Initialize += new EventHandler(RecortarPoligono_Initialize);
            this.SetFocus += new EventHandler(RecortarPoligono_SetFocus);
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(RecortarPoligono_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(RecortarPoligono_EntitySelected);
        }

        void RecortarPoligono_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void RecortarPoligono_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void RecortarPoligono_EntitySelected(object sender, EntitySelectedEventArgs e)
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

        void RecortarPoligono_DataUp(object sender, Digi21.Math.Point3DEventArgs e)
        {
            if (entidadADividir == null)
                DigiNG.SelectEntity(e.Coordinates, entidad => DigiNG.DrawingFile.Contains(entidad) && entidad is ReadOnlyPolygon);
            else
                DigiNG.SelectEntity(e.Coordinates, entidad => entidad is ReadOnlyLine);
        }

        void SolicitaSeleccionaEntidad()
        {
            if (entidadADividir == null)
                Digi3D.StatusBar.Text = "Selecciona el polígono a recortar";
            else
                Digi3D.StatusBar.Text = "Selecciona la línea de corte";
        }

    }
}
