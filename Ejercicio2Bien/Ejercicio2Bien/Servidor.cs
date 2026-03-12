using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Ejercicio2Bien
{
    internal class Servidor
    {
        List<Usuario> usuarios = new List<Usuario>();
        public bool ServerRunning { set; get; } = true;
        public int Port { set; get; } = 31416;
        private Socket s;
        int opc = 1;
        public void Init()
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, Port);

            using (s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                bool bandera = false;
                while (!bandera)
                {
                    try
                    {
                        ie = new IPEndPoint(IPAddress.Any, Port);
                        s.Bind(ie);
                        bandera = true;
                    }
                    catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                    {
                        Console.WriteLine($"El puerto {Port} está en uso");
                        Port++;
                    }
                }
            


            s.Listen(10);
            Console.WriteLine($"Servidor iniciado. " +
            $"Escuchando en {ie.Address}:{ie.Port}");
            Console.WriteLine("Esperando conexiones... (Ctrl+C para salir)");

            while (ServerRunning)
            {

                try
                {
                    Socket client = s.Accept();

                    Thread hilo = new Thread(() => ClientDispatcher(client));

                    hilo.IsBackground = true;
                    hilo.Start();
                }
                catch (SocketException e)
                {

                }
                catch (ObjectDisposedException e2)
                {

                }
            }
            }
        }

        public void ClientDispatcher(Socket client)
        {
            using (client)
            {
                IPEndPoint ieClient = (IPEndPoint)client.RemoteEndPoint;
                Console.WriteLine($"Cliente conectado:{ieClient.Address} " +
                $"en puerto {ieClient.Port}");
                Encoding codificacion = Console.OutputEncoding;
                using (NetworkStream ns = new NetworkStream(client))
                using (StreamReader sr = new StreamReader(ns, codificacion))
                using (StreamWriter sw = new StreamWriter(ns, codificacion))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine("Escribe tu nombre de usuario:");
                    string nombre = sr.ReadLine();

                    if (nombre == null)
                    {
                        return;
                    }

                    Usuario usuario = new Usuario();

                    usuario.setNombre(nombre);
                    usuario.setIp(ieClient.Address.ToString());
                    usuario.setStreamWriter(sw);

                    lock (usuarios)
                    {
                        usuarios.Add(usuario);

                        for (int i = 0; i < usuarios.Count; i++)
                        {
                            if (usuarios[i] != usuario)
                            {
                                usuarios[i].getStreamWriter().WriteLine($"{usuario.getNombre()}@{usuario.getIp()} se ha conectado");
                            }
                        }
                    }

                    bool conectado = true;
                    try
                    {
                        while (conectado)
                        {
                            string mensaje = sr.ReadLine();

                            if (mensaje == null)
                            {
                                conectado = false;
                            }
                            else if (mensaje.Length == 0)
                            {

                            }
                            else if (mensaje.Equals("#list"))
                            {

                                lock (usuarios)
                                {
                                    sw.WriteLine("Lista usuarios:");
                                    for (int i = 0; i < usuarios.Count; i++)
                                    {
                                        sw.WriteLine($"{usuarios[i].getNombre()}@{usuarios[i].getIp()}");
                                    }
                                }

                            }
                            else if (mensaje.Equals("#exit"))
                            {
                                sw.WriteLine("Desconectandose");
                                conectado = false;
                            }
                            else
                            {
                                lock (usuarios)
                                {
                                    for (int i = 0; i < usuarios.Count; i++)
                                    {
                                        if (usuarios[i] != usuario)
                                        {
                                            usuarios[i].getStreamWriter().WriteLine($"{usuario.getNombre()}@{usuario.getIp()}:{mensaje}");
                                        }

                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }

                        lock (usuarios)
                        {
                            usuarios.Remove(usuario);

                            for (int i = 0; i < usuarios.Count; i++)
                            {
                                usuarios[i].getStreamWriter().WriteLine($"{usuario.getNombre()}@{usuario.getIp()} se a desconectado");
                            }
                        }
                    
                }
            }
        }
    }
}
