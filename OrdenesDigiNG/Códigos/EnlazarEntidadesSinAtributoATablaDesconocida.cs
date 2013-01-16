using System;
using System.Collections.Generic;
using Digi21.Digi3D;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;

namespace Ordenes.Códigos
{
    [Command(Name = "enlazar_entidades_sin_atributo_a_tabla_desconocida")]
    public class EnlazarEntidadesSinAtributoATablaDesconocida : Command
    {
        public EnlazarEntidadesSinAtributoATablaDesconocida()
        {
            this.Initialize += new EventHandler(EnlazarEntidadesSinAtributoATablaDesconocida_Initialize);
        }

        void EnlazarEntidadesSinAtributoATablaDesconocida_Initialize(object sender, EventArgs e)
        {
            try
            {
                var diccionarioNúmerosDeTabla = DigiNG.DrawingFile.DatabaseTables;
                if (0 == diccionarioNúmerosDeTabla.Count)
                {
                    Digi3D.Music(MusicType.Error);
                    Digi3D.ShowBallon("enlazar_entidades_sin_atributo_a_tabla_desconocida", "No está conectado a la base de datos", 2);
                    return;
                }

                List<Entity> entidadesAAñadir = new List<Entity>();
                List<Entity> entidadesAEliminar = new List<Entity>();

                foreach (var entidad in DigiNG.DrawingFile)
                {
                    if (entidad.Deleted)
                        continue;

                    bool sw = false;
                    List<Code> códigosNuevos = new List<Code>(entidad.Codes);
                    for (int i = 0; i < códigosNuevos.Count; i++)
                    {
                        string tablaConLaQueEnlazaElCódigoPrincipal = DigiNG.DigiTab[códigosNuevos[i].Name].Table;

                        if (!códigosNuevos[i].Table.HasValue && tablaConLaQueEnlazaElCódigoPrincipal != "")
                        {
                            sw = true;

                            // Lo hago con un try-catch porque es posible que no exista el número de tabla
                            try {
                                códigosNuevos[i] = new Code(
                                    códigosNuevos[i].Name, 
                                    diccionarioNúmerosDeTabla[tablaConLaQueEnlazaElCódigoPrincipal],
                                    900000000);
                            }
                            catch(Exception)
                            {
                                Digi3D.OutputWindow.WriteLine(string.Format("No se pudo averiguar el número de tabla para el código {0}", códigosNuevos[i].Name));
                            }

                        }
                    }

                    if (!sw)
                        continue;

                    entidadesAEliminar.Add(entidad);

                    Entity clón = entidad.Clone();
                    for (int i = 0; i < códigosNuevos.Count; i++)
                        clón.Codes[i] = códigosNuevos[i];

                    entidadesAAñadir.Add(clón);
                }

                if (0 != entidadesAAñadir.Count)
                {
                    DigiNG.DrawingFile.Add(entidadesAAñadir);
                    DigiNG.DrawingFile.Delete(entidadesAEliminar);
                }

                Digi3D.Music(MusicType.EndOfLongProcess);
            }
            finally
            {
                Dispose();
            }
        }
    }
}
