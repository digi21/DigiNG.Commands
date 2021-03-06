﻿using System;
using System.Collections.Generic;
using Digi21.Digi3D;
using Digi21.DigiNG;
using Digi21.DigiNG.Entities;
using Digi21.DigiNG.Plugin.Command;
using Digi21.Utilities;

namespace DigiNG.Commands.Modelo_de_datos
{
    /// <summary>
    /// Almacena el valor del área mínima así como el código del puntual por el cual sustituir una determinada línea si su área es inferior al valor indicado.
    /// </summary>
    internal struct DatoSustituyeAreaPorPuntual
    {
        public double Area { get; set; }
        public string CódigoPunto { get; set; }
    }

    // Pongo la clase internal porque esta funcionalidad ha sido implementada en Digi.tab con la funcionalidad de desencadenadores

    /// <summary>
    ///  Implementa un proceso que sustituye líneas (cerradas) por puntuales en función de su area.
    /// </summary>
    /// <remarks>
    /// Esta orden es un proceso, ya que auto-elimina de la pila de órdenes con una llamada a <c>DigiNG.Commands.Pop()</c> y se conecta con el evento
    /// <c>DigiNG.AddingEntity</c>. Si la entidad que se está almacenando tiene entre sus códigos alguno de los almacenados en el diccionario de códigos, y su
    /// área es inferior a la mínima para ese determinado código, el proceso la escritura de esa entidad modificando la propiedad <c>AddingEntityEventArgs.Cancel</c>,
    /// ejecutará la orden PUNTO_R y enviará la coordenada del centro de la entidad que se acaba de cancelar como punto de origen de la orden PUNTO_R de modo que esta
    /// únicamente le solicite al usuario por el segundo punto (el que indica la rotación del puntual).
    /// </remarks>
    [Command(Name = "sustituye_area_por_puntual")]
    internal class SustituyeAreaPorPuntual : Command
    {
        /// <summary>
        /// Diccionario que almacena el código de la línea y los valores de área mínima y el código del puntual para el caso de que sea necesario sustituir la línea por un puntual.
        /// </summary>
        private readonly Dictionary<string, DatoSustituyeAreaPorPuntual> códigos = new Dictionary<string, DatoSustituyeAreaPorPuntual>();

        /// <summary>
        /// Constructor de la orden.
        /// </summary>
        /// <remarks>
        /// Subscribe la orden al evento <c>DigiNG.AddingEntity</c>
        /// </remarks>
        public SustituyeAreaPorPuntual()
        {
            Initialize += SustituyeAreaPorPuntual_Initialize;
            Disposing += SustituyeAreaPorPuntual_Disposing;

            // Nos suscribimos al evento AddingEntity para analizar las entidades que se pretenden almacenar.
            Digi21.DigiNG.DigiNG.AddingEntity += DigiNG_AddingEntity;
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
        private void SustituyeAreaPorPuntual_Initialize(object sender, EventArgs e)
        {
            // Analizamos los argumentos y rellenamos el diccionario de códigos a analizar.
            if (Args.Length % 3 != 0)
                throw new Exception("Número incorrecto de parámetros. El formato es: sustituye_area_por_puntual=([código de área] [área mínima] [código de puntual])*");

            for (var i = 0; i < Args.Length; i += 3)
            {
                if (!double.TryParse(Args[i + 1], out var area))
                    throw new Exception(
                        $"El parámetro número {i + 2} ({Args[i + 1]}) no es un valor de distancia válido.");

                códigos[Args[i + 0]] = new DatoSustituyeAreaPorPuntual { Area = area, CódigoPunto = Args[i + 2] };
            }

            // Eliminamos la orden de la pila de órdenes de DigiNG para que se convierta en un proceso, de modo que si el usuario pulsa la tecla Esc o ejecuta
            // alguna orden como ELIMINA_ORDENES, no se pueda destruir esta orden.
            Digi21.DigiNG.DigiNG.Commands.Pop();
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
        private void SustituyeAreaPorPuntual_Disposing(object sender, EventArgs e)
        {
            Digi21.DigiNG.DigiNG.AddingEntity -= DigiNG_AddingEntity;
        }

        /// <summary>
        /// Manejador de eventos para el evento <c>DigiNG.AddingEntity</c>
        /// </summary>
        /// <remarks>
        /// En caso de que la entidad a almacenar sea una lína, comprueba esta tiene entre sus códigos alguno de entre los enumerados por el usuario en la línea de 
        /// comandos. Si es así, comprueba el área y si esta es inferior al mínimo indicado por el usuario en la línea de comandos, cancela su escritura y ejecuta la
        /// orden PUNTO_R para que el usuario introduzca un segundo punto con la rotación del puntual.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigiNG_AddingEntity(object sender, AddingEntityEventArgs e)
        {
            if (!SustituirOEliminarEntidadesPorArea.Sustituir)
                return;

            if (!(e.Entity is ReadOnlyLine))
                return;

            var línea = (ReadOnlyLine)e.Entity;

            if (!línea.Closed)
                return;

            foreach (var nombreCódigo in códigos.Keys)
            {
                if (!línea.TieneElCódigo(nombreCódigo)) continue;

                var area = Math.Abs(Digi21.DigiNG.DigiNG.GeographicCalculator.CalculateArea(línea));
                if (!(area < códigos[nombreCódigo].Area)) continue;

                Digi3D.ShowBallon(
                    "Sustitución de área por puntual",
                    $"Se va a sustituir su área con código {nombreCódigo} por un puntual con código {códigos[nombreCódigo].CódigoPunto} porque su área {area} es inferior a {códigos[nombreCódigo].Area}",
                    2);

                Digi21.DigiNG.DigiNG.DrawEntity(e.Entity, DrawingMode.Hide);
                e.Cancel = true;

                // En vez de ejecutar la orden PUNTO_R, ejecutamos su GUID, de modo que esta extensión sea compatible independientemente del idioma seleccionado por el usuario para Digi3D.
                Digi21.DigiNG.DigiNG.Commands.Push("{D768CDA8-0BFA-465f-BC53-92F8C3D52786}=" + códigos[nombreCódigo].CódigoPunto, false);

                // Por último, generamos un evento de pulsación de pedal/botón de dato con las coordenadas del centro de la entidad que no se va a almacenar, para así hacer que la orden PUNTO_R
                // únicamente le solicite al usuario el segundo punto (el que indica la rotación del punto).
                Digi21.DigiNG.DigiNG.Commands.Top.OnDataDown(e.Entity.Center);
                return;
            }
        }

    }
}
