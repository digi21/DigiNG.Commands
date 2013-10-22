using System;
using System.Collections.Generic;
using Digi21.Digi3D;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Utilities;

namespace Ordenes.ModeloDeDatos
{
    /// <summary>
    ///  Implementa un proceso que cancela la escritura de entidades de tipo línea con un área inferior a la especificada por parámetros.
    /// </summary>
    /// <remarks>
    /// Esta orden es un proceso, ya que auto-elimina de la pila de órdenes con una llamada a <c>DigiNG.Commands.Pop()</c> y se conecta con el evento
    /// <c>DigiNG.AddingEntity</c>. Si la entidad que se está almacenando tiene entre sus códigos alguno de los almacenados en el diccionario de códigos, y su
    /// área es inferior a la mínima para ese determinado código, el proceso bloqueará la escritura de esa entidad modificando la propiedad <c>AddingEntityEventArgs.Cancel</c>
    /// </remarks>
    [Command(Name="eliminar_entidades_area_pequeña")]
    public class EliminarEntidadesAreaPequeña : Command
    {
        /// <summary>
        /// Diccionario donde almacenaremos por cada código el valor del área mínima exigible.
        /// </summary>
        private Dictionary<string, double> códigos = new Dictionary<string, double>();

        /// <summary>
        /// Constructor de la orden.
        /// </summary>
        /// <remarks>
        /// Subscribe la orden al evento <c>DigiNG.AddingEntity</c>
        /// </remarks>
        public EliminarEntidadesAreaPequeña()
        {
            this.Initialize += new EventHandler(EliminarEntidadesAreaPequeña_Initialize);
            this.Disposing += new EventHandler(EliminarEntidadesAreaPequeña_Disposing);
            DigiNG.AddingEntity += new EventHandler<AddingEntityEventArgs>(DigiNG_AddingEntity);
        }

        /// <summary>
        /// Manejador de eventos del evento <c>Command.Initialize</c>  
        /// </summary>
        /// <remarks>
        /// Rellena el diccionario de código-área en función de los parámetros proporcionados por el usuario en la línea de comandos.
        /// Convierte la orden en un proceso/servicio al desapilarse de la pila de órdenes mediante una llamada a <c>DigiNG.Commands.Pop</c>.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EliminarEntidadesAreaPequeña_Initialize(object sender, EventArgs e)
        {
            // Analizamos los argumentos y rellenamos el diccionario de códigos a analizar.
            if (Args.Length % 2 != 0)
                throw new Exception("Número incorrecto de parámetros. El formato es: eliminar_entidades_area_pequeña=([código] [área])*");

            for (int i = 0; i < Args.Length; i += 2)
            {
                double distancia;
                if( !double.TryParse(Args[i + 1], out distancia) )
                    throw new Exception(string.Format("El parámetro número {0} ({1}) no es un valor de distancia válido.", i+2, Args[i + 1]));

                códigos[Args[i + 0]] = distancia;
            }

            // Eliminamos la orden de la pila de órdenes de DigiNG para que se convierta en un proceso, de modo que si el usuario pulsa la tecla Esc o ejecuta
            // alguna orden como ELIMINA_ORDENES, no se pueda destruir esta orden.
            DigiNG.Commands.Pop();
        }

        /// <summary>
        /// Manejador de eventos del evento <c>Command.Disposing</c>
        /// </summary>
        /// <remarks>
        /// Cancelam la subscripción al evento <c>DigiNG.AddingEntity</c> para que no exista ninguna referencia a este objeto, de modo que pueda ser destruida en el 
        /// siguiente ciclo de recolección de basura.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EliminarEntidadesAreaPequeña_Disposing(object sender, EventArgs e)
        {
            DigiNG.AddingEntity -= DigiNG_AddingEntity;
        }
        
        /// <summary>
        /// Manejador de eventos para el evento <c>DigiNG.AddingEntity</c>
        /// </summary>
        /// <remarks>
        /// En caso de que la entidad a almacenar sea una lína, comprueba esta tiene entre sus códigos alguno de entre los enumerados por el usuario en la línea de 
        /// comandos. Si es así, comprueba el área y si esta es inferior al mínimo indicado por el usuario en la línea de comandos, cancela su escritura.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DigiNG_AddingEntity(object sender, AddingEntityEventArgs e)
        {
            if (!SustituirOEliminarEntidadesPorArea.Sustituir)
                return;

            if (!(e.Entity is ReadOnlyLine))
                return;

            ReadOnlyLine línea = e.Entity as ReadOnlyLine;

            if (!línea.Closed)
                return;

            foreach (var nombreCódigo in códigos.Keys)
            {
                if (línea.TieneElCódigo(nombreCódigo))
                {
                    var area = System.Math.Abs(DigiNG.GeographicCalculator.CalculateArea(línea));

                    if (area < códigos[nombreCódigo])
                    {
                        Digi3D.ShowBallon(
                            "Entidad descartada",
                            string.Format("Se descartó la línea con código {0} porque su área {1} es inferior a {2}", nombreCódigo, area, códigos[nombreCódigo]),
                            2);

                        DigiNG.DrawEntity(e.Entity, DrawingMode.Hide);
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

    }
}
