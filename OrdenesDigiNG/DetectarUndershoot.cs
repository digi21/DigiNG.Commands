using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Topology;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.Utilities;
using Digi21.Math;
using Digi21.Digi3D;

namespace OrdenesDigiNG
{
    [LocalizableCommand(typeof(Recursos), "DetectarUndershootName")]
    public class DetectarUndershoot : Command
    {
        public DetectarUndershoot()
        {
            this.Initialize += new EventHandler(DetectarUndershoot_Initialize);
        }

        void DetectarUndershoot_Initialize(object sender, EventArgs e)
        {
            double tolerancia = double.Parse(this.Args[0]);

            var líneasDeInterés = DigiNG.DrawingFile.NoEliminadas().OfType<ReadOnlyLine>().QueTenganAlgúnCódigoConComodín(this.Args.Skip(1));
            var nodos = líneasDeInterés.DetectNodes();

            foreach (var nodo in nodos)
            {
                Digi3D.OutputWindow.WriteLine(nodo.Key.X.ToString());
            }

            foreach (var línea in líneasDeInterés)
            {
                var coordenadasCercanasAlOrigen = nodos.FindAll(
                    new Point2D(línea.FirstVertex.X, línea.FirstVertex.Y), 
                    tolerancia,
                    nodo => !nodo.LíneaLlegaANodo(línea));
                if (coordenadasCercanasAlOrigen.Count() != 0)
                    CreaTareas(línea, línea.FirstVertex, coordenadasCercanasAlOrigen);

                var coordenadasCercanasAlDestino = nodos.FindAll(
                    new Point2D(línea.LastVertex.X, línea.LastVertex.Y), 
                    tolerancia,
                    nodo => !nodo.LíneaLlegaANodo(línea));
                if (coordenadasCercanasAlDestino.Count() != 0)
                    CreaTareas(línea, línea.LastVertex, coordenadasCercanasAlDestino);
            }
        }

        private static void CreaTareas(ReadOnlyLine línea, Point3D puntoGotoTareaPrincipal, IEnumerable<Point2D> coordenadasCercanas)
        {
            var tareasPuntos = new List<ITask>();
            foreach (var coordenadaCercanaAlOrigen in coordenadasCercanas)
            {
                var mensaje = string.Format(Recursos.MagnitudX, (new Point2D(puntoGotoTareaPrincipal.X, puntoGotoTareaPrincipal.Y) - coordenadaCercanaAlOrigen).Module);
                tareasPuntos.Add(new TaskGotoPoint(new Point3D(coordenadaCercanaAlOrigen.X, coordenadaCercanaAlOrigen.Y, 0.0), mensaje, TaskSeverity.Error));
            }

            var mensajeTareaPrincipal = string.Format(Recursos.LocalizadoUndershootEnLaLíneaConCódigoX, línea.Codes[0].Name);
            var tareaPrincipal = new TaskEntityGotoPoint(
                puntoGotoTareaPrincipal,
                línea,
                2,
                mensajeTareaPrincipal,
                TaskSeverity.Error,
                DigiNG.DrawingFile.Path,
                Recursos.DetectarUndershootName,
                tareasPuntos.ToArray());

            Digi3D.Tasks.Add(tareaPrincipal);
        }
    }
}
