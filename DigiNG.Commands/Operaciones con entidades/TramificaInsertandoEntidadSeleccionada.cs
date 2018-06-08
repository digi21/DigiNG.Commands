using System;
using System.Linq;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Shell;
using Digi21.DigiNG.Topology;
using Digi21.Math;
using Digi21.Utilities;

namespace DigiNG.Commands.Operaciones_con_entidades
{
    /// <summary>
    /// Esta orden solicita al usuario que se seleccione una entidad y localiza todas las intersecciones de esta entidad con el resto de entidades del archivo
    /// de dibujo. Insertará en la entidad seleccionada un vértice por cada vértice localizado en otras entidades.
    /// </summary>
    [LocalizableCommand(typeof(Recursos), "TramificaInsertandoEntidadSeleccionadaName")]
    [LocalizableCommandInMenu(typeof(Recursos), "TramificaInsertandoEntidadSeleccionadaTitle", MenuItemGroup.GeometricAnalysisGroup1Group1)]
    public class TramificaInsertandoEntidadSeleccionada : Command
    {
        public TramificaInsertandoEntidadSeleccionada()
        {
            Initialize += InvitaAlUsuarioASeleccionarLínea;
            SetFocus += InvitaAlUsuarioASeleccionarLínea;
            DataUp += TramificaInsertandoEntidadSeleccionada_DataUp;
            EntitySelected += TramificaInsertandoEntidadSeleccionada_EntitySelected;
        }

        private static void TramificaInsertandoEntidadSeleccionada_DataUp(object sender, Point3DEventArgs e)
        {
            Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates, entidad => entidad is ReadOnlyLine);
        }

        private static void InvitaAlUsuarioASeleccionarLínea(object sender, EventArgs e)
        {
            Digi3D.StatusBar.Text = Recursos.SeleccionaLíneaATramificar;
        }

        private void TramificaInsertandoEntidadSeleccionada_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            var líneaATramificar = e.Entity as ReadOnlyLine;

            // Seleccionamos las líneas visibles del archivo de dibujo (excluyendo la línea que vamos a tramificar)
            var líneasContraLasCualesTramificar = from entidad in Digi21.DigiNG.DigiNG.DrawingFile.ExplotaAReadOnlyLine().Visibles()
                                                  where entidad != e.Entity
                                                  select entidad;

            // Obtenemos las intersecciones de la línea seleccionada con el reto de líneas visibles
            var intersecciones = líneaATramificar.DetectIntersections(líneasContraLasCualesTramificar);

            // Construimos una línea nueva con los códigos de la línea a tramificar
            var líneaNueva = new Line(líneaATramificar.Codes);

            // Recorremos todos los segmentos de la línea a tramificar
            for (var vértice = 0; vértice < líneaATramificar.Points.Count - 1; vértice++)
            {
                // Añadimos el vértice de la línea original
                líneaNueva.Points.Add(líneaATramificar.Points[vértice]);

                // Ahora localizamos únicamente las intersecciones localizadas para el segmento actual en la línea a tramificar
                var vérticesAAñadirEnEsteSegmento = intersecciones.SoloDeSegmento(líneaATramificar, vértice).ToArray();

                // Tenemos una lista de vértices, pero pueden venir desordenados. Vamos a ordenarlos calculando su distancia a la coordenada del primer vértice de este segmento
                var vérticeComienzoSegmento = (Point2D)líneaATramificar.Points[vértice];

                var vérticesOrdenados = from v in vérticesAAñadirEnEsteSegmento
                                        let distancia = (v.Key - vérticeComienzoSegmento).Module
                                        orderby distancia
                                        select v.Key;

                // Ahora insertamos estos vértices en la línea nueva. Los vértices son 2D (los métodos de extensión proporcionados por el tipo IntersectionDetector trabajan con Point2D
                // así que tendremos que ínterpolar la coordenada Z
                foreach (var v in vérticesOrdenados)
                {
                    var segmento = new Segment(líneaATramificar.Points[vértice], líneaATramificar.Points[vértice + 1]);
                    var z = segmento.InterpolatedZ(v);

                    líneaNueva.Points.Add(new Point3D(v.X, v.Y, z));
                }
            }

            // Por último añadimos el último vértice
            líneaNueva.Points.Add(líneaATramificar.Points.Last());

            Digi21.DigiNG.DigiNG.DrawingFile.Add(líneaNueva);
            Digi21.DigiNG.DigiNG.DrawingFile.Delete(e.Entity);
            Dispose();
        }
    }
}
