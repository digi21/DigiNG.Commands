using System;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Shell;
using Digi21.Math;

namespace DigiNG.Commands.Operaciones_con_entidades
{
    [LocalizableCommand(typeof(Recursos), "CortarPoligonoName")]
    [LocalizableCommandInMenu(typeof(Recursos), "CortarPoligonoTitle", MenuItemGroup.EditGroup9Group15)]
    public class CortarPoligono : Command
    {
        private Entity entidadADividir;

        public CortarPoligono()
        {
            Initialize += CortarPoligono_Initialize;
            SetFocus += CortarPoligono_SetFocus;
            DataUp += CortarPoligono_DataUp;
            EntitySelected += CortarPoligono_EntitySelected;
        }

        private void CortarPoligono_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        private void CortarPoligono_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        private void CortarPoligono_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (entidadADividir == null)
            {
                entidadADividir = e.Entity;
                SolicitaSeleccionaEntidad();
                return;
            }

            var límite = e.Entity as ReadOnlyLine;

            try
            {
                var polígonos = ((IClippable)entidadADividir).Clip(límite);
                Digi21.DigiNG.DigiNG.DrawingFile.Add(polígonos);
                Digi21.DigiNG.DigiNG.DrawingFile.Delete(entidadADividir);

                if (Args.Length != 0 && Digi21.DigiNG.DigiNG.DrawingFile.Contains(límite))
                    Digi21.DigiNG.DigiNG.DrawingFile.Delete(límite);
            }
            catch (Exception ex)
            {
                Digi3D.Music(MusicType.Error);
                Digi3D.ShowBallon(
                    Recursos.PartirPoligonosName,
                    ex.Message,
                    2);
            }
            finally
            {
                Dispose();
            }
        }

        private void CortarPoligono_DataUp(object sender, Point3DEventArgs e)
        {
            if (entidadADividir == null)
                Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates, entidad => Digi21.DigiNG.DigiNG.DrawingFile.Contains(entidad) && (entidad is ReadOnlyPolygon || entidad is ReadOnlyLine && ((ReadOnlyLine)entidad).Closed));
            else
                Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates, entidad => entidad is ReadOnlyLine);
        }

        private void SolicitaSeleccionaEntidad()
        {
            Digi3D.StatusBar.Text = entidadADividir == null ? Recursos.SeleccionaElPoligonoACortar : Recursos.SeleccionaLaLíneaDeCorte;
        }
    }
}
