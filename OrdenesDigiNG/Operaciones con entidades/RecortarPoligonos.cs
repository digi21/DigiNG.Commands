using System;
using System.Collections.Generic;
using System.Linq;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG;
using Digi21.Math;
using Digi21.Utilities;
using Digi21.DigiNG.Plugin.Shell;

namespace Ordenes.OperacionesConEntidades
{
    [Command(Name = "recortar_poligonos")]
    [CommandInMenu("Recortar polígonos", MenuItemGroup.EditGroup9Group15)]
    public class RecortarPoligonos : Command
    {
        public RecortarPoligonos()
        {
            this.Initialize += new EventHandler(RecortarPoligonos_Initialize);
            this.SetFocus += new EventHandler(RecortarPoligonos_SetFocus);
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(RecortarPoligonos_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(RecortarPoligonos_EntitySelected);
        }

        void RecortarPoligonos_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            try
            {
                ReadOnlyLine límite = e.Entity as ReadOnlyLine;

                var entidadesRecortables = (from entidad in DigiNG.DrawingFile
                                            where entidad != límite
                                            where entidad is IClippable
                                            where !entidad.Deleted
                                            where entidad.AlgúnCódigoVisible()
                                            where Window2D.Intersects(límite, entidad)
                                            select entidad as ITrimable).ToList();

                List<Entity> entidadesAAñadir = new List<Entity>();
                List<Entity> entidadesAEliminar = new List<Entity>();
                int oldPorciento = -1;
                for (int i = 0; i < entidadesRecortables.Count; i++)
                {
                    int porciento = i * 100 / entidadesRecortables.Count;
                    if (porciento != oldPorciento)
                    {
                        oldPorciento = porciento;
                        Digi3D.StatusBar.ProgressBar.Value = porciento;
                    }
                    try
                    {
                        var partes = entidadesRecortables[i].Trim(límite);
                        entidadesAAñadir.AddRange(partes);
                        entidadesAEliminar.Add(entidadesRecortables[i] as Entity);
                    }
                    catch (Exception)
                    {
                    }
                }

                string descripción = string.Format("Se partieron {0} entidades y se formaron {1} entidades nuevas",
                    entidadesAEliminar.Count,
                    entidadesAAñadir.Count);
                Digi3D.Music(MusicType.EndOfLongProcess);
                Digi3D.ShowBallon("Recortar poligonos",
                    descripción,
                    2);

                DigiNG.DrawingFile.Add(entidadesAAñadir);
                DigiNG.DrawingFile.Delete(entidadesAEliminar);

                if (this.Args.Length != 0 && DigiNG.DrawingFile.Contains(límite))
                    DigiNG.DrawingFile.Delete(límite);
            }
            finally
            {
                Dispose();
            }
        }

        void RecortarPoligonos_DataUp(object sender, Digi21.Math.Point3DEventArgs e)
        {
            DigiNG.SelectEntity(e.Coordinates, entidad => entidad is ReadOnlyLine);
        }

        void RecortarPoligonos_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void RecortarPoligonos_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void SolicitaSeleccionaEntidad()
        {
            Digi3D.StatusBar.Text = "Selecciona la línea de límite";
        }

    }
}
