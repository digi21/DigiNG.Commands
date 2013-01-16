using System;
using System.Linq;
using System.Windows.Forms;
using Digi21.Digi3D;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Math;

namespace Ordenes.Códigos
{
    [Command(Name = "copiar_tabla_registro")]
    public class CopiarTablaRegistro : Command
    {
        private Entity entidadOrigen = null;

        public CopiarTablaRegistro()
        {
            this.Initialize += new EventHandler(CopiarTablaRegistro_Initialize);
            this.SetFocus += new EventHandler(CopiarTablaRegistro_SetFocus);
            this.DataUp += new EventHandler<Digi21.Math.Point3DEventArgs>(CopiarTablaRegistro_DataUp);
            this.EntitySelected += new EventHandler<EntitySelectedEventArgs>(CopiarTablaRegistro_EntitySelected);
        }

        private void CopiarTablaRegistro_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (e.Entity.Codes.Count > 1)
            {
                Digi3D.Music(MusicType.Error);
                MessageBox.Show("Has seleccionado una entidad con más de un código. Esta orden aún no está preparada para este escenario.");
                SolicitaSeleccionarEntidad();
                return;
            }

            if (null == entidadOrigen)
            {
                if (!e.Entity.Codes[0].Table.HasValue || !e.Entity.Codes[0].Id.HasValue)
                    MessageBox.Show("La entidad que has seleccionado no tiene ningún enlace de base de datos.");
                else
                    entidadOrigen = e.Entity;

                SolicitaSeleccionarEntidad();
                return;
            }

            Entity entidadClonada = e.Entity.Clone();
            entidadClonada.Codes[0] = new Code(entidadClonada.Codes[0].Name, entidadOrigen.Codes[0].Table, entidadOrigen.Codes[0].Id);
            DigiNG.DrawingFile.Add(entidadClonada);
            DigiNG.DrawingFile.Delete(e.Entity);
            entidadOrigen = null;
            Digi3D.Music(MusicType.EndOfLongProcess);
            SolicitaSeleccionarEntidad();
        }

        private void CopiarTablaRegistro_DataUp(object sender, Point3DEventArgs e)
        {
            // Aquí ordenamos a DigiNG que localice entidades en las coordenadas en las que el usuario ha hecho clic con el ratón.

            // Tenemos dos casos: Si estamos solicitando al usuario la entidad origen permitimos seleccionar todas las entidades posibles, pero si estamos seleccionando el destino
            // únicamente si la entidad pertenece al archivo de dibujo (y no a los archivos de referencia). Eso se resuelve con una expresión lambda que devuelve verdadero únicamente
            // si la entidad indicada pertenece al archivo de dibujo.
            if (entidadOrigen == null )
                DigiNG.SelectEntity(e.Coordinates);
            else
                DigiNG.SelectEntity(e.Coordinates, entidad => DigiNG.DrawingFile.Contains(entidad));
        }

        private void CopiarTablaRegistro_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void CopiarTablaRegistro_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void SolicitaSeleccionarEntidad()
        {
            if (entidadOrigen == null)
                Digi3D.StatusBar.Text = "Selecciona la entidad origen...";
            else
                Digi3D.StatusBar.Text = "Selecciona la entidad destino...";
        }
    }
}
