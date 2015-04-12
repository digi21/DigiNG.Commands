using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG;
using Digi21.Math;
using Digi21.Utilities;
using Digi21.DigiNG.Plugin.Shell;
using System.Windows.Forms;


namespace Ordenes.OperacionesConEntidades
{
    [LocalizableCommand(typeof(OrdenesDigiNG.Recursos), "UnirPolígonosName")]
    [LocalizableCommandInMenuAttribute(typeof(OrdenesDigiNG.Recursos), "UnirPolígonosTitle", MenuItemGroup.EditGroup9Group15)]
    public class UnirPolígonos : Command
    {
        private Entity entidadAUnir = null;

        public UnirPolígonos()
        {
            this.Initialize += new EventHandler(UnirPolígonos_Initialize);
            this.DataUp += new EventHandler<Point3DEventArgs>(UnirPolígonos_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(UnirPolígonos_EntitySelected);
            this.SetFocus += new EventHandler(UnirPolígonos_SetFocus);
            this.AllowRepeat = true;
        }

        void UnirPolígonos_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void UnirPolígonos_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (null == entidadAUnir)
            {
                entidadAUnir = e.Entity;
                SolicitaSeleccionaEntidad();
                return;
            }

            List<string> listaErrores = new List<string>();

            if (!entidadAUnir.CódigosIdénticos(e.Entity, listaErrores))
            {
                MessageBox.Show(OrdenesDigiNG.Recursos.LosCódigosDeLasEntidadesSonDistintosYPoLoTantoNoSePuedenUnir);
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

                DigiNG.DrawingFile.Add(entidadAAñadir);
                DigiNG.DrawingFile.Delete(entidadAUnir);
                DigiNG.DrawingFile.Delete(e.Entity);
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

        void UnirPolígonos_DataUp(object sender, Point3DEventArgs e)
        {
            DigiNG.SelectEntity(
                e.Coordinates,
                entidad => entidad != entidadAUnir &&
                    DigiNG.DrawingFile.Contains(entidad) && 
                    ((entidad is ReadOnlyLine && (entidad as ReadOnlyLine).Closed) || entidad is ReadOnlyPolygon));
        }

        void UnirPolígonos_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void SolicitaSeleccionaEntidad()
        {
            if (null == entidadAUnir)
                Digi3D.StatusBar.Text = OrdenesDigiNG.Recursos.SeleccionaElPrimerPolígnoAUnir;
            else
                Digi3D.StatusBar.Text = OrdenesDigiNG.Recursos.SeleccionaElSegundoPolígonoAUnir;
        }

    }
}
