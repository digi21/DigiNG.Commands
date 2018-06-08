using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Shell;
using Digi21.Math;
using Digi21.Utilities;

namespace DigiNG.Commands
{
    [LocalizableCommand(typeof(Recursos), "UnirPolígonosName")]
    [LocalizableCommandInMenu(typeof(Recursos), "UnirPolígonosTitle", MenuItemGroup.EditGroup9Group15)]
    public class UnirPolígonos : Command
    {
        private Entity entidadAUnir;

        public UnirPolígonos()
        {
            Initialize += UnirPolígonos_Initialize;
            DataUp += UnirPolígonos_DataUp;
            EntitySelected += UnirPolígonos_EntitySelected;
            SetFocus += UnirPolígonos_SetFocus;
            AllowRepeat = true;
        }

        private void UnirPolígonos_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        private void UnirPolígonos_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (null == entidadAUnir)
            {
                entidadAUnir = e.Entity;
                SolicitaSeleccionaEntidad();
                return;
            }

            var listaErrores = new List<string>();

            if (!entidadAUnir.CódigosIdénticos(e.Entity, listaErrores))
            {
                MessageBox.Show(Recursos.LosCódigosDeLasEntidadesSonDistintosYPoLoTantoNoSePuedenUnir);
                return;
            }

            try
            {
                var polígono = Polygon.JoinPolygons(entidadAUnir, e.Entity);
                Entity entidadAAñadir = polígono;

                // Me han solicitado que si las dos líneas que han formado el polígono son líneas y el polígono resultante no tiene
                // ningún hueco, que el resultado siga siendo una línea y no un polígono.
                if (entidadAUnir is ReadOnlyLine && e.Entity is ReadOnlyLine && 0 == polígono.Holes.Count)
                {
                    var temp = new Line(polígono.Codes);
                    temp.Points.Add(polígono.Points);
                    entidadAAñadir = temp;
                }

                Digi21.DigiNG.DigiNG.DrawingFile.Add(entidadAAñadir);
                Digi21.DigiNG.DigiNG.DrawingFile.Delete(entidadAUnir);
                Digi21.DigiNG.DigiNG.DrawingFile.Delete(e.Entity);
            }
            catch (Exception excepción)
            {
                MessageBox.Show(excepción.Message);
            }
            finally
            {
                Dispose();
            }
        }

        private void UnirPolígonos_DataUp(object sender, Point3DEventArgs e)
        {
            Digi21.DigiNG.DigiNG.SelectEntity(
                e.Coordinates,
                entidad => entidad != entidadAUnir && Digi21.DigiNG.DigiNG.DrawingFile.Contains(entidad) && ((entidad is ReadOnlyLine line && line.Closed) || entidad is ReadOnlyPolygon));
        }

        private void UnirPolígonos_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        private void SolicitaSeleccionaEntidad()
        {
            Digi3D.StatusBar.Text = null == entidadAUnir ? Recursos.SeleccionaElPrimerPolígnoAUnir : Recursos.SeleccionaElSegundoPolígonoAUnir;
        }
    }
}
