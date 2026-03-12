using System;
using System.Collections.Generic;
using System.Text;

namespace Ejercicio2Bien
{
    internal class Usuario
    {
        private string nombre;
        private string ip;
        private StreamWriter streamWriter;

        public void setNombre(string nombre)
        {
            this.nombre = nombre;
        }
        public string getNombre()
        {
            return nombre;
        }

        public void setIp(string ip)
        {
            this.ip = ip;
        }
        public string getIp()
        {
            return ip;
        }

        public void setStreamWriter(StreamWriter streamWriter)
        {
            this.streamWriter = streamWriter;
        }
        public StreamWriter getStreamWriter()
        {
            return streamWriter;
        }
    }
}
