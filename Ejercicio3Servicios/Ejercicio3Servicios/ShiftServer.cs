using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ejercicio3Servicios
{
    internal class ShiftServer
    {
        public bool ServerRunning { get; set; } = true;
        String[] users = new string[0];
        List<String> waitQueue = new List<string>();

        public void ReadNames(string ruta)
        {
            string a = File.ReadAllText(ruta);
            users = a.Split(";");
        }

        public int ReadPin(string ruta)
        {
            try
            {
                using (StreamReader sr = new StreamReader(ruta))
                {
                    char[] buffer = new char[4];
                    sr.Read(buffer, 0, 4);
                    int num = int.Parse(new String(buffer));
                    return num;
                }
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int Port = 31416;
        int opc = 1;
        public void Init()
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, Port);

            using (Socket sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                bool bandera = false;
                while (!bandera)
                {
                    try
                    {
                        ie = new IPEndPoint(IPAddress.Any, Port);
                        sc.Bind(ie);
                        bandera = true;

                    }
                    catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                    {
                        Console.WriteLine($"El puerto {Port} está ocupado");
                        if (opc == 1)
                        {
                            Port = 1023;
                            opc = 0;
                        }
                        Port++;
                        bandera = false;
                    }
                }

                sc.Listen(5);

                Console.WriteLine($"Servidor iniciado. " +
                $"Escuchando en {ie.Address}:{ie.Port}");
                Console.WriteLine("Esperando conexiones... (Ctrl+C para salir)");

                string ruta = Environment.GetEnvironmentVariable("userprofile");
                string dire = ruta + "\\" + "usuarios.txt";

                ReadNames(dire);

                while (ServerRunning)
                {
                    try
                    {
                        Socket cliente = sc.Accept();

                        Thread hilo = new Thread(() => scCliente(cliente));

                        hilo.Start();

                    }
                    catch (SocketException s)
                    {

                    }
                }
            }
        }


        private void scCliente(Socket scCliente)
        {
            using (scCliente)
            {
                IPEndPoint ieClient = (IPEndPoint)scCliente.RemoteEndPoint;
                Console.WriteLine($"Cliente conectado:{ieClient.Address} " +
                $"en puerto {ieClient.Port}");
                Encoding codificacion = Console.OutputEncoding;

                using (NetworkStream ns = new NetworkStream(scCliente))
                using (StreamReader sr = new StreamReader(ns, codificacion))
                using (StreamWriter sw = new StreamWriter(ns, codificacion))
                {
                    sw.AutoFlush = true;

                    sw.WriteLine("Bienvenido, como te llamas?");
                    string nombre = sr.ReadLine();

                    if (nombre == null || (!users.Contains(nombre) && !nombre.Equals("admin")))
                    {
                        sw.WriteLine("Usuario no encontrado");

                        scCliente.Close();
                    }
                    else if (nombre.Equals("admin"))
                    {
                        string ruta = Environment.GetEnvironmentVariable("userprofile");
                        string dire = ruta + "\\" + "pin.txt";
                        int pin;

                        try
                        {
                            pin = ReadPin(dire);
                        }
                        catch (Exception ex)
                        {
                            pin = 1234;
                        }

                        sw.WriteLine("Cual es el PIN?");
                        int pinCliente;

                        try
                        {
                            pinCliente = int.Parse(sr.ReadLine());
                        }
                        catch
                        {
                            scCliente.Close();
                            return;
                        }


                        if (pinCliente.Equals(pin))
                        {
                            bool adminActivo = true;

                            while (adminActivo)
                            {
                                string[] msg = sr.ReadLine().Split(" ");


                                if (msg.Length == 2 && msg[0].Equals("del"))
                                {
                                    try
                                    {
                                        waitQueue.RemoveAt(int.Parse(msg[1]));
                                    }
                                    catch
                                    {
                                        sw.WriteLine("delete error");
                                    }
                                }
                                else if (msg.Length == 2 && msg[0].Equals("chpin"))
                                {
                                    if (msg[1].Length == 4)
                                    {
                                        try
                                        {
                                            File.WriteAllText(dire, msg[1]);
                                            sw.WriteLine("Se ha guardado el pin");
                                        }
                                        catch
                                        {
                                            sw.WriteLine("No Se ha guardado el pin");
                                        }

                                    }
                                    else
                                    {
                                        sw.WriteLine("No se ha podido guardar el pin");
                                    }
                                }
                                else if (msg[0].Equals("exit"))
                                {
                                    sw.WriteLine("Saliendo...");
                                    adminActivo = false;
                                }
                                else if (msg[0].Equals("shutdown"))
                                {
                                    string rutaQueue = Environment.GetEnvironmentVariable("userprofile")
                                                        + "\\" + "waitqueue.txt";

                                    File.WriteAllLines(rutaQueue, waitQueue);

                                    sw.WriteLine("Servidor apagándose...");
                                    ServerRunning = false;
                                    adminActivo = false;
                                }
                            }
                        }
                        else
                        {
                            scCliente.Close();
                        }
                    }
                    else
                    {
                        string? msg;

                        msg = sr.ReadLine();
                        if (msg == null)
                        {

                        }
                        else if (msg.Equals("list"))
                        {
                            for (int i = 0; i < waitQueue.Count; i++)
                            {
                                sw.WriteLine(waitQueue[i]);
                            }
                            scCliente.Close();
                        }
                        else if (msg.Equals("add"))
                        {
                            if (!waitQueue.Contains(nombre))
                            {
                                waitQueue.Add(nombre + ";" + DateTime.Now);
                                sw.WriteLine("OK");
                                ;
                            }
                            scCliente.Close();
                        }
                        else
                        {
                            scCliente.Close();
                        }
                    }
                }
                Console.WriteLine("Cliente desconectado");
            }
        }
    }
}
