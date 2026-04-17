/* *********************************************************************
Arquivo: controle_estoque_json.cs
Autor: Vladmir Cruz
Data: 16/04/2026
Descrição: Sistema de controle de estoque demonstrando operações 
de CRUD usando Serialização e Desserialização de JSON nativo.
********************************************************************* */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

// 1. Criamos um "Molde" (Classe) para representar nossos dados fortemente tipados
class ItemEstoque
{
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public string Quantidade { get; set; }
}

class Program
{
    // Caminho adaptado para o sistema de arquivos do Ubuntu
    static string caminhoArquivo = "c:\\temp\\estoque.json";

    // Opção para salvar o JSON formatado com quebras de linha e tabulações (Pretty Print)
    static JsonSerializerOptions opcoesJson = new JsonSerializerOptions { WriteIndented = true };

    static void Main()
    {
        // Se o arquivo não existir, criamos com um array JSON vazio "[]"
        if (!File.Exists(caminhoArquivo))
        {
            File.WriteAllText(caminhoArquivo, "[]");
        }

        bool executando = true;
        while (executando)
        {
            Console.Clear();
            Console.WriteLine("--- SISTEMA DE CONTROLE DE ESTOQUE (JSON) ---");
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

    /* LER: Desserializa o JSON direto para uma List<ItemEstoque> */
    static void Consultar()
    {
        Console.Clear();
        Console.WriteLine("--- ESTOQUE ATUAL ---");
        
        string conteudoJson = File.ReadAllText(caminhoArquivo);
        List<ItemEstoque> itens = JsonSerializer.Deserialize<List<ItemEstoque>>(conteudoJson);
        
        if (itens == null || itens.Count == 0)
        {
            Console.WriteLine("[Estoque vazio]");
        }
        else
        {
            Console.WriteLine("IDX | Código | Nome            | Qtd");
            Console.WriteLine(new string('-', 40));

            for (int i = 0; i < itens.Count; i++)
            {
                // Como agora é um objeto, acessamos via Propriedades (.Codigo, .Nome)
                Console.WriteLine($"{i,3} | {itens[i].Codigo,-6} | {itens[i].Nome,-15} | {itens[i].Quantidade}");
            }
        }
        Console.WriteLine("\nPressione ENTER para voltar...");
        Console.ReadLine();
    }

    /* Controlador unificado: Manipulação de uma Lista de Objetos */
    static void ExecutarOperacao(string tipoOperacao)
    {
        Console.Clear();
        
        // Puxa do disco e converte para Lista de Objetos
        string conteudoJson = File.ReadAllText(caminhoArquivo);
        List<ItemEstoque> itens = JsonSerializer.Deserialize<List<ItemEstoque>>(conteudoJson) ?? new List<ItemEstoque>();

        if (itens.Count == 0 && tipoOperacao != "Inserir")
        {
            Console.WriteLine($"Não é possível {tipoOperacao} pois o estoque está vazio.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"--- {tipoOperacao.ToUpper()} ITEM ---");
        Console.WriteLine("Onde deseja realizar a operação?");
        Console.WriteLine("1. No Início da lista");
        Console.WriteLine("2. No Fim da lista");
        Console.WriteLine("3. No Meio (Referência de índice)");
        Console.Write("Opção: ");
        string pos = Console.ReadLine();

        int indice = -1;

        // Voltamos à lógica base de uma List<> na memória
        if (pos == "1") 
            indice = 0;
        else if (pos == "2") 
            indice = itens.Count; 
        else if (pos == "3")
        {
            Console.Write("Digite o número do índice de referência (veja na Consulta): ");
            if (!int.TryParse(Console.ReadLine(), out indice) || indice < 0 || indice >= itens.Count)
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
            indice = itens.Count - 1;

        // Executa a operação na Memória (manipulando a List<ItemEstoque>)
        if (tipoOperacao == "Inserir")
        {
            // Instancia um novo objeto da nossa classe
            ItemEstoque novoItem = new ItemEstoque();

            Console.Write("Digite o Código: ");
            novoItem.Codigo = Console.ReadLine();
            Console.Write("Digite o Nome do Item: ");
            novoItem.Nome = Console.ReadLine();
            Console.Write("Digite a Quantidade: ");
            novoItem.Quantidade = Console.ReadLine();
            
            if (pos == "3")
            {
                Console.WriteLine("Deseja inserir (A)ntes ou (D)epois do índice de referência?");
                if (Console.ReadLine().ToUpper() == "D") 
                {
                    indice++;
                }
            }

            if (indice >= itens.Count) 
            {
                itens.Add(novoItem);
            }
            else 
            {
                itens.Insert(indice, novoItem);
            }
        }
        else if (tipoOperacao == "Alterar")
        {
            Console.WriteLine($"Alterando: {itens[indice].Nome}");
            Console.Write("Digite o novo Código: ");
            itens[indice].Codigo = Console.ReadLine();
            
            Console.Write("Digite o novo Nome: ");
            itens[indice].Nome = Console.ReadLine();
            
            Console.Write("Digite a nova Quantidade: ");
            itens[indice].Quantidade = Console.ReadLine();
        }
        else if (tipoOperacao == "Deletar")
        {
            Console.WriteLine($"Item '{itens[indice].Nome}' deletado.");
            itens.RemoveAt(indice);
        }

        /* GRAVAR: Serializa a Lista de volta para texto JSON e salva no disco */
        string jsonAtualizado = JsonSerializer.Serialize(itens, opcoesJson);
        File.WriteAllText(caminhoArquivo, jsonAtualizado);
        
        Console.WriteLine("\nOperação realizada e salva no disco com sucesso!");
        Console.ReadLine();
    }
}