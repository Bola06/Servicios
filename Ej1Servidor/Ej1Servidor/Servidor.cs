using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ej1Servidor
{
    internal class Ej1Servidor
                                
    {
        bool bandera = false;
        public bool ServerRunning { set; get; } = true;
        public int Port { get; set; } = 31416;
        private Socket s;
        public void InitServer()
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, Port);
            using (s = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp))
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
                        bandera = false;
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

        private void ClientDispatcher(Socket sClient)
        {
            using (sClient)
            {
                IPEndPoint ieClient = (IPEndPoint)sClient.RemoteEndPoint;
                Console.WriteLine($"Cliente conectado:{ieClient.Address} " +
                $"en puerto {ieClient.Port}");
                Encoding codificacion = Console.OutputEncoding;
                using (NetworkStream ns = new NetworkStream(sClient))
                using (StreamReader sr = new StreamReader(ns, codificacion))
                using (StreamWriter sw = new StreamWriter(ns, codificacion))
                {
                    sw.AutoFlush = true;
                    string welcome = "Hola, que quieres hacer?  -time  -date  -all  -close";
                    sw.WriteLine(welcome);
                    string? msg = "";
                    try
                    {
                        msg = sr.ReadLine();
                        string[] opc = [];
                      

                        if (msg != null)
                        {
                            opc = msg.Split(" ");

                            if (opc[0] == "time")
                            {
                                DateTime dt = DateTime.Now;
                                msg = dt.ToString("HH:mm:ss");
                                sw.WriteLine("Son las " + msg);
                                sClient.Close();
                                //  sw.AutoFlush = false;
                            }
                            else if (opc[0] == "date")
                            {
                                DateTime dt2 = DateTime.Now;
                                msg = dt2.ToString("d");
                                sw.Write(msg);
                                sClient.Close();
                            }
                            else if (opc[0] == "all")
                            {
                                sw.WriteLine(DateTime.Now);
                                sClient.Close();
                            }
                            else if (opc[0] == "close")
                            {
                                String contraseña = "";

                                string dire = Environment.GetEnvironmentVariable("PROGRAMDATA");
                                string ruta = dire + "\\" + "password.txt";

                                using (StreamReader sr2 = new StreamReader(ruta))
                                {
                                    contraseña = sr2.ReadToEnd().Trim();
                                }

                                try
                                {
                                    if (opc.Length >1)
                                    {
                                        if (opc[1].Trim() == contraseña)
                                        {
                                            sw.WriteLine("Servidor cerrado");
                                            sw.Flush();
                                            
                                            ServerRunning = false;
                                            s.Close();
                                            
                                        }
                                        else
                                        {
                                            sw.WriteLine("Error, la contraseña es incorrecta");
                                            //   s.Close();
                                            sw.AutoFlush = false;
                                        }
                                    }
                                }
                                catch (IndexOutOfRangeException e)
                                {
                                    sw.WriteLine("Error, no has seleccionado una contraseña");
                                }
                            }
                            else
                            {
                                sw.WriteLine("Error");

                                sClient.Close();
                                //  s.Close();
                                // sw.AutoFlush =false;
                            }
                        }
                        else
                        {

                        }
                    }
                    catch (IOException)
                    {
                        msg = null;
                    }

                    Console.WriteLine("Cliente desconectado.\nConexión cerrada");
                }
            }
        }
    }
}
