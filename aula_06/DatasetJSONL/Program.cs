/* *********************************************************************
Arquivo: gerador_dataset_ia.cs
Autor: Vladmir Cruz
Data: 16/04/2026
Descrição: Sistema para formatação de arquivos JSONL (JSON Lines) 
voltados para o treinamento (fine-tuning) de modelos de Inteligência Artificial.
********************************************************************* */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

// Classe que representa a estrutura exigida pelas APIs de Fine-Tuning
class ExemploTreinamento
{
    public string prompt { get; set; }
    public string completion { get; set; }
}

class Program
{
    static string caminhoArquivo = "c:\\temp\\dataset_treinamento.jsonl";

    // ATENÇÃO: No JSONL, a indentação DEVE ser falsa. Cada objeto precisa ocupar exatamente uma linha.
    static JsonSerializerOptions opcoesJson = new JsonSerializerOptions { WriteIndented = false };

    static void Main()
    {
        // No JSONL, não inicializamos com "[]". Um arquivo vazio é apenas um arquivo com zero bytes.
        if (!File.Exists(caminhoArquivo))
        {
            File.Create(caminhoArquivo).Close();
        }

        bool executando = true;
        while (executando)
        {
            Console.Clear();
            Console.WriteLine("--- GERADOR DE DATASET PARA IA (JSONL) ---");
            Console.WriteLine("1. Consultar Dataset");
            Console.WriteLine("2. Inserir Exemplo (Prompt/Completion)");
            Console.WriteLine("3. Alterar Exemplo");
            Console.WriteLine("4. Deletar Exemplo");
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

    /* LER: Lê o arquivo linha por linha. Ideal para evitar estouro de memória em datasets gigantes. */
    static void Consultar()
    {
        Console.Clear();
        Console.WriteLine("--- DATASET DE TREINAMENTO ATUAL ---");
        
        string[] linhas = File.ReadAllLines(caminhoArquivo);
        
        if (linhas.Length == 0)
        {
            Console.WriteLine("[Dataset vazio]");
        }
        else
        {
            for (int i = 0; i < linhas.Length; i++)
            {
                // Ignora linhas em branco que podem ocorrer no final do arquivo
                if (string.IsNullOrWhiteSpace(linhas[i])) continue;

                // Desserializa apenas a linha atual
                ExemploTreinamento exemplo = JsonSerializer.Deserialize<ExemploTreinamento>(linhas[i]);
                
                Console.WriteLine($"\n[Índice {i}]");
                Console.WriteLine($"Prompt.....: {exemplo.prompt}");
                Console.WriteLine($"Completion.: {exemplo.completion}");
                Console.WriteLine(new string('-', 40));
            }
        }
        Console.WriteLine("\nPressione ENTER para voltar...");
        Console.ReadLine();
    }

    /* Controlador unificado para manipulação do JSONL */
    static void ExecutarOperacao(string tipoOperacao)
    {
        Console.Clear();
        
        // Carrega as linhas brutas
        List<string> linhas = File.ReadAllLines(caminhoArquivo).ToList();
        // Remove linhas vazias acidentais para não quebrar os índices
        linhas.RemoveAll(string.IsNullOrWhiteSpace);

        if (linhas.Count == 0 && tipoOperacao != "Inserir")
        {
            Console.WriteLine($"Não é possível {tipoOperacao} pois o dataset está vazio.");
            Console.ReadLine();
            return;
        }

        string pos;

        if(tipoOperacao == "Inserir")
        {
            Console.WriteLine($"--- {tipoOperacao.ToUpper()} EXEMPLO DE TREINAMENTO ---");
            Console.WriteLine("Onde deseja realizar a operação?");
            Console.WriteLine("1. No Início");
            Console.WriteLine("2. No Fim (Mais rápido para datasets gigantes)");
            Console.WriteLine("3. No Meio (Referência de índice)");
            Console.Write("Opção: ");
            pos = Console.ReadLine();
        }
        else
        {
            Console.WriteLine($"--- {tipoOperacao.ToUpper()} ITEM ---");
            pos = "3";
        }

        int indice = -1;

        if (pos == "1") 
            indice = 0;
        else if (pos == "2") 
            indice = linhas.Count; 
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

        if (pos == "2" && tipoOperacao != "Inserir")
            indice = linhas.Count - 1;

        if (tipoOperacao == "Inserir")
        {
            ExemploTreinamento novoExemplo = new ExemploTreinamento();

            Console.WriteLine("Digite o Prompt (A entrada do usuário/sistema):");
            novoExemplo.prompt = Console.ReadLine();
            
            Console.WriteLine("\nDigite o Completion (A resposta que a IA deve aprender a dar):");
            novoExemplo.completion = Console.ReadLine();
            
            // Serializa o objeto para uma única linha (sem quebras)
            string linhaJson = JsonSerializer.Serialize(novoExemplo, opcoesJson);

            if (pos == "3")
            {
                Console.WriteLine("Deseja inserir (A)ntes ou (D)epois do índice de referência?");
                if (Console.ReadLine().ToUpper() == "D") indice++;
            }

            if (indice >= linhas.Count) 
                linhas.Add(linhaJson);
            else 
                linhas.Insert(indice, linhaJson);
        }
        else if (tipoOperacao == "Alterar")
        {
            // Desserializa a linha específica para mostrar o valor atual
            ExemploTreinamento atual = JsonSerializer.Deserialize<ExemploTreinamento>(linhas[indice]);
            
            Console.WriteLine($"\nPrompt Atual: {atual.prompt}");
            Console.WriteLine("Digite o NOVO Prompt (ou deixe em branco para manter):");
            string novoPrompt = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(novoPrompt)) atual.prompt = novoPrompt;

            Console.WriteLine($"\nCompletion Atual: {atual.completion}");
            Console.WriteLine("Digite o NOVO Completion (ou deixe em branco para manter):");
            string novoComp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(novoComp)) atual.completion = novoComp;

            // Substitui a linha antiga pela nova versão serializada
            linhas[indice] = JsonSerializer.Serialize(atual, opcoesJson);
        }
        else if (tipoOperacao == "Deletar")
        {
            Console.WriteLine($"Exemplo no índice {indice} deletado do dataset.");
            linhas.RemoveAt(indice);
        }

        /* GRAVAR: Regrava o JSONL. Note que não há colchetes e nem vírgulas entre as linhas */
        File.WriteAllLines(caminhoArquivo, linhas);
        
        Console.WriteLine("\nOperação realizada e salva no disco com sucesso!");
        Console.ReadLine();
    }
}