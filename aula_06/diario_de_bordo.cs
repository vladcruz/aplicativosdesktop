/* *********************************************************************
Arquivo: diario_de_bordo.cs
Autor: Vladmir Cruz
Data: 11/04/2026
Descrição: Programa que mostra funções básicas de operação com arquivos
do tipo .txt
********************************************************************* */

using System;
using System.IO;

class Program
{
    static void Main()
    {
        /* Caminho do arquivo que vamos utilizar para gravar e ler */
        string caminhoArquivo = "c:\\temp\\meu_log.txt";

        Console.WriteLine("--- SISTEMA DE AUDITORIA ---");
        Console.Write("Digite o seu nome de usuário: ");
        string usuario = Console.ReadLine();

        /* Montando a mensagem com a data e hora atual */
        string logMensagem = $"[{DateTime.Now}] O usuário '{usuario}' acessou o sistema.\n";

        /* INSERIR: Usamos o Append para adicionar no final sem apagar os acessos anteriores */
        File.AppendAllText(caminhoArquivo, logMensagem);
        Console.WriteLine("Acesso registrado no disco com sucesso!\n");

        Console.WriteLine("--- HISTÓRICO DE ACESSOS (LEITURA) ---");
        
        /* LER: Puxando o disco e jogando na tela */
        if (File.Exists(caminhoArquivo))
        {
            string conteudo = File.ReadAllText(caminhoArquivo);
            Console.WriteLine(conteudo);
        }
        
        Console.ReadLine();
    }
}