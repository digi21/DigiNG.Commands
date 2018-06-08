using System;
using System.Linq;
using System.Windows.Forms;
using Digi21.Digi3D;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.DigiNG.Plugin.Shell;
using Digi21.Math;

namespace DigiNG.Commands.Códigos
{
    [LocalizableCommand(typeof(Recursos), "CopiarAtributosBBDDName")]
    [LocalizableCommandInMenu(typeof(Recursos), "CopiarAtributosBBDDTitle", MenuItemGroup.EditGroup6)]
    public class CopiarAtributosBBDD : Command
    {
        private Entity entidadOrigen;

        public CopiarAtributosBBDD()
        {
            Initialize += CopiarAtributosBBDD_Initialize;
            SetFocus += CopiarAtributosBBDD_SetFocus;
            DataUp += CopiarAtributosBBDD_DataUp;
            EntitySelected += CopiarAtributosBBDD_EntitySelected;
        }

        private void CopiarAtributosBBDD_EntitySelected(object sender, EntitySelectedEventArgs e)
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

            // Copiamos los atributos de los códigos coincidentes
            var nombresCódigosEntidadOrigen = from código in entidadOrigen.Codes
                                              select código.Name;
            var nombresCodigosEntidadDestino = from código in e.Entity.Codes
                                               select código.Name;
            var códigosComunes = nombresCódigosEntidadOrigen.Intersect(nombresCodigosEntidadDestino).ToList();

            if (códigosComunes.Count == 0)
            {
                Digi3D.ShowBallon(
                    Recursos.CopiarAtributosBBDDName, 
                    Recursos.LaEntidadOrigenYDestinoNoTienenCódigosComunes, 
                    1000);
                Digi3D.Music(MusicType.Error);
            }
            else
            {
                var entidadClonada = e.Entity.Clone();

                EliminaEnlacesABBDD(entidadClonada);

                Digi21.DigiNG.DigiNG.DrawingFile.Add(entidadClonada,
                    entidadOrigen.Owner.get_DatabaseAttributes(entidadOrigen));
                Digi3D.Music(MusicType.EndOfLongProcess);
                Digi21.DigiNG.DigiNG.DrawingFile.Delete(e.Entity);
            }

            entidadOrigen = null;
            SolicitaSeleccionarEntidad();
        }

        private static void EliminaEnlacesABBDD(Entity entidadClonada)
        {
            // Eliminamos los enlaces a BBDD de los distintos códigos para asegurarnos de que al añadir la entidad se generan altas nuevas. No queremos que los códigos 
            // apunten a los registros a los que están apuntando, queremos altas nuevas, porque puede incluso que la entidad de la cual estamos obteniendo los atributos 
            // ni siquiera pertenezca al archivo de dibujo activo.
            for (var i = 0; i < entidadClonada.Codes.Count; i++)
                entidadClonada.Codes[i] = new Code(entidadClonada.Codes[i].Name);
        }

        private void CopiarAtributosBBDD_DataUp(object sender, Point3DEventArgs e)
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

        private void CopiarAtributosBBDD_SetFocus(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void CopiarAtributosBBDD_Initialize(object sender, EventArgs e)
        {
            SolicitaSeleccionarEntidad();
        }

        private void SolicitaSeleccionarEntidad()
        {
            Digi3D.StatusBar.Text = entidadOrigen == null ? Recursos.SeleccionaLaEntidadOrigen : Recursos.SeleccionaLaEntidadDestino;
        }
    }
}
