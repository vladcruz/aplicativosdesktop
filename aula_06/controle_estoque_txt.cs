/* *********************************************************************
Arquivo: controle_estoque.cs
Autor: Vladmir Cruz
Data: 16/04/2026
Descrição: Sistema de controle de estoque demonstrando operações 
de CRUD (inserção, leitura, alteração e deleção) no início, meio 
e fim de um arquivo .txt carregado em memória.
********************************************************************* */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static string caminhoArquivo = "c:\\temp\\estoque.txt";

    static void Main()
    {
        // Garante que o arquivo e o diretório existam antes de começar
        Directory.CreateDirectory("c:\\temp");
        if (!File.Exists(caminhoArquivo))
        {
            File.Create(caminhoArquivo).Close();
        }

        bool executando = true;
        while (executando)
        {
            Console.Clear();
            Console.WriteLine("--- SISTEMA DE CONTROLE DE ESTOQUE ---");
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

    /* LER: Apenas puxa do disco e exibe */
    static void Consultar()
    {
        Console.Clear();
        Console.WriteLine("--- ESTOQUE ATUAL ---");
        string[] linhas = File.ReadAllLines(caminhoArquivo);
        
        if (linhas.Length == 0)
        {
            Console.WriteLine("[Estoque vazio]");
        }
        else
        {
            for (int i = 0; i < linhas.Length; i++)
            {
                Console.WriteLine($"{i} - {linhas[i]}");
            }
        }
        Console.WriteLine("\nPressione ENTER para voltar...");
        Console.ReadLine();
    }

    /* Controlador unificado para Inserir, Alterar e Deletar nas 3 posições */
    static void ExecutarOperacao(string tipoOperacao)
    {
        Console.Clear();
        List<string> linhas = File.ReadAllLines(caminhoArquivo).ToList();

        if (linhas.Count == 0 && tipoOperacao != "Inserir")
        {
            Console.WriteLine($"Não é possível {tipoOperacao} pois o estoque está vazio.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"--- {tipoOperacao.ToUpper()} ITEM ---");
        Console.WriteLine("Onde deseja realizar a operação?");
        Console.WriteLine("1. No Início");
        Console.WriteLine("2. No Fim");
        Console.WriteLine("3. No Meio (Referência de índice)");
        Console.Write("Opção: ");
        string pos = Console.ReadLine();

        int indice = -1;

        if (pos == "1") 
        {
            indice = 0;
        }
        else if (pos == "2") 
        {
            indice = linhas.Count; // Para inserção no fim
        }
        else if (pos == "3")
        {
            Console.Write("Digite o número do índice de referência (veja na Consulta): ");
            if (!int.TryParse(Console.ReadLine(), out indice) || indice < 0 || indice >= linhas.Count)
            {
                Console.WriteLine("Índice inválido.");
                Console.ReadLine();
                return;
            }
        }
        else
        {
            Console.WriteLine("Posição inválida.");
            return;
        }

        // Ajuste de índice para alterações e deleções no FIM (deve ser o último elemento, não o Count)
        if (pos == "2" && tipoOperacao != "Inserir")
        {
            indice = linhas.Count - 1; 
        }

        // Executa a operação na Memória (List<string>)
        if (tipoOperacao == "Inserir")
        {
            Console.Write("Digite o nome do novo item: ");
            string novoItem = Console.ReadLine();
            
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
            Console.Write($"Digite o novo nome para substituir '{linhas[indice]}': ");
            linhas[indice] = Console.ReadLine();
        }
        else if (tipoOperacao == "Deletar")
        {
            Console.WriteLine($"Item '{linhas[indice]}' deletado.");
            linhas.RemoveAt(indice);
        }

        /* GRAVAR: Sobrescrevemos o arquivo inteiro com a lista atualizada */
        File.WriteAllLines(caminhoArquivo, linhas);
        
        Console.WriteLine("\nOperação realizada e salva no disco com sucesso!");
        Console.ReadLine();
    }
}