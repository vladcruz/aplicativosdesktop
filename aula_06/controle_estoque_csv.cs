/* *********************************************************************
Arquivo: controle_estoque_csv.cs
Autor: Vladmir Cruz
Data: 16/04/2026
Descrição: Sistema de controle de estoque demonstrando operações 
de CRUD em um arquivo .csv carregado em memória, preservando o cabeçalho.
********************************************************************* */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class Program
{
    // Caminho adaptado para o sistema de arquivos do Ubuntu
    static string caminhoArquivo = "c:\\temp\\estoque.csv";

    static void Main()
    {
        // Garante que o arquivo exista e já injeta o cabeçalho padrão se for novo
        if (!File.Exists(caminhoArquivo))
        {
            File.WriteAllText(caminhoArquivo, "Codigo,Nome,Quantidade\n");
        }

        bool executando = true;
        while (executando)
        {
            Console.Clear();
            Console.WriteLine("--- SISTEMA DE CONTROLE DE ESTOQUE (CSV) ---");
            Console.WriteLine("1. Consultar Estoque");
            Console.WriteLine("2. Inserir Item");
            Console.WriteLine("3. Alterar Item");
            Console.WriteLine("4. Deletar Item");
            Console.WriteLine("5. Sair");
            Console.Write("\nEscolha uma opção: ");
            
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1": Consultar(); break;
                case "2": ExecutarOperacao("Inserir"); break;
                case "3": ExecutarOperacao("Alterar"); break;
                case "4": ExecutarOperacao("Deletar"); break;
                case "5": executando = false; break;
                default: Console.WriteLine("Opção inválida."); break;
            }
        }
    }

    /* LER: Lê o CSV, separa pelas vírgulas e exibe em formato de tabela */
    static void Consultar()
    {
        Console.Clear();
        Console.WriteLine("--- ESTOQUE ATUAL ---");
        string[] linhas = File.ReadAllLines(caminhoArquivo);
        
        // Se só tiver a linha 0 (cabeçalho), o estoque está vazio
        if (linhas.Length <= 1)
        {
            Console.WriteLine("[Estoque vazio]");
        }
        else
        {
            // Imprime o cabeçalho formatado
            Console.WriteLine($"IDX | {linhas[0].Replace(",", " | ")}");
            Console.WriteLine(new string('-', 40));

            // Começa do índice 1 para pular o cabeçalho
            for (int i = 1; i < linhas.Length; i++)
            {
                string[] colunas = linhas[i].Split(',');
                
                // Formatação básica para alinhar as colunas na tela
                if (colunas.Length == 3)
                {
                    Console.WriteLine($"{i,3} | {colunas[0],-6} | {colunas[1],-15} | {colunas[2]}");
                }
                else
                {
                    Console.WriteLine($"{i,3} | {linhas[i]} (Linha mal formatada)");
                }
            }
        }
        Console.WriteLine("\nPressione ENTER para voltar...");
        Console.ReadLine();
    }

    /* Controlador unificado: Protege o cabeçalho e formata a string CSV */
    static void ExecutarOperacao(string tipoOperacao)
    {
        Console.Clear();
        List<string> linhas = File.ReadAllLines(caminhoArquivo).ToList();

        if (linhas.Count <= 1 && tipoOperacao != "Inserir")
        {
            Console.WriteLine($"Não é possível {tipoOperacao} pois o estoque está vazio.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"--- {tipoOperacao.ToUpper()} ITEM ---");
        Console.WriteLine("Onde deseja realizar a operação?");
        Console.WriteLine("1. No Início (Logo após o cabeçalho)");
        Console.WriteLine("2. No Fim");
        Console.WriteLine("3. No Meio (Referência de índice)");
        Console.Write("Opção: ");
        string pos = Console.ReadLine();

        int indice = -1;

        if (pos == "1") 
        {
            indice = 1; // O Início dos DADOS agora é o índice 1
        }
        else if (pos == "2") 
        {
            indice = linhas.Count; 
        }
        else if (pos == "3")
        {
            Console.Write("Digite o número do índice de referência (veja na Consulta): ");
            if (!int.TryParse(Console.ReadLine(), out indice) || indice < 1 || indice >= linhas.Count)
            {
                Console.WriteLine("Índice inválido. Lembre-se que o índice 0 é o cabeçalho.");
                Console.ReadLine();
                return;
            }
        }
        else
        {
            Console.WriteLine("Posição inválida.");
            return;
        }

        // Ajuste para alterar/deletar no FIM
        if (pos == "2" && tipoOperacao != "Inserir")
        {
            indice = linhas.Count - 1; 
        }

        // Executa a operação na Memória
        if (tipoOperacao == "Inserir")
        {
            Console.Write("Digite o Código: ");
            string cod = Console.ReadLine();
            Console.Write("Digite o Nome do Item: ");
            string nome = Console.ReadLine();
            Console.Write("Digite a Quantidade: ");
            string qtd = Console.ReadLine();

            // Monta a linha no padrão CSV
            string novoItem = $"{cod},{nome},{qtd}";
            
            if (pos == "3")
            {
                Console.WriteLine("Deseja inserir (A)ntes ou (D)epois do índice de referência?");
                if (Console.ReadLine().ToUpper() == "D") 
                {
                    indice++;
                }
            }

            if (indice >= linhas.Count) 
            {
                linhas.Add(novoItem);
            }
            else 
            {
                linhas.Insert(indice, novoItem);
            }
        }
        else if (tipoOperacao == "Alterar")
        {
            Console.WriteLine($"Item atual: {linhas[indice]}");
            Console.Write("Digite o novo Código: ");
            string cod = Console.ReadLine();
            Console.Write("Digite o novo Nome: ");
            string nome = Console.ReadLine();
            Console.Write("Digite a nova Quantidade: ");
            string qtd = Console.ReadLine();

            linhas[indice] = $"{cod},{nome},{qtd}";
        }
        else if (tipoOperacao == "Deletar")
        {
            Console.WriteLine($"Item '{linhas[indice]}' deletado.");
            linhas.RemoveAt(indice);
        }

        /* GRAVAR: O cabeçalho no índice 0 continua intacto, gravamos o arquivo todo */
        File.WriteAllLines(caminhoArquivo, linhas);
        
        Console.WriteLine("\nOperação realizada e salva no disco com sucesso!");
        Console.ReadLine();
    }
}