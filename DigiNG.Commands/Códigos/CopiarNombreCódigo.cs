using System;
using System.Linq;
using System.Windows.Forms;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Menu;
using Digi21.Math;

namespace DigiNG.Commands.Códigos
{
    /// <summary>
    /// Esta orden solicita que se seleccione una entidad origen y una destino. Copiará el nombre del código de la entidad origen en la destino sin sustituir
    /// los enlaces de BBDD que tuviera la entidad destino.
    /// </summary>
    [LocalizableCommand(typeof(Recursos), "CopiarNombreCódigoName")]
    [LocalizableCommandInMenu(typeof(Recursos), "CopiarNombreCódigoTitle", MenuItemGroup.EditGroup6)]
    public class CopiarNombreCódigo : Command
    {
        private Entity entidadOrigen;

        public CopiarNombreCódigo()
        {
            Initialize += CopiarNombreCódigo_Initialize;
            SetFocus += CopiarNombreCódigo_SetFocus;
            DataUp += CopiarNombreCódigo_DataUp;
            EntitySelected += CopiarNombreCódigo_EntitySelected;
        }

        private void CopiarNombreCódigo_EntitySelected(object sender, EntitySelectedEventArgs e)
        {
            if (e.Entity.Codes.Count > 1)
            {
                Digi3D.Music(MusicType.Error);
                MessageBox.Show(Recursos.HasSeleccionadoUnaEntidadConMasDeUnCodigoEstaOrdenNoEstaPreparada);
                SolicitaSeleccionarEntidad();
                return;
            }

            if (null == entidadOrigen)
            {
                entidadOrigen = e.Entity;
                SolicitaSeleccionarEntidad();
                return;
            }

            var entidadClonada = e.Entity.Clone();
            entidadClonada.Codes[0] = new Code(entidadOrigen.Codes[0].Name, entidadClonada.Codes[0].Table, entidadClonada.Codes[0].Id);
            Digi21.DigiNG.DigiNG.DrawingFile.Add(entidadClonada);
            Digi21.DigiNG.DigiNG.DrawingFile.Delete(e.Entity);
            entidadOrigen = null;
            Digi3D.Music(MusicType.EndOfLongProcess);
            SolicitaSeleccionarEntidad();
        }

        private void CopiarNombreCódigo_DataUp(object sender, Point3DEventArgs e)
        {
            // Aquí ordenamos a DigiNG que localice entidades en las coordenadas en las que el usuario ha hecho clic con el ratón.

            // Tenemos dos casos: Si estamos solicitando al usuario la entidad origen permitimos seleccionar todas las entidades posibles, pero si estamos seleccionando el destino
            // únicamente si la entidad pertenece al archivo de dibujo (y no a los archivos de referencia). Eso se resuelve con una expresión lambda que devuelve verdadero únicamente
            // si la entidad indicada pertenece al archivo de dibujo.
            if (entidadOrigen == null)
                Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates);
            else
                Digi21.DigiNG.DigiNG.SelectEntity(e.Coordinates, entidad => Digi21.DigiNG.DigiNG.DrawingFile.Contains(entidad));
        }

        private void CopiarNombreCódigo_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void CopiarNombreCódigo_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void SolicitaSeleccionarEntidad()
        {
            Digi3D.StatusBar.Text = entidadOrigen == null ? Recursos.SeleccionaLaEntidadOrigen : Recursos.SeleccionaLaEntidadDestino;
        }
    }
}
