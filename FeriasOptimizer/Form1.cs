using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FeriasOptimizer
{
    /*
    cada intervalo de ferias tem que ter no minimo 10 dias
    as ferias tem que somar 30 ou 20 dias. 
    No caso de 20 dias serao vendidos 10 dias

    Os feriados nacionais podem ser pegos no site
    www.calendarr.com/brasil/calendario-2016
    www.calendarr.com/brasil/feriados-2016

    Com base nesse nacionais da para se calcular a 4ª de cinza. Os outros estaduais sao fixos
    
     Nas 2 ultimas semanas do ano pode ter o balanço anual e alguns feriados extras.

     */

    public partial class Form1 : Form
    {
        private bool vender10Dias = false;
        private bool doisPeriodos = false;
        private int diasDeFerias = 30;
        private int anoAtual = DateTime.Now.Year;

        private List<DateTime> outros = new List<DateTime>();
        private List<DateTime> nacionais = new List<DateTime>();

        private Dictionary<DateTime,int> optimus;


        public Form1()
        {
            InitializeComponent();

            //feriados $nacionais
            getNationalHolidays(anoAtual);
            getNationalHolidays(anoAtual+1);

            //feriados estaduais
            getStateHolidays(anoAtual);
            getStateHolidays(anoAtual+1);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            vender10Dias = true;
            diasDeFerias = 20;

            if (doisPeriodos){
                numericUpDown1.Value = diasDeFerias/2;
                numericUpDown2.Value = diasDeFerias/2;
            }
            else
                numericUpDown1.Value = diasDeFerias;

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            vender10Dias = false;
            diasDeFerias = 30;

            if (doisPeriodos){
                numericUpDown1.Value = diasDeFerias/2;
                numericUpDown2.Value = diasDeFerias/2;
            }
            else
                numericUpDown1.Value = diasDeFerias;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            doisPeriodos = true;
            groupBox4.Enabled = true;
            if (vender10Dias){
                numericUpDown1.Value = 10;
                numericUpDown2.Value = 10;
            }
            else{
                numericUpDown1.Value = 15;
                numericUpDown2.Value = 15;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            doisPeriodos = false;
            groupBox4.Enabled = false;
            if (vender10Dias)
                numericUpDown1.Value = 20;
            else
                numericUpDown1.Value = 30;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            DateTime dataInicio = DateTime.Parse(dateTimePicker1.Text), dataInicio2 = DateTime.Parse(dateTimePicker4.Text); //data de inicio para o algoritmo
            DateTime dataFim = DateTime.Parse(dateTimePicker2.Text), dataFim2 = DateTime.Parse(dateTimePicker3.Text); //data de fim para o algoritmo

            if (dataFim < dataInicio){
                MessageBox.Show("A data de fim é menor que a data de inicio");
                return;
            }
            int dias1 = (int)numericUpDown1.Value; //qtd de dias de ferias do primeiro perido
            if ((dataFim - dataInicio).TotalDays < dias1)
            {
                MessageBox.Show(String.Format("Entre {0} e {1} existem menos de {2} dias", dataInicio.ToString("dd/MM/yyyy"), dataFim.ToString("dd/MM/yyyy"), dias1));
                return;
            }

            int dias2 = 0; //qtd de dias de ferias do segundo perido
            if (doisPeriodos){
                dias2 = (int)numericUpDown2.Value;
                if (dataFim2 < dataInicio2){
                    MessageBox.Show("A data de fim é menor que a data de inicio do 2º período");
                    return;
                }
                if ((dataFim2 - dataInicio2).TotalDays < dias2)
                {
                    MessageBox.Show(String.Format("Entre {0} e {1} existem menos de {2} dias", dataInicio2.ToString("dd/MM/yyyy"), dataFim2.ToString("dd/MM/yyyy"), dias2));
                    return;
                }
                if (dataInicio2 < dataInicio)
                {
                    MessageBox.Show("O início do seu 2º período de férias tem que ser maior que o 1º");
                    return;
                }
            }

            int diasTotais = dias1 + dias2;
            if (vender10Dias && diasTotais != 20){
                MessageBox.Show( "Os dias totais de férias tem que ser 20 dias");
                return;
            }
            else if (!vender10Dias && diasTotais != 30){
	            MessageBox.Show( "Os dias totais de férias tem que ser 30 dias");
                return;
            }

            //verifica se as férias ocorrem no período de fim de ano onde tem balanço anual, natal e costumam dar alguns feriados
            bool inDecember = false;
            if ( (dataInicio.Month == 12 && (isNoBalancoAnual(dataInicio))) || (doisPeriodos && (dataInicio2.Month == 12 && (isNoBalancoAnual(dataInicio2))) ) )
            {
                if (MessageBox.Show("Tem certeza que deseja marcar férias próximas do balanço anual?", "Pense bem", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            //Calculando melhor data do primeiro periodo
            DateTime inicio = dataInicio;
            int maxDias = 0, qtdDiasTotais = 0;
            optimus = new Dictionary<DateTime,int>();
            while (inicio < (dataFim.AddDays(-dias1))){
                if (inicio.DayOfWeek == DayOfWeek.Sunday || inicio.DayOfWeek == DayOfWeek.Saturday || nacionais.Contains(inicio) || outros.Contains(inicio))
                {
                    //nunca começar as ferias num domingo, sábado ou feriado
                }
                else
                {
                    qtdDiasTotais = daysBefore(inicio) + dias1 + daysAfter(inicio.AddDays(dias1 - 1));
                    if (qtdDiasTotais >= maxDias)
                    {
                        optimizer(inicio, qtdDiasTotais);
                        maxDias = qtdDiasTotais;
                    }
                }
	            inicio = inicio.AddDays(1);
            }
            String resultado;
            if (doisPeriodos)
                resultado = String.Format("O Ferias Optimizer encontrou os seguintes intervalos com {0} dias para seu primeiro período de férias\n", maxDias);
            else
                resultado = String.Format("O Ferias Optimizer encontrou os seguintes intervalos com {0} dias para seu período de férias\n", maxDias);
            foreach(KeyValuePair<DateTime,int> entry in optimus){
	            DateTime fim = entry.Key.AddDays(dias1-1);
                resultado = resultado + String.Format("Do dia {0} até o dia {1} ", entry.Key.ToString("dd/MM/yyyy"), fim.ToString("dd/MM/yyyy"));
	            resultado = resultado + String.Format("\tVocê terá {0} dias antes e {1} dias depois\n",daysBefore(entry.Key),daysAfter(fim));
            }
            if (doisPeriodos)
            {
                //Calculando melhor data do segundo periodo
                inicio = dataInicio2;
                maxDias = 0;
                qtdDiasTotais = 0;
                optimus = new Dictionary<DateTime,int>();
                while (inicio < (dataFim2.AddDays(-dias2))){
                    if (inicio.DayOfWeek == DayOfWeek.Sunday || inicio.DayOfWeek == DayOfWeek.Saturday || nacionais.Contains(inicio) || outros.Contains(inicio))
                    {
                        //nunca começar as ferias num domingo, sábado ou feriado
                    }
                    else{
	                    qtdDiasTotais = daysBefore(inicio) + dias2 + daysAfter(inicio.AddDays(dias2-1));
                        if (qtdDiasTotais >= maxDias)
                        {
                            optimizer(inicio, qtdDiasTotais);
                            maxDias = qtdDiasTotais;
                        }
                    }
	                inicio = inicio.AddDays(1);
                }
                resultado = resultado + String.Format("\nO Ferias Optimizer encontrou os seguintes intervalos com {0} dias para seu segundo período de férias\n",maxDias);
                foreach(KeyValuePair<DateTime,int> entry in optimus){
                    DateTime fim = entry.Key.AddDays(dias2 - 1);
                    resultado = resultado + String.Format("Do dia {0} até o dia {1}", entry.Key.ToString("dd/MM/yyyy"), fim.ToString("dd/MM/yyyy"));
	                resultado = resultado + String.Format("\tVocê terá {0} dias antes e {1} dias depois\n",daysBefore(entry.Key),daysAfter(fim));
                }
            }
            richTextBox1.Text = resultado;
        }

        /// <summary>
        /// Retorna true se as férias ocorrer perto próxima do período de balanço anual, natal e ano novo onde costumam enforcar alguns dias.
        /// </summary>
        /// <param name="inicio"></param>
        /// <returns></returns>
        private bool isNoBalancoAnual(DateTime inicio)
        {
            DateTime natal = new DateTime(inicio.Year, 12, 25, 0, 0, 0), anoNovo = new DateTime(inicio.Year + 1, 1, 1, 0, 0, 0);
            DateTime sabadoAntesDoNatal = natal.AddDays(-1);
            while (sabadoAntesDoNatal.DayOfWeek != DayOfWeek.Saturday)
                sabadoAntesDoNatal = sabadoAntesDoNatal.AddDays(-1);
            if ((inicio >= sabadoAntesDoNatal) && (inicio <= anoNovo))
                return true;
            return false;
        }

        /// <summary>
        /// Insere os feriados estaduais do ano year na lista outros.
        /// Os feriados do RJ são:
        /// - 23/04 - São Jorge
        /// - 08/12 - Dia de Nossa Senhora da Conceição
        /// - 20/11 - Zumbi dos Palmares
        /// </summary>
        /// <param name="year"></param>
        private void getStateHolidays(int year)
        {
            DateTime date;
            date = new DateTime(year, 4, 23, 0, 0, 0);//São Jorge
            outros.Add(date);
            monthCalendar1.AddBoldedDate(date);
            date = new DateTime(year, 11, 20, 0, 0, 0);//Zumbi
            outros.Add(date);
            monthCalendar1.AddBoldedDate(date);
            date = new DateTime(year, 12, 8, 0, 0, 0);//Nossa Senhora da Conceição
            outros.Add(date);
            monthCalendar1.AddBoldedDate(date);
        }

        /// <summary>
        /// Insere os feriados nacionais do ano year na lista nacionais.
        /// A data dos feriados é pesquisada no site https://www.calendarr.com/brasil/feriados-{year}
        /// É utilizado o HtmlAgilityPack para parse do html. Com o tempo esse parse pode para de funcionar caso haja modificações no site.
        /// </summary>
        /// <param name="year"></param>
        private void getNationalHolidays(int year)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(String.Format("https://www.calendarr.com/brasil/feriados-{0}/", year));

                int dia, mes, qtd;
                string nomeDoFeriado;
                DateTime date;
                qtd = doc.DocumentNode.SelectNodes("//*[@id='main']/div[4]/div[3]/span").Count; //qtd de meses com feriados
                for (int i = 1; i <= qtd; i++)
                {
                    mes = DateTime.ParseExact(doc.DocumentNode.SelectSingleNode(String.Format("//*[@id='main']/div[4]/div[3]/span[{0}]", i)).InnerText, "MMMM", CultureInfo.CurrentCulture).Month;
                    foreach (var node in doc.DocumentNode.SelectNodes(String.Format("//*[@id='main']/div[4]/div[3]/ul[{0}]/li",i))){
                        dia = int.Parse(node.SelectSingleNode("div/span").InnerText);
                        nomeDoFeriado = node.SelectSingleNode("div[2]/a").InnerText;
                        date = new DateTime(year, mes, dia, 0, 0, 0);
                        nacionais.Add(date); 
                        monthCalendar1.AddBoldedDate(date);
                        if (nomeDoFeriado == "Carnaval")//adicionar 4ª feira de cinzas
                        {
                            nacionais.Add(date.AddDays(1));
                            monthCalendar1.AddBoldedDate(date.AddDays(1));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ERRO: " + e.Message);
            }
        }

        /// <summary>
        /// Retorna quantos de feriado ou fds existem antes da data
        /// Quando 3ª é um feriado nacional também é considerado a 2ª.
        /// Quando 5ª é um feriado nacional também é considerado a 6ª.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int  daysBefore(DateTime data){
	        int dias = 0;
	        while (true){
		        data = data.AddDays(-1);
		        if (data.DayOfWeek == DayOfWeek.Friday && nacionais.Contains(data.AddDays(-1))){ //feriado nacional na quinta enforca a sexta
			        dias = dias + 2;
			        data = data.AddDays(-1);
                }
		        else if (data.DayOfWeek == DayOfWeek.Tuesday && nacionais.Contains(data)){ //feriado nacional na terca enforca a segunda
			        dias = dias + 2;
			        data = data.AddDays(-1);
                }
		        else if (data.DayOfWeek == DayOfWeek.Sunday || data.DayOfWeek == DayOfWeek.Saturday || outros.Contains(data) || nacionais.Contains(data)){
			        dias = dias + 1;
                }
		        else
			        break;
	        }	
	        return dias;
        }

        /// <summary>
        /// Retorna quantos de feriado ou fds existem depois da data
        /// Quando 3ª é um feriado nacional também é considerado a 2ª.
        /// Quando 5ª é um feriado nacional também é considerado a 6ª.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int daysAfter(DateTime data){
	        int dias = 0;
	        while (true){
		        data = data.AddDays(1);
		        if (data.DayOfWeek == DayOfWeek.Thursday && nacionais.Contains(data)){ //feriado nacional na quinta enforca a sexta
			        dias = dias + 2;
			        data = data.AddDays(1);
                }
		        else if (data.DayOfWeek == DayOfWeek.Monday && nacionais.Contains(data.AddDays(1))){ //feriado nacional na terca enforca a segunda
			        dias = dias + 2;
			        data = data.AddDays(1);
                }
		        else if (data.DayOfWeek == DayOfWeek.Sunday || data.DayOfWeek == DayOfWeek.Saturday || outros.Contains(data) || nacionais.Contains(data))
			        dias = dias + 1;
		        else
			        break;
            }	
	        return dias;
        }

        /// <summary>
        /// Não lembro o que essa porra faz, é legado do código em ruby que fiz há 1 ano. 
        /// Mas ta funcionando, então melhor deixar né ? rsrs
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dias"></param>
        private void optimizer(DateTime data, int dias){
	        if (optimus.Count == 0)
		        optimus[data] = dias;
	        else{
		        int valorAntigo = optimus[optimus.Keys.First()];
		        if (dias < valorAntigo)
			        return;
		        if (dias > valorAntigo)
			        optimus.Clear();
		        optimus[data] = dias;
	        }
        }

        //setando 3x o número de dias na frente a data de fim para facilitar a vida.
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker2.Value = dateTimePicker1.Value.AddDays((int)numericUpDown1.Value*3);
        }

        //setando 3x o número de dias na frente a data de fim para facilitar a vida.
        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker3.Value = dateTimePicker4.Value.AddDays((int)numericUpDown2.Value*3);
        }

    }
}
