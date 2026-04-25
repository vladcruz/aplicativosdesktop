using Aventureiro_Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
namespace Guilda_Repository
{
    public class GuildaRepository
    {
        /* Aqui acontece a mágica: estruturamos a string de conexão de forma segura e organizada */
        NpgsqlConnectionStringBuilder connBuilder = new NpgsqlConnectionStringBuilder
        {
            /* Substitua pelo seu host do Supabase */
            Host = "aws-1-sa-east-1.pooler.supabase.com",
            Port = 6543,
            Database = "postgres",
            /* O nosso novo usuário seguro */
            Username = "app_repository.mcisftqfjluxrpvssyni",
            /* A senha que definimos no SQL para o usuário app_repository */
            Password = "sua_senha_super_forte",
            SslMode = SslMode.Require,
            Pooling = false,
            MaxPoolSize = 20,
            ConnectionIdleLifetime = 10,
            TrustServerCertificate = true
        };
        /* ====================================================================
        CREATE (INSERÇÃO ÚNICA)
        ==================================================================== */
        public void Adicionar(Aventureiro adv)
        {
            /* Usamos 'using var' para garantir que a conexão será fechada sozinha
           ao final do método */
            using (NpgsqlConnection conn = new
           NpgsqlConnection(connBuilder.ToString()))
            {
                conn.Open();
                string sql = "INSERT INTO Aventureiros (nome, classe, nivel, pontos_vida) VALUES(@nome, @classe, @nivel, @hp)";
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    /* Blindando contra SQL Injection */
                    cmd.Parameters.AddWithValue("@nome", adv.Nome);
                    cmd.Parameters.AddWithValue("@classe", adv.Classe);
                    cmd.Parameters.AddWithValue("@nivel", adv.Nivel);
                    cmd.Parameters.AddWithValue("@hp", adv.PontosVida);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            Console.WriteLine("Aventureiro adicionado com sucesso!");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erro ao adicionar o aventureiro.");
                        Console.WriteLine($"ERRO: {e.Message}");
                    }
                }
            }
        }
        /* ====================================================================
         READ (CONSULTA EXATA OU PARCIAL)
         ==================================================================== */
        public List<Aventureiro> BuscarTodos(string busca = "")
        {
            var lista = new List<Aventureiro>();
            using (NpgsqlConnection conn = new
           NpgsqlConnection(connBuilder.ToString()))
            {
                conn.Open();
                string sql;
                if (string.IsNullOrWhiteSpace(busca))
                {
                    sql = "SELECT id, nome, classe, nivel, pontos_vida FROM Aventureiros ORDER BY id";
                }
                else
                {
                    /* ILIKE ignora maiúsculas. O '%' permite buscar parte do nome
                   (ex: '%Ganda%') */
                    sql = "SELECT id, nome, classe, nivel, pontos_vida FROM Aventureiros WHERE nome ILIKE @busca ORDER BY id";
                }
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(busca))
                    {
                        cmd.Parameters.AddWithValue("@busca", $"%{busca}%");
                    }
                    using (var reader = cmd.ExecuteReader()) /* Retorna a tabela
do banco */
                    {
                        while (reader.Read())
                        {
                            /* Montando o objeto Aventureiro a partir dos dados
                           lidos do banco */
                            Aventureiro adv = new Aventureiro()
                            {
                                Id = reader.GetInt32(0),
                                Nome = reader.GetString(1),
                                Classe = reader.GetString(2),
                                Nivel = reader.GetInt32(3),
                                PontosVida = reader.GetInt32(4)
                            };
                            lista.Add(adv); /* Guarda na lista da memória RAM */
                        }
                    }
                }
            }
            return lista;
        }
        /* ====================================================================
        UPDATE (ATUALIZAÇÃO DE 1 REGISTRO)
        ==================================================================== */
        public int PromoverAventureiro(int id, int novoNivel, int novoHp)
        {
            using (NpgsqlConnection conn = new
           NpgsqlConnection(connBuilder.ToString()))
            {
                conn.Open();
                string sql = "UPDATE Aventureiros SET nivel = @nivel, pontos_vida = @hp WHERE id = @id";
            using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nivel", novoNivel);
                    cmd.Parameters.AddWithValue("@hp", novoHp);
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery(); /* Devolve quantas linhas foram
alteradas */
                }
            }
        }
        /* ====================================================================
        UPDATE EM LOTE (CONDIÇÃO GLOBAL)
        ==================================================================== */
        public int BuffGlobalPorClasse(string classe)
        {
            using (NpgsqlConnection conn = new
           NpgsqlConnection(connBuilder.ToString()))
            {
                conn.Open();
                /* Sobe o nível e o HP de TODOS os que pertencerem a esta classe,
               de uma vez só! */
                string sql = "UPDATE Aventureiros SET nivel = nivel + 1, pontos_vida = pontos_vida + 20 WHERE classe ILIKE @classe";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@classe", classe);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        /* ====================================================================
        DELETE ÚNICO E EM LOTE
        ==================================================================== */
        public int DeletarAventureiro(int id)
        {
            using (NpgsqlConnection conn = new
           NpgsqlConnection(connBuilder.ToString()))
            {
                conn.Open();
                string sql = "DELETE FROM Aventureiros WHERE id = @id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public int LimparNovatos()
        {
            using (NpgsqlConnection conn = new
           NpgsqlConnection(connBuilder.ToString()))
            {
                conn.Open();
                /* Limpeza em lote: Remove todos que estiverem abaixo do nível 5
               */
                string sql = "DELETE FROM Aventureiros WHERE nivel < 5";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        /* ====================================================================
        EXPORTAR PARA CSV (Nuvem -> PC Local)
        ==================================================================== */
        public int ExportarParaCSV(string caminhoArquivo)
        {
            /* Reaproveitamos o nosso próprio método de busca! */
            List<Aventureiro> todos = BuscarTodos();

            /* Usamos o StreamWriter para criar e escrever no arquivo */
            using var writer = new StreamWriter(caminhoArquivo);
            writer.WriteLine("Nome;Classe;Nivel;PontosVida"); /* Cabeçalho */
            foreach (var adv in todos)
            {
                writer.WriteLine($"{adv.Nome};{adv.Classe};{adv.Nivel};{adv.PontosVida}");
            }

            return todos.Count;
        }
        /* ====================================================================
        IMPORTAR DE CSV (PC Local -> Nuvem)
        ==================================================================== */
        public int ImportarDeCSV(string caminhoArquivo)
        {
            if (!File.Exists(caminhoArquivo)) return 0;
            string[] linhas = File.ReadAllLines(caminhoArquivo);
            int cadastrados = 0;
            foreach (string linha in linhas)
            {
                if (linha.StartsWith("Nome")) continue; /* Ignora cabeçalho */
                if (string.IsNullOrWhiteSpace(linha)) continue;
                /* Quebra a linha nas colunas divididas por ponto e vírgula */
                string[] colunas = linha.Split(';');
                Aventureiro novoAdv = new Aventureiro(
                colunas[0],
                colunas[1],
                int.Parse(colunas[2]),
                int.Parse(colunas[3])
                );
                /* Reutilizamos nosso próprio método de inserir! */
                Adicionar(novoAdv);
                cadastrados++;
            }
            return cadastrados;
        }
    } /* Fim da classe GuildaRepository */
}