using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

/*
 *   Copyright (C) 2017 Free Software Vagabundation, Inc.
 *   VOCÊ PODE USAR, ABUSAR, MODIFICAR, COPIAR E FAZER O CARALHO A4 COM ESTE CÓDIGO SEMPRE QUE ESTIVER COM ALGUM TRABALHO
 *   CHATO PARA FAZER OU SE TIVER NADA MELHOR PARA FAZER
 * 
 */ 
namespace FeriasOptimizer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
