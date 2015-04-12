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
    [LocalizableCommand(typeof(OrdenesDigiNG.Recursos), "CortarName")]
    [LocalizableCommandInMenuAttribute(typeof(OrdenesDigiNG.Recursos), "CortarTitle", MenuItemGroup.EditGroup4)]
    public class Cortar : Command
    {
        public Cortar()
        {
            this.Initialize += new EventHandler(Cortar_Initialize);
            this.SetFocus += new EventHandler(Cortar_SetFocus);
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(Cortar_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(Cortar_EntitySelected);
        }

        void Cortar_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            try
            {
                ReadOnlyLine límite = e.Entity as ReadOnlyLine;

                var entidadesRecortables = (from entidad in DigiNG.DrawingFile
                                            where entidad is IClippable
                                            where !entidad.Deleted
                                            where entidad.AlgúnCódigoVisible()
                                            where Window2D.Intersects(límite, entidad)
                                            select entidad as IClippable).ToList();

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
                        var partes = entidadesRecortables[i].Clip(límite);
                        entidadesAAñadir.AddRange(partes);
                        entidadesAEliminar.Add(entidadesRecortables[i] as Entity);

                    }
                    catch (Exception)
                    {
                    }
                }

                string descripción = string.Format(OrdenesDigiNG.Recursos.SePartieronXEntidadesYSeFormaronYEntidadesNuevas,
                    entidadesAEliminar.Count,
                    entidadesAAñadir.Count);
                Digi3D.Music(MusicType.EndOfLongProcess);
                Digi3D.ShowBallon("Cortar",
                    descripción,
                    2);

                DigiNG.DrawingFile.Add(entidadesAAñadir);
                DigiNG.DrawingFile.Delete(entidadesAEliminar);
            }
            finally
            {
                Dispose();
            }
        }

        void Cortar_DataUp(object sender, Digi21.Math.Point3DEventArgs e)
        {
            DigiNG.SelectEntity(e.Coordinates, entidad => entidad is ReadOnlyLine);
        }

        void Cortar_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void Cortar_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionaEntidad();
        }

        void SolicitaSeleccionaEntidad()
        {
            Digi3D.StatusBar.Text = OrdenesDigiNG.Recursos.SeleccionaLaLíneaLímite;
        }

    }
}
