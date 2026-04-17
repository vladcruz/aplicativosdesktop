/* *********************************************************************
Arquivo: controle_estoque_xml.cs
Autor: Vladmir Cruz
Data: 16/04/2026
Descrição: Sistema de controle de estoque demonstrando operações 
de CRUD em uma estrutura hierárquica XML usando LINQ to XML.
********************************************************************* */

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

class Program
{
    // Mantendo o caminho do Ubuntu
    static string caminhoArquivo = "c:\\temp\\estoque.xml";

    static void Main()
    {
        // Se o arquivo não existe, cria a estrutura base (Nó Raiz)
        if (!File.Exists(caminhoArquivo))
        {
            XDocument docBase = new XDocument(
                new XElement("Estoque") // Raiz do nosso documento
            );
            docBase.Save(caminhoArquivo);
        }

        bool executando = true;
        while (executando)
        {
            Console.Clear();
            Console.WriteLine("--- SISTEMA DE CONTROLE DE ESTOQUE (XML) ---");
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

    /* LER: Carrega o XML e itera sobre os nós <Item> */
    static void Consultar()
    {
        Console.Clear();
        Console.WriteLine("--- ESTOQUE ATUAL ---");
        
        // Carrega a árvore XML para a memória
        XDocument doc = XDocument.Load(caminhoArquivo);
        var itens = doc.Root.Elements("Item").ToList();
        
        if (itens.Count == 0)
        {
            Console.WriteLine("[Estoque vazio]");
        }
        else
        {
            Console.WriteLine("IDX | Código | Nome            | Qtd");
            Console.WriteLine(new string('-', 40));

            for (int i = 0; i < itens.Count; i++)
            {
                // Extraindo os valores diretamente das tags filhas
                string cod = itens[i].Element("Codigo")?.Value ?? "N/A";
                string nome = itens[i].Element("Nome")?.Value ?? "N/A";
                string qtd = itens[i].Element("Quantidade")?.Value ?? "N/A";

                Console.WriteLine($"{i,3} | {cod,-6} | {nome,-15} | {qtd}");
            }
        }
        Console.WriteLine("\nPressione ENTER para voltar...");
        Console.ReadLine();
    }

    /* Controlador unificado: Manipulação de Nós da Árvore XML */
    static void ExecutarOperacao(string tipoOperacao)
    {
        Console.Clear();
        
        // Carrega o documento inteiro
        XDocument doc = XDocument.Load(caminhoArquivo);
        var itens = doc.Root.Elements("Item").ToList();

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

        // Note que o índice 0 volta a ser válido, pois não há linha de cabeçalho
        if (pos == "1")
        {
            indice = 0;
        }
        else if (pos == "2") 
        {
            indice = itens.Count;
        }
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
        {
            indice = itens.Count - 1;
        }

        // Executa a operação na Memória (manipulando XElements)
        if (tipoOperacao == "Inserir")
        {
            Console.Write("Digite o Código: ");
            string cod = Console.ReadLine();
            Console.Write("Digite o Nome do Item: ");
            string nome = Console.ReadLine();
            Console.Write("Digite a Quantidade: ");
            string qtd = Console.ReadLine();

            // Monta o novo nó XML
            XElement novoItem = new XElement("Item",
                new XElement("Codigo", cod),
                new XElement("Nome", nome),
                new XElement("Quantidade", qtd)
            );
            
            if (itens.Count == 0)
            {
                // Se a raiz está vazia, apenas adiciona
                doc.Root.Add(novoItem);
            }
            else if (pos == "1")
            {
                // Adiciona como primeiro filho da raiz
                doc.Root.AddFirst(novoItem);
            }
            else if (pos == "2")
            {
                // Adiciona no final
                doc.Root.Add(novoItem);
            }
            else if (pos == "3")
            {
                Console.WriteLine("Deseja inserir (A)ntes ou (D)epois do índice de referência?");
                if (Console.ReadLine().ToUpper() == "D")
                {
                    itens[indice].AddAfterSelf(novoItem);
                }
                else
                {
                    itens[indice].AddBeforeSelf(novoItem);
                }
            }
        }
        else if (tipoOperacao == "Alterar")
        {
            Console.WriteLine($"Alterando: {itens[indice].Element("Nome")?.Value}");
            Console.Write("Digite o novo Código: ");
            itens[indice].Element("Codigo").Value = Console.ReadLine();
            
            Console.Write("Digite o novo Nome: ");
            itens[indice].Element("Nome").Value = Console.ReadLine();
            
            Console.Write("Digite a nova Quantidade: ");
            itens[indice].Element("Quantidade").Value = Console.ReadLine();
        }
        else if (tipoOperacao == "Deletar")
        {
            Console.WriteLine($"Item '{itens[indice].Element("Nome")?.Value}' deletado.");
            // Remove o nó da árvore
            itens[indice].Remove();
        }

        /* GRAVAR: Salva o documento XML formatado de volta no disco */
        doc.Save(caminhoArquivo);
        
        Console.WriteLine("\nOperação realizada e salva no disco com sucesso!");
        Console.ReadLine();
    }
}